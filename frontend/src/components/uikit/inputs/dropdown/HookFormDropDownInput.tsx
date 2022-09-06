import { DropDownInput } from './DropDownInput';
import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { convertPropertyAccessorToFunction } from './StyledAutocomplete';
import { DropDownInputProps } from './types';

type HookFormProps<
  T,
  TFieldValues extends FieldValues = FieldValues,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
> = Omit<
  DropDownInputProps<T, Required, UseIdAsValue>,
  'onChange' | 'onValueChanged' | 'value'
> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormDropDownInput<
  T,
  TFieldValues extends FieldValues = FieldValues,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
>(props: HookFormProps<T, TFieldValues, Required, UseIdAsValue>) {
  const { control, name, rules, useIdFunctionAsValue = true, ...rest } = props;
  const idFunction = props.idFunction
    ? convertPropertyAccessorToFunction<T, false, Required, false>(
        props.idFunction,
      )
    : undefined;
  return (
    <Controller
      control={control}
      name={name}
      rules={rules}
      render={({ field: { onChange, onBlur, value } }) => {
        if (useIdFunctionAsValue && idFunction && value) {
          value =
            (props.options.find((x) => idFunction(x as any) == value) as any) ??
            value;
        }
        return (
          <DropDownInput
            {...rest}
            value={value}
            onBlur={onBlur}
            required={props.required}
            variant={props.variant ?? 'formInput'}
            onValueChanged={(v: T | null) => {
              if (useIdFunctionAsValue && idFunction && v) {
                onChange(idFunction(v));
              } else {
                onChange(v);
              }
            }}
          />
        );
      }}
    />
  );
}
