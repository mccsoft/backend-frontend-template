import { useCallback, useEffect, useLayoutEffect, useRef } from 'react';
import { useConfirm } from './useBlocker';
import { Action as HistoryAction, Location } from '@remix-run/router';
import type { unstable_BlockerFunction as BlockerFunction } from 'react-router';

type ArgsType = {
  currentLocation: Location;
  nextLocation: Location;
  historyAction: HistoryAction;
};
/*
 * Hook to conditionally block user from navigating away from the page.
 * - shouldBlockNavigation: called when user tries to leave the current page.
 *      - if the function returns `true` the navigation is blocked (user stays on current page)
 *      - if the function returns `false` the navigation is allowed
 */
export function useBlockNavigation(
  shouldBlockNavigation: boolean | ((args: ArgsType) => Promise<boolean>),
) {
  const shouldBlockNavigationRef = useRef(shouldBlockNavigation);
  useLayoutEffect(() => {
    shouldBlockNavigationRef.current = shouldBlockNavigation;
  });

  // save args passed to `when` to later pass them to `shouldBlockNavigation`
  const whenArgsRef = useRef<ArgsType>(null!);
  const whenCallback: BlockerFunction = useCallback((args: ArgsType) => {
    whenArgsRef.current = args;
    return true;
  }, []);

  const { isActive, onConfirm, resetConfirmation } = useConfirm(
    typeof shouldBlockNavigation === 'boolean'
      ? shouldBlockNavigation
      : whenCallback,
  );
  useEffect(() => {
    if (isActive) {
      const shouldBlockNavigation = shouldBlockNavigationRef.current;
      if (typeof shouldBlockNavigation === 'boolean') {
        if (shouldBlockNavigation) resetConfirmation();
        else confirm();
      } else {
        void shouldBlockNavigation(whenArgsRef.current).then((result) => {
          if (result) resetConfirmation();
          else onConfirm();
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isActive]);
}
