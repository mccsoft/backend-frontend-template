#!/usr/bin/env node
import yargs from 'yargs';
import fs from 'fs';
import path from 'path';
import { hideBin } from 'yargs/helpers';
import { execSync } from 'child_process';

const args = yargs(hideBin(process.argv))
  .version('0.1')
  .options({
    templateFolder: {
      alias: 't',
      type: 'string',
      description: 'Path to clone template folder',
    },
  })
  .demandOption(['templateFolder'])
  .help()
  .parseSync();

// Here you can do anything to make structure of cloned template look like your project
console.warn('Running pull-template-post-processor script');

const templateFolder = args.templateFolder;

// If you need NOT to upgrade certain files from template, you could easily remove them here, like:
// fs.rmdirSync(path.join(templateFolder, "webapi/MccSoft.TemplateApp.App/Utils"), {
//   recursive: true,
// });

// if you want to move Docs to a subfolder you could uncomment:
// const docsFolder = path.join(templateFolder, 'docs');
// fs.renameSync(docsFolder, docsFolder + '_');
// fs.mkdirSync(docsFolder);
// fs.renameSync(
//   docsFolder + '_',
//   path.join(templateFolder, 'docs/Template'),
// );
