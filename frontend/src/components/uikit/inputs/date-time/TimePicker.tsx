import { addMilliseconds, format, startOfToday } from 'date-fns';
import React, { FC, useCallback, useMemo } from 'react';
import clsx from 'clsx';
import { ComboBoxInput } from 'components/uikit/inputs/dropdown/ComboBoxInput';

import styles from './TimePicker.module.scss';

export type TimePickerProps = {
  minTimeInMills?: number;
  timeInMills: number | null | undefined;
  onTimeChanged?: (newTime: number | null, isInvalid: boolean) => void;
  error?: boolean;
  errorText?: string;
  /*
  interval between generated time entries, e.g. 01:00 and 01:30
   */
  timeEntriesIntervalInMinutes?: number;
  className?: string;
};

interface TimeEntry {
  timeInMills: number;
  label: string;
}

function parseTime(timeString: string): number | null {
  if (timeString == '') return null;

  const time = timeString.match(/(\d+)(:(\d\d))?/i);
  const hasAmPm =
    timeString.toLowerCase().includes('am') ||
    timeString.toLowerCase().includes('pm');
  const hasPm = timeString.toLowerCase().includes('pm');
  if (time == null) return null;

  let hours = parseInt(time[1], 10);
  if (hours == 12 && hasAmPm && !hasPm) {
    hours = 0;
  } else {
    hours += hours < 12 && hasPm ? 12 : 0;
  }
  const minutes = parseInt(time[3], 10) || 0;

  if (hours < 0 || hours > 24 || minutes < 0 || minutes > 60) {
    return null;
  }

  //convert to milliseconds
  return (hours * 60 + minutes) * 60000;
}

function millsToTimeString(timeInMills: number) {
  return format(addMilliseconds(startOfToday(), timeInMills), 'p');
}

export const TimePicker: FC<TimePickerProps> = (props) => {
  const {
    minTimeInMills,
    onTimeChanged,
    timeInMills,
    errorText,
    timeEntriesIntervalInMinutes = 30,
    className,
  } = props;

  const timeEntries: TimeEntry[] = useMemo(() => {
    const totalMillsInADay = 24 * 60 * 60000;
    const intervalInMills = timeEntriesIntervalInMinutes * 60000;
    const entries: TimeEntry[] = [];
    for (
      let timeInMills = 0;
      timeInMills < totalMillsInADay;
      timeInMills += intervalInMills
    ) {
      const label = millsToTimeString(timeInMills);
      entries.push({
        timeInMills,
        label: label,
      });
    }
    return entries.filter(
      (entry) => entry.timeInMills >= (minTimeInMills || 0),
    );
  }, [minTimeInMills, timeEntriesIntervalInMinutes]);

  const onChange = useCallback(
    (value: string | TimeEntry | null) => {
      if (value === null) {
        onTimeChanged?.(null, false);
      } else if (typeof value === 'string') {
        let newTime = parseTime(value);
        if (
          minTimeInMills !== undefined &&
          newTime !== null &&
          newTime < minTimeInMills
        ) {
          newTime = null;
        }
        onTimeChanged?.(newTime, newTime === null);
      } else {
        onTimeChanged?.(value.timeInMills, false);
      }
    },
    [onTimeChanged, minTimeInMills],
  );

  return (
    <div className={clsx(styles.container, className)}>
      <ComboBoxInput
        options={timeEntries}
        getOptionLabel={(item) =>
          typeof item === 'string' ? item : item.label
        }
        isOptionEqualToValue={(item1, item2) =>
          item1?.timeInMills === item2?.timeInMills
        }
        value={
          timeEntries.find((x) => x.timeInMills === timeInMills) ??
          (timeInMills ? millsToTimeString(timeInMills) : undefined)
        }
        errorText={errorText}
        onValueChanged={onChange}
        enableSearch={true}
      />
    </div>
  );
};
