import React from 'react';

export const SignUrlImagePartial = React.forwardRef<
  HTMLImageElement,
  React.ImgHTMLAttributes<HTMLImageElement>
>((props, ref) => {
  return <img ref={ref} {...props} />;
});
