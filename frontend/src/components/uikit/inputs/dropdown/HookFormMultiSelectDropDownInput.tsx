import { DropDownInputProps } from 'components/uikit/inputs/dropdown/DropDownInput';
import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { MultiSelectDropDownInput } from './MultiSelectDropDownInput';

type HookFormProps<D, TFieldValues extends FieldValues = FieldValues> = Omit<
  DropDownInputProps<D>,
  'onSelectedOptionChanged'
> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormMultiSelectDropDownInput<
  D,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<D, TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, value } }) => (
        <MultiSelectDropDownInput
          {...props}
          values={value as any}
          onSelectedOptionsChanged={(value: D[]) => {
            onChange(value);
          }}
        />
      )}
    />
  );
}
