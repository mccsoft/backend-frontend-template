import fs from 'fs';
import path from 'path';
import semver from 'semver';
import {
  copyProjectFolder,
  patchFile,
  removePackageReference,
  updatePlaywright,
} from './update-helper.js';

// current version is stored here
const templateJsonFileName = '.template.json';

const updateList = [
  { from: '1.3.0', update: updateFrom_1p3_to_1p4 },
  { from: '1.4.0', update: updateFrom_1p4_to_1p5 },
  { from: '1.5.0', update: updateFrom_1p5_to_1p6 },
];

export function updateVersion(prefix: string) {
  const currentFolder = process.cwd();
  const templateFolder = process.cwd() + '_template';

  const currentFolderJsonFileName = path.join(
    currentFolder,
    templateJsonFileName,
  );
  var currentTemplateSettings = fs.existsSync(currentFolderJsonFileName)
    ? JSON.parse(fs.readFileSync(currentFolderJsonFileName).toString())
    : { version: '1.0.0' };
  var newTemplateSettings = JSON.parse(
    fs.readFileSync(path.join(templateFolder, templateJsonFileName)).toString(),
  );

  process.chdir(templateFolder.replace('_template', ''));

  try {
    const currentVersion = currentTemplateSettings.version;
    for (const update of updateList) {
      if (semver.gte(update.from, currentVersion)) {
        update.update(currentFolder, templateFolder, prefix);
      }
    }
    updateAll(currentFolder, templateFolder, prefix);
  } finally {
    process.chdir(currentFolder);
  }

  currentTemplateSettings.version = newTemplateSettings.version;
  fs.writeFileSync(
    currentFolderJsonFileName,
    JSON.stringify(currentTemplateSettings),
  );
}

function patchPackageJson(regExp: RegExp | string, replacement: string) {
  patchFile('package.json', regExp, replacement);
  patchFile('frontend/package.json', regExp, replacement);
  patchFile('e2e/package.json', regExp, replacement);
}

function updateFrom_1p3_to_1p4(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  copyProjectFolder(`webapi/src/${prefix}.App/Features/TestApi`);
  copyProjectFolder(`webapi/tests/${prefix}.App.Tests/TestApiServiceTests.cs`);
}

function updateFrom_1p4_to_1p5(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  patchPackageJson(/\"nswag\": \".*?\",/, '');
  patchPackageJson(
    'nswag openapi2csclient',
    'react-query-swagger openapi2csclient /nswag',
  );

  // required for openiddict 4
  patchFile(
    'frontend/src/pages/unauthorized/openid/openid-manager.ts',
    'extraTokenParams: { scope: scopes },',
    '',
  );

  patchFile('webapi/Directory.Build.props', '</noWarn>', ';1570;1998</noWarn>');
  updatePlaywright('1.33.0');
}

function updateFrom_1p5_to_1p6(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {}

/*
 * This function is run for every `pull-template-changes`.
 * It makes sense to put all modifications here, and once there's a good number of them,
 * create a new version and move them to versioned update.
 */
function updateAll(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  removePackageReference(
    `webapi/src/${prefix}.App/${prefix}.App.csproj`,
    'OpenIddict.AspNetCore',
  );
  removePackageReference(
    `webapi/src/${prefix}.App/${prefix}.App.csproj`,
    'OpenIddict.EntityFrameworkCore',
  );
  removePackageReference(
    `webapi/src/${prefix}.Domain/${prefix}.Domain.csproj`,
    'Newtonsoft.Json',
  );
  removePackageReference(
    `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
    'Npgsql.EntityFrameworkCore.PostgreSQL',
  );
  removePackageReference(
    `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
    'Npgsql.Json.NET',
  );
  removePackageReference(
    `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
    'Audit.NET',
  );
  removePackageReference(
    `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
    'Audit.EntityFramework.Core',
  );
  removePackageReference(
    `webapi/src/${prefix}.Persistence/${prefix}.Persistence.csproj`,
    'System.IdentityModel.Tokens.Jwt',
  );

  removePackageReference(
    `webapi/Lib/Testing/MccSoft.Testing/MccSoft.Testing.csproj`,
    'Newtonsoft.Json',
  );
  copyProjectFolder('frontend/src/components/uikit/inputs/dropdown/types.ts');
  copyProjectFolder(
    'frontend/src/components/uikit/inputs/dropdown/StyledAutocomplete.tsx',
  );
}
