import { format } from 'date-fns';

export function convertDateToString(
  date: Date | null | undefined,
  withTime?: boolean,
) {
  if (date === undefined) return undefined;

  if (!date) return '';

  return format(date, withTime === true ? "yyyy-MM-dd'T'HH:mm" : 'yyyy-MM-dd');
}
