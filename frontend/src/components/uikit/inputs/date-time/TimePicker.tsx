import { addMilliseconds, format, startOfToday } from 'date-fns';
import React, { FC, useCallback, useMemo } from 'react';
import clsx from 'clsx';
import { ComboBoxInput } from 'components/uikit/inputs/dropdown/ComboBoxInput';

const styles = require('./TimePicker.module.scss');

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

  const time = timeString.match(/(\d+)(:(\d\d))?\s*(p?)/i);
  if (time == null) return null;

  let hours = parseInt(time[1], 10);
  if (hours == 12 && !time[4]) {
    hours = 0;
  } else {
    hours += hours < 12 && time[4] ? 12 : 0;
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
    error,
    errorText,
    timeEntriesIntervalInMinutes = 30,
    className,
  } = props;

  const timeEntries = useMemo(() => {
    const totalMillsInADay = 24 * 60 * 60000;
    const intervalInMills = timeEntriesIntervalInMinutes * 60000;
    const entries: TimeEntry[] = [];
    for (
      let timeInMills = 0;
      timeInMills <= totalMillsInADay;
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
    (timeAsString: string | null, selectedEntry: TimeEntry | null) => {
      if (selectedEntry) {
        onTimeChanged?.(selectedEntry.timeInMills, false);
      } else if (timeAsString) {
        let newTime = parseTime(timeAsString);
        if (
          minTimeInMills !== undefined &&
          newTime !== null &&
          newTime < minTimeInMills
        ) {
          newTime = null;
        }
        onTimeChanged?.(newTime, newTime === null);
      } else {
        onTimeChanged?.(null, false);
      }
    },
    [onTimeChanged, minTimeInMills],
  );

  return (
    <div className={clsx(styles.container, className)}>
      <ComboBoxInput
        options={timeEntries}
        labelFunction={(item) => item.label}
        idFunction={(item) => item.timeInMills.toString()}
        label={
          timeInMills !== null && timeInMills !== undefined
            ? millsToTimeString(timeInMills)
            : undefined
        }
        value={timeEntries.find((x) => x.timeInMills === timeInMills)}
        variant={'formInput'}
        calloutMaxHeight={200}
        noPlaceholder={true}
        error={error}
        errorText={errorText}
        onValueChanged={onChange}
      />
    </div>
  );
};
