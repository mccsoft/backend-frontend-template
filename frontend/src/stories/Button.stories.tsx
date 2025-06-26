import React from 'react';
import { Meta } from '@storybook/react-vite';
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
} as Meta<typeof StoryComponent>;

export const Default = {};

export const Primary = {
  args: {
    color: ButtonColor.Primary,
  },
};

export const Secondary = {
  args: {
    color: ButtonColor.Secondary,
  },
};

export const Danger = {
  args: {
    color: ButtonColor.Danger,
  },
};
