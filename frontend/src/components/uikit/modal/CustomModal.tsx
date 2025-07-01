import { Dialog, DialogContent, DialogTitle } from '@mui/material';
import CloseIcon from 'assets/icons/close-button.svg?react';
import styles from './CustomModal.module.scss';
import modalStyles from './Modal.module.scss';
import { Button, ButtonColor } from '../buttons/Button';
import clsx from 'clsx';
import { useScopedTranslation } from 'application/localization/useScopedTranslation';

export type CustomModalProps<T = string> = {
  onClose: () => void;
  isOpen: boolean;
  hideClose?: boolean;
  isBlocking: boolean;
  title: string;
  children: React.ReactNode;
} & (
  | { buttons?: undefined }
  | {
      buttons: 'ok';
      onButtonClick?: () => void;
      okButtonText?: string;
      okButtonColor?: ButtonColor;
    }
  | {
      buttons: 'ok-cancel';
      onButtonClick?: (buttonId: 'ok' | 'cancel') => void;
      okButtonText?: string;
      okButtonColor?: ButtonColor;

      cancelButtonText?: string;
      cancelButtonColor?: ButtonColor;
    }
  | {
      buttons: { id: T; text: string; color?: ButtonColor }[];
      onButtonClick?: (buttonId: T) => void;
    }
);

export const CustomModal: React.FC<CustomModalProps> = (props) => {
  const i18n = useScopedTranslation('uikit.dialog');
  return (
    <Dialog
      open={props.isOpen}
      onClose={() => {
        props.onClose();
      }}
      onClick={(e) => {
        e.stopPropagation();
      }}
    >
      <DialogTitle className={styles.header}>
        {props.title}
        {!props.hideClose && (
          <CloseIcon
            className={styles.closeButton}
            onClick={props.onClose}
          ></CloseIcon>
        )}
      </DialogTitle>
      <DialogContent className={styles.content}>
        {props.children}
        {!!props.buttons && (
          <div className={modalStyles.footer}>
            {props.buttons === 'ok' ? (
              <Button
                className={modalStyles.button}
                /* autofocus allows to close modals via Escape button if there are no inputs inside the modal */
                autoFocus={true}
                color={props.okButtonColor ?? ButtonColor.Default}
                type={'submit'}
                title={props.okButtonText ?? i18n.t('ok_button')}
                onClick={async () => {
                  props.onButtonClick?.();
                }}
              />
            ) : props.buttons === 'ok-cancel' ? (
              <>
                <Button
                  className={modalStyles.button}
                  color={ButtonColor.Secondary}
                  title={i18n.t('cancel_button')}
                  onClick={() => {
                    props.onButtonClick?.('cancel');
                  }}
                  data-test-id="dialog-cancelButton"
                />
                <Button
                  className={styles.button}
                  /* autofocus allows to close modals via Escape button if there are no inputs inside the modal */
                  autoFocus={true}
                  color={props.okButtonColor ?? ButtonColor.Default}
                  type={'submit'}
                  title={props.okButtonText ?? i18n.t('ok_button')}
                  onClick={async () => {
                    props.onButtonClick?.('ok');
                  }}
                />
              </>
            ) : (
              props.buttons.map((x, index) => (
                <Button
                  key={x.id}
                  autoFocus={index === props.buttons!.length - 1}
                  color={x.color ?? ButtonColor.Default}
                  className={clsx(styles.button, styles.multibutton)}
                  title={x.text}
                  onClick={() => {
                    props.onButtonClick?.(x.id);
                  }}
                />
              ))
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};
