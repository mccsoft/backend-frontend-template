import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultExclude } from '../../.storybook/preview';
import { DatePicker } from 'components/uikit/inputs/date-time/DatePicker';
const StoryComponent = DatePicker;

export default {
  title: 'Example/DatePicker',
  component: StoryComponent,
  args: {},
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
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (args) => (
  <StoryComponent {...args} />
);

export const Default = Template.bind({});

export const WithTime = Template.bind({});
WithTime.args = {
  withTime: true,
};

// export const TimeSetToNonPresentValue = Template.bind({});
// TimeSetToNonPresentValue.args = {
//   timeInMills: 1000 * 60 * 2, // 00:02
// };

// export const MinTime = Template.bind({});
// MinTime.args = {
//   minTimeInMills: 1000 * 60 * 60 * 2.1,
// };

// export const Interval10Minutes = Template.bind({});
// Interval10Minutes.args = {
//   timeEntriesIntervalInMinutes: 10,
// };
