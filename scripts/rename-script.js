#!/usr/bin/env node
execSync(`npx ts-node ${__filename.replace('.js', '.ts')}`, {
  stdio: 'inherit',
});
