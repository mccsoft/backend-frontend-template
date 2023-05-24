#!/usr/bin/env node
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';
const __filename = fileURLToPath(import.meta.url);
execSync(
  `npx ts-node --esm ${__filename.replace('.js', '.ts')} ${process.argv
    .slice(2)
    .join(' ')}`,
  {
    cwd: __filename.replace('pull-template-changes.js', '..'),
    stdio: 'inherit',
  },
);
