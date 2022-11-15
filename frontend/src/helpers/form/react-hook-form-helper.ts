import i18n from 'i18next';
import { useEffect } from 'react';
import { DefaultValues, FieldValues, UseFormReturn } from 'react-hook-form';

/*
This hook is useful, when defaultValues of the Form become available AFTER form is initially rendered.
E.g. when defaultValues are loaded via react-query.
When defaultValues change, form fields are updated according to changed values.
 */
export function useResetFormWhenDataIsLoaded<
  TFieldValues extends FieldValues = FieldValues,
  // ignored because it's used in react-hook-form as well
  // eslint-disable-next-line @typescript-eslint/ban-types
  TContext extends object = object,
>(
  form: UseFormReturn<TFieldValues, TContext>,
  defaultValues?: DefaultValues<TFieldValues>,
) {
  useEffect(() => {
    if (defaultValues) {
      form.reset(defaultValues, { keepDirtyValues: true });
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
