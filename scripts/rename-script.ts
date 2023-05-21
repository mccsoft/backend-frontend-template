#!/usr/bin/env node
import Renamer from 'renamer';
import yargs from 'yargs';
import fs from 'fs';
import { hideBin } from 'yargs/helpers';
import replace from 'node-replace';
import {
  camelCase,
  capitalCase,
  constantCase,
  paramCase,
  pascalCase,
  snakeCase,
} from 'change-case';

const args = yargs(hideBin(process.argv))
  .version('0.1')
  .options({
    name: {
      alias: 'n',
      type: 'string',
      description: 'Name of the project (e.g. StudyApp)',
    },
    company: {
      alias: 'c',
      type: 'string',
      description: 'Name of the root level namespace (e.g. MccSoft)',
      default: 'MccSoft',
    },
  })
  .demandOption(['name'])
  .help()
  .parseSync();

const companyName = args.company;
const projectName = args.name;
console.log(`ProjectName: ${projectName}, CompanyName: ${companyName}`);
const frontendPortNumber = Math.round(Math.random() * 1000) + 3100;
console.log(`FrontendPortNumber: ${frontendPortNumber}`);
const backendPortNumber = Math.round(Math.random() * 1000) + 49000;
console.log(`BackendPortNumber: ${backendPortNumber}`);
const storybookPortNumber = Math.round(Math.random() * 1000) + 6006;
console.log(`StorybookPortNumber: ${storybookPortNumber}`);

const replacements = [
  { find: /MccSoft\./g, replace: `${companyName}.` },
  { find: /mccsoft:/g, replace: `${companyName.toLowerCase()}:` },
  { find: `${companyName}.IntegreSql.EF`, replace: `MccSoft.IntegreSql.EF` },
  { find: 'TemplateApp', replace: projectName },
  { find: 'templateapp', replace: projectName.toLowerCase() },
  {
    find: 'templateApp',
    replace:
      camelCase(projectName) /* converts 'ProjectName' to 'projectName'*/,
  },
  {
    find: 'template-app',
    replace:
      paramCase(projectName) /* converts 'projectName' to 'project-name' */,
  },
  {
    find: 'template_app',
    replace:
      snakeCase(projectName) /* converts 'projectName' to 'project-name' */,
  },
  {
    find: 'TemplateApp',
    replace:
      pascalCase(projectName) /* converts 'ProjectName' to 'ProjectName' */,
  },
  {
    find: 'Template App',
    replace:
      capitalCase(projectName) /* converts 'ProjectName' to 'ProjectName' */,
  },
  {
    find: 'TEMPLATE_APP',
    replace:
      constantCase(projectName) /* converts 'ProjectName' to 'Project Name' */,
  },
];

await changeFrontendPortNumber(frontendPortNumber);
await changeBackendPortNumber(backendPortNumber);
await changeStorybookPortNumber(storybookPortNumber);
await changePasswordsInAppsettings();

await fs.rename(
  'webapi/.idea/.idea.MccSoft.TemplateApp',
  `webapi/.idea/.idea.${companyName}.${pascalCase(projectName)}`,
  () => {},
);
await renameFiles(replacements);
await replaceInFiles(replacements);

async function replaceInFiles(
  replacements: { find: string | RegExp; replace: string }[],
) {
  for (const replacement of replacements) {
    replace({
      regex: replacement.find,
      replacement: replacement.replace,
      paths: ['./'],
      exclude: 'node_modules',
      recursive: true,
      silent: true,
    });
  }
}

async function renameFiles(
  replacements: { find: string | RegExp; replace: string }[],
) {
  const renamer = new Renamer();
  for (const replacement of replacements) {
    await renamer.rename({
      files: ['!(node_modules)/**/*'],
      find: replacement.find,
      replace: replacement.replace,
    });
  }
}

function changeFrontendPortNumber(port: string | number) {
  replace({
    regex: /react-query-swagger \/input:https:\/\/localhost\d+;/g,
    replacement: `react-query-swagger /input:https://localhost${port};`,
    paths: ['./e2e/package.json'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /vite preview --port \d+/g,
    replacement: `vite preview --port ${port};`,
    paths: ['./package.json'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /var frontendPort = process.env.PORT \?\? \d+;/g,
    replacement: `var frontendPort = process.env.PORT ?? ${port};`,
    paths: ['./frontend/vite.config.ts'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /<SpaProxyServerUrl>https:\/\/localhost:\d+<\/SpaProxyServerUrl>/,
    replacement: `<SpaProxyServerUrl>https://localhost:${port}</SpaProxyServerUrl>`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/MccSoft.TemplateApp.App.csproj',
    ],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /https:\/\/localhost:\d+/g,
    replacement: `https://localhost:${port}`,
    paths: ['./webapi/src/MccSoft.TemplateApp.App/appsettings.json'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /https:\/\/localhost:\d+/g,
    replacement: `https://localhost:${port}`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/appsettings.Development.json',
    ],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /BASE_URL=https:\/\/localhost:\d+/g,
    replacement: `BASE_URL=https://localhost:${port}`,
    paths: ['./e2e/.env.local_sample'],
    silent: true,
    recursive: true,
  });
}

function changeBackendPortNumber(httpsPort: number) {
  const nonHttpsPort = httpsPort + 1;

  replace({
    regex: /https:\/\/localhost:(\d+)/g,
    replacement: `https://localhost:${httpsPort}`,
    paths: ['./frontend/vite.config.ts'],
    silent: true,
  });
  replace({
    regex: /"Url": "http:\/\/\*:\d+"/g,
    replacement: `"Url": "http://*:${nonHttpsPort}"`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/appsettings.Development.json',
    ],
    silent: true,
  });
  replace({
    regex: /"Url": "https:\/\/\*:\d+"/g,
    replacement: `"Url": "https://*:${httpsPort}"`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/appsettings.Development.json',
    ],
    silent: true,
  });
  replace({
    regex: /"https:\/\/localhost:\d+"/g,
    replacement: `"https://localhost:${httpsPort}"`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/Properties/launchSettings.json',
    ],
    silent: true,
  });
  replace({
    regex: /"https:\/\/\*:\d+"/g,
    replacement: `"https://*:${httpsPort}"`,
    paths: [
      './webapi/src/MccSoft.TemplateApp.App/Properties/launchSettings.json',
    ],
    silent: true,
  });
  replace({
    regex: /"proxy": "https:\/\/localhost:\d+"/g,
    replacement: `"proxy": "http://localhost:${nonHttpsPort}"`,
    paths: ['./frontend/package.json'],
    silent: true,
  });
  replace({
    regex: /\/input:https:\/\/localhost:\d+/g,
    replacement: `/input:https://localhost:${httpsPort}`,
    paths: ['./frontend/package.json'],
    silent: true,
  });
}

function changeStorybookPortNumber(port: number) {
  replace({
    regex: /start-storybook -p \d+/g,
    replacement: `start-storybook -p ${port}`,
    paths: ['./frontend/package.json'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /STORYBOOK_URL=http:\/\/localhost:\d+/g,
    replacement: `STORYBOOK_URL=http://localhost:${port}`,
    paths: ['./e2e/.env'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /STORYBOOK_URL=http:\/\/localhost:\d+/g,
    replacement: `STORYBOOK_URL=http://localhost:${port}`,
    paths: ['./e2e/.env.local_sample'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /STORYBOOK_URL=http:\/\/host.docker.internal:\d+/g,
    replacement: `STORYBOOK_URL=http://host.docker.internal:${port}`,
    paths: ['./e2e/package.json'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /storybook-static -p \d+/g,
    replacement: `storybook-static -p ${port}`,
    paths: ['./e2e/playwright.config.ts'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /port:\d+/g,
    replacement: `port:${port}`,
    paths: ['./e2e/playwright.config.ts'],
    silent: true,
    recursive: true,
  });
  replace({
    regex: /storybook dev -p \d+/g,
    replacement: `storybook dev -p ${port}`,
    paths: ['./frontend/package.json'],
    silent: true,
    recursive: true,
  });
}

function changePasswordsInAppsettings() {
  replace({
    regex: /"DashboardPassword": "(.*?)"/,
    replacement: `"DashboardPassword": "${generatePassword(12)}"`,
    paths: ['./webapi/src/MccSoft.TemplateApp.App/appsettings.json'],
    silent: true,
  });
  replace({
    regex: /("DefaultUser".*?"Password": ").*?"/gims,
    replacement: `$1${generatePassword(12)}"`,
    paths: ['./webapi/src/MccSoft.TemplateApp.App/appsettings.json'],
    silent: true,
  });
}

function generatePassword(length: number) {
  var result = '';
  var characters =
    'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  var charactersLength = characters.length;
  for (var i = 0; i < length; i++) {
    result += characters.charAt(Math.floor(Math.random() * charactersLength));
  }
  return result;
}
