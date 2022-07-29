export function assertNeverDontThrow(x: never): never {
  return x;
}
export function assertNever(x: never): never {
  throw new Error(x);
}
