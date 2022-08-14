import { RefObject, useEffect } from 'react';

export function useTriggerOnClickOutsideElement(
  elementRef: RefObject<HTMLElement>,
  onClickOutside: () => void,
  enabled: boolean,
  preventDefault = false,
) {
  useEffect(() => {
    if (enabled) {
      const clickListener = (e: MouseEvent) => {
        if (preventDefault) e.preventDefault();
        if (!elementRef.current?.contains(e.target as Node)) {
          onClickOutside();
        }
      };
      window.addEventListener('mousedown', clickListener, false);
      return () => {
        window.removeEventListener('mousedown', clickListener);
      };
    }
    return;
  }, [enabled, onClickOutside]);
}
