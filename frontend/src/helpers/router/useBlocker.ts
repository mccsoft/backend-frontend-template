/*Copied from https://dev.to/bangash1996/detecting-user-leaving-page-with-react-router-dom-v602-33ni*/
import * as React from 'react';
import { UNSAFE_NavigationContext } from 'react-router-dom';
import type { History, Blocker, Transition } from 'history';

export function useBlocker(
  blocker: Blocker,
  when: boolean | (() => boolean),
): void {
  const navigator = React.useContext(UNSAFE_NavigationContext)
    .navigator as History;

  React.useEffect(() => {
    const unblock = navigator.block((tx: Transition) => {
      let shouldBlock = false;
      if (typeof when === 'function') {
        shouldBlock = when();
      } else {
        shouldBlock = when;
      }
      if (!shouldBlock) {
        unblock();
        tx.retry();
        return;
      }

      const autoUnblockingTx = {
        ...tx,
        retry() {
          unblock();
          tx.retry();
        },
      };

      blocker(autoUnblockingTx);
    });

    return unblock;
  }, [navigator, blocker, when]);
}
