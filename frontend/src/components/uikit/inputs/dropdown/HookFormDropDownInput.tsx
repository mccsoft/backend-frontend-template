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
import { StyledAutocompleteProps } from './types';

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
  /*
   * If true, we assume that form `value` contains the result of `idFunction` of the option.
   * true by default.
   */
  useIdFunctionAsValue?: boolean;
};

export function HookFormDropDownInput<
  T,
  Required extends boolean,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<T, Required, TFieldValues>) {
  const { useIdFunctionAsValue = true, ...rest } = props;
  const idFunction = props.idFunction
    ? convertPropertyAccessorToFunction<T, false, Required, false>(
        props.idFunction,
      )
    : undefined;
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field: { onChange, onBlur, value } }) => {
        if (useIdFunctionAsValue && idFunction && value) {
          value =
            (props.options.find((x) => idFunction(x) == value) as any) ?? value;
        }
        return (
          <DropDownInput
            {...rest}
            value={value}
            onBlur={onBlur}
            required={props.required}
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
