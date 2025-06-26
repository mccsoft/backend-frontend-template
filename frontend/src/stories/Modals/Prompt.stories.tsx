import React from 'react';
import { Meta } from '@storybook/react-vite';
import { defaultExclude } from '../../../.storybook/preview';
import { useModal } from 'components/uikit/modal/useModal';
import { Button } from 'components/uikit/buttons/Button';
import { PromptOptions } from 'components/uikit/modal/useModal.types';
const StoryComponent = (props: PromptOptions) => {
  const modal = useModal();
  return (
    <Button
      title="Click me"
      onClick={async () => {
        alert('Result: ' + (await modal.showPrompt(props)));
      }}
    />
  );
};

export default {
  title: 'Example/Modal/Prompt',
  component: StoryComponent,
  args: {} as Partial<React.ComponentProps<typeof StoryComponent>>,
  parameters: {
    controls: {
      exclude: [...defaultExclude] as (keyof React.ComponentProps<
        typeof StoryComponent
      >)[],
    },
  },
} as Meta<typeof StoryComponent>;

export const Default = {
  args: {
    text: 'Custom_Text',
    title: 'Custom_Title',
  },
};

export const WithAllParams = {
  args: {
    fieldName: 'Field name',
    cancelButtonText: 'Cancel_Custom',
    okButtonText: 'Custom_Ok',
    text: 'Custom_Text',
    title: 'Custom_Title',
  },
};
