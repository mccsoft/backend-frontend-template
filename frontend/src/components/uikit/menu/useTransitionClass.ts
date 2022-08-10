import { useEffect, useState } from 'react';

/*
 * Use this hook to apply CSS transition class when element appears in DOM.
 * CSS animation works only when styles change.
 * So to animate the appearance you need to apply the animated styles AFTER element is mounted.
 * This hook actually returns your `className` with a small delay,
 * so that animations could kick in.
 * This is similar in spirit to https://reactcommunity.org/react-transition-group/css-transition
 * except that I could not get the latter to work :)
 */
export function useTransitionClass(className: string, deps?: any[]) {
  const [isAnimationStylesApplied, setIsAnimationStylesApplied] =
    useState(false);
  useEffect(() => {
    setTimeout(() => {
      setIsAnimationStylesApplied(true);
    });
    return () => setIsAnimationStylesApplied(false);
  }, deps ?? []);
  return isAnimationStylesApplied ? className : undefined;
}
