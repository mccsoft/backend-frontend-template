import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import React from 'react';
import { TimePicker, TimePickerProps } from './TimePicker';

type HookFormTimePickerProps<TFieldValues extends FieldValues = FieldValues> =
  Omit<TimePickerProps, 'onTimeChanged' | 'timeInMills'> & {
    name: Path<TFieldValues>;
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
export function HookFormTimePicker<
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormTimePickerProps<TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field }) => (
        <TimePicker
          {...props}
          timeInMills={field.value as any}
          onTimeChanged={(newTime: number | null, _isInvalid: boolean) => {
            field.onChange(newTime);
          }}
        />
      )}
    />
  );
}
