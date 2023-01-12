import React from 'react';
import { ButtonColor } from '../buttons/Button';

export type UseModalOptions = {
  id: string;
  title: string;
  text: string | React.ReactNode;
  okButtonText?: string;
} & (
  | {
      type: 'alert';
      resolve: () => void;
    }
  | (ConfirmOptions & {
      type: 'confirm';
      resolve: (result: boolean) => void;
    })
  | (PromptOptions & {
      type: 'prompt';
      resolve: (result: string | null) => void;
    })
  | (MultiButtonOptions & {
      type: 'multibutton';
      resolve: (result: string | null) => void;
    })
);
export type AlertOptions = {
  title: string;
  text: string;
  okButtonText?: string;
};
export type ConfirmOptions = {
  title: string;
  text: string;
  okButtonText?: string;
  cancelButtonText?: string;
};
export type PromptOptions = {
  title: string;
  text: string;
  defaultValue: string;
  fieldName: string;
  okButtonText?: string;
  cancelButtonText?: string;
};
export type MultiButtonOptions = {
  title: string;
  text: string | React.ReactNode;
  buttons: { id: string; text: string; color?: ButtonColor }[];
};
export type ModalContextType = {
  showAlert: (options: AlertOptions) => Promise<void>;
  showConfirm: (options: ConfirmOptions) => Promise<boolean>;
  showPrompt: (options: PromptOptions) => Promise<string | null>;
  showMultiButton: (options: MultiButtonOptions) => Promise<string | null>;
};
