import { Page } from '@playwright/test';
import { waitForFinishedLoadings } from 'infrastructure/helpers';
import { findDialog } from 'page-objects/components/DialogPageObject';

export const popupBaseSelector = '[data-test-id=popup-window]';

export async function closeConfirmDialogWithOk(page: Page) {
  const dialog = await findDialog(page);
  await dialog.submit();
  await dialog.ensureClosed();
}
export async function closeConfirmDialogWithCancel(page: Page) {
  await findDialog(page).then((x) => x.cancel());
}

export async function closePopupViaXAndConfirm(page: Page) {
  await page.click(popupBaseSelector + ' >> [data-test-id="popup-closeIcon"]');
  await closeConfirmDialogWithOk(page);
}

export async function getPopupTitle(page: Page): Promise<string | null> {
  const popup = await page.waitForSelector(
    popupBaseSelector + ' >> [data-test-id=popup-window-title]',
  );
  return await popup.textContent();
}

export async function submitFormInPopup(page: Page): Promise<void> {
  const button = await page.waitForSelector(popupBaseSelector + ' >> button[type=submit]');
  await button.click();
  await waitForFinishedLoadings(page);
}
