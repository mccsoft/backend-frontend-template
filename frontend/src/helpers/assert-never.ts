export class NeverError extends Error {
  // Typescript will give a compile-time error if we try to call this constructor
  constructor(value: never) {
    super(`Unreachable statement: ${value}`);
  }
}

export function assertNeverDontThrow(x: never): never {
  return x;
}
export function assertNever(x: never): never {
  throw new NeverError(x);
}
