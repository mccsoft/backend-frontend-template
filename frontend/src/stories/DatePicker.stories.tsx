import React from 'react';
import { StoryFn, Meta } from '@storybook/react-vite';
import { defaultExclude } from '../../.storybook/preview';
import { DatePicker } from 'components/uikit/inputs/date-time/DatePicker';
const StoryComponent = DatePicker;

export default {
  title: 'Example/DatePicker',
  component: StoryComponent,
  args: {} as Partial<React.ComponentProps<typeof StoryComponent>>,
  parameters: {
    controls: {
      exclude: [
        ...defaultExclude,
        'containerRef',
        'endAdornmentClassname',
        'onEnterPressed',
        'onChange',
      ] as (keyof React.ComponentProps<typeof StoryComponent>)[],
    },
  },
} as Meta<typeof StoryComponent>;

export const Default = {};

export const WithTime = {
  args: {
    withTime: true,
  },
};
