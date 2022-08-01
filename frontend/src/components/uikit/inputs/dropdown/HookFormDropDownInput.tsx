import { DropDownInput } from './DropDownInput';
import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { StyledAutocompleteProps } from './StyledAutocomplete';

type HookFormProps<
  T,
  Required extends boolean | undefined,
  TFieldValues extends FieldValues = FieldValues,
> = Omit<StyledAutocompleteProps<T, false, Required, false>, 'onChange'> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormDropDownInput<
  D,
  Required extends boolean,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<D, Required, TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, onBlur, value } }) => {
        return (
          <DropDownInput
            {...props}
            value={value}
            onBlur={onBlur}
            required={props.required}
            onValueChanged={(v: D | null) => {
              onChange(v);
            }}
          />
        );
      }}
    />
  );
}
