import fs from 'fs';
import path from 'path';
export function copyProjectFolder(
  relativePathInsideProject: string,
  options: {
    ignorePattern: string | RegExp | string[] | RegExp[] | undefined;
  } = {
    ignorePattern: undefined,
  },
) {
  const templateFolder = process.cwd() + '_template';

  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  console.log(`Copying from '${copyFrom}' to '${copyTo}'`);
  copyRecursively(copyFrom, copyTo, options);
}

function copyRecursively(
  src: string,
  dest: string,
  options: {
    ignorePattern: string | RegExp | string[] | RegExp[] | undefined;
  } = {
    ignorePattern: undefined,
  },
) {
  const exists = fs.existsSync(src);
  if (!exists) return;
  const stats = fs.statSync(src);
  if (stats.isDirectory()) {
    if (!fs.existsSync(dest)) {
      fs.mkdirSync(dest);
    }

    fs.readdirSync(src).forEach(function (childItemName) {
      copyRecursively(
        path.join(src, childItemName),
        path.join(dest, childItemName),
        options,
      );
    });
  } else {
    if (fs.existsSync(dest)) {
      if (src.includes('.partial')) {
        return;
      }
      if (options?.ignorePattern) {
        const ignorePatterns = Array.isArray(options.ignorePattern)
          ? options.ignorePattern
          : [options.ignorePattern];

        for (const ignorePattern of ignorePatterns) {
          if (src.match(ignorePattern)) {
            return;
          }
        }
      }
      const sourceFileContent = fs.readFileSync(src);
      const destinationFileContent = fs.readFileSync(dest);
      if (sourceFileContent !== destinationFileContent) {
        fs.cpSync(src, dest, { force: true, preserveTimestamps: true });
      }
    } else {
      fs.cpSync(src, dest, { force: true, preserveTimestamps: true });
    }
  }
}

function getPartialFileName(fileName: string) {
  return fileName.replace(
    path.extname(fileName),
    '.partial' + path.extname(fileName),
  );
}

export function patchFile(
  relativePath: string,
  search: string | RegExp,
  replace: string,
) {
  const filePath = path.join(process.cwd(), relativePath);
  let contents = fs.readFileSync(filePath).toString('utf8');

  contents = contents.replace(search, replace);

  fs.writeFileSync(filePath, contents);
}
export function removePackageReference(
  relativePath: string,
  packageName: string,
) {
  // <PackageReference Include="OpenIddict.AspNetCore" Version="4.2.0" />
  patchFile(
    relativePath,
    new RegExp(`<PackageReference\s+Include="${packageName}".*?/>`),
    '',
  );
}

export function updatePlaywright(version: string) {
  patchFile('e2e/package.json', /playwright:v.*-/, `playwright:v${version}-`);
  patchFile(
    '.ci/azure-pipelines-template.yml',
    /playwright:v.*-/,
    `playwright:v${version}-`,
  );
}
