import React from 'react';
import { QueryFactory } from 'services/api';

type Props = Omit<PropsGeneric, 'src'> & {
  /**
   *  Url to get the photo from
   *
   *  @remarks
   *  Used only when `photoId` is not set
   */
  src?: string | null;

  /**
   *  Photo id for `/files/{photoId}`
   */
  photoId?: string | null;
};

/*
 * You could add your own properties (e.g. width/height and add them as parameters to SRC)
 */
export const SignUrlImage = React.forwardRef<HTMLImageElement, Props>(
  (props, ref) => {
    /* you could build src based on e.g. imageId, width/height/etc. parameters */
    const src = props.photoId ? '/files/' + props.photoId : props.src;
    return <SignUrlImageGeneric {...props} src={src} ref={ref} />;
  },
);

const InnerImage = React.forwardRef<
  HTMLImageElement,
  React.ImgHTMLAttributes<HTMLImageElement>
>((props, ref) => {
  return <img ref={ref} {...props} />;
});

// -------------------------------------
// Everything below comes from template,
// try not to modify it.
// -------------------------------------
type PropsGeneric = Omit<
  React.DetailedHTMLProps<
    React.ImgHTMLAttributes<HTMLImageElement>,
    HTMLImageElement
  >,
  'src'
> & {
  src?: string | null;
  fallback?: string;
};

export const SignUrlImageGeneric = React.forwardRef<
  HTMLImageElement,
  PropsGeneric
>((props, ref) => {
  const { src, fallback, ...restProps } = props;
  const cookieQuery = QueryFactory.SignUrlQuery.useSetSignatureCookieQuery({
    refetchIntervalInBackground: true,
    refetchInterval: 1 * 60 * 1000 /* once in 10 minutes*/,
  });

  return (
    <InnerImage
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
          : cookieQuery.isLoading
          ? ''
          : cookieQuery.isError
          ? fallback
          : src
      }
    />
  );
});
