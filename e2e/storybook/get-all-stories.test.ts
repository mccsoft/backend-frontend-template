import { test } from '@playwright/test';
import fs from 'fs';
import { storybookTest } from './storybook-fixture';

storybookTest.describe('Get a list of all stories', () => {
  storybookTest('Get a list of all stories', async ({ page, baseURL }) => {
    await page.goto(baseURL!);
    const componentList: Record<string, string[]> = {};

    const expandAllButton = page.locator("button[data-action='expand-all']");
    await expandAllButton.click();

    const tree = page.locator('#storybook-explorer-tree');
    const componentsLocator = tree.locator('[data-nodetype="component"]');

    const componentNames = await componentsLocator.allTextContents();
    for (const componentName of componentNames) {
      componentList[componentName] = [];

      const componentLocator = componentsLocator.filter({
        hasText: componentName,
      });
      const dataItemId = await componentLocator.getAttribute('data-item-id');

      const stories = tree.locator(
        `//*[@data-nodetype="story" and @data-parent-id="${dataItemId}"]`,
      );
      const count = await stories.count();

      for (let i = 0; i < count; ++i) {
        componentList[componentName].push(
          (await stories.nth(i).getAttribute('data-item-id'))!,
        );
      }
    }

    const pathForpageList = `resources`;
    !fs.existsSync(pathForpageList) &&
      fs.mkdir(`resources`, (err) => {
        if (err) throw err;
      });

    const filePath = `./storybook/componentList.json`;
    if (process.env.CI) {
      const contents = fs.readFileSync(filePath).toString('ascii');
      if (
        JSON.stringify(componentList) !== JSON.stringify(JSON.parse(contents))
      ) {
        throw new Error(
          `Contents of componentList.json is outdated. Please run this test locally and commit the changes to the file. Current component list: ${JSON.stringify(
            componentList,
          )}`,
        );
      }
    }
    fs.writeFile(filePath, JSON.stringify(componentList), function (err) {
      if (err) throw err;
    });
  });
});
