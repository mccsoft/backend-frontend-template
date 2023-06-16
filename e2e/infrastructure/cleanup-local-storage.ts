import type { Page } from '@playwright/test';

export async function cleanupLocalStorage(page: Page) {
  await page.addInitScript((tick: number) => {
    // We use this trick to only reset localStorage once before the test (not on every page reload).
    // `addInitScript` is normally executed after every page reload
    // But we don't want to reset Redux storage if we reload the page during the test
    const key = 'reset_redux_tick';
    const value = window.localStorage.getItem(key);
    if (!value || parseInt(value) < tick) {
      window.localStorage.setItem('i18nextLng', 'en-US');
      const persistedRedux = window.localStorage.getItem('persist:root');
      if (persistedRedux) {
        const persistedReduxAsJson = JSON.parse(persistedRedux);
        delete persistedReduxAsJson['settings'];
        delete persistedReduxAsJson['navigation'];
        delete persistedReduxAsJson['uiConfigurations'];
        window.localStorage.setItem(
          'persist:root',
          JSON.stringify(persistedReduxAsJson),
        );
      }
      window.localStorage.setItem(key, tick.toString());
    }
  }, new Date().getTime());
  // we need to do 'goto' so that `addInitScript` above is executed.
  await page.goto('/');
}
