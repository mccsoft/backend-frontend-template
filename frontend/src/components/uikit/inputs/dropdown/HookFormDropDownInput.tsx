import { DropDownInput } from './DropDownInput';
import {
  Controller,
  FieldValues,
  FieldPath,
  UseControllerProps,
} from 'react-hook-form';
import { convertPropertyAccessorToFunction } from './StyledAutocomplete';
import { DropDownInputProps } from './types';

type HookFormProps<
  T,
  TFieldValues extends FieldValues = FieldValues,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = Omit<
  DropDownInputProps<T, Required, UseIdAsValue>,
  'onChange' | 'onValueChanged' | 'value'
> & {
  name: TName;
  control: UseControllerProps<TFieldValues, TName, TFieldValues>['control'];
  rules?: UseControllerProps<TFieldValues, TName, TFieldValues>['rules'];
  onFocus?: () => void;
  defaultValue?: unknown;
};

export function HookFormDropDownInput<
  T,
  TFieldValues extends FieldValues = FieldValues,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>(props: HookFormProps<T, TFieldValues, Required, UseIdAsValue, TName>) {
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
        return (
          <DropDownInput
            {...rest}
            value={value}
            onBlur={onBlur}
            variant={props.variant ?? 'formInput'}
            useIdFunctionAsValue={useIdFunctionAsValue}
            idFunction={props.idFunction}
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
