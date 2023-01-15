import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultExclude } from '../../../.storybook/preview';
import { useModal } from 'components/uikit/modal/useModal';
import { Button } from 'components/uikit/buttons/Button';
import { ConfirmOptions } from 'components/uikit/modal/useModal.types';
const StoryComponent = (props: ConfirmOptions) => {
  const modal = useModal();
  return (
    <Button
      title="Click me"
      onClick={async () => {
        alert('Result: ' + (await modal.showConfirm(props)));
      }}
    />
  );
};

export default {
  title: 'Example/Modal/Confirm',
  component: StoryComponent,
  args: {},
  parameters: {
    controls: {
      exclude: [...defaultExclude] as (keyof React.ComponentProps<
        typeof StoryComponent
      >)[],
    },
  },
} as ComponentMeta<typeof StoryComponent>;

const Template: ComponentStory<typeof StoryComponent> = (args) => (
  <StoryComponent {...args} />
);

export const Default = Template.bind({});
Default.args = {
  text: 'Custom_Text',
  title: 'Custom_Title',
};

export const WithAllParams = Template.bind({});
WithAllParams.args = {
  cancelButtonText: 'Cancel_Custom',
  okButtonText: 'Custom_Ok',
  text: 'Custom_Text',
  title: 'Custom_Title',
};
