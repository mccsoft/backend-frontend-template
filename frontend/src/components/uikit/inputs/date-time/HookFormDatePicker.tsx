import { DatePicker, DatePickerProps } from './DatePicker';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import React from 'react';
import { DatePath } from '../hook-form';

type HookFormDatePickerProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends DatePath<TFieldValues> = DatePath<TFieldValues>,
> = Omit<DatePickerProps, 'onChange' | 'name'> & {
  name: TName;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

/*
We have to use Controlled components for native html datepickers,
because they require values in a form of 'yyyy-MM-DD', but in uncontrolled form react-hook-form
supply them with Date object.
 */
export function HookFormDatePicker<
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormDatePickerProps<TFieldValues>) {
  const { name, rules, control, ...rest } = props;
  return (
    <Controller
      control={control}
      name={name}
      rules={rules}
      render={({ field }) => (
        <DatePicker
          {...rest}
          value={field.value as any}
          onChange={field.onChange}
        />
      )}
    />
  );
}
