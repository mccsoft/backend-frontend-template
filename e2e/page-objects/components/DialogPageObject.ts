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
  text = () => this.locator('[data-test-id=dialog-text]').textContent();
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
    expect(dialog.title()).toContain(title);
  }
  if (text) {
    expect(await dialog.text()).toContain(text);
  }

  await dialog.submit();
  await dialog.ensureHidden();
}
