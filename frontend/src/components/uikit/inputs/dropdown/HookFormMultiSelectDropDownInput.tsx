import { MultiSelectDropDownInput } from './MultiSelectDropDownInput';
import * as React from 'react';
import {
  Control,
  Controller,
  FieldValues,
  Path,
  RegisterOptions,
} from 'react-hook-form';
import { StyledAutocompleteProps } from './types';

type HookFormProps<
  T,
  Required extends boolean | undefined,
  TFieldValues extends FieldValues = FieldValues,
> = Omit<StyledAutocompleteProps<T, true, Required, false>, 'onChange'> & {
  name: Path<TFieldValues>;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: T | null;

  hasSearchFilter?: boolean;
  /*
   * Hides the header with `All/None` buttons and a title
   */
  hideHeader?: boolean;
  /*
   * Text that is put in the Header. Usually a name of a field
   */
  headerTitle?: string;
};

export function HookFormMultiSelectDropDownInput<
  D,
  Required extends boolean,
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormProps<D, Required, TFieldValues>) {
  const { control, name, rules, ...rest } = props;
  return (
    <Controller
      control={control}
      name={name}
      rules={rules}
      render={({ field: { onChange, onBlur, value } }) => {
        return (
          <MultiSelectDropDownInput
            {...rest}
            value={value}
            onBlur={onBlur}
            required={props.required}
            variant={props.variant ?? 'formInput'}
            onValueChanged={(v: ReadonlyArray<D> | null) => {
              onChange(v!);
            }}
          />
        );
      }}
    />
  );
}
