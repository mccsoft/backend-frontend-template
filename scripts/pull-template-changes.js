#!/usr/bin/env node
import yargs from "yargs";
import fs from "fs";
import path from "path";
import {hideBin} from "yargs/helpers";
import {execSync} from 'child_process'

const args = yargs(hideBin(process.argv))
    .version("0.1")
    .option("name", {
      alias: "n",
      type: "string",
      description: "Name of the project (e.g. StudyApp)",
    })
    //.demandOption(["name"])
    .help().argv;

if (!fs.existsSync("./scripts/pull-template-changes.js")) {
  console.error('You should run the script from repository root folder');
  process.exit();
}

const templateFolder = process.cwd() + "_template";

const projectName = args.name || detectProjectName();
console.log(`ProjectName: ${projectName}`);
if (!projectName) {
  console.error('Unable to determine the project name. Please, use --name option');
  process.exit();
}

cloneTemplate(templateFolder);
renameFilesInTemplate(templateFolder, projectName);

// run post-processor, so each specific project could modify template files before they are copied over
execSync(`node scripts/pull-template-post-processor.js --templateFolder "${templateFolder}"`)

console.log('Starting to copy files...');
copyProjectFolder(`scripts/pull-template-changes.js`);
copyProjectFolder("webapi/Lib");
copyProjectFolder("docs");
copyProjectFolder(`webapi/src/MccSoft.${projectName}.Http/GeneratedClientOverrides.cs`);
copyProjectFolder(`webapi/src/MccSoft.${projectName}.App/Utils`);
copyProjectFolder(`webapi/src/MccSoft.${projectName}.App/Setup`, {
  ignorePattern: /partial\.cs/
});

process.exit();

function copyProjectFolder(relativePathInsideProject, options = {ignorePattern: undefined}) {
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  console.log(`Copying from '${copyFrom}' to '${copyTo}'`);
  copyRecursively(copyFrom, copyTo, {recursive: true});
}

function renameFilesInTemplate(templateFolder, projectName) {
  console.log('Calling `yarn install` in template...');
  execSync(`yarn install`, {
    cwd: templateFolder
  });

  console.log('Renaming files in template...');
  execSync(`yarn rename -n ${projectName}`, {
    cwd: templateFolder
  });
}

function cloneTemplate(folder) {
  if (fs.existsSync(folder)) {
    fs.rmdirSync(folder, {recursive: true});
  }
  execSync(`git clone https://github.com/mcctomsk/backend-frontend-template.git ${templateFolder}`)
}

function detectProjectName() {
  const regex = /MccSoft\.(.*)\.sln/
  const solutionFile = findFileMatching('webapi', regex);
  if (!solutionFile)
    return null;

  console.log('Found solution file:', solutionFile);
  const result = solutionFile.match(regex);
  if (!result)
    return null;

  return result[1];
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

function copyRecursively(src, dest, options = {ignorePattern: undefined}) {
  var exists = fs.existsSync(src);
  var stats = exists && fs.statSync(src);
  var isDirectory = exists && stats.isDirectory();
  if (isDirectory) {
    if (!fs.existsSync(dest)) {
      fs.mkdirSync(dest);
    }

    fs.readdirSync(src).forEach(function (childItemName) {
      copyRecursively(path.join(src, childItemName),
          path.join(dest, childItemName));
    });
  } else {
    if (options?.ignorePattern) {
      if (src.match(options.ignorePattern)) {
        return;
      }
    }
    if (fs.existsSync(dest)) {
      const sourceFileContent = fs.readFileSync(src);
      const destinationFileContent = fs.readFileSync(dest);
      if (sourceFileContent !== destinationFileContent) {
        fs.cpSync(src, dest, {force: true, preserveTimestamps: true});
      }
    } else {
      fs.cpSync(src, dest, {force: true, preserveTimestamps: true});
    }
  }
}
