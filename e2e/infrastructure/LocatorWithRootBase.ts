import { Locator, Page } from '@playwright/test';
import { LocatorOrPageObject } from './types';
import { getLocator, waitForFinishedLoadings } from './helpers';
import {
  findField as externalFindField,
  findFieldByTestId as externalFindFieldByTestId,
} from 'page-objects/components/FieldPageObject';

export class PageObjectBase {
  root: Page;
  locator: Locator['locator'];
  region: Locator;

  constructor(locator: LocatorOrPageObject) {
    this.region = getLocator(locator);
    this.root = this.region.page();
    this.locator = locator.locator.bind(locator);
  }

  public get page(): Page {
    return this.region.page();
  }

  waitForLoadings = () => waitForFinishedLoadings(this.root);
  async ensureVisible(options?: Parameters<Locator['waitFor']>[0]) {
    await this.region.waitFor(options);
    await waitForFinishedLoadings(this.root);
    return this;
  }
  async ensureHidden(options?: Parameters<Locator['waitFor']>[0]) {
    await this.region.waitFor({ ...options, state: 'hidden' });
    return this;
  }

  findField(name: Parameters<typeof externalFindField>[1]) {
    return externalFindField(this, name);
  }

  findFieldByTestId(name: Parameters<typeof externalFindFieldByTestId>[1]) {
    return externalFindFieldByTestId(this, name);
  }
}
