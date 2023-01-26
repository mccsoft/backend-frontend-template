import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { Button, ButtonColor } from '../components/uikit/buttons/Button';
import { defaultExclude } from '../../.storybook/preview';

const StoryComponent = Button;

export default {
  title: 'Example/Button',
  component: StoryComponent,

  args: {
    color: ButtonColor.Default,
    title: 'Button',
  } as Partial<React.ComponentProps<typeof StoryComponent>>,
  parameters: {
    controls: {
      exclude: [
        ...defaultExclude,
        'icon',
        'iconClassName',
      ] as (keyof React.ComponentProps<typeof StoryComponent>)[],
    },
  },
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (
  args: React.ComponentProps<typeof StoryComponent>,
) => <StoryComponent {...args} />;

export const Default = Template.bind({});

export const Primary = Template.bind({});
Primary.args = {
  color: ButtonColor.Primary,
};

export const Secondary = Template.bind({});
Secondary.args = {
  color: ButtonColor.Secondary,
};

export const Danger = Template.bind({});
Danger.args = {
  color: ButtonColor.Danger,
};
