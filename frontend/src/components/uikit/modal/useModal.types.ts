import React from 'react';
import { ButtonColor } from '../buttons/Button';

export type UseModalOptions = {
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
  | (MultiButtonOptions & {
      type: 'multibutton';
      resolve: (result: string | null) => void;
    })
);
export type ErrorOptions = {
  /*
   * `id` is kinda 'out' parameter: it will be set by `showError` function.
   * You could use it to control the modal (e.g. `hide` it later)
   */
  id?: string;
  title?: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
};

export type AlertOptions = {
  /*
   * `id` is kinda 'out' parameter: it will be set by `showAlert` function.
   * You could use it to control the modal (e.g. `hide` it later)
   */
  id?: string;
  title: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
};
export type ConfirmOptions = {
  /*
   * `id` is kinda 'out' parameter: it will be set by `showConfirm` function.
   * You could use it to control the modal (e.g. `hide` it later)
   */
  id?: string;
  title: string;
  text: React.ReactNode;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
  cancelButtonText?: string;
  cancelButtonColor?: ButtonColor;
};
export type PromptOptions = {
  /*
   * `id` is kinda 'out' parameter: it will be set by `showPrompt` function.
   * You could use it to control the modal (e.g. `hide` it later)
   */
  id?: string;
  title: string;
  text: React.ReactNode;
  defaultValue: string;
  fieldName: string;
  okButtonText?: string;
  okButtonColor?: ButtonColor;
  cancelButtonText?: string;
  cancelButtonColor?: ButtonColor;
};
export type MultiButtonOptions = {
  /*
   * `id` is kinda 'out' parameter: it will be set by `showMultiButton` function.
   * You could use it to control the modal (e.g. `hide` it later)
   */
  id?: string;
  title: string;
  text: React.ReactNode;
  buttons: { id: string; text: string; color?: ButtonColor }[];
};
export type ModalContextType = {
  hide: (id: string) => void;
  showError: (options: ErrorOptions) => Promise<void>;
  showAlert: (options: AlertOptions) => Promise<void>;
  showConfirm: (options: ConfirmOptions) => Promise<boolean>;
  showPrompt: (options: PromptOptions) => Promise<string | null>;
  showMultiButton: (options: MultiButtonOptions) => Promise<string | null>;
};
