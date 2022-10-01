import React from 'react';
import { QueryFactory } from 'services/api';
import { SignUrlImagePartial } from './SignUrlImage.partial';

type Props = Omit<
  React.DetailedHTMLProps<
    React.ImgHTMLAttributes<HTMLImageElement>,
    HTMLImageElement
  >,
  'src'
> & {
  src?: string | null;
  fallback?: string;
};

export const SignUrlImage = React.forwardRef<HTMLImageElement, Props>(
  (props, ref) => {
    const { src, fallback, ...restProps } = props;
    const cookieQuery = QueryFactory.SignUrlQuery.useSetSignatureCookieQuery({
      refetchIntervalInBackground: true,
      refetchInterval: 1 * 60 * 1000 /* once in 10 minutes*/,
    });

    return (
      <SignUrlImagePartial
        {...restProps}
        ref={ref}
        src={
          /*
          We show fallback if the `url` is null or if the Cookie query has failed.
          While Cookie query is loading (initial loading) we show nothing, because it will probably load fast
          and it doesn't make sense to show `fallback` (we might want to show `loading` later)
           */
          !src
            ? fallback
            : cookieQuery.isInitialLoading
            ? ''
            : cookieQuery.isError
            ? fallback
            : src
        }
      />
    );
  },
);
