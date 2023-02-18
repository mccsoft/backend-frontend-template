import { Dialog, DialogContent, DialogTitle } from '@mui/material';
import { ReactComponent as CloseIcon } from 'assets/icons/close-button.svg';
import styles from './CustomModal.module.scss';

export type CustomModalProps = {
  onClose: () => void;
  isOpen: boolean;
  isBlocking: boolean;
  title: string;
  children: React.ReactNode;
};

export const CustomModal: React.FC<CustomModalProps> = (props) => {
  return (
    <Dialog open={props.isOpen} onClose={props.onClose}>
      <DialogTitle className={styles.header}>
        {props.title}
        <CloseIcon
          className={styles.closeButton}
          onClick={props.onClose}
          data-test-id={'dialog-close-button'}
        ></CloseIcon>
      </DialogTitle>
      <DialogContent className={styles.content}>{props.children}</DialogContent>
    </Dialog>
  );
};
