import { Locator, Page } from '@playwright/test';
import { startOfDay } from 'date-fns';
import { generate } from 'randomstring';
import { LocatorOrPageObject } from './types';

export const DefaultDateString = '2021-07-21T14:40:00.000';
export const DefaultDateStart = startOfDay(new Date(DefaultDateString));

export function createId() {
  return generate({ charset: 'alphabetic', length: 7 });
}

export function getLocator(locator: LocatorOrPageObject): Locator {
  if (isPage(locator)) {
    return locator.locator('body');
  } else {
    if ('region' in locator) {
      return locator.region;
    } else {
      return locator;
    }
  }
}

export async function fillInput(input: Locator, value: string) {
  await input.click({ clickCount: 3, position: { x: 5, y: 5 } });
  await input.fill(value);
}

export async function waitForFinishedLoadings(page: Page) {
  await page.waitForSelector('data-test-id=loading', { state: 'hidden' });
}

export async function gotoRoot(page: Page) {
  // we need to restore date before reloading the page, otherwise tests hang (timeout)
  // await page.evaluate(() => {
  //   (window as any).restoreDate();
  // });

  await page.goto('/');
  await waitForFinishedLoadings(page);
}

/*
Reloads current URL (like pressing enter in the browser address bar), or goes to passed url
 */
export async function reloadPage(page: Page, url?: string) {
  if (url) {
    await page.goto(url);
  } else {
    await page.goto(page.url());
  }

  await waitForFinishedLoadings(page);
}

export async function blurField(input: Locator) {
  await input.evaluate((e) => {
    if (e.ariaAutoComplete) {
      //it's not possible to close MUI autocomplete by blurring the input, so let's press Enter
      e.dispatchEvent(
        new KeyboardEvent('keydown', {
          bubbles: true,
          cancelable: true,
          keyCode: 13,
        }),
      );
      e.blur();
    } else {
      e.blur();
    }
  });
}

export async function uploadFile(
  file: string,
  page: Page,
  showUploadAction: () => Promise<void>,
): Promise<void> {
  // Note that Promise.all prevents a race condition
  // between clicking and waiting for the file chooser.
  const [fileChooser] = await Promise.all([
    // It is important to call waitForEvent before click to set up waiting.
    page.waitForEvent('filechooser'),
    // Opens the file chooser.
    showUploadAction(),
  ]);
  await fileChooser.setFiles(file);
}

export async function downloadFile(
  page: Page,
  startDownload: () => Promise<void>,
): Promise<string> {
  // Note that Promise.all prevents a race condition
  // between clicking and waiting for the download.
  const [download] = await Promise.all([
    // It is important to call waitForEvent before click to set up waiting.
    page.waitForEvent('download'),
    // Triggers the download.
    startDownload(),
  ]);
  // wait for download to complete
  const filename = download.suggestedFilename();
  await download.saveAs(download.suggestedFilename());
  return filename!;
}

export function isInVsCode() {
  return !!process.env.VSCODE_PID;
}
export function isPage(obj: any | undefined): obj is Page {
  return obj && obj._type === 'Page';
}
