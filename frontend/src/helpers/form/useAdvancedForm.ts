import { useResetFormWhenDataIsLoaded } from './react-hook-form-helper';
import {
  FieldValues,
  useForm,
  SubmitErrorHandler,
  UseFormReturn,
  SubmitHandler,
  UseFormProps,
  DefaultValues,
} from 'react-hook-form';
import * as React from 'react';
import { useRef } from 'react';
import { useErrorHandler } from './useErrorHandler';
import { NavigateFunction } from 'react-router';

export type AdvancedFormReturnType<
  TFieldValues extends FieldValues = FieldValues,
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
  TContext extends object = object,
>(
  submitHandler: (
    data: TFieldValues,
    navigate: NavigateFunction,
  ) => Promise<void>,
  options?: {
    shouldResetOnSuccess?: boolean;
    initialize?: (form: UseFormReturn<TFieldValues, TContext>) => void;
    defaultValues?: DefaultValues<TFieldValues>;
  } & Omit<UseFormProps<TFieldValues, TContext>, 'defaultValues'>,
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
    handleSubmit: (
      onValid: SubmitHandler<TFieldValues>,
      onInvalid: SubmitErrorHandler<TFieldValues> | undefined,
    ) => {
      return form.handleSubmit(async (values) => {
        if (form.formState.isSubmitting) return;
        if (isSubmitting.current) return;
        isSubmitting.current = true;
        try {
          await onValid(values);
          return await handler.handler(values as any);
        } finally {
          isSubmitting.current = false;
        }
      }, onInvalid);
    },
    overallError: handler.overallServerError,
    formErrorCombined: handler.serverErrorsCombined,
    handleSubmitDefault: form.handleSubmit(async (values) => {
      return await handler.handler(values as any);
    }),
  };
}
