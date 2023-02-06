import { Locator, Page } from '@playwright/test';

//export type LocatorOrPageObject = Pick<Locator, 'locator'>;

export type ScreenshotOptions = Parameters<Page['screenshot']>[0];

export type LocatorOrPageObject =
  | Locator
  | Page
  | { region: Locator; locator: Locator['locator'] };

export type BackendInfo = {
  uniqueId: string;
  user: string;
  password: string;
  baseUrl: string;
  /*
   * If tenant is preconfigured we shouldn't try to perform tenant creation and/or execute .resetTenant
   */
  preconfiguredTenant: boolean;

  auth: {
    accessToken: string;
  };

  /*
   * This flag is `true` for first test within a Worker, and `false` for all consequent tests.
   * It is used to optimize setup of the first test
   * (e.g. we don't need to clean up localstorage and reload pages)
   * It's important, because in Debug we almost always run a single test.
   */
  isFirstTest: boolean;
};
