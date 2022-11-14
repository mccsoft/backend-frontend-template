import React, { useCallback, useContext, useMemo, useState } from 'react';
import { CustomModal } from './CustomModal';
import { Field } from '../Field';
import { Input } from '../inputs/Input';
import { Button, ButtonColor } from '../buttons/Button';
import { useScopedTranslation } from '../../../application/localization/useScopedTranslation';
import { ModalContextType, UseModalOptions } from './useModal.types';
import styles from './CustomModal.module.scss';
import { assertNever } from 'helpers/assert-never';
import clsx from 'clsx';

const ModalContext = React.createContext<ModalContextType>({} as any);

export const useModal = () => {
  return useContext(ModalContext);
};

export const ModalProvider: React.FC<React.PropsWithChildren> = (props) => {
  const i18n = useScopedTranslation('uikit.dialog');
  const [isShown, setIsShown] = useState<boolean>(false);
  const [options, setOptions] = useState<UseModalOptions | undefined>();
  const [fieldValue, setFieldValue] = useState('');

  const contextValue: ModalContextType = useMemo(() => {
    return {
      showAlert: async (options) => {
        return new Promise<void>((resolve, reject) => {
          setOptions({
            type: 'alert',
            ...options,
            resolve: resolve,
          });
          setIsShown(true);
        });
      },
      showConfirm: async (options) => {
        return new Promise<boolean>((resolve, reject) => {
          setOptions({
            type: 'confirm',
            ...options,
            resolve: resolve,
          });
          setIsShown(true);
        });
      },
      showPrompt: async (options) => {
        return new Promise<string | null>((resolve, reject) => {
          setFieldValue('');
          setOptions({
            type: 'prompt',
            ...options,
            resolve: resolve,
          });
          setIsShown(true);
        });
      },
      showMultiButton: async (options) => {
        return new Promise<string | null>((resolve, reject) => {
          setOptions({
            type: 'multibutton',
            ...options,
            resolve: resolve,
          });
          setIsShown(true);
        });
      },
    };
  }, []);

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
    <ModalContext.Provider value={contextValue}>
      {props.children}
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
                      title={
                        options.cancelButtonText ?? i18n.t('cancel_button')
                      }
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
    </ModalContext.Provider>
  );
};
