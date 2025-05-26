import { expect, Page } from '@playwright/test';
import componentList from './componentList.json';
import { openStory, storybookTest } from './storybook-fixture';

for (const [component, stories] of Object.entries(componentList)) {
  storybookTest.describe(`Smart tests for component: ${component}`, () => {
    for (const story of stories) {
      const storyName = story.split('--')[1];
      storybookTest(
        `Smart test for story: ${storyName}`,
        async ({ page, browserName }, testInfo) => {
          function getScreenshotName(postfix?: string) {
            return `${component}-${story.split('--')[1]}-${
              postfix ? postfix : ''
            }.png`;
          }
          await openStory(page, story);

          expect(await page.screenshot()).toMatchSnapshot(
            getScreenshotName('1'),
          );

          await clickOnSingleButtonIfExists(page, getScreenshotName('2'));
        },
      );
    }
  });
}

async function clickOnSingleButtonIfExists(page: Page, screenshotName: string) {
  const singleButtonLocator = page.getByRole('button');
  if ((await singleButtonLocator.count()) === 1) {
    await singleButtonLocator.click();

    await page.waitForTimeout(1000);

    expect(await page.screenshot({ animations: 'disabled' })).toMatchSnapshot(
      screenshotName,
    );
  }
}
