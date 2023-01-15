import { wrapInModal, wrapInRouter } from './wrapper';

export const defaultExclude = [
  'className',
  'onClick',
  'style',
  'testId',
  'id',
  'error',
];

export const parameters = {
  actions: { argTypesRegex: '^on[A-Z].*' },
  controls: {
    matchers: {
      color: /(background|color)$/i,
      date: /Date$/,
    },
    exclude: defaultExclude,
  },
};

export const decorators = [wrapInRouter, wrapInModal];
