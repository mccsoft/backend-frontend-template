import { useScopedTranslation } from 'application/localization/useScopedTranslation';
import { Button } from 'components/uikit/buttons/Button';
import { Field } from 'components/uikit/Field';
import { FormError } from 'components/uikit/FormError';
import { Input } from 'components/uikit/inputs/Input';
import { Loading } from 'components/uikit/suspense/Loading';
import { useAdvancedForm } from 'helpers/form/useAdvancedForm';
import { requiredRule } from 'helpers/form/react-hook-form-helper';
import Logger from 'js-logger';
import React, { useCallback } from 'react';
import { sendLoginRequest } from 'services/auth-client';
import Grid from '@material-ui/core/Grid';
import { setAuthData } from '../../../helpers/interceptors/auth/auth-interceptor';

type LoginForm = {
  login: string;
  password: string;
};

export const LoginPage: React.FC = () => {
  const i18n = useScopedTranslation('Page.Login');
  const form = useAdvancedForm<LoginForm>(
    useCallback(async (data) => {
      const response = await sendLoginRequest(data.login, data.password);
      setAuthData(response);
      Logger.info('Logged in successfully');
    }, []),
  );
  return (
    <Loading loading={form.formState.isSubmitting}>
      <Grid container justify={'center'}>
        <Grid item xs={12} md={6} lg={4}>
          <form onSubmit={form.handleSubmitDefault}>
            <Field title={i18n.t('login_field')}>
              <Input
                {...form.register('login', { ...requiredRule() })}
                errorText={form.formState.errors.login?.message}
              />
            </Field>
            <Field title={i18n.t('password_field')}>
              <Input
                type={'password'}
                {...form.register('password', { ...requiredRule() })}
                errorText={form.formState.errors.password?.message}
              />
            </Field>
            <FormError>{form.overallError}</FormError>
            <Button type={'submit'} title={i18n.t('login_button')} />
          </form>
        </Grid>
      </Grid>
    </Loading>
  );
};
