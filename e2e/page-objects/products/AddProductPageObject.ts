import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';
import { LocatorOrPageObject } from 'infrastructure/types';

export class AddProductPageObject extends PageObjectBase {
  constructor(locator: LocatorOrPageObject) {
    super(locator.locator('[data-test-id="create-product-page"]'));
  }

  title = () => this.findField('Title');
  type = () => this.findField('Type');
  lastStockUpdate = () => this.findField('Last Stock Update');
  submitButton = () =>
    this.region.getByRole('button').filter({ hasText: 'Create' });

  async submit() {
    await this.submitButton().click();
  }

  async fill(data: Partial<AddProductForm>) {
    if (data.title !== undefined) await this.title().fillTextField(data.title);
    if (data.type !== undefined) await this.type().fillTextField(data.type);
    if (data.lastStockUpdate !== undefined)
      await this.lastStockUpdate().fillTextField(data.lastStockUpdate);
  }
}

type AddProductForm = { title: string; type: string; lastStockUpdate: string };
