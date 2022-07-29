import { Modal } from '@fluentui/react';
import { ReactComponent as CloseIcon } from 'assets/icons/close-button.svg';
import styles from './CustomModal.module.scss';

export type CustomModalProps = {
  onClose: () => void;
  isOpen: boolean;
  isBlocking: boolean;
  title: String;
  children: React.ReactNode;
};

export const CustomModal: React.FC<CustomModalProps> = (props) => {
  return (
    <Modal
      isOpen={props.isOpen}
      onDismiss={props.onClose}
      isBlocking={props.isBlocking}
      allowTouchBodyScroll={false}
    >
      <div className={styles.modal}>
        <div className={styles.header}>
          <div className={styles.title}>{props.title}</div>
          <CloseIcon
            className={styles.closeButton}
            onClick={props.onClose}
          ></CloseIcon>
        </div>
        <div className={styles.content}>{props.children}</div>
      </div>
    </Modal>
  );
};
