import i18n from 'i18next';
import { useEffect } from 'react';
import { DefaultValues, FieldValues, UseFormReturn } from 'react-hook-form';
import superjson from 'superjson';

/*
This hook is useful, when defaultValues of the Form become available AFTER form is initially rendered.
E.g. when defaultValues are loaded via react-query.
When defaultValues change, form fields are updated according to changed values.
 */
export function useResetFormWhenDataIsLoaded<
  TFieldValues extends FieldValues = FieldValues,
  TContext extends object = object,
>(
  form: UseFormReturn<TFieldValues, TContext>,
  defaultValues?: DefaultValues<TFieldValues>,
) {
  useEffect(() => {
    if (defaultValues) {
      // { ...defaultValues } is required to not modify original defaultValues
      const clonedValues = superjson.parse<DefaultValues<TFieldValues>>(
        superjson.stringify(defaultValues),
      );
      form.reset(clonedValues, {
        keepDirtyValues: true,
      });
    }
  }, [superjson.stringify(defaultValues)]);
}

export function requiredRule() {
  return {
    required: {
      value: true,
      message: i18n.t('translation:uikit.inputs.required'),
    },
  };
}
