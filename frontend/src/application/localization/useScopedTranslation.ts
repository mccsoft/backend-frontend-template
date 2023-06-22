import {
  FallbackNs,
  useTranslation,
  UseTranslationOptions,
  UseTranslationResponse,
} from 'react-i18next';
import { FlatNamespace, KeyPrefix } from 'i18next';
import { $Tuple } from 'react-i18next/helpers';
/*
Allows to specify the initial path of all translations returned by the function.
E.g. useScopedTranslation('Page.Login'). All translations will start with 'Page.Login.*'
 */
export function useScopedTranslation<
  Ns extends FlatNamespace | $Tuple<FlatNamespace> | undefined = undefined,
  TKPrefix extends KeyPrefix<FallbackNs<Ns>> = undefined,
>(
  path: TKPrefix,
  ns?: Ns,
  options?: UseTranslationOptions<TKPrefix>,
): UseTranslationResponse<FallbackNs<Ns>, TKPrefix> {
  const i18n = useTranslation(ns, { ...options, keyPrefix: path });
  return i18n;
}
