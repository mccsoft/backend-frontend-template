import { useResetFormWhenDataIsLoaded } from './react-hook-form-helper';
import {
  FieldValues,
  useForm,
  SubmitErrorHandler,
  UseFormReturn,
  SubmitHandler,
  UnpackNestedValue,
  UseFormProps,
} from 'react-hook-form';
import * as React from 'react';
import { useRef } from 'react';
import { useErrorHandler } from './useErrorHandler';
import { NavigateFunction } from 'react-router';

type AdvancedFormReturnType<
  TFieldValues extends FieldValues = FieldValues,
  // ignored because it's used in react-hook-form as well
  // eslint-disable-next-line @typescript-eslint/ban-types
  TContext extends object = object,
> = UseFormReturn<TFieldValues, TContext> & {
  overallError: string;
  formErrorCombined: string;
  handleSubmitDefault: (e?: React.BaseSyntheticEvent) => Promise<void>;
};

/*
 * useAdvancedForm adds some advanced options on top of useForm (react-hook-form).
 * These advanced functions include:
 * 1. Ability to specify submitHandler when defining the form
 * 2. Parse error messages returned from backend and assign them to proper fields (assuming that server-side complies to ProblemDetails RFC)
 * 3. Ability to provide defaultValues 'asynchronously' (i.e. after form has been initially rendered, e.g. when getting the data via react-query hooks).
 */
export function useAdvancedForm<
  TFieldValues extends FieldValues = FieldValues,
  // ignored because it's used in react-hook-form as well
  // eslint-disable-next-line @typescript-eslint/ban-types
  TContext extends object = object,
>(
  submitHandler: (
    data: UnpackNestedValue<TFieldValues>,
    navigate: NavigateFunction,
  ) => Promise<void>,
  options?: {
    shouldResetOnSuccess?: boolean;
    initialize?: (form: UseFormReturn<TFieldValues, TContext>) => void;
  } & UseFormProps<TFieldValues, TContext>,
): AdvancedFormReturnType<TFieldValues, TContext> {
  const form = useForm<TFieldValues, TContext>(options);
  const isSubmitting = useRef(false);
  const shouldResetOnSuccess = options?.shouldResetOnSuccess ? true : false;
  const handler = useErrorHandler<TFieldValues>(
    submitHandler,
    form.setError,
    shouldResetOnSuccess ? (form.reset as any) : undefined,
  );
  options?.initialize?.(form);
  useResetFormWhenDataIsLoaded(form, options?.defaultValues);

  return {
    ...form,
    handleSubmit: <TSubmitFieldValues extends FieldValues = TFieldValues>(
      onValid: SubmitHandler<TSubmitFieldValues>,
      onInvalid: SubmitErrorHandler<TFieldValues> | undefined,
    ) => {
      return form.handleSubmit<TSubmitFieldValues>(
        async (values: UnpackNestedValue<TSubmitFieldValues>) => {
          if (form.formState.isSubmitting) return;
          if (isSubmitting.current) return;
          isSubmitting.current = true;
          try {
            await onValid(values);
            return await handler.handler(values as any);
          } finally {
            isSubmitting.current = false;
          }
        },
        onInvalid,
      );
    },
    overallError: handler.overallServerError,
    formErrorCombined: handler.serverErrorsCombined,
    handleSubmitDefault: form.handleSubmit<TFieldValues>(
      async (values: UnpackNestedValue<TFieldValues>) => {
        return await handler.handler(values as any);
      },
    ),
  };
}
