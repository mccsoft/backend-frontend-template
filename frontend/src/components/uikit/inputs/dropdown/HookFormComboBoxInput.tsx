import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { ComboBoxInput } from './ComboBoxInput';
import { StyledAutocompleteProps } from './StyledAutocomplete';

type HookFormProps<T, TFieldValues extends FieldValues = FieldValues> = Omit<
  StyledAutocompleteProps<T, false, false, true>,
  'onChange'
> & {
  onValueChanged: (value: string | null, option: T | null) => void;
} & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormComboBoxInput<
  T,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<T, TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, value } }) => (
        <ComboBoxInput
          {...props}
          value={value}
          onValueChanged={(value) => {
            onChange(value);
          }}
        />
      )}
    />
  );
}
