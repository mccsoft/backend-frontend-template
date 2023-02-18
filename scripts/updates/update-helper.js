import fs from 'fs';
import path from 'path';
export function copyProjectFolder(
  relativePathInsideProject,
  options = { ignorePattern: undefined },
) {
  const templateFolder = process.cwd() + '_template';

  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  console.log(`Copying from '${copyFrom}' to '${copyTo}'`);
  copyRecursively(copyFrom, copyTo, options);
}

function copyRecursively(src, dest, options = { ignorePattern: undefined }) {
  const exists = fs.existsSync(src);
  if (!exists) return;
  const stats = exists && fs.statSync(src);
  const isDirectory = exists && stats.isDirectory();
  if (isDirectory) {
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
    const srcPartialFile = getPartialFileName(src);
    const destPartialFile = getPartialFileName(dest);
    if (fs.existsSync(srcPartialFile) && !fs.existsSync(destPartialFile)) {
      fs.copyFileSync(srcPartialFile, destPartialFile);
    }
    if (fs.existsSync(dest)) {
      if (options?.ignorePattern) {
        if (src.match(options.ignorePattern)) {
          return;
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
