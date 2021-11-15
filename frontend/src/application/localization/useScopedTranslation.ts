import {
  DefaultNamespace,
  KeyPrefix,
  Namespace,
  useTranslation,
  UseTranslationOptions,
  UseTranslationResponse,
} from 'react-i18next';

/*
Allows to specify the initial path of all translations returned by the function.
E.g. useScopedTranslation('hl7.mapping'). All translations will start with 'hl7.mapping.*'
 */
export function useScopedTranslation<
  N extends Namespace = DefaultNamespace,
  TKPrefix extends KeyPrefix<N> = undefined,
>(
  path: TKPrefix,
  ns?: N | Readonly<N>,
  options?: UseTranslationOptions<TKPrefix>,
): UseTranslationResponse<N, TKPrefix> {
  const i18n = useTranslation(ns, { ...options, keyPrefix: path });
  return i18n;
}
