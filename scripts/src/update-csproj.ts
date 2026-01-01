import fs from 'fs';
import path from 'path';
import { XMLParser, XMLBuilder } from 'fast-xml-parser';
import semver from 'semver';

let templateFolder = '';
export function setTemplateFolder(folder: string) {
  templateFolder = folder;
}

export function syncReferencesDirectoryPackagesProps(
  relativePathInsideProject: string,
) {
  console.log(
    `syncReferencesDirectoryPackagesProps '${relativePathInsideProject}'`,
  );
  const copyFrom = path.join(templateFolder, relativePathInsideProject);
  const copyTo = path.join(process.cwd(), relativePathInsideProject);
  doSyncReferencesDirectoryPackagesProps(copyFrom, copyTo);
}

export function doSyncReferencesDirectoryPackagesProps(
  src: string,
  dest: string,
) {
  if (!fs.existsSync(src) || !fs.existsSync(dest)) return;
  const sourceFileContent = fs.readFileSync(src).toString('utf8');
  let destinationFileContent = fs.readFileSync(dest).toString('utf8');

  const parser = new XMLParser({ ignoreAttributes: false });
  const sourceXml = parser.parse(sourceFileContent);
  const destinationXml = parser.parse(destinationFileContent);
  const projectPackageVersions = getPackageVersions(sourceXml);
  const destinationPackageVersions = getPackageVersions(destinationXml);

  let firstItemDestinationGroup = destinationXml.Project?.ItemGroup;
  if (Array.isArray(firstItemDestinationGroup))
    firstItemDestinationGroup = firstItemDestinationGroup[0];

  for (const sourcePackageVersion of projectPackageVersions) {
    const sourceVersion = sourcePackageVersion['@_Version'];
    if (!semver.valid(sourceVersion)) continue;
    const found = destinationPackageVersions.find(
      (x) => x['@_Include'] === sourcePackageVersion['@_Include'],
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
      if (!firstItemDestinationGroup.PackageVersion)
        firstItemDestinationGroup.PackageVersion = [];
      if (!Array.isArray(firstItemDestinationGroup.PackageVersion))
        firstItemDestinationGroup.PackageVersion = [
          firstItemDestinationGroup.PackageVersion,
        ];

      firstItemDestinationGroup.PackageVersion.push(sourcePackageVersion);
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

    if (!found) {
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

function getPackageVersions(root: any /* csproj parsed as XML*/) {
  const packageVersions: any[] = [];
  let itemGroups = root.Project?.ItemGroup;
  if (!itemGroups) return [];
  if (!Array.isArray(itemGroups)) itemGroups = [itemGroups];
  itemGroups.forEach((x: any) => {
    let groupPackageVersions = x.PackageVersion;
    if (!groupPackageVersions) return;
    if (!Array.isArray(groupPackageVersions))
      groupPackageVersions = [groupPackageVersions];
    packageVersions.push(...groupPackageVersions);
  });
  return packageVersions;
}
