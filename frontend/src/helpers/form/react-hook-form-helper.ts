import i18n from 'i18next';
import { useEffect } from 'react';
import { DefaultValues, UseFormReturn } from 'react-hook-form';

/*
This hook is useful, when defaultValues of the Form become available AFTER form is initially rendered.
E.g. when defaultValues are loaded via react-query.
When defaultValues change, form fields are updated according to changed values.
 */
export function useResetFormWhenDataIsLoaded<T>(
  form: UseFormReturn<T>,
  defaultValues?: DefaultValues<T>,
) {
  useEffect(() => {
    if (defaultValues) {
      if (form.formState.isDirty) {
        const defaultValuesUntyped = defaultValues as any;
        const currentValues = form.getValues() as any;
        Object.keys(form.formState.dirtyFields)
          .filter((x) => (form.formState.dirtyFields as any)[x])
          .forEach((dirtyKey) => {
            defaultValuesUntyped[dirtyKey] = currentValues[dirtyKey] as any;
          });
      }
      form.reset(defaultValues as any);
    }
  }, [defaultValues]);
}

export function requiredRule() {
  return {
    required: {
      value: true,
      message: i18n.t('uikit.inputs.required'),
    },
  };
}
