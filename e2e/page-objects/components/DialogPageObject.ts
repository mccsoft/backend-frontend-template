import { LocatorOrPageObject } from 'infrastructure/types';
import { fillInput, waitForFinishedLoadings } from 'infrastructure/helpers';
import { expect, Page } from '@playwright/test';
import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';

export const dialogBaseSelector = '.MuiDialog-root';

export class DialogPageObject extends PageObjectBase {
  constructor(page: LocatorOrPageObject) {
    super(page.locator(dialogBaseSelector));
  }
  getButtons = () => this.region.getByRole('button');
  getButton = (text: string) => this.getButtons().filter({ hasText: text });
  clickButton = (text: string) => this.getButton(text).click();
  title = () => this.locator('.MuiDialogTitle-root');
  textLocator = () => this.locator('[data-test-id=dialog-text]');
  text = () => this.textLocator().textContent();
  closeButton = () =>
    this.title().locator('[data-test-id=dialog-close-button]');
  submitButton = () => this.region.locator('button[type="submit"]');

  async close() {
    await this.closeButton().click();
    await this.ensureHidden();
  }

  async enterText(text: string): Promise<void> {
    await fillInput(this.locator('input'), text);
  }

  async submit(): Promise<void> {
    await this.submitButton().click();
    await waitForFinishedLoadings(this.root);
  }

  async cancel(): Promise<void> {
    await this.locator('button[data-test-id="dialog-cancelButton"]').click();
    await waitForFinishedLoadings(this.root);
  }
}

export async function findDialog(
  page: LocatorOrPageObject,
): Promise<DialogPageObject> {
  return await new DialogPageObject(page).ensureVisible();
}

export async function expectNoDialog(page: LocatorOrPageObject) {
  await new DialogPageObject(page).ensureHidden();
}

export async function expectValidationPopup(
  page: Page,
  title: string | undefined = undefined,
  text: string | undefined = undefined,
) {
  const dialog = await new DialogPageObject(page).ensureVisible();
  if (title) {
    await expect(dialog.title()).toContainText(title);
  }
  if (text) {
    await expect(dialog.textLocator()).toContainText(text);
  }

  await dialog.submit();
  await dialog.ensureHidden();
}
