import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';

export class MainPageObject extends PageObjectBase {
  logOutButton = () =>
    this.region.getByRole('button').filter({ hasText: 'Log Out' });

  async logOut() {
    await this.logOutButton().click();
    await this.waitForLoadings();
    await this.ensureHidden();
  }
}
