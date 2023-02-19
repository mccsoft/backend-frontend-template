import { expect } from '@playwright/test';
import { authenticatedTest } from 'infrastructure/fixtures';

authenticatedTest.describe('products', () => {
  authenticatedTest('create product - validation', async ({ mainPage }) => {
    const createProduct = await mainPage.productList().gotoCreateProduct();
    await createProduct.submit();
    await expect(createProduct.title().error()).toHaveText('Required');
  });

  authenticatedTest(
    'Create product - make sure its displayed in List',
    async ({ mainPage }) => {
      const createProduct = await mainPage.productList().gotoCreateProduct();
      await createProduct.fill({ title: 'qwe' });
      await createProduct.submit();

      await createProduct.ensureHidden();

      const productList = await mainPage.productList().ensureVisible();

      await expect
        .poll(() => productList.resultsTable().allTexts(), {
          timeout: 5000,
        })
        .toStrictEqual(['qwe (Undefined) - 01/01/0001']);
    },
  );
});
