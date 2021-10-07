import { DatePicker, DatePickerProps } from './DatePicker';
import {
  Control,
  Controller,
  FieldName,
  FieldValues,
  RegisterOptions,
} from 'react-hook-form';
import React from 'react';

type HookFormDatePickerProps = Omit<
  DatePickerProps,
  'onSelectedOptionChanged'
> & {
  name: FieldName<FieldValues>;
  control: Control;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

/*
We have to use Controlled components for native html datepickers,
because they require values in a form of 'yyyy-MM-DD', but in uncontrolled form react-hook-form
supply them with Date object.
 */
export function HookFormDatePicker(props: HookFormDatePickerProps) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field }) => (
        <DatePicker {...props} value={field.value} onChange={field.onChange} />
      )}
    />
  );
}
