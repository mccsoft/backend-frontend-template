#!/usr/bin/env node
import yargs from 'yargs';
import fs from 'fs';
import path from 'path';
import { hideBin } from 'yargs/helpers';
import { execSync } from 'child_process';
import semver from 'semver';
import { copyProjectFolder } from './updates/update-helper.js';
import { updateVersion } from './updates/update-version.js';

const args = yargs(hideBin(process.argv))
  .version('0.1')
  .option('name', {
    alias: 'n',
    type: 'string',
    description: 'Name of the project (e.g. StudyApp)',
  })
  .option('company', {
    alias: 'c',
    type: 'string',
    description: 'Name of the root level namespace (e.g. MccSoft)',
  })
  //.demandOption(["name"])
  .help().argv;

if (!fs.existsSync('./scripts/pull-template-changes.js')) {
  console.error('You should run the script from repository root folder');
  process.exit();
}

const currentDir = process.cwd();
if (!currentDir.endsWith('_template')) {
  const detectedInfo = detectProjectAndCompanyName();
  const companyName = args.company || detectedInfo.company;
  const projectName = args.name || detectedInfo.project;

  // if we are run from project folder do the following:
  // 1. Clone template and rename files according to project & company
  // 2. Run pull-template-changes from cloned folder
  const templateFolder = process.cwd() + '_template';

  cloneTemplate(templateFolder);
  renameFilesInTemplate(templateFolder, projectName, companyName);

  execSync(`node scripts/pull-template-changes.js`, {
    cwd: templateFolder,
    stdio: 'inherit',
  });

  process.exit();
}

const templateFolder = process.cwd();
process.chdir(templateFolder.replace('_template', ''));

const detectedInfo = detectProjectAndCompanyName();
const companyName = args.company || detectedInfo.company;
const projectName = args.name || detectedInfo.project;

const prefix = `${companyName}.${projectName}`;

console.log(`ProjectName: ${projectName}, CompanyName: ${companyName}`);
if (!projectName) {
  console.error(
    'Unable to determine the project name. Please, use --name option',
  );
  process.exit();
}

// run post-processor, so each specific project could modify template files before they are copied over
execSync(
  `node scripts/pull-template-post-processor.js --templateFolder "${templateFolder}"`,
);

console.log('Starting to copy files...');
const defaultOptions = {
  ignorePattern: /partial\./,
};
copyProjectFolder(`.ci`);
copyProjectFolder(`scripts`, { ignorePattern: 'pull-template-post-processor' });
copyProjectFolder('webapi/Lib', defaultOptions);
copyProjectFolder('docs');
copyProjectFolder(`webapi/src/${prefix}.Http/GeneratedClientOverrides.cs`);
copyProjectFolder(
  `webapi/tests/${prefix}.ComponentTests/Infrastructure/ComponentTestFixture.cs`,
);
copyProjectFolder(`webapi/src/${prefix}.Domain/BaseEntity.cs`);
copyProjectFolder(`webapi/src/${prefix}.App/Utils`, defaultOptions);
copyProjectFolder(`webapi/src/${prefix}.App/Setup`, defaultOptions);

copyProjectFolder(`frontend/src/application/constants/create-link.ts`);
copyProjectFolder(`frontend/src/components/sign-url`, defaultOptions);
copyProjectFolder(`frontend/src/components/animations`, defaultOptions);
copyProjectFolder(`frontend/src/helpers`, defaultOptions);

syncPacketsInPackageJson('package.json');
syncPacketsInPackageJson('frontend/package.json');
syncPacketsInPackageJson('e2e/package.json');
syncReferencesInProjects(`webapi/src/${prefix}.App/${prefix}.App.csproj`);
syncReferencesInProjects(`webapi/src/${prefix}.Common/${prefix}.Common.csproj`);
syncReferencesInProjects(`webapi/src/${prefix}.Domain/${prefix}.Domain.csproj`);
syncReferencesInProjects(`webapi/src/${prefix}.Http/${prefix}.Http.csproj`);
syncReferencesInProjects(
  `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
);
syncReferencesInProjects(
  `webapi/tests/${prefix}.App.Tests/${prefix}.App.Tests.csproj`,
);
syncReferencesInProjects(
  `webapi/tests/${prefix}.Domain.Tests/${prefix}.Domain.Tests.csproj`,
);
syncReferencesInProjects(
  `webapi/tests/${prefix}.ComponentTests/${prefix}.ComponentTests.csproj`,
);
syncReferencesInProjects(
  `webapi/tests/${prefix}.TestUtils/${prefix}.TestUtils.csproj`,
);
console.log(`folder syncing is finished`);

updateVersion(prefix);
console.log(`finished successfully`);

function renameFilesInTemplate(templateFolder, projectName, companyName) {
  console.log('Calling `yarn install` in template...');
  execSync(`yarn install`, {
    cwd: templateFolder,
  });

  console.log('Renaming files in template...');
  execSync(`yarn rename -n ${projectName} -c ${companyName}`, {
    cwd: templateFolder,
  });
}

function cloneTemplate(folder) {
  if (fs.existsSync(folder)) {
    fs.rmdirSync(folder, { recursive: true });
  }
  execSync(
    `git clone --depth=1 https://github.com/mccsoft/backend-frontend-template.git \"${folder}\"`,
  );
}

function detectProjectAndCompanyName() {
  const regex = /(.*?)\.(.*)\.App/;
  const appFolder = findFileMatching('webapi/src', regex);

  console.log('Found App folder:', appFolder);
  const result = appFolder.match(regex);
  if (!result) return null;

  return {
    company: result[1],
    project: result[2],
  };
}

function findFileMatching(dir, regex) {
  const files = fs.readdirSync(dir);

  for (const file of files) {
    if (file.match(regex)) {
      return file;
    }
  }

  return null;
}

function syncReferencesInProjects(relativePathInsideProject) {
  console.log(`syncReferencesInProjects '${relativePathInsideProject}'`);
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  doSyncReferencesInProjects(copyFrom, copyTo);
}

function doSyncReferencesInProjects(src, dest) {
  if (!fs.existsSync(src) || !fs.existsSync(dest)) return;
  const sourceFileContent = fs.readFileSync(src).toString('utf8');
  let destinationFileContent = fs.readFileSync(dest).toString('utf8');

  const matches = sourceFileContent.matchAll(
    /<PackageReference Include="(.*?)" Version="(.*?)" \/>/gm,
  );
  let addition = '';

  for (const match of matches) {
    const found = destinationFileContent.match(
      `<PackageReference.*?Include="${match[1]}".*?Version="(.*?)".*?\/>`,
    );

    if (found) {
      if (semver.gt(match[2], found[1])) {
        destinationFileContent = destinationFileContent.replace(
          `<PackageReference Include="${match[1]}" Version="${found[1]}" />`,
          `<PackageReference Include="${match[1]}" Version="${match[2]}" />`,
        );
      }
    } else {
      // add package to file
      addition += `<PackageReference Include="${match[1]}" Version="${match[2]}" \/>`;
    }
  }

  if (addition) {
    destinationFileContent = destinationFileContent.replace(
      '</Project>',
      `  <ItemGroup>
${addition}
  </ItemGroup>
</Project>`,
    );
  }

  fs.writeFileSync(dest, destinationFileContent);
}

function syncPacketsInPackageJson(relativePathInsideProject) {
  console.log(`syncPacketsInPackageJson '${relativePathInsideProject}'`);
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  doSyncPacketsInPackageJson(copyFrom, copyTo);
}

function doSyncPacketsInPackageJson(src, dest) {
  if (!fs.existsSync(src) || !fs.existsSync(dest)) return;
  const sourceFileContent = fs.readFileSync(src);
  const destinationFileContent = fs.readFileSync(dest);

  const sourceJson = JSON.parse(sourceFileContent);
  const destJson = JSON.parse(destinationFileContent);
  if (sourceJson.dependencies) {
    Object.keys(sourceJson.dependencies).forEach((key) => {
      try {
        const value = sourceJson.dependencies[key];
        const sourceVersion = semver.coerce(value)?.version;
        const destVersion = semver.coerce(
          destJson.dependencies[key] ?? '',
        )?.version;
        if (!destVersion || semver.gt(sourceVersion, destVersion)) {
          destJson.dependencies[key] = value;
        }
      } catch (e) {
        console.error(`Key: ${key}`);
        throw e;
      }
    });
  }

  if (sourceJson.devDependencies) {
    Object.keys(sourceJson.devDependencies).forEach((key) => {
      try {
        const value = sourceJson.devDependencies[key];
        const sourceVersion = semver.coerce(value)?.version;
        const destVersion = semver.coerce(
          destJson.dependencies?.[key] ?? '',
        )?.version;
        if (!destVersion || semver.gt(sourceVersion, destVersion)) {
          destJson.devDependencies[key] = value;
        }
      } catch (e) {
        console.error(`Key: ${key}`);
        throw e;
      }
    });
  }

  fs.writeFileSync(dest, JSON.stringify(destJson, undefined, 2));
}

function getPartialFileName(fileName) {
  return fileName.replace(
    path.extname(fileName),
    '.partial' + path.extname(fileName),
  );
}
