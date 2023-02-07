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
  okButtonText?: string;
  okButtonColor?: ButtonColor;
  cancelButtonText?: string;
  cancelButtonColor?: ButtonColor;
};
export type MultiButtonOptions = {
  title: string;
  text: React.ReactNode;
  buttons: { id: string; text: string; color?: ButtonColor }[];
};
export type ModalContextType = {
  hide: (id: string) => void;
  showError: (options: ErrorOptions) => Promise<void> & { id: string };
  showAlert: (options: AlertOptions) => Promise<void> & { id: string };
  showConfirm: (options: ConfirmOptions) => Promise<boolean> & { id: string };
  showPrompt: (
    options: PromptOptions,
  ) => Promise<string | null> & { id: string };
  showMultiButton: (
    options: MultiButtonOptions,
  ) => Promise<string | null> & { id: string };
};
