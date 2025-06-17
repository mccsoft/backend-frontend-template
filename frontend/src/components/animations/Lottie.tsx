/*
 * This file is copied from https://github.com/chenqingspring/react-lottie/issues/139
 */
import { useEffect, useMemo, useRef } from 'react';
import lottie from 'lottie-web';

type LottieProps = {
  animationData: any;
  width?: number;
  height?: number;
  className?: string;
};

export const Lottie = ({
  animationData,
  width,
  height,
  className,
}: LottieProps) => {
  const element = useRef<HTMLDivElement>(null);
  const lottieInstance = useRef<any>(undefined);

  useEffect(() => {
    if (element.current) {
      lottieInstance.current = lottie.loadAnimation({
        animationData,
        container: element.current,
      });
    }
    return () => {
      lottieInstance.current?.destroy();
    };
  }, [animationData]);
  const style = useMemo(() => ({ width, height }), [width, height]);
  return <div style={style} ref={element} className={className}></div>;
};
