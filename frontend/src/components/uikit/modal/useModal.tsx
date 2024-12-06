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
import useEventCallback from '@mui/material/utils/useEventCallback';
import { Loading } from '../suspense/Loading';

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
      showCustom(options) {
        const result = context.showCustom({ ...options });
        idsRef.current.add(result.id);
        void result.then(() => idsRef.current.delete(result.id));
        return result;
      },
    };
  }, [context]);
};

export const ModalProvider: React.FC<React.PropsWithChildren> = (props) => {
  const [modals, setModals] = useState<UseModalOptions<any>[]>([]);
  const i18n = useScopedTranslation('uikit.dialog');
  const addModal = useCallback(
    (modal: UseModalOptions<any>, promise: Promise<unknown>) => {
      setModals((o) => [...o, modal]);
      void promise.finally(() => {
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
      showCustom: async (options) => {
        const id = createId();
        const promise = new Promise<any | null>((resolve, reject) => {
          // we use setTimout to be able to access the promise
          setTimeout(() =>
            addModal(
              {
                type: 'custom',
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
  const [isShown, setIsShown] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [fieldValue, setFieldValue] = useState(
    options.type === 'prompt' ? options.defaultValue ?? '' : '',
  );
  const [fieldError, setFieldError] = useState(
    options.type === 'prompt' ? options.fieldError ?? '' : '',
  );
  const [customValue, setCustomValue] = useState<any>(
    options.type === 'custom' ? options.defaultValue ?? null : null,
  );
  const [_, rerender] = useState(1);
  const commonClose = useCallback(() => {
    setIsShown(false);
  }, []);
  const onClose = useCallback(() => {
    commonClose();
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
      case 'multibutton':
      case 'custom':
        options.resolve(null);
        break;

      default:
        assertNever(type);
    }
  }, [commonClose, options]);

  const onCloseCustom = useEventCallback((value: any) => {
    setIsShown(false);
    if (!options || options.type !== 'custom') return;
    options.resolve(value);
  });

  const inputRef = useRef<HTMLInputElement>(null);
  useEffect(
    function focusInputWhenShown() {
      if (!isShown) return;
      inputRef.current?.focus();
      setTimeout(() => inputRef.current?.focus(), 100);
      setTimeout(() => inputRef.current?.focus(), 1000);
    },
    [isShown],
  );

  return (
    <CustomModal
      isOpen={isShown}
      isBlocking={true}
      title={options?.title ?? ''}
      onClose={onClose}
    >
      <Loading loading={isLoading}>
        {options ? (
          <>
            <div className={styles.body}>
              {options.type === 'custom' ? (
                <options.Component
                  value={customValue}
                  setValue={setCustomValue}
                  onClose={onCloseCustom}
                />
              ) : (
                <>
                  {options.text}
                  {options.type === 'prompt' ? (
                    <Field title={options.fieldName}>
                      <Input
                        value={fieldValue}
                        autoFocus={true}
                        ref={inputRef}
                        onChange={(e) => setFieldValue(e.target.value)}
                        errorText={fieldError}
                        maxLength={options.maxLength}
                        onKeyDown={(event) => {
                          if (event.key === 'Enter') {
                            commonClose();
                            options.resolve(fieldValue);
                            setFieldValue('');
                          }
                        }}
                      />
                    </Field>
                  ) : null}
                </>
              )}
            </div>
            <div className={styles.footer}>
              {options.type === 'custom' && options.Controls ? (
                <options.Controls
                  value={customValue}
                  setValue={setCustomValue}
                  onClose={onCloseCustom}
                />
              ) : (
                <>
                  {options.type === 'multibutton' ? (
                    options.buttons.map((x, index) => (
                      <Button
                        key={x.id}
                        autoFocus={index === options.buttons.length - 1}
                      color={x.color ?? ButtonColor.Default}
                        className={clsx(styles.button, styles.multibutton)}
                        title={x.text}
                        onClick={() => {
                          options.resolve(x.id);
                          commonClose();
                        }}
                      />
                    ))
                  ) : (
                    <>
                      {options.type === 'confirm' ||
                      options.type === 'prompt' ||
                      options.type === 'custom' ? (
                        <Button
                          className={styles.button}
                          color={
                            options.cancelButtonColor ??
                            (options.okButtonColor
                              ? ButtonColor.Primary
                              : ButtonColor.Secondary)
                          }
                          title={
                            options.cancelButtonText ?? i18n.t('cancel_button')
                          }
                          onClick={() => {
                            if (options.type === 'confirm')
                              options.resolve(false);
                            else options.resolve(null);
                            commonClose();
                          }}
                          data-test-id="dialog-cancelButton"
                        />
                      ) : null}
                      <Button
                        className={styles.button}
                        /* autofocus allows to close modals via Escape button if there are no inputs inside the modal */
                        autoFocus={true}
                      color={options.okButtonColor ?? ButtonColor.Default}
                        type={'submit'}
                        title={options.okButtonText ?? i18n.t('ok_button')}
                        onClick={async () => {
                          const type = options.type;
                          switch (type) {
                            case 'alert':
                              options.resolve();
                              break;

                            case 'confirm':
                              options.resolve(true);
                              break;

                            case 'prompt':
                              if (options.onSubmit) {
                                const resultPromise =
                                  options.onSubmit(fieldValue);
                                if (isPromise(resultPromise)) {
                                  setIsLoading(true);
                                }
                                try {
                                  const result = await resultPromise;

                                  if (result === false) {
                                    return;
                                  } else if (typeof result === 'string') {
                                    setFieldError(result);
                                    return;
                                  }
                                } finally {
                                  setIsLoading(false);
                                }
                              }
                              options.resolve(fieldValue);
                              setFieldValue('');
                              break;

                            case 'custom':
                              if (options.onSubmit) {
                                const resultPromise =
                                  options.onSubmit(customValue);
                                if (isPromise(resultPromise)) {
                                  setIsLoading(true);
                                }
                                try {
                                  const result = await resultPromise;

                                  if (!result) {
                                    setCustomValue(customValue);
                                    rerender((x) => x + 1);
                                    return;
                                  }
                                } finally {
                                  setIsLoading(false);
                                }
                              }

                              options.resolve(customValue);
                              setCustomValue(null);
                              break;

                            default:
                              assertNever(type);
                          }

                          setIsShown(false);
                        }}
                      />
                    </>
                  )}
                </>
              )}
            </div>
          </>
        ) : null}
      </Loading>
    </CustomModal>
  );
};
function isPromise(value: any) {
  return Boolean(value && typeof value.then === 'function');
}
