import { expect, test } from 'vitest';
import fs from 'fs';

const allowedMissingVariables: string[] = ['EMAIL'];

const configMapFile = '../k8s/aks-rancher/02-configMaps.yaml';
const configMapFileContents = fs.readFileSync(configMapFile, 'utf-8');

test.each(['prod'])(
  'Ensure all variables defined in %s.env are present in dev.env',
  (stage) => {
    const variables = extractVariables(stage).map((x) => x.key);
    const todayVariables = extractVariables('dev').map((x) => x.key);

    expect(variables).toEqual(todayVariables);
  },
);

test.each(['dev', 'prod'])(
  'Ensure all variables defined in %s.env are present in 02-config.yaml',
  (stage) => {
    const variables = extractVariables(stage);

    const missingVariables = variables
      .filter(({ key }) => !configMapFileContents.includes(`\${${key}}`))
      .map((x) => x.key);

    expect(missingVariables).toEqual(allowedMissingVariables);
  },
);

function extractVariables(stage: string) {
  const envFile = `../k8s/aks-rancher/stages/${stage}.env`;
  const envFileContents = fs.readFileSync(envFile, 'utf-8').split('\n');
  const configMapsIndex = envFileContents.findIndex((x) =>
    x.includes('configMaps.yaml'),
  );

  const variables = envFileContents
    .slice(configMapsIndex + 1)
    .filter(Boolean)
    .filter((x) => !x.startsWith('#'))
    .map((line) => line.replace('export ', ''))
    .map((line) => line.split('='))
    .map((line) => ({
      key: line[0].trim(),
      // trim quotes from value
      value: line[1].trim().replace(/^['"]|['"]$/g, ''),
    }));
  return variables;
}
