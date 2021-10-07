import {
  DefaultNamespace,
  Namespace,
  useTranslation,
  UseTranslationOptions,
  UseTranslationResponse,
} from 'react-i18next';
import { TFunction } from 'i18next';

/*
Allows to specify the initial path of all translations returned by the function.
E.g. useScopedTranslation('hl7.mapping'). All translations will start with 'hl7.mapping.*'
 */
export function useScopedTranslation<N extends Namespace = DefaultNamespace>(
  path: string,
  ns?: N,
  options?: UseTranslationOptions,
): UseTranslationResponse<N> {
  const i18n = useTranslation(ns, options);
  return {
    ...i18n,
    t: <TFunction>(
      ((key: any, defaultValue?: any, options?: any) =>
        i18n.t(
          (Array.isArray(key)
            ? key.map((x: string) => path + '.' + x)
            : path + '.' + key) as any,
          defaultValue,
          options,
        ))
    ),
  };
}
