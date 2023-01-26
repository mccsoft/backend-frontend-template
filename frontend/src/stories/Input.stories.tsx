import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultExclude } from '../../.storybook/preview';
import { Input } from 'components/uikit/inputs/Input';
const StoryComponent = Input;

export default {
  title: 'Example/Input',
  component: StoryComponent,
  args: {} as Partial<React.ComponentProps<typeof StoryComponent>>,
  parameters: {
    controls: {
      exclude: [
        ...defaultExclude,
        'containerRef',
        'endAdornmentClassname',
        'onEnterPressed',
      ] as (keyof React.ComponentProps<typeof StoryComponent>)[],
    },
  },
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (
  args: React.ComponentProps<typeof StoryComponent>,
) => <StoryComponent {...args} />;

export const Default = Template.bind({});

export const WithError = Template.bind({});
WithError.args = {
  errorText: 'Required',
};

export const Password = Template.bind({});
Password.args = {
  type: 'password',
};

export const HelperText = Template.bind({});
HelperText.args = {
  helperText: 'some description for a field',
};
