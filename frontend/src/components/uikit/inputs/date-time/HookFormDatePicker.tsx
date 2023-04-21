import { DatePath } from '../hook-form';
import { DatePicker, DatePickerProps } from './DatePicker';
import {
  Control,
  Controller,
  FieldValues,
  RegisterOptions,
} from 'react-hook-form';

type HookFormDatePickerProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends DatePath<TFieldValues> = DatePath<TFieldValues>,
> = Omit<DatePickerProps, 'onChange' | 'name'> & {
  name: TName;
  control: Control<TFieldValues>;
  rules?: Exclude<RegisterOptions, 'valueAsDate' | 'setValueAs'>;
  onFocus?: () => void;
  defaultValue?: unknown;
};

/*
We have to use Controlled components for native html datepickers,
because they require values in a form of 'yyyy-MM-DD', but in uncontrolled form react-hook-form
supply them with Date object.
 */
export function HookFormDatePicker<
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormDatePickerProps<TFieldValues>) {
  const { name, rules, control, ...rest } = props;
  return (
    <Controller
      control={control}
      // Without `any` here TS complains about `Type instantiation is excessively deep and possibly infinite`
      // Type types are correct though.
      name={name as any}
      rules={rules}
      render={({ field }) => (
        <DatePicker
          {...rest}
          value={field.value as any}
          onChange={field.onChange}
        />
      )}
    />
  );
}
