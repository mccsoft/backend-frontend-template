#!/usr/bin/env node
import { execSync } from 'child_process';
execSync(`npx ts-node ${__filename.replace('.js', '.ts')}`, {
  cwd: __filename.replace('pull-template-changes.js', '..'),
  stdio: 'inherit',
});
