import type {
  BrowserNativeObject,
  FieldValues,
  IsEqual,
  Primitive,
} from 'react-hook-form';
import type {
  ArrayKey,
  IsTuple,
  TupleKeys,
} from 'react-hook-form/dist/types/path/common';

/*
Same as FieldPath<T> (from react-hook-form), but requires the type of a target property to be a Date
 */
export type DatePath<T extends FieldValues = FieldValues> = TypedFieldPath<
  Date,
  T
>;

/*
Same as Path<T> (from react-hook-form), but requires the type of a target property to be TFieldType
 */
export type TypedPath<TFieldType, T> = T extends any
  ? PathInternal<TFieldType, T>
  : never;

/*
Same as FieldPath<T> (from react-hook-form), but requires the type of a target property to be TFieldType
 */
export type TypedFieldPath<
  TFieldType,
  TFieldValues extends FieldValues,
> = TypedPath<TFieldType, TFieldValues>;

/**
 * Helper type for recursively constructing paths through a type.
 * This obscures the internal type param TraversedTypes from exported contract.
 *
 * See {@link Path}
 */
type PathInternal<TFieldType, T, TraversedTypes = T> = T extends ReadonlyArray<
  infer V
>
  ? IsTuple<T> extends true
    ? {
        [K in TupleKeys<T>]-?: TFieldType extends T[K]
          ? PathImpl<TFieldType, K & string, T[K], TraversedTypes>
          : never;
      }[TupleKeys<T>]
    : PathImpl<TFieldType, ArrayKey, V, TraversedTypes>
  : {
      [K in keyof T]-?: TFieldType extends T[K]
        ? PathImpl<TFieldType, K & string, T[K], TraversedTypes>
        : never;
    }[keyof T];

export type PathImpl<
  TFieldType,
  K extends string | number,
  V,
  TraversedTypes,
> = V extends Primitive | BrowserNativeObject
  ? `${K}`
  : true extends AnyIsEqual<TraversedTypes, V>
  ? `${K}`
  : `${K}` | `${K}.${PathInternal<TFieldType, V, TraversedTypes | V>}`;
/**
 * Helper function to break apart T1 and check if any are equal to T2
 *
 * See {@link IsEqual}
 */
type AnyIsEqual<T1, T2> = T1 extends T2
  ? IsEqual<T1, T2> extends true
    ? true
    : never
  : never;

// type Q = { a: Date; b: string };
// type A = TypedFieldPath<Date, Q>;
// const a: A = 'a';
// const b: A = 'b';
// console.log(a, b);
