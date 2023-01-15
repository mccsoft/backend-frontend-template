import React from 'react';
import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultExclude } from '../../../.storybook/preview';
import { useModal } from 'components/uikit/modal/useModal';
import { Button } from 'components/uikit/buttons/Button';
import { AlertOptions } from 'components/uikit/modal/useModal.types';
const StoryComponent = (props: AlertOptions) => {
  const modal = useModal();
  return (
    <Button
      title="Click me"
      onClick={async () => {
        alert('Result: ' + (await modal.showAlert(props)));
      }}
    />
  );
};

export default {
  title: 'Example/Modal/Alert',
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

export const WithAllParams = Template.bind({});
WithAllParams.args = {
  okButtonText: 'Custom_Ok',
  text: 'Custom_Text',
  title: 'Custom_Title',
};

export const TwoModals = (props: AlertOptions) => {
  const modal = useModal();
  return (
    <Button
      title="Click me"
      onClick={async () => {
        await Promise.all([
          modal.showAlert({
            ...props,
            text:
              props.text +
              'asdasdshdagfjashgdfjkagsdjkfaghdkjfaghsdjfads asdkg fajsdgfjadg fjkagd fjha',
          }),
          modal.showAlert(props),
        ]);
      }}
    />
  );
};
