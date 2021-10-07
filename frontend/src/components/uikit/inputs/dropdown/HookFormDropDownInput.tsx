import {
  DropDownInput,
  DropDownInputProps,
} from 'components/uikit/inputs/dropdown/DropDownInput';
import { FieldsWithType } from 'components/uikit/type-utils';
import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';

type HookFormProps<
  D extends FieldValues & Record<V, string | number>,
  V extends FieldsWithType<D, string | number>,
  TFieldValues extends FieldValues = FieldValues
> = Omit<DropDownInputProps<D, V>, 'onSelectedOptionChanged'> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormDropDownInput<
  D extends FieldValues & Record<V, string | number>,
  V extends FieldsWithType<D, string | number>,
  TFieldValues extends FieldValues = FieldValues
>(props: HookFormProps<D, V, TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, value } }) => (
        <DropDownInput
          {...props}
          value={value as any}
          onSelectedOptionChanged={(value: D | null) => {
            onChange(
              value
                ? props.valueField
                  ? value[props.valueField]
                  : value
                : null,
            );
          }}
        />
      )}
    />
  );
}
