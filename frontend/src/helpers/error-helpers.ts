export function mapServerErrorToFormReadableResult(errorKey: string) {
  return errorKey.charAt(0).toLowerCase() + errorKey.substr(1);
}
