import i18next from 'i18next';

export const DEFAULT_PATTERN: RegExp =
  /^[A-Za-z0-9 !"#$%&'()*+,\-\.\/:;<=>?@\[\\\]\^_{|}~]+$/;

export const validatePassword = (
  value: string,
  customPattern?: RegExp,
): true | string => {
  const pattern = customPattern ?? DEFAULT_PATTERN;

  if (value.trim() !== value) {
    return i18next.t('password_validation_trim_error');
  }

  return pattern.test(value) ? true : i18next.t('password_validation_error');
};
