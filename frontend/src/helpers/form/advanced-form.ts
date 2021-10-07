import { useResetFormWhenDataIsLoaded } from './react-hook-form-helper';
import { SubmitErrorHandler, UseFormReturn } from 'react-hook-form/dist/types';
import {
  SubmitHandler,
  UnpackNestedValue,
  UseFormProps,
} from 'react-hook-form/dist/types/form';
import { History } from 'history';
import { FieldValues, useForm } from 'react-hook-form';
import * as React from 'react';
import { useRef } from 'react';
import { useErrorHandler } from './useErrorHandler';

type AdvancedFormReturnType<
  TFieldValues extends FieldValues = FieldValues
> = UseFormReturn<TFieldValues> & {
  overallError: string;
  formErrorCombined: string;
  handleSubmitDefault: (e?: React.BaseSyntheticEvent) => Promise<void>;
};

export function useAdvancedForm<
  TFieldValues extends FieldValues = FieldValues,
  // eslint-disable-next-line @typescript-eslint/ban-types
  TContext extends object = object
>(
  submitHandler: (
    data: UnpackNestedValue<TFieldValues>,
    history: History,
  ) => Promise<void>,
  options?: {
    shouldResetOnSuccess?: boolean;
    initialize?: (form: UseFormReturn<TFieldValues>) => void;
  } & UseFormProps<TFieldValues, TContext>,
): AdvancedFormReturnType<TFieldValues> {
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
