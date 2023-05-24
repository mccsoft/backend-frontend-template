#!/usr/bin/env node
console.log(process.argv.slice(2));
execSync(
  `npx ts-node ${__filename.replace('.js', '.ts')} ${process.argv
    .slice(2)
    .join(' ')}`,
  {
    stdio: 'inherit',
  },
);
