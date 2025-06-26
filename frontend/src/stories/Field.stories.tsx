import React from 'react';
import { StoryFn, Meta } from '@storybook/react-vite';
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
} as Meta<typeof StoryComponent>;

export const Default = {
  render: (args: React.ComponentProps<typeof StoryComponent>) => (
    <StoryComponent {...args}>
      <Input />
    </StoryComponent>
  ),
};

export const WithLinkProps = {
  render: (args: React.ComponentProps<typeof StoryComponent>) => (
    <StoryComponent {...args}>
      <Input />
    </StoryComponent>
  ),

  args: {
    linkProps: {
      title: 'Add',
      onClick() {
        alert('qwe');
      },
    },
  },
};

export const WithHint = {
  render: (args: React.ComponentProps<typeof StoryComponent>) => (
    <StoryComponent {...args}>
      <Input />
    </StoryComponent>
  ),

  args: {
    hint: 'hint',
  },
};
