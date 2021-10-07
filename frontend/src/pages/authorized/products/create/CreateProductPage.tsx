import { Grid } from '@material-ui/core';
import { Routes } from 'application/constants/routes';
import { useScopedTranslation } from 'application/localization/useScopedLocalization';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { Button, ButtonColor } from 'components/uikit/buttons/Button';
import { Field } from 'components/uikit/Field';
import { FormError } from 'components/uikit/FormError';
import { Input } from 'components/uikit/inputs/Input';
import { Loading } from 'components/uikit/suspense/Loading';
import { useAdvancedForm } from 'helpers/form/advanced-form';
import { requiredRule } from 'helpers/form/react-hook-form-helper';
import React, { useCallback } from 'react';
import { useQueryClient } from 'react-query';
import { useHistory } from 'react-router';
import { QueryFactory } from 'services/api';
import { CreateProductDto, ICreateProductDto } from 'services/api/api-client';

export const CreateProductPage: React.FC = () => {
  const i18n = useScopedTranslation('Page.Products.Create');
  const queryClient = useQueryClient();
  const history = useHistory();

  const form = useAdvancedForm<ICreateProductDto>(
    useCallback(async (data) => {
      const response = await QueryFactory.ProductQuery.Client.create(
        new CreateProductDto(data),
      );
      await queryClient.invalidateQueries(
        QueryFactory.ProductQuery.searchQueryKey(),
      );
      history.push(Routes.Authorized.Products);
    }, []),
  );
  return (
    <Loading loading={form.formState.isSubmitting}>
      <AppLink
        color={ButtonColor.Primary}
        onClick={() => {
          history.push(Routes.Authorized.Products);
        }}
      >
        Back
      </AppLink>
      <Grid container justify={'center'}>
        <Grid item xs={12} md={6} lg={4}>
          <form onSubmit={form.handleSubmitDefault}>
            <Field title={i18n.t('title')}>
              <Input
                {...form.register('title', { ...requiredRule() })}
                errorText={form.formState.errors.title?.message}
              />
            </Field>
            <FormError>{form.overallError}</FormError>
            <Button type={'submit'} title={i18n.t('create_button')} />
          </form>
        </Grid>
      </Grid>
    </Loading>
  );
};
