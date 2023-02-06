import { LocatorOrPageObject } from 'infrastructure/types';
import { fillInput, waitForFinishedLoadings } from 'infrastructure/helpers';
import { expect, Page } from '@playwright/test';
import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';

export const dialogBaseSelector = '[data-test-id=dialog-container]';

export class DialogPageObject extends PageObjectBase {
  constructor(page: LocatorOrPageObject) {
    super(page.locator(dialogBaseSelector));
  }
  getButtons = () => this.locator('button');
  getButton = (text: string) => this.getButtons().filter({ hasText: text });
  clickButton = (text: string) => this.getButton(text).click();
  getTitle = () => this.locator('[data-test-id=dialog-title]').textContent();
  getText = () => this.locator('[data-test-id=dialog-text]').textContent();

  async enterText(text: string): Promise<void> {
    await fillInput(this.locator('input'), text);
  }

  async submit(): Promise<void> {
    await this.locator('button[data-test-id="dialog-confirmButton"]').click();
    await waitForFinishedLoadings(this.root);
  }

  async cancel(): Promise<void> {
    await this.locator('button[data-test-id="dialog-cancelButton"]').click();
    await waitForFinishedLoadings(this.root);
  }

  async ensureVisible() {
    await this.root.waitForSelector(dialogBaseSelector);
    await waitForFinishedLoadings(this.root);
    return this;
  }

  async ensureClosed(): Promise<void> {
    await this.root.waitForSelector(dialogBaseSelector, { state: 'detached' });
  }
}

export async function findDialog(
  page: LocatorOrPageObject,
): Promise<DialogPageObject> {
  return await new DialogPageObject(page).ensureVisible();
}

export async function expectNoDialog(page: LocatorOrPageObject) {
  await expect(page.locator(dialogBaseSelector)).toBeHidden();
}

export async function expectValidationPopup(
  page: Page,
  title: string | undefined = undefined,
  text: string | undefined = undefined,
) {
  const dialog = await new DialogPageObject(page).ensureVisible();
  if (title) {
    expect(await dialog.getTitle()).toContain(title);
  }
  if (text) {
    expect(await dialog.getText()).toContain(text);
  }

  await dialog.submit();
  await dialog.ensureClosed();
}
