declare module '@lottiefiles/jlottie' {
  export type JLottieOptions = {
    loop?: boolean;
    autoplay: boolean;
    animationData: any;
    debug?: boolean;
    container: HTMLElement;
    rendererSettings?: {
      preserveAspectRatio?: string;
    };
  };

  export function loadAnimation(options: JLottieOptions): {
    pause();
    play();
    destroy();
    stop();
  };
  export function destroy(element: HTMLElement);
  export function stop(element: HTMLElement);
  export function play(element: HTMLElement);
}
