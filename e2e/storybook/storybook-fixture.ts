import { Page, test, expect } from '@playwright/test';

const baseURL = process.env.STORYBOOK_URL ?? '';

export const storybookTest = test.extend({});
storybookTest.use({ baseURL });

export async function openStory(page: Page, storyName: string) {
  const iFrameUrl = '/iframe.html?id=';
  await page.goto(baseURL + iFrameUrl + storyName);

  await expect
    .poll(() => page.locator('#storybook-root').innerHTML())
    .not.toBe('');
}
