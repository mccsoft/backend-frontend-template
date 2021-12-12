import { addSeconds, differenceInHours, format, startOfToday } from 'date-fns';

export function convertDateToString(
  date: Date | null | undefined,
  withTime?: boolean,
) {
  if (date === undefined) return undefined;

  if (!date) return '';

  return format(date, withTime === true ? "yyyy-MM-dd'T'HH:mm" : 'yyyy-MM-dd');
}

export const convertToShortDate = (date: Date) => {
  return localFormat(date, 'do MMM');
};

export function formatDurationInSecondsAsHHMM(
  durationInSeconds: number,
): string {
  const startDate = startOfToday();
  const endDate = addSeconds(startOfToday(), durationInSeconds);
  return `${differenceInHours(startDate, endDate)}:${format(endDate, 'mm')}`;
}

let _locale: Locale;

/*
 * Use `localFormat` everywhere, otherwise it's not possible to specify locale globally
 * That's an advice from https://github.com/date-fns/date-fns/issues/816#issuecomment-961280538
 */
export function localFormat(date: Date, dateFormat: string): string {
  return format(date, dateFormat, { locale: _locale });
}

export function setLocale(locale: Locale) {
  _locale = locale;
}
