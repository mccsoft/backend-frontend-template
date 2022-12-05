const emptyArraySingleton: unknown[] = [];
export function emptyArray<T>(): T[] {
  return emptyArraySingleton as T[];
}
