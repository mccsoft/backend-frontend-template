// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom';

if (import.meta.env.DEV) {
  // set loooong timeout in debug (so that it doesn't timeout with breakpoints set)
  jest.setTimeout(10000000);
}
