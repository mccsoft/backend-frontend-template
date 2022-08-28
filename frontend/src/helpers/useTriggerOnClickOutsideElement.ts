import { RefObject, useEffect } from 'react';

export function useTriggerOnClickOutsideElement(
  elementRef: RefObject<HTMLElement>,
  onClickOutside: (e: MouseEvent) => void,
  enabled: boolean,
  preventDefault = false,
) {
  useEffect(() => {
    if (enabled) {
      const clickListener = (e: MouseEvent) => {
        // if preventDefault is used, it's not possible to focus inputs within popups
        if (preventDefault) e.preventDefault();
        if (!elementRef.current?.contains(e.target as Node)) {
          onClickOutside(e);
        }
      };
      let unmounted = false;
      // without setTimeout the StyledAutocomplete immediately closes after opening
      setTimeout(
        () =>
          !unmounted &&
          window.addEventListener('mousedown', clickListener, false),
      );

      return () => {
        unmounted = true;
        window.removeEventListener('mousedown', clickListener);
      };
    }
    return;
  }, [enabled, onClickOutside]);
}
