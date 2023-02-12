import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';
import { LocatorOrPageObject } from 'infrastructure/types';
import { AddProductPageObject } from './AddProductPageObject';
import { Locator } from '@playwright/test';

export class ProductListPageObject extends PageObjectBase {
  constructor(locator: LocatorOrPageObject) {
    super(locator.locator('[data-test-id="list-product-page"]'));
  }

  async gotoCreateProduct() {
    await this.region.getByText('Create product').click();
    return new AddProductPageObject(this.page).ensureVisible();
  }

  searchField = () => this.region.getByPlaceholder('search');
  async search(search: string) {
    await this.searchField().fill(search);
    await this.waitForLoadings();
  }
  resultsTable = () => new TableRow(this.locator('tr'));
}

export class TableRow {
  region: Locator;
  constructor(locator: Locator) {
    this.region = locator;
  }

  textCell = () => this.region.locator('td').nth(0);
  allTexts = () =>
    this.textCell()
      .allInnerTexts()
      .then((x) => x.map((z) => z?.substring(z.indexOf('.') + 2)));
}
