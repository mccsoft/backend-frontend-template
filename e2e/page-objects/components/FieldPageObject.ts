import { LocatorOrPageObject } from 'infrastructure/types';
import { expect, Locator } from '@playwright/test';
import { blurField, fillInput } from 'infrastructure/helpers';
import { DropDownPageObject } from './DropDownPageObject';

export const fieldBaseSelector = (title: string) =>
  title ? `[data-app-field="${title}"]` : `[data-app-field]`;

export class FieldPageObject {
  region: Locator;
  private fieldTitle: string;
  locator: Locator['locator'];

  constructor(page: LocatorOrPageObject, selector: string) {
    this.region = page.locator(selector);
    this.fieldTitle = selector;
    this.locator = this.region.locator.bind(this.region);
  }

  public async focusInput() {
    await this.input.click();
  }

  public async toggleCheckbox() {
    await this.label.click();
  }
  public async setCheckbox(value: boolean) {
    const currentValue = await this.getCheckboxValue();
    if (currentValue !== value) {
      await this.toggleCheckbox();
    }
  }
  public async getCheckboxValue(): Promise<boolean> {
    return await this.input.isChecked();
  }

  get customAction() {
    return this.region.locator('[data-test-id=field-link]');
  }

  public async getCustomActionText() {
    return await this.customAction.innerText();
  }

  /*
  Some fields have 'action' button at top-right corner.
  This function finds it and clicks on it.
   */
  public async executeCustomAction() {
    await this.customAction.click();
  }

  /*
   * returns text value, NOT the value of an Input.
   * Text value is used when form is in non-editable state (i.e. there's no Input and just a text value is shown)
   */
  public async getTextValue(): Promise<string | null> {
    return await this.textValueLocator().textContent();
  }

  textValueLocator = () => this.region.locator('> div').last();

  /*
   * expects a text to be equal to some value.
   * this is NOT the value of an Input.
   * Text value is used when form is in non-editable state (i.e. there's no Input and just a text value is shown)
   */
  public async expectTextValue(value: string) {
    await expect(this.textValueLocator()).toHaveText(value, { timeout: 5000 });
  }

  public async expectTextValueToContain(value: string) {
    await expect(this.textValueLocator()).toContainText(value);
  }

  public async fillTextField(value: string, noBlur = false): Promise<void> {
    // this will select all to make sure that previous value is removed
    await fillInput(this.input, value);
    if (!noBlur) {
      await blurField(this.input);
    }
  }

  public async selectDropDownValue(value: string): Promise<void> {
    await this.dropDown.selectDropDownValue(value);
  }

  public async getSelectValue(): Promise<string> {
    return await this.dropDown.getSelectValue();
  }

  public async expectSelectValueToMatch(value: string) {
    await expect.poll(() => this.getSelectValue()).toMatch(value);
  }

  public async getInputValue(): Promise<string> {
    return await this.input.inputValue();
  }

  public async expectInputValue(value: string) {
    await expect.poll(() => this.getInputValue()).toBe(value);
  }

  public get dropDown() {
    return new DropDownPageObject(this.region.locator('.MuiAutocomplete-root'));
  }

  public get input() {
    return this.region.locator('input');
  }

  public get label() {
    return this.region.locator('label');
  }

  error = () => this.region.locator('div[data-error=true]').last();
  public expectNoError() {
    return expect(this.error()).toBeHidden();
  }

  async blur() {
    await blurField(this.input);
  }

  public async click() {
    await this.region.click();
  }

  async ensureOpened(options?: WaitOptions): Promise<FieldPageObject> {
    await expect(this.region).toBeVisible(options);
    await this.region.elementHandle(options);

    return this;
  }

  async ensureHidden(options?: WaitOptions): Promise<void> {
    await expect(this.region).toBeHidden(options);
  }
}

export function findField(
  page: LocatorOrPageObject,
  title: string,
): FieldPageObject {
  return new FieldPageObject(page, fieldBaseSelector(title));
}

export function findFieldByTestId(
  page: LocatorOrPageObject,
  testId: string,
): FieldPageObject {
  return new FieldPageObject(page, `[data-test-id="${testId}"]`);
}
export function findInputByPlaceholder(
  page: LocatorOrPageObject,
  placeholder: string,
) {
  return page.locator(
    `input[placeholder='${placeholder}'], textarea[placeholder='${placeholder}']`,
  );
}

type WaitOptions = {
  timeout?: number;
};
