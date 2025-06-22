import React, { useState } from 'react';
import styles from './LoginPage.module.scss';
import { QueryFactory } from 'services/api';
import { format } from 'date-fns';
import { useScopedTranslation } from 'application/localization/useScopedTranslation';
import clsx from 'clsx';
import { Button } from 'components/uikit/buttons/Button';
import { Field } from 'components/uikit/Field';
import { Input } from 'components/uikit/inputs/Input';
import { useAdvancedForm } from 'helpers/form/useAdvancedForm';
import { requiredRule } from 'helpers/form/react-hook-form-helper';
import { FormError } from 'components/uikit/FormError';
import { Loading } from 'components/uikit/suspense/Loading';
import { handleLoginErrors, sendLoginRequest } from 'helpers/auth/auth-client';
import { queryClient } from 'helpers/queryClientHelper';
import { openExternalLoginRedirect } from 'helpers/auth/openid/openid-manager';

type Form = {
  login: string;
  password: string;
};

export const LoginPage: React.FC = (props) => {
  const [isLoading, setIsLoading] = useState(false);
  const serverVersionQuery = QueryFactory.VersionQuery.useVersionQuery({
    throwOnError: false,
  });
  const i18n = useScopedTranslation('Page.Login');
  const form = useAdvancedForm<Form>(async (data) => {
    setIsLoading(true);
    try {
      await sendLoginRequest(data.login, data.password);
      await queryClient.resetQueries();
    } catch (e) {
      handleLoginErrors(e);
    } finally {
      setIsLoading(false);
    }
  });
  return (
    <div className={styles.root}>
      <div className={styles.gridContainer}>
        <div className={styles.mainBackground}></div>
        <div className={styles.infoContainer}>
          <div className={styles.appName}>TemplateApp</div>
          <div className={styles.appInfo}>
            <span>
              © 2016—<span>{format(new Date(), 'yyyy')}</span> MCC Soft GmbH +
              Co. KG
            </span>
            <span>
              Server-Version: <span>{serverVersionQuery.data}</span>
            </span>
          </div>
        </div>
        <div className={styles.loginContainer}>
          <Loading loading={isLoading}>
            <form onSubmit={form.handleSubmitDefault}>
              <Field
                title={i18n.t('login_field')}
                className={styles.inputField}
                testId="Login"
              >
                <Input
                  {...form.register('login', { ...requiredRule() })}
                  errorText={form.formState.errors.login?.message}
                />
              </Field>

              <Field
                title={i18n.t('password_field')}
                className={styles.inputField}
                testId="Password"
              >
                <Input
                  {...form.register('password', { ...requiredRule() })}
                  type="password"
                  errorText={form.formState.errors.password?.message}
                />
              </Field>
              <FormError>
                {form.overallError
                  ? i18n.t(form.overallError.toLowerCase() as any, {
                      defaultValue: form.overallError,
                    })
                  : null}
              </FormError>
              <div className={styles.buttonContainer}>
                <Button title={i18n.t('login_button')} type="submit" />
                <Button
                  title={'Google'}
                  onClick={async () => {
                    try {
                      setIsLoading(true);
                      await openExternalLoginRedirect('Google');
                    } catch (e) {
                    } finally {
                      setIsLoading(false);
                    }
                  }}
                />
              </div>
            </form>
          </Loading>
        </div>
      </div>
    </div>
  );
};
