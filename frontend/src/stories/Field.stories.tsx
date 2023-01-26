import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultExclude } from '../../.storybook/preview';
import { Field } from 'components/uikit/Field';
import { Input } from 'components/uikit/inputs/Input';
const StoryComponent = Field;

export default {
  title: 'Example/Field',
  component: StoryComponent,
  args: {
    title: 'Field',
  } as Partial<React.ComponentProps<typeof StoryComponent>>,
  argTypes: {
    id: {
      control: false,
    },
  },
  parameters: {
    controls: {
      exclude: [...defaultExclude] as (keyof React.ComponentProps<
        typeof StoryComponent
      >)[],
    },
  },
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (
  args: React.ComponentProps<typeof StoryComponent>,
) => (
  <StoryComponent {...args}>
    <Input />
  </StoryComponent>
);

export const Default = Template.bind({});

export const WithLinkProps = Template.bind({});
WithLinkProps.args = {
  linkProps: {
    title: 'Add',
    onClick() {
      alert('qwe');
    },
  },
};

export const WithHint = Template.bind({});
WithHint.args = {
  hint: 'hint',
};
