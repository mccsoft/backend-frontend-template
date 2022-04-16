import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { ComboBoxInput, ComboBoxInputProps } from './ComboBoxInput';

type HookFormProps<
  D extends unknown = unknown,
  TFieldValues extends FieldValues = FieldValues,
> = Omit<
  ComboBoxInputProps<D>,
  'onSelectedOptionChanged' | 'onValueChanged'
> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormComboBoxInput<
  D extends unknown = unknown,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<D, TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, value } }) => (
        <ComboBoxInput
          {...props}
          value={value}
          onValueChanged={(value, option: D | null) => {
            onChange(option ?? value);
          }}
        />
      )}
    />
  );
}
