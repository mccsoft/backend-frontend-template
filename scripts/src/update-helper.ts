import fs from 'fs-extra';
import path from 'path';
export function copyProjectFolder(
  relativePathInsideProject: string,
  options: {
    ignorePattern: string | RegExp | (string | RegExp)[] | undefined;
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
    ignorePattern: string | RegExp | (string | RegExp)[] | undefined;
  } = {
    ignorePattern: undefined,
  },
) {
  const exists = fs.existsSync(src);
  if (!exists) return;

  let ignorePatterns: (string | RegExp)[] = [];
  if (options?.ignorePattern) {
    ignorePatterns = Array.isArray(options.ignorePattern)
      ? options.ignorePattern
      : [options.ignorePattern];
  }

  fs.copySync(src, dest, {
    overwrite: true,
    filter: (src) => {
      for (const ignorePattern of ignorePatterns) {
        if (src.match(ignorePattern)) {
          return false;
        }
      }
      return true;
    },
  });
}

function getPartialFileName(fileName: string) {
  return fileName.replace(
    path.extname(fileName),
    '.partial' + path.extname(fileName),
  );
}

export function searchAndReplaceInFiles(options: {
  relativePath: string;
  fileNameRegex?: RegExp;
  search: string | RegExp;
  replace: string;
}) {
  const { relativePath, fileNameRegex, search, replace } = options;

  if (!fs.existsSync(relativePath)) {
    console.warn(
      `!!! We were about to patch the file '${relativePath}', but it doesn't exist`,
    );
    return;
  }
  const files = readdirRecursiveSync(relativePath);
  for (const file of files) {
    if (!fileNameRegex || file.match(fileNameRegex)) {
      searchAndReplaceInFile(file, search, replace);
    }
  }
}

export function searchAndReplaceInFile(
  relativePath: string,
  search: string | RegExp,
  replace: string,
) {
  const filePath = path.join(process.cwd(), relativePath);
  if (!fs.existsSync(filePath)) {
    console.warn(
      `!!! We were about to patch the file '${relativePath}', but it doesn't exist`,
    );
    return;
  }
  let contents = fs.readFileSync(filePath).toString('utf8');

  contents = contents.replace(search, replace);

  fs.writeFileSync(filePath, contents);
}
export function removePackageReference(
  relativePath: string,
  packageName: string,
) {
  // <PackageReference Include="OpenIddict.AspNetCore" Version="4.2.0" />
  searchAndReplaceInFile(
    relativePath,
    new RegExp(`<PackageReference\s+Include="${packageName}".*?/>`),
    '',
  );
}

export function updatePlaywright(version: string) {
  searchAndReplaceInFile(
    'e2e/package.json',
    /playwright:v.*-/,
    `playwright:v${version}-`,
  );
  searchAndReplaceInFile(
    '.ci/azure-pipelines-template.yml',
    /playwright:v.*-/,
    `playwright:v${version}-`,
  );
}

function readdirRecursiveSync(dirPath: string, result: string[] = []) {
  if (fs.statSync(dirPath).isDirectory())
    fs.readdirSync(dirPath).map((f) => {
      readdirRecursiveSync(path.join(dirPath, f), result);
    });
  else result.push(dirPath);
  return result;
}
export const copyProjectFolderDefaultOptions = {
  ignorePattern: /partial\./,
};
