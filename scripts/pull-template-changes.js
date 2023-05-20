#!/usr/bin/env node
execSync(`npx ts-node ${__filename.replace('.js', '.ts')}`, {
  cwd: __filename.replace('pull-template-changes.js', '..'),
  stdio: 'inherit',
});
