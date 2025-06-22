import { expect } from '@playwright/test';
import { authenticatedTest } from 'infrastructure/fixtures';
import { MainPageObject } from 'page-objects/MainPageObject';

authenticatedTest.describe('login', () => {
  authenticatedTest(
    'Wrong password - error is shown',
    async ({ anotherUserPageWorker, backendInfo }) => {
      await anotherUserPageWorker.logIn(
        backendInfo.user,
        backendInfo.password + '1',
      );
      await expect(anotherUserPageWorker.region).toContainText(
        'Invalid login or password',
      );
    },
  );

  authenticatedTest(
    'Correct password - main page is shown',
    async ({ anotherUserPageWorker, backendInfo }) => {
      await anotherUserPageWorker.logIn(backendInfo.user, backendInfo.password);
      await anotherUserPageWorker.ensureHidden();
      await new MainPageObject(anotherUserPageWorker.page).ensureVisible();
    },
  );
});
