module.exports = {
  parser: '@typescript-eslint/parser', // Specifies the ESLint parser
  plugins: ['@typescript-eslint'],
  parserOptions: {
    ecmaVersion: 2020, // Allows for the parsing of modern ECMAScript features
    sourceType: 'module', // Allows for the use of imports
    ecmaFeatures: {
      jsx: true, // Allows for the parsing of JSX
    },
    project: ['./tsconfig.json'], // Specify it only for TypeScript files
  },
  settings: {},
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/eslint-recommended',
    //'plugin:prettier/recommended', // Enables eslint-plugin-prettier and eslint-config-prettier. This will display prettier errors as ESLint errors. Make sure this is always the last configuration in the extends array.
  ],
  rules: {
    // Place to specify ESLint rules. Can be used to overwrite rules specified from the extended configs
    // e.g. "@typescript-eslint/explicit-function-return-type": "off",
    'react/prop-types': 'off',
    'react/display-name': 'off', // it's sometimes inconvenient
    '@typescript-eslint/no-var-requires': 'off', // since in Electron we have to import styles with require
    '@typescript-eslint/no-use-before-define': 'off', // it's quite usual that we define helper functions somewhere at the bottom
    '@typescript-eslint/explicit-function-return-type': 'off', // we don't have a rule to specify return type for a function
    '@typescript-eslint/no-non-null-assertion': 'off',
    'no-unused-vars': 'off',
    'no-self-assign': 'off',
    'prefer-const': 'off',
    'no-empty-pattern': 'off',
    '@typescript-eslint/no-floating-promises': 'error',
    'no-inner-declarations': 'off',
    '@typescript-eslint/await-thenable': 'error',
  },
};
