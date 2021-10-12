import React, {
  HTMLAttributes,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';

import { loadAnimation, JLottieOptions } from '@lottiefiles/jlottie';
import { createId } from 'components/uikit/type-utils';

export type JLottieExternalOptions = Omit<JLottieOptions, 'container'>;

export const JLottie: React.FC<
  {
    options: JLottieExternalOptions;
  } & HTMLAttributes<HTMLDivElement>
> = ({ options, ...rest }) => {
  const lottieDiv = useRef<HTMLDivElement>(null);
  useLayoutEffect(() => {
    if (!lottieDiv.current) return;

    const animation = loadAnimation({
      ...options,
      container: lottieDiv.current,
    });
    return () => animation.destroy();
  }, []);
  const [id] = useState(() => createId());

  return <div ref={lottieDiv} id={id} {...rest} />;
};
