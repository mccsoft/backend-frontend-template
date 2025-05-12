import { Page, test } from '@playwright/test';
import { MainPageObject } from 'page-objects/MainPageObject';
import { LoginPageObject } from 'page-objects/LoginPageObject';
import {
  initializeBackendForConsequentTests,
  initializeBackendForFirstTest,
} from 'infrastructure/initialize-backend';
import { BackendInfo } from 'infrastructure/types';
import * as os from 'os';
import path from 'path';
import { cleanupLocalStorage } from './cleanup-local-storage';

const base = test.extend({
  baseURL: process.env.BASE_URL,
  contextOptions: {
    timezoneId: 'UTC',
    ignoreHTTPSErrors: true,
  },
});

type WorkerFixtures = {
  backendInfo: BackendInfo;
  mainPageWorker: MainPageObject;
  anotherUserPageWorker: LoginPageObject;
};

export const authenticatedTest = base
  .extend<{}, WorkerFixtures>({
    backendInfo: [
      async ({}, use, workerInfo) => {
        const uniqueId =
          (process.env.UNIQUE_ID || os.hostname()) +
          '1' +
          workerInfo.workerIndex +
          '-' +
          workerInfo.parallelIndex;

        const backendInfo: BackendInfo = {
          uniqueId: uniqueId,
          user: 'admin_' + uniqueId + '@mcc-soft.de',
          password: 'passworD1!',
          baseUrl: process.env.BASE_URL!,
          preconfiguredTenant: false,
          auth: { accessToken: '' },
          isFirstTest: true,
        };

        if (!backendInfo.preconfiguredTenant) {
          base.setTimeout(350000);
          const date = new Date().getTime();

          await initializeBackendForFirstTest(backendInfo);
          const initializationTime = new Date().getTime() - date;
          console.log('initializationTime', initializationTime);
        }

        await use(backendInfo);
      },
      { scope: 'worker' },
    ],
    /*
     * This is an authenticated page. Please never log out from it.
     * If you want to test login/logout please use `page` fixture
     */
    mainPageWorker: [
      async ({ backendInfo, browser }, use) => {
        const page = await browser.newPage({
          viewport: {
            width: 1280,
            height: 800,
          },
          baseURL: backendInfo.baseUrl,
        });

        await setTimeTo(page, '2021-07-21T14:40:00.000');

        await page.goto('/');

        const loginPage = new LoginPageObject(page);
        await loginPage.logIn(backendInfo.user, backendInfo.password);

        const mainPage = await new MainPageObject(page).ensureVisible();
        await use(mainPage);

        await browser.close();
      },
      { scope: 'worker' },
    ],
    /*
     * This is an authenticated page. Please never log out from it.
     * If you want to test login/logout please use `page` fixture
     */
    anotherUserPageWorker: [
      async ({ backendInfo, browser }, use) => {
        const page = await browser.newPage({
          viewport: {
            width: 1280,
            height: 800,
          },
          baseURL: backendInfo.baseUrl,
        });

        await setTimeTo(page, '2021-07-21T14:40:00.000');

        await page.goto('/');

        const loginPage = new LoginPageObject(page);

        await use(loginPage);

        await browser.close();
      },
      { scope: 'worker' },
    ],
  })
  .extend<{ mainPage: MainPageObject; anotherUserPage: LoginPageObject }>({
    mainPage: async ({ mainPageWorker, backendInfo }, use) => {
      const page = mainPageWorker.page;
      if (!backendInfo.isFirstTest) {
        await initializeBackendForConsequentTests(backendInfo);
        await cleanupLocalStorage(page);
      }

      await use(mainPageWorker);

      backendInfo.isFirstTest = false;
    },
    anotherUserPage: async ({ anotherUserPageWorker }, use) => {
      const page = anotherUserPageWorker.page;

      await use(anotherUserPageWorker);

      const mainPage = new MainPageObject(page);

      await mainPage.logOut();
    },
  });

async function setTimeTo(page: Page, time: string) {
  const date = new Date(time);
  await page.addInitScript({
    path: path.join(__dirname, '../..', './node_modules/sinon/pkg/sinon.js'),
  });
  await page.addInitScript((dt: number) => {
    // @ts-ignore
    window.__clock = sinon.useFakeTimers({
      now: dt,
      shouldAdvanceTime: true,
    });
  }, date.getTime() - date.getTimezoneOffset() * 60000);
}
