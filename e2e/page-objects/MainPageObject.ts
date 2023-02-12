import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';
import { LocatorOrPageObject } from 'infrastructure/types';
import { ProductListPageObject } from './products/ProductListPageObject';

export class MainPageObject extends PageObjectBase {
  constructor(locator: LocatorOrPageObject) {
    super(locator.locator('[data-test-id="main-page-container"]'));
  }

  productList = () => new ProductListPageObject(this.page);

  logOutButton = () =>
    this.region.getByRole('button').filter({ hasText: 'Log Out' });

  async logOut() {
    await this.logOutButton().click();
    await this.waitForLoadings();
    await this.ensureHidden();
  }
}
