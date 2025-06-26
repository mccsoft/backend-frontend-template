import React from 'react';
import { StoryFn, Meta } from '@storybook/react-vite';
import { TimePicker } from 'components/uikit/inputs/date-time/TimePicker';
import { defaultExclude } from '../../.storybook/preview';
const StoryComponent = TimePicker;

export default {
  title: 'Example/TimePicker',
  component: StoryComponent,
  args: {} as Partial<React.ComponentProps<typeof StoryComponent>>,
  parameters: {
    controls: {
      exclude: [
        ...defaultExclude,
        'onTimeChanged',
      ] as (keyof React.ComponentProps<typeof StoryComponent>)[],
    },
  },
} as Meta<typeof StoryComponent>;

export const Default = {};

export const TimeSet = {
  args: {
    timeInMills: 1000 * 60 * 30, // 00:30
  },
};

export const TimeSetToNonPresentValue = {
  args: {
    timeInMills: 1000 * 60 * 2, // 00:02
  },
};

export const MinTime = {
  args: {
    minTimeInMills: 1000 * 60 * 60 * 2.1,
  },
};

export const Interval10Minutes = {
  args: {
    timeEntriesIntervalInMinutes: 10,
  },
};
