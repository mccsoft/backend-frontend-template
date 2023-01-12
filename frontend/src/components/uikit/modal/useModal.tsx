import React, { useCallback, useContext, useMemo, useState } from 'react';
import { CustomModal } from './CustomModal';
import { Field } from '../Field';
import { Input } from '../inputs/Input';
import { Button, ButtonColor } from '../buttons/Button';
import { useScopedTranslation } from '../../../application/localization/useScopedTranslation';
import { ModalContextType, UseModalOptions } from './useModal.types';
import styles from './Modal.module.scss';
import { assertNever } from 'helpers/assert-never';
import clsx from 'clsx';
import { createId } from '../type-utils';

const ModalContext = React.createContext<ModalContextType>({} as any);

export const useModal = () => {
  return useContext(ModalContext);
};

export const ModalProvider: React.FC<React.PropsWithChildren> = (props) => {
  const [modals, setModals] = useState<UseModalOptions[]>([]);
  const addModal = useCallback(
    (modal: Omit<UseModalOptions, 'id'>, promise: Promise<unknown>) => {
      const id = createId();
      const options: UseModalOptions = { id: id, ...modal } as any;

      setModals((o) => [...o, options]);
      promise.finally(() => {
        // we use setTimeout to allow hiding form animations to finish
        setTimeout(
          () => setModals((o) => [...o.filter((x) => x.id !== id)]),
          1000,
        );
      });
    },
    [],
  );
  const contextValue: ModalContextType = useMemo(() => {
    return {
      showAlert: async (options) => {
        const promise = new Promise<void>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'alert',
                ...options,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        return promise;
      },
      showConfirm: async (options) => {
        const promise = new Promise<boolean>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'confirm',
                ...options,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        return promise;
      },
      showPrompt: async (options) => {
        const promise = new Promise<string | null>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'prompt',
                ...options,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        return promise;
      },
      showMultiButton: async (options) => {
        const promise = new Promise<string | null>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'multibutton',
                ...options,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        return promise;
      },
    };
  }, []);

  return (
    <ModalContext.Provider value={contextValue}>
      {props.children}
      {modals.map((x) => (
        <SingleModal key={x.id} options={x} />
      ))}
    </ModalContext.Provider>
  );
};

type SingleModalProps = { options: UseModalOptions };
const SingleModal: React.FC<SingleModalProps> = (props) => {
  const options = props.options;
  const i18n = useScopedTranslation('uikit.dialog');
  const [isShown, setIsShown] = useState<boolean>(true);
  const [fieldValue, setFieldValue] = useState(
    options.type === 'prompt' ? options.defaultValue : '',
  );

  const onClose = useCallback(() => {
    setIsShown(false);
    if (!options) return;

    const type = options.type;
    switch (type) {
      case 'alert':
        options.resolve();
        break;

      case 'confirm':
        options.resolve(false);
        break;

      case 'prompt':
        options.resolve(null);
        break;

      case 'multibutton':
        options.resolve(null);
        break;

      default:
        assertNever(type);
    }
  }, [options]);

  return (
    <CustomModal
      isOpen={isShown}
      isBlocking={true}
      title={options?.title ?? ''}
      onClose={onClose}
    >
      {options ? (
        <>
          <div className={styles.body}>
            {options.text}
            {options.type === 'prompt' ? (
              <Field title={options.fieldName}>
                <Input
                  value={fieldValue}
                  onChange={(e) => setFieldValue(e.target.value)}
                />
              </Field>
            ) : null}
          </div>
          <div className={styles.footer}>
            {options.type === 'multibutton' ? (
              options.buttons.map((x) => (
                <Button
                  key={x.id}
                  color={x.color ?? ButtonColor.Default}
                  className={clsx(styles.button, styles.multibutton)}
                  title={x.text}
                  onClick={() => {
                    options.resolve(x.id);
                    onClose();
                  }}
                />
              ))
            ) : (
              <>
                {options.type === 'confirm' || options.type === 'prompt' ? (
                  <Button
                    className={styles.button}
                    color={ButtonColor.Secondary}
                    title={options.cancelButtonText ?? i18n.t('cancel_button')}
                    onClick={onClose}
                  />
                ) : null}
                <Button
                  className={styles.button}
                  color={ButtonColor.Default}
                  type={'submit'}
                  title={options.okButtonText ?? i18n.t('ok_button')}
                  onClick={async () => {
                    if (options.type === 'confirm') options.resolve(true);
                    else if (options.type === 'prompt') {
                      options.resolve(fieldValue);
                      setFieldValue('');
                    } else if (options.type === 'alert') options.resolve();
                    setIsShown(false);
                  }}
                />
              </>
            )}
          </div>
        </>
      ) : null}
    </CustomModal>
  );
};
