import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { ComboBoxInput } from './ComboBoxInput';
import { StyledAutocompleteProps } from './types';

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
  const { control, name, rules, ...rest } = props;

  return (
    <Controller
      control={control}
      name={name}
      rules={rules}
      render={({ field: { onChange, value } }) => (
        <ComboBoxInput
          {...rest}
          variant={props.variant ?? 'formInput'}
          value={value}
          onValueChanged={(value) => {
            onChange(value);
          }}
        />
      )}
    />
  );
}
