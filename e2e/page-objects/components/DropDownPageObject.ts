import { LocatorOrPageObject } from 'infrastructure/types';
import { getLocator, waitForFinishedLoadings } from 'infrastructure/helpers';
import { Locator } from '@playwright/test';

export class DropDownPageObject {
  protected region: Locator;

  constructor(element: LocatorOrPageObject) {
    this.region = getLocator(element);
  }
  popper = () => this.region.page().locator(`.MuiAutocomplete-popper`);
  listbox = () => this.popper().locator(`.MuiAutocomplete-listbox`);

  public async getValues(): Promise<string[]> {
    await this.region.click();

    const itemsHandle = await this.listbox().elementHandle({ timeout: 5000 });
    const items = await itemsHandle?.$$('.MuiAutocomplete-option');
    const result = await Promise.all(items!.map((x) => x.innerText()));

    // close the dropdown without changing
    await this.region.click();
    return result;
  }

  open = () => this.region.click();

  public async selectDropDownValue(
    value: string,
    skipClickOnInput = false,
  ): Promise<void> {
    if (!skipClickOnInput)
      await this.region
        .locator('.MuiAutocomplete-input')
        .click({ force: true });

    // Here we change the default timeout, because sometimes Unicorn has a bug which causes DropDownList to be closed.
    // If that happens, it doesn't make sense to wait for 30s (or more), because DropDown will never be opened again by himself.
    // We shall fix this in Unicorn itself (and hopefully it's already fixed)

    const dropDown = this.listbox();
    await dropDown.waitFor({
      timeout: 5000,
    });
    await dropDown.locator(`text="${value}"`).click({ timeout: 5000 });

    await waitForFinishedLoadings(this.region.page());
  }

  public async toggleDropDownValueInMultiselect(value: string): Promise<void> {
    await this.selectDropDownValue(value);

    // need to close dropdown, since in Multiselect it isn't closed automatically
    await this.region.click();

    await waitForFinishedLoadings(this.region.page());
  }

  public get selectValueLocator() {
    return this.region.locator('input');
  }

  public async getSelectValue(): Promise<string> {
    return (await this.selectValueLocator.inputValue()) || 'Not selected';
  }

  public async getSelectedValues(): Promise<string[]> {
    await this.region.click();

    const selectedValues = this.listbox()
      .locator(`[aria-selected=true]`)
      .allInnerTexts();

    // close the dropdown without changing
    await this.region.click();

    return selectedValues;
  }

  async ensureOpened() {
    await this.region.waitFor();

    return this;
  }
}

/*
Useful to find input elements which are not wrapped in <Field /> (e.g. table filter in Audit Log)
 */
export async function findDropDownByPlaceholder(
  page: LocatorOrPageObject,
  placeholder: string,
): Promise<DropDownPageObject> {
  const locator = page.locator(
    `.MuiAutocomplete-root:has(input[placeholder="${placeholder}"])`,
  );
  return new DropDownPageObject(locator).ensureOpened();
}
