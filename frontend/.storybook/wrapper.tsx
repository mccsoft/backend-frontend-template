import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ModalProvider } from '../src/components/uikit/modal/useModal';
export const wrapInRouter = (Story: any) => {
  return (
    <BrowserRouter>
      <Story />
    </BrowserRouter>
  );
};
export const wrapInModal = (Story: any) => {
  return (
    <ModalProvider>
      <Story />
    </ModalProvider>
  );
};
