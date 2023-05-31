#!/usr/bin/env node
import yargs from 'yargs';
import fs from 'fs';
import path from 'path';
import { XMLParser, XMLBuilder } from 'fast-xml-parser';
import { hideBin } from 'yargs/helpers';
import { execSync } from 'child_process';
import semver from 'semver';
import {
  copyProjectFolder,
  copyProjectFolderDefaultOptions,
} from './updates/update-helper.ts';
import { updateVersion } from './updates/update-version.ts';

const args = yargs(
  hideBin(process.argv).filter(
    (x) =>
      x !== 'ts-node' && x !== '--esm' && x !== 'npx' && !x.includes('.ts'),
  ),
)
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
    },
  })
  //.demandOption(["name"])
  .help()
  .parseSync();

if (!fs.existsSync('./scripts/pull-template-changes.ts')) {
  console.error('You should run the script from repository root folder');
  process.exit();
}

const currentDir = process.cwd();
if (!currentDir.endsWith('_template')) {
  const detectedInfo = detectProjectAndCompanyName();
  const companyName = args.company || detectedInfo?.company;
  const projectName = args.name || detectedInfo?.project;

  if (!companyName) throw new Error('Unable to determine the company name');
  if (!projectName) throw new Error('Unable to determine the project name');

  // if we are run from project folder do the following:
  // 1. Clone template and rename files according to project & company
  // 2. Run pull-template-changes from cloned folder
  const templateFolder = process.cwd() + '_template';

  cloneTemplate(templateFolder);
  renameFilesInTemplate(templateFolder, projectName, companyName);

  console.log(`Calling pull-template-changes from template`);
  execSync(`npx ts-node --esm scripts/pull-template-changes.ts`, {
    cwd: templateFolder,
    stdio: 'inherit',
  });

  process.exit();
}

const templateFolder = process.cwd();
process.chdir(templateFolder.replace('_template', ''));

const detectedInfo = detectProjectAndCompanyName();
const companyName = args.company || detectedInfo?.company;
const projectName = args.name || detectedInfo?.project;
if (!companyName) throw new Error('Unable to determine the company name');
if (!projectName) throw new Error('Unable to determine the project name');

const prefix = `${companyName}.${projectName}`;

console.log(`ProjectName: ${projectName}, CompanyName: ${companyName}`);
if (!projectName) {
  console.error(
    'Unable to determine the project name. Please, use --name option',
  );
  process.exit();
}

// run post-processor, so each specific project could modify template files before they are copied over
if (fs.existsSync('scripts/pull-template-post-processor.ts')) {
  execSync(
    `npx ts-node --esm scripts/pull-template-post-processor.ts --templateFolder "${templateFolder}"`,
  );
} else {
  execSync(
    `node scripts/pull-template-post-processor.js --templateFolder "${templateFolder}"`,
  );
}

console.log('Starting to copy files...');
copyProjectFolder(`.ci`, { ignorePattern: ['_stages', 'partial.'] });
copyProjectFolder(`scripts`, { ignorePattern: 'pull-template-post-processor' });
copyProjectFolder('webapi/Lib', copyProjectFolderDefaultOptions);
copyProjectFolder('docs');
copyProjectFolder(
  `webapi/src/${prefix}.App/Setup`,
  copyProjectFolderDefaultOptions,
);

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
console.log(`Finished successfully`);

function renameFilesInTemplate(
  templateFolder: string,
  projectName: string,
  companyName: string,
) {
  console.log('Calling `yarn install` in template...');
  execSync(`yarn install`, {
    cwd: templateFolder,
  });

  console.log('Renaming files in template...');
  execSync(`yarn rename -n ${projectName} -c ${companyName}`, {
    cwd: templateFolder,
  });
  console.log('Rename finished.');
}

function cloneTemplate(folder: string) {
  if (fs.existsSync(folder)) {
    fs.rmdirSync(folder, { recursive: true });
  }
  execSync(
    `git clone --depth=1 https://github.com/mccsoft/backend-frontend-template.git \"${folder}\"`,
  );
}

function detectProjectAndCompanyName(): {
  company: string;
  project: string;
} | null {
  const regex = /(.*?)\.(.*)\.App/;
  const appFolder = findFileMatching('webapi/src', regex);

  console.log('Found App folder:', appFolder);
  const result = appFolder?.match(regex);
  if (!result) return null;

  return {
    company: result[1],
    project: result[2],
  };
}

function findFileMatching(dir: string, regex: RegExp) {
  const files = fs.readdirSync(dir);

  for (const file of files) {
    if (file.match(regex)) {
      return file;
    }
  }

  return null;
}

export function syncReferencesInProjects(relativePathInsideProject: string) {
  console.log(`syncReferencesInProjects '${relativePathInsideProject}'`);
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  doSyncReferencesInProjects(copyFrom, copyTo);
}

export function doSyncReferencesInProjects(src: string, dest: string) {
  if (!fs.existsSync(src) || !fs.existsSync(dest)) return;
  const sourceFileContent = fs.readFileSync(src).toString('utf8');
  let destinationFileContent = fs.readFileSync(dest).toString('utf8');

  const parser = new XMLParser({ ignoreAttributes: false });
  const sourceXml = parser.parse(sourceFileContent);
  const destinationXml = parser.parse(destinationFileContent);
  const sourcePackageReferences = getPackageReferences(sourceXml);
  const destinationPackageReferences = getPackageReferences(destinationXml);

  let firstItemDestinationGroup = destinationXml.Project?.ItemGroup;
  if (Array.isArray(firstItemDestinationGroup))
    firstItemDestinationGroup = firstItemDestinationGroup[0];

  for (const sourcePackageReference of sourcePackageReferences) {
    const sourceVersion = sourcePackageReference['@_Version'];
    if (!semver.valid(sourceVersion)) continue;
    const found = destinationPackageReferences.find(
      (x) => x['@_Include'] === sourcePackageReference['@_Include'],
    );

    if (found) {
      const destinationVersion = found['@_Version'];
      if (
        semver.valid(destinationVersion) &&
        semver.gt(sourceVersion, destinationVersion)
      ) {
        found['@_Version'] = sourceVersion;
      }
    } else {
      // add package to file
      if (!firstItemDestinationGroup.PackageReference)
        firstItemDestinationGroup.PackageReference = [];
      if (!Array.isArray(firstItemDestinationGroup.PackageReference))
        firstItemDestinationGroup.PackageReference = [
          firstItemDestinationGroup.PackageReference,
        ];

      firstItemDestinationGroup.PackageReference.push(sourcePackageReference);
    }
  }

  const builder = new XMLBuilder({
    ignoreAttributes: false,
    format: true,
    suppressEmptyNode: true,
  });
  destinationFileContent = builder.build(destinationXml);

  fs.writeFileSync(dest, destinationFileContent.replaceAll('&apos;', "'"));
}

function syncPacketsInPackageJson(relativePathInsideProject: string) {
  console.log(`syncPacketsInPackageJson '${relativePathInsideProject}'`);
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  doSyncPacketsInPackageJson(copyFrom, copyTo);
}

function doSyncPacketsInPackageJson(src: string, dest: string) {
  if (!fs.existsSync(src) || !fs.existsSync(dest)) return;
  const sourceFileContent = fs.readFileSync(src);
  const destinationFileContent = fs.readFileSync(dest);

  const sourceJson = JSON.parse(sourceFileContent.toString());
  const destJson = JSON.parse(destinationFileContent.toString());
  if (sourceJson.dependencies) {
    Object.keys(sourceJson.dependencies).forEach((key) => {
      try {
        const value = sourceJson.dependencies[key];
        const sourceVersion = semver.coerce(value)?.version;
        if (sourceVersion) {
          const destVersion = semver.coerce(
            destJson.dependencies[key] ?? '',
          )?.version;
          if (!destVersion || semver.gt(sourceVersion, destVersion)) {
            destJson.dependencies[key] = value;
          }
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
        if (sourceVersion) {
          const destVersion = semver.coerce(
            destJson.dependencies?.[key] ?? '',
          )?.version;
          if (!destVersion || semver.gt(sourceVersion, destVersion)) {
            destJson.devDependencies[key] = value;
          }
        }
      } catch (e) {
        console.error(`Key: ${key}`);
        throw e;
      }
    });
  }

  fs.writeFileSync(dest, JSON.stringify(destJson, undefined, 2));
}

function getPackageReferences(root: any /* csproj parsed as XML*/) {
  const packageReferences: any[] = [];
  let itemGroups = root.Project?.ItemGroup;
  if (!itemGroups) return [];
  if (!Array.isArray(itemGroups)) itemGroups = [itemGroups];
  itemGroups.forEach((x: any) => {
    let groupPackageReferences = x.PackageReference;
    if (!groupPackageReferences) return;
    if (!Array.isArray(groupPackageReferences))
      groupPackageReferences = [groupPackageReferences];
    packageReferences.push(...groupPackageReferences);
  });
  return packageReferences;
}
