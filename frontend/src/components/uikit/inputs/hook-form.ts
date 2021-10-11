import { Path, Primitive } from 'react-hook-form';
declare type TupleKey<T extends ReadonlyArray<any>> = Exclude<
  keyof T,
  keyof any[]
>;
declare type ArrayKey = number;
export type IsTuple<T extends ReadonlyArray<any>> = number extends T['length']
  ? false
  : true;
export type PathImpl<K extends string | number, V> = V extends Primitive
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
        [K in TupleKey<T>]-?: TTargetType extends T[K]
          ? PathImpl<K & string, T[K]>
          : never;
      }[TupleKey<T>]
    : PathImpl<ArrayKey, V>
  : {
      [K in keyof T]-?: TTargetType extends T[K]
        ? PathImpl<K & string, T[K]>
        : never;
    }[keyof T];
