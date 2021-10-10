export type PickFieldsWithType<Base, Type> = Pick<
  Base,
  {
    [Key in keyof Base]: Base[Key] extends Type ? Key : never;
  }[keyof Base]
>;

export function hasOwnProperty<X extends {}, Y extends PropertyKey>(
  obj: X,
  prop: Y,
): obj is X & Record<Y, unknown> {
  return obj.hasOwnProperty(prop);
}

export function createId() {
  return '_' + Math.random().toString(36).substr(2, 9);
}
