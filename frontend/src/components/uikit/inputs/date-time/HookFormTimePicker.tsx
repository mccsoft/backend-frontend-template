import {
  Controller,
  FieldValues,
  FieldPath,
  UseControllerProps,
} from 'react-hook-form';
import { TimePicker, TimePickerProps } from './TimePicker';

type HookFormTimePickerProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = Omit<TimePickerProps, 'onTimeChanged' | 'timeInMills'> & {
  name: TName;
  control: UseControllerProps<TFieldValues>['control'];
  rules?: UseControllerProps<TFieldValues>['rules'];
  onFocus?: () => void;
  defaultValue?: unknown;
};

/*
We have to use Controlled components for native html datepickers,
because they require values in a form of 'yyyy-MM-DD', but in uncontrolled form react-hook-form
supply them with Date object.
 */
export function HookFormTimePicker<
  TFieldValues extends FieldValues = FieldValues,
>(props: HookFormTimePickerProps<TFieldValues>) {
  return (
    <Controller
      control={props.control}
      name={props.name}
      rules={props.rules}
      render={({ field }) => (
        <TimePicker
          {...props}
          timeInMills={field.value as any}
          onTimeChanged={(newTime: number | null, _isInvalid: boolean) => {
            field.onChange(newTime);
          }}
        />
      )}
    />
  );
}
