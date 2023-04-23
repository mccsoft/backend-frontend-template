import React, {
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { CustomModal } from './CustomModal';
import { Field } from '../Field';
import { Input } from '../inputs/Input';
import { Button, ButtonColor } from '../buttons/Button';
import { useScopedTranslation } from '../../../application/localization/useScopedTranslation';
import type { ModalContextType, UseModalOptions } from './useModal.types';
import styles from './Modal.module.scss';
import { assertNever } from 'helpers/assert-never';
import clsx from 'clsx';
import { createId } from '../type-utils';

const ModalContext = React.createContext<ModalContextType>({} as any);

export const useModal = (): ModalContextType => {
  const context = useContext(ModalContext);
  const idsRef = useRef(new Set<string>());
  useEffect(() => {
    return () => {
      idsRef.current.forEach((x) => context.hide(x));
    };
  }, []);
  return useMemo(() => {
    return {
      hide: context.hide,
      showAlert(options) {
        const result = context.showAlert({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
      showConfirm(options) {
        const result = context.showConfirm({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
      showError(options) {
        const result = context.showError({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
      showPrompt(options) {
        const result = context.showPrompt({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
      showMultiButton(options) {
        const result = context.showMultiButton({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
    };
  }, [context]);
};

export const ModalProvider: React.FC<React.PropsWithChildren> = (props) => {
  const [modals, setModals] = useState<UseModalOptions[]>([]);
  const i18n = useScopedTranslation('uikit.dialog');
  const addModal = useCallback(
    (modal: UseModalOptions, promise: Promise<unknown>) => {
      setModals((o) => [...o, modal]);
      promise.finally(() => {
        // we use setTimeout to allow hiding form animations to finish
        setTimeout(
          () => setModals((o) => [...o.filter((x) => x.id !== modal.id)]),
          1000,
        );
      });
    },
    [],
  );
  const contextValue: ModalContextType = useMemo(() => {
    return {
      hide: (id: string) => {
        setModals((modals) => {
          return modals.filter((x) => x.id === id);
        });
      },
      showError: async (options) => {
        const id = createId();
        const promise = new Promise<void>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'alert',
                title: i18n.t('error_title'),
                ...options,
                id,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        (promise as any).id = id;
        return promise;
      },

      showAlert: async (options) => {
        const id = createId();
        const promise = new Promise<void>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'alert',
                ...options,
                id,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        (promise as any).id = id;
        return promise;
      },
      showConfirm: async (options) => {
        const id = createId();
        const promise = new Promise<boolean>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'confirm',
                ...options,
                id,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        (promise as any).id = id;
        return promise;
      },
      showPrompt: async (options) => {
        const id = createId();
        const promise = new Promise<string | null>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'prompt',
                ...options,
                id,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        (promise as any).id = id;
        return promise;
      },
      showMultiButton: async (options) => {
        const id = createId();
        const promise = new Promise<string | null>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'multibutton',
                ...options,
                id,
                resolve: resolve,
              },
              promise,
            ),
          );
        });
        (promise as any).id = id;
        return promise;
      },
    } as ModalContextType;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [addModal, i18n.i18n.language]);

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
                  errorText={options.fieldError}
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
                    color={
                      options.cancelButtonColor ?? options.okButtonColor
                        ? ButtonColor.Primary
                        : ButtonColor.Secondary
                    }
                    title={options.cancelButtonText ?? i18n.t('cancel_button')}
                    onClick={onClose}
                    data-test-id="dialog-cancelButton"
                  />
                ) : null}
                <Button
                  className={styles.button}
                  color={options.okButtonColor ?? ButtonColor.Default}
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
