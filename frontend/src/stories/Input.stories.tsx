import React from 'react';
import { StoryFn, Meta } from '@storybook/react-vite';
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
} as Meta<typeof StoryComponent>;

export const Default = {};

export const WithError = {
  args: {
    errorText: 'Required',
  },
};

export const Password = {
  args: {
    type: 'password',
  },
};

export const HelperText = {
  args: {
    helperText: 'some description for a field',
  },
};
