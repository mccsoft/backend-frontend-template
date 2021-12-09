export type PickFieldsWithType<Base, Type> = Pick<
  Base,
  {
    [Key in keyof Base]: Base[Key] extends Type ? Key : never;
  }[keyof Base]
>;

export function createId() {
  return Math.random().toString(36).substr(2, 9);
}
