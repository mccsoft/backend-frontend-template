import { Page } from '@playwright/test';
import { findDialog } from 'page-objects/components/DialogPageObject';
import { PopupPageObject } from './PopupPageObject';

export const popupBaseSelector = '[data-test-id=popup-window]';

export async function closeConfirmDialogWithOk(page: Page) {
  const dialog = await findDialog(page);
  await dialog.submit();
  await dialog.ensureHidden();
}
export async function closeConfirmDialogWithCancel(page: Page) {
  await findDialog(page).then((x) => x.cancel());
}

export async function closePopupViaXAndConfirm(page: Page) {
  const popup = new PopupPageObject(page);
  await popup.ensureVisible();
  await popup.closeViaXButton();
  await closeConfirmDialogWithOk(page);
}

export async function getPopupTitle(page: Page): Promise<string | null> {
  const popup = new PopupPageObject(page);
  await popup.ensureVisible();
  return await popup.getPopupTitle();
}

export async function submitFormInPopup(page: Page): Promise<void> {
  const popup = new PopupPageObject(page);
  await popup.ensureVisible();
  return await popup.clickSubmit();
}
