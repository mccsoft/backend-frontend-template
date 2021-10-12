import React, { FC } from 'react';
import { QueryFactory } from 'services/api';

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

export const SignUrlImage: FC<Props> = (props) => {
  const { src, fallback, ...restProps } = props;
  const cookieQuery = QueryFactory.SignUrlQuery.useSetSignatureCookieQuery({
    refetchIntervalInBackground: true,
    refetchInterval: 1 * 60 * 1000 /* once in 10 minutes*/,
  });

  return (
    <img
      {...restProps}
      src={
        /*
          We show fallback if the `url` is null or if the Cookie query has failed.
          While Cookie query is loading (initial loading) we show nothing, because it will probably load fast
          and it doesn't make sense to show `fallback` (we might want to show `loading` later)
           */
        !src
          ? fallback
          : cookieQuery.isLoading
          ? ''
          : cookieQuery.isError
          ? fallback
          : src
      }
    />
  );
};
