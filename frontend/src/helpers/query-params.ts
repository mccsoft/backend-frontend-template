import {
  NumberParam,
  NumericArrayParam,
  QueryParamConfig,
} from 'react-router-url-params';

export function createEnumArrayParam<T>(): QueryParamConfig<
  T[] | null | undefined,
  T[] | null | undefined
> {
  return NumericArrayParam as any;
}
export function createEnumParam<T>(): QueryParamConfig<
  T | null | undefined,
  T | null | undefined
> {
  return NumberParam as any;
}
