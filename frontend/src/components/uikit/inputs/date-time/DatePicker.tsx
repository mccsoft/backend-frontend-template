import { Input, Props as InputProps } from '../Input';
import React from 'react';
import { format, parse, parseISO } from 'date-fns';

export type DatePickerProps = {
  withTime?: boolean;
  value?: Date | null;
  defaultValue?: Date | null;
  onChange?: (value: Date | null) => void;
} & Omit<InputProps, 'value' | 'defaultValue' | 'onChange'>;
const minPossibleDate = new Date(1900, 1, 1);
const maxPossibleDate = new Date(2200, 1, 1);

export function dateToInputString(
  date: Date | null | undefined,
  withTime: boolean,
) {
  if (date === undefined) return undefined;

  if (!date) return '';

  return format(date, withTime === true ? "yyyy-MM-dd'T'HH:mm" : 'yyyy-MM-dd');
}

export function setValueAsDate(value: any) {
  if (value === undefined) return undefined;
  if (value === null) return null;
  return parse(value, "yyyy-MM-dd'T'HH:mm", new Date());
}

function convertDateToString(
  date?: Date | null | undefined,
  withTime?: boolean,
) {
  if (date === undefined) return undefined;

  if (!date) return '';

  const result = dateToInputString(date, withTime == true);
  return result;
}
export const DatePicker = React.forwardRef<HTMLInputElement, DatePickerProps>(
  function DatePicker(props, ref) {
    const { withTime, value, defaultValue, onChange, min, max, ...rest } =
      props;
    return (
      <Input
        {...rest}
        ref={ref}
        type={withTime ? 'datetime-local' : 'date'}
        min={min ?? convertDateToString(minPossibleDate, withTime)}
        max={max ?? convertDateToString(maxPossibleDate, withTime)}
        defaultValue={convertDateToString(defaultValue, withTime)}
        value={convertDateToString(value, withTime)}
        onChange={(e) => {
          if (!e || !e.target) return;

          const parsedTime: Date | null = e.target.value
            ? parseISO(e.target.value)
            : null;
          if (isNaN(parsedTime as any)) {
            return;
          }
          onChange?.(parsedTime);
        }}
      />
    );
  },
);
