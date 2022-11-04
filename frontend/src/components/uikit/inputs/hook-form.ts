import { BrowserNativeObject, Path, Primitive } from 'react-hook-form';
import {
  ArrayKey,
  IsTuple,
  TupleKeys,
} from 'react-hook-form/dist/types/path/common';

declare type PathImpl<K extends string | number, V> = V extends
  | Primitive
  | BrowserNativeObject
  ? `${K}`
  : `${K}` | `${K}.${Path<V>}`;

/*
Same as Path<T> (from react-hook-form), but requires the type of a target property to be Date
 */
export type DatePath<T> = TypedPath<T, Date>;

/*
Same as Path<T> (from react-hook-form), but requires the type of a target property to be TTargetType
 */
export type TypedPath<T, TTargetType> = T extends ReadonlyArray<infer V>
  ? IsTuple<T> extends true
    ? {
        [K in TupleKeys<T>]-?: TTargetType extends T[K]
          ? PathImpl<K & string, T[K]>
          : never;
      }[TupleKeys<T>]
    : PathImpl<ArrayKey, V>
  : {
      [K in keyof T]-?: TTargetType extends T[K]
        ? PathImpl<K & string, T[K]>
        : never;
    }[keyof T];
