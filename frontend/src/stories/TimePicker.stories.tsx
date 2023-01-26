import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
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
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (
  args: React.ComponentProps<typeof StoryComponent>,
) => <StoryComponent {...args} />;

export const Default = Template.bind({});

export const TimeSet = Template.bind({});
TimeSet.args = {
  timeInMills: 1000 * 60 * 30, // 00:30
};

export const TimeSetToNonPresentValue = Template.bind({});
TimeSetToNonPresentValue.args = {
  timeInMills: 1000 * 60 * 2, // 00:02
};

export const MinTime = Template.bind({});
MinTime.args = {
  minTimeInMills: 1000 * 60 * 60 * 2.1,
};

export const Interval10Minutes = Template.bind({});
Interval10Minutes.args = {
  timeEntriesIntervalInMinutes: 10,
};
