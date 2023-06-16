import { MainPageObject } from 'page-objects/MainPageObject';
import { authenticatedTest } from './fixtures';
import { initializeBackendForConsequentTests } from './initialize-backend';
import { cleanupLocalStorage } from './cleanup-local-storage';

export function cacheDataBeforeAll(
  testType: typeof authenticatedTest,
  initFunction: (args: { mainPage: MainPageObject }) => Promise<unknown>,
) {
  testType.beforeAll(async ({ mainPageWorker, backendInfo }) => {
    const page = mainPageWorker.page;
    if (!backendInfo.isFirstTest) {
      await initializeBackendForConsequentTests(backendInfo);
      await cleanupLocalStorage(page);
    }

    await initFunction({ mainPage: mainPageWorker });
  });
  testType.beforeEach(({ backendInfo }) => {
    // this is to make `mainPage` fixture skip the setup (cleanup tenant)
    // we need to skip cleanup to preserve the data created in beforeAll above
    backendInfo.isFirstTest = true;
  });
  testType.afterAll(({ backendInfo }) => {
    backendInfo.isFirstTest = false;
  });
}
