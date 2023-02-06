import { PageObjectBase } from 'infrastructure/LocatorWithRootBase';
import { waitForFinishedLoadings } from 'infrastructure/helpers';
import { LocatorOrPageObject } from 'infrastructure/types';

export const popupBaseSelector = '[data-test-id=popup-window]';

export class PopupPageObject extends PageObjectBase {
  constructor(page: LocatorOrPageObject, hasSelector?: string) {
    super(
      page.locator(
        popupBaseSelector,
        hasSelector ? { has: page.locator(hasSelector) } : undefined,
      ),
    );
  }

  async getPopupTitle(): Promise<string | null> {
    return await this.locator(
      '[data-test-id=popup-window-title]',
    ).textContent();
  }

  private get content() {
    return this.locator('[data-test-id=popup-window-content]');
  }

  async clickSubmit() {
    await this.content.locator('[type="submit"]').click();
    await waitForFinishedLoadings(this.root);
  }

  async clickSave() {
    await this.clickButton('Save');
  }

  async clickButton(text: string) {
    await this.content.locator(`button:has-text("${text}")`).click();
    await waitForFinishedLoadings(this.root);
  }

  closeIcon = () => this.locator('[data-test-id="popup-closeIcon"]');

  async closeViaXButton() {
    await this.closeIcon().click();
    await this.ensureClosed();
  }

  async ensureVisible() {
    await this.root.waitForSelector(popupBaseSelector);
    await waitForFinishedLoadings(this.root);
    return this;
  }

  async ensureClosed() {
    await this.root.waitForSelector(popupBaseSelector, { state: 'detached' });
  }
}
