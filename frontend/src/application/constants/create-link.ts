import { ParamParseKey, PathPattern } from 'react-router/lib/router';
import { useParams } from 'react-router';

declare type Params<Key extends string = string> = {
  readonly [key in Key]: string | number;
};
declare type StringParams<Key extends string = string> = {
  readonly [key in Key]: string;
};

declare type ParamsFunctionType<
  Path extends string,
  ParamKey extends string,
> = Path extends `${infer Start}:${infer End}`
  ? (params: Params<ParamKey>) => string
  : () => string;

export function createLink<
  ParamKey extends ParamParseKey<Path>,
  Path extends string,
>(
  pattern: Path,
): {
  /*
   * Use this to create link to certain page from another page, e.g. <Link to={Links.Authorized.ProductDetails({id:123})}>link</Link>
   */
  link: ParamsFunctionType<Path, ParamKey>;
  /*
   * Use this when configuring routes, e.g. <Route path={Links.Authorized.ProductDetails.path} element={<ProductDetailsPage />} />
   */
  route: string;
  /*
   * Use this when getting parameters inside of component via useMatch, e.g. useMatch(Links.Authorized.ProductDetails.pattern)
   */
  pattern: PathPattern<Path> | Path;
  /*
   * Use this when getting parameters inside of component via useParams, e.g. Links.Authorized.ProductDetails.useParams
   */
  useParams: () => StringParams<ParamKey>;
} {
  return {
    pattern: pattern,
    route: (pattern as any)?.toString(),
    link: ((params?: Params<ParamKey> | undefined) => {
      let patternWithValues = pattern.toString();

      if (params) {
        const sortedKeys = Object.keys(params).sort((a, b) =>
          a.length > b.length ? -1 : 1,
        );
        for (const paramName of sortedKeys) {
          console.log(paramName, (params as any)[paramName]);
          patternWithValues = patternWithValues.replace(
            ':' + paramName,
            (params as any)[paramName]?.toString(),
          );
        }
      }
      return patternWithValues;
    }) as any,
    useParams: useParams as any,
  };
}
