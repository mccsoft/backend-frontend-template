import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
  FieldPath,
} from 'react-hook-form';
import { ComboBoxInput } from './ComboBoxInput';

type HookFormProps<
  T,
  TFieldValues extends FieldValues = FieldValues,
  Required extends boolean | undefined = undefined,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = Omit<
  React.ComponentProps<typeof ComboBoxInput<T, Required>>,
  'onValueChanged'
> & {
  name: TName;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
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
          value={value as any}
          onValueChanged={(value: any) => {
            onChange(value);
          }}
        />
      )}
    />
  );
}
