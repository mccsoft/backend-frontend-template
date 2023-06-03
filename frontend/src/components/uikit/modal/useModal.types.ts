import React from 'react';
import { ButtonColor } from '../buttons/Button';

export type UseModalOptions<T = string> = {
  id: string;
  title: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
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
  | (MultiButtonOptions<T> & {
      type: 'multibutton';
      resolve: (result: T | null) => void;
    })
);
export type ErrorOptions = {
  title?: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
};

export type AlertOptions = {
  title: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
};
export type ConfirmOptions = {
  title: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
  cancelButtonText?: string;
  cancelButtonColor?: ButtonColor;
};
export type PromptOptions = {
  title: string;
  text: React.ReactNode;
  defaultValue: string;
  fieldName: string;
  fieldError?: string;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
  cancelButtonText?: string;
  cancelButtonColor?: ButtonColor;
};
export type MultiButtonOptions<T = string> = {
  title: string;
  text: React.ReactNode;
  buttons: { id: T; text: string; color?: ButtonColor }[];
};
export type ModalContextType = {
  hide: (id: string) => void;
  showError: (options: ErrorOptions) => Promise<void> & { id: string };

  /*
   * Shows confirmation message with a single Ok button.
   * Returns a Promise which is resolved when Ok is pressed or Modal is closed.
   */
  showAlert: (options: AlertOptions) => Promise<void> & { id: string };

  /*
   * Shows confirmation message with 2 buttons: Cancel and Ok.
   * Returns `true` if Ok was pressed; `false` if Cancel was pressed (or if Modal was closed).
   * If you want to differentiate between Cancel and Close, please use `showMultiButton`.
   */
  showConfirm: (options: ConfirmOptions) => Promise<boolean> & { id: string };
  showPrompt: (
    options: PromptOptions,
  ) => Promise<string | null> & { id: string };
  showMultiButton: <T = string>(
    options: MultiButtonOptions<T>,
  ) => Promise<T | null> & { id: string };
};
