import {
  addSeconds,
  differenceInHours,
  format,
  setDefaultOptions,
  startOfToday,
} from 'date-fns';

export function convertDateToString(
  date: Date | null | undefined,
  withTime?: boolean,
) {
  if (date === undefined) return undefined;

  if (!date) return '';

  return format(date, withTime === true ? "yyyy-MM-dd'T'HH:mm" : 'yyyy-MM-dd');
}

export const convertToShortDate = (date: Date) => {
  return format(date, 'do MMM');
};

export function formatDurationInSecondsAsHHMM(
  durationInSeconds: number,
): string {
  const startDate = startOfToday();
  const endDate = addSeconds(startOfToday(), durationInSeconds);
  return `${differenceInHours(startDate, endDate)}:${format(endDate, 'mm')}`;
}

export function setLocale(locale: Locale) {
  setDefaultOptions({ locale: locale });
}
