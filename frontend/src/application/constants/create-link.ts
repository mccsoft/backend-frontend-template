import { ParamParseKey, PathMatch } from 'react-router/lib/router';
import { generatePath, useMatch, useParams } from 'react-router';
import { createSearchParams } from 'react-router-dom';

declare type Params<Key extends string = string> = {
  readonly [key in Key]: string | number;
};
declare type StringParams<Key extends string = string> = {
  readonly [key in Key]: string;
};
declare type URLSearchParamsInit =
  | string
  | number
  | [string, string | number]
  | Record<string, string | string[] | number | number[]>
  | URLSearchParams;

declare type ParamsFunctionType<
  Path extends string,
  ParamKey extends string,
> = Path extends `${infer Start}:${infer End}`
  ? (params: Params<ParamKey>, search?: URLSearchParamsInit) => string
  : (search?: URLSearchParamsInit) => string;

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
   * Use this as a strong-type replacement of useParams for the route
   */
  useParams: () => StringParams<ParamKey>;
  /*
   * Use this as a strong-type replacement of useMatch for the route
   */
  useMatch: () => PathMatch<ParamKey> | null;
} {
  return {
    route: (pattern as any)?.toString(),
    link: ((
      params?: Params<ParamKey> | undefined,
      search?: URLSearchParamsInit,
    ) => {
      let result = generatePath(pattern, params as any);
      if (search) {
        result = result + '?' + createSearchParams(search as any);
      }
      return result.replace('*', '');
    }) as any,
    useParams: useParams as any,
    useMatch: () => useMatch(pattern),
  };
}

export function parseIntOrThrow(t: string): number {
  const result = parseInt(t);
  if (isNaN(result)) {
    throw new Error(`Passed string '${t}' was not a number`);
  }
  return result;
}

export function parseIntOrDefault(
  t: string | undefined,
  defaultValue: number,
): number;
export function parseIntOrDefault(t: string | undefined): number | undefined;
export function parseIntOrDefault(
  t: string | undefined,
  defaultValue?: number,
): number | undefined {
  if (t === null || t === undefined) return defaultValue;

  const result = parseInt(t);
  if (isNaN(result)) {
    return defaultValue;
  }
  return result;
}
