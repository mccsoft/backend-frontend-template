import fs from 'fs-extra';
import path from 'path';
import semver from 'semver';
import {
  copyProjectFolder,
  copyProjectFolderDefaultOptions,
  searchAndReplaceInFile,
  searchAndReplaceInFiles,
  removePackageReference,
  updatePlaywright,
} from './update-helper.ts';
import type * as TemplateJson from '../../.template.json';
import * as Diff from 'diff';
// current version is stored here
const templateJsonFileName = '.template.json';

const patchVersionRegex = '^\\d\\d\\d\\d-\\d\\d-\\d\\d-\\d\\d';
const updateList = [
  { from: '1.3.0', update: updateFrom_1p3_to_1p4 },
  { from: '1.4.0', update: updateFrom_1p4_to_1p5 },
  { from: '1.5.0', update: updateFrom_1p5_to_1p6 },
  { from: '1.6.0', update: updateFrom_1p6_to_1p7 },
  { from: '1.7.0', update: updateFrom_1p7_to_1p8 },
];

export function updateVersion(prefix: string) {
  const currentFolder = process.cwd();
  const templateFolder = process.cwd() + '_template';

  const currentFolderJsonFileName = path.join(
    currentFolder,
    templateJsonFileName,
  );
  var currentTemplateSettings: typeof TemplateJson = fs.existsSync(
    currentFolderJsonFileName,
  )
    ? JSON.parse(fs.readFileSync(currentFolderJsonFileName).toString())
    : { version: '1.0.0', lastPatch: '0000-00-00-00' };

  var newTemplateSettings: typeof TemplateJson = JSON.parse(
    fs.readFileSync(path.join(templateFolder, templateJsonFileName)).toString(),
  );

  applyUpdates();
  currentTemplateSettings.version = newTemplateSettings.version;
  saveTemplateJson();

  const lastPatch = applyPatches();
  if (lastPatch) {
    currentTemplateSettings.lastPatch = lastPatch.match(patchVersionRegex)![0];
    console.log(`Last applied patch: ${currentTemplateSettings.lastPatch}`);
  }
  saveTemplateJson();

  function saveTemplateJson() {
    fs.writeFileSync(
      currentFolderJsonFileName,
      JSON.stringify(currentTemplateSettings),
    );
  }

  function applyUpdates() {
    console.log('Applying manual updates...');
    const currentVersion = currentTemplateSettings.version;
    for (const update of updateList) {
      if (semver.gte(update.from, currentVersion)) {
        console.log(`Applying manual update for version ${update.from}.`);
        update.update(currentFolder, templateFolder, prefix);
      }
    }
    console.log(`Applying manual update for any version.`);
    updateAll(currentFolder, templateFolder, prefix);
    console.log('Applying manual updates finished.');
  }

  /*
   * Returns last applied patch
   */
  function applyPatches(): string | null {
    console.log(`Applying patches...`);
    var patches = getNewPatches();
    for (const patch of patches) {
      console.log(`Applying patch '${patch}'`);

      const patchContents = fs
        .readFileSync(path.join(templateFolder, 'patches', patch))
        .toString();
      let _err: any = null;
      Diff.applyPatches(patchContents, {
        fuzzFactor: 10,
        autoConvertLineEndings: true,
        loadFile(index, callback) {
          const fullPath = path.join(currentFolder, index.index!);
          if (fs.existsSync(fullPath)) {
            callback(null, fs.readFileSync(fullPath).toString());
          } else {
            callback(null, '');
          }
        },
        patched(index, content, callback) {
          const templateFileName = path.join(templateFolder, index.index!);
          const fileName = path.join(currentFolder, index.index!);

          fs.mkdirSync(path.dirname(fileName), {
            recursive: true,
          });

          if (!content) {
            console.error(
              `Error applying patch '${patch}' to file '${index.index}'. We have overwritten your file. Please check the cahnges carefully!`,
            );
            fs.copyFileSync(templateFileName, fileName);
            callback(null);
            return;
          }

          fs.writeFileSync(fileName, content);
          callback(null);
        },
        complete(err) {
          _err = err;
        },
      });
      if (_err) throw _err;
    }
    console.log(`Finished applying patches.`);
    return patches.length > 0 ? patches[patches.length - 1] : null;
  }

  function getNewPatches(): string[] {
    const files = fs.readdirSync(path.join(templateFolder, 'patches'));
    const filteredFiles = files.filter((x) => {
      if (x.match(patchVersionRegex)) return true;
      console.warn(
        `File with name 'patches/${x}' doesn't match the expected pattern.`,
      );
    });
    const sortedFiles = filteredFiles.sort();
    const filesToApply = sortedFiles.filter(
      (x) => x > (currentTemplateSettings.lastPatch ?? ''),
    );
    return filesToApply;
  }
}
function searchAndReplaceInPackageJson(
  regExp: RegExp | string,
  replacement: string,
) {
  searchAndReplaceInFile('package.json', regExp, replacement);
  searchAndReplaceInFile('frontend/package.json', regExp, replacement);
  searchAndReplaceInFile('e2e/package.json', regExp, replacement);
}

function updateFrom_1p3_to_1p4(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  copyProjectFolder(`webapi/src/${prefix}.App/Features/TestApi`);
  copyProjectFolder(`webapi/tests/${prefix}.App.Tests/TestApiServiceTests.cs`);
  copyProjectFolder(`webapi/src/${prefix}.Http/GeneratedClientOverrides.cs`);
  copyProjectFolder(`webapi/src/${prefix}.Domain/BaseEntity.cs`);
  copyProjectFolder(
    `webapi/src/${prefix}.App/Utils`,
    copyProjectFolderDefaultOptions,
  );
}

function updateFrom_1p4_to_1p5(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  searchAndReplaceInPackageJson(/\"nswag\": \".*?\",/, '');
  searchAndReplaceInPackageJson(
    'nswag openapi2csclient',
    'react-query-swagger openapi2csclient /nswag',
  );

  // required for openiddict 4
  searchAndReplaceInFile(
    'frontend/src/pages/unauthorized/openid/openid-manager.ts',
    'extraTokenParams: { scope: scopes },',
    '',
  );

  searchAndReplaceInFile(
    'webapi/Directory.Build.props',
    '</noWarn>',
    ';1570;1998</noWarn>',
  );
  updatePlaywright('1.33.0');
}

function updateFrom_1p5_to_1p6(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  copyProjectFolder(`frontend/src/application/constants/create-link.ts`);

  copyProjectFolder(`frontend/src/components/sign-url/SignUrlImage.tsx`);
  copyProjectFolder(
    `frontend/src/components/sign-url`,
    copyProjectFolderDefaultOptions,
  );
  fs.removeSync(
    path.join(
      currentFolder,
      `frontend/src/components/sign-url/SignUrlImage.partial.tsx`,
    ),
  );
  copyProjectFolder(
    `frontend/src/components/animations`,
    copyProjectFolderDefaultOptions,
  );
  copyProjectFolder(`frontend/src/helpers`, copyProjectFolderDefaultOptions);

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
  copyProjectFolder(
    'frontend/src/components/uikit/inputs/dropdown/StyledAutocomplete.tsx',
  );
  searchAndReplaceInFiles({
    relativePath: 'frontend/src',
    search: 'helpers/interceptors/auth',
    replace: 'helpers/auth',
  });
  fs.moveSync(
    path.join(currentFolder, 'frontend/src/helpers/interceptors/auth'),
    path.join(currentFolder, 'frontend/src/helpers/auth'),
    {
      overwrite: true,
    },
  );

  searchAndReplaceInFiles({
    relativePath: 'frontend/src',
    search: './pages/unauthorized/openid',
    replace: 'helpers/auth/openid',
  });
  searchAndReplaceInFiles({
    relativePath: 'frontend/src',
    search: 'pages/unauthorized/openid',
    replace: 'helpers/auth/openid',
  });
  searchAndReplaceInFile(
    'frontend/src/pages/unauthorized/LoginPage.tsx',
    './openid/',
    'helpers/auth/openid/',
  );
  searchAndReplaceInFiles({
    relativePath: 'frontend/src',
    search: 'helpers/interceptors/auth',
    replace: 'helpers/auth',
  });
  fs.moveSync(
    path.join(currentFolder, 'frontend/src/pages/unauthorized/openid'),
    path.join(currentFolder, 'frontend/src/helpers/auth/openid'),
    {
      overwrite: true,
    },
  );
  //
}

function updateFrom_1p6_to_1p7(
  currentFolder: string,
  templateFolder: string,
  prefix: string,
) {
  searchAndReplaceInFiles({
    relativePath: 'webapi',
    fileNameRegex: /\.csproj$/,
    search: /<TargetFramework>net\d.*<\/TargetFramework>/,
    replace: '<TargetFramework>net10.0</TargetFramework>',
  });
  searchAndReplaceInFiles({
    relativePath: 'webapi/Directory.Packages.props',
    search: /<PackageVersion Include="(Microsoft.*)" Version="9.0.4"\/>/,
    replace: '<PackageVersion Include="$1" Version="10.0.1"/>',
  });
}

function updateFrom_1p7_to_1p8(
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
  copyProjectFolder('frontend/src/helpers/router/useBlockNavigation.ts');
  copyProjectFolder('frontend/src/helpers/router/useBlocker.ts');
  fs.removeSync(
    path.join(
      currentFolder,
      'frontend/src/helpers/router/useCallbackPrompt.ts',
    ),
  );

  updatePlaywright('1.55.0');
}
