import Logger from 'js-logger';
import { createId } from 'components/uikit/type-utils';

const _logoutHandlers: Record<string, () => void> = {};
export function executeLogoutHandler() {
  try {
    Object.values(_logoutHandlers).forEach((x) => x());
  } catch (e) {
    Logger.error(e);
  }
}

export function addLogoutHandler(handler: () => void) {
  const id = createId();
  _logoutHandlers[id] = handler;
  return () => {
    delete _logoutHandlers[id];
  };
}
