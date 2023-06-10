import { Links } from 'application/constants/links';
import { useScopedTranslation } from 'application/localization/useScopedTranslation';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { Button, ButtonColor } from 'components/uikit/buttons/Button';
import { Field } from 'components/uikit/Field';
import { FormError } from 'components/uikit/FormError';
import { Input } from 'components/uikit/inputs/Input';
import { Loading } from 'components/uikit/suspense/Loading';
import { useAdvancedForm } from 'helpers/form/useAdvancedForm';
import { requiredRule } from 'helpers/form/react-hook-form-helper';
import React from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { QueryFactory } from 'services/api';
import { PatchProductDto, ProductType } from 'services/api/api-client';
import { HookFormDropDownInput } from 'components/uikit/inputs/dropdown/HookFormDropDownInput';
import { useNavigate } from 'react-router';
import { HookFormDatePicker } from '../../../../components/uikit/inputs/date-time/HookFormDatePicker';
import { Grid } from '@mui/material';

export const EditProductPage: React.FC = () => {
  const { id: productId } = Links.Authorized.EditProduct.useParams();
  const i18n = useScopedTranslation('Page.Products.Edit');
  const productQuery = QueryFactory.ProductQuery.useGetQuery(productId);
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const form = useAdvancedForm<PatchProductDto>(
    async (data) => {
      await QueryFactory.ProductQuery.Client.patch(productId, data);
      await queryClient.invalidateQueries(
        QueryFactory.ProductQuery.searchQueryKey(),
      );
      // await queryClient.invalidateQueries(
      //   QueryFactory.ProductQuery.getQueryKey(productId),
      // );
      navigate(Links.Authorized.Products.link());
    },
    {
      defaultValues: productQuery.data,
    },
  );

  return (
    <Loading
      loading={form.formState.isSubmitting || productQuery.isInitialLoading}
    >
      <AppLink
        color={ButtonColor.Primary}
        to={Links.Authorized.Products.link()}
      >
        Back
      </AppLink>
      <Grid
        container
        justifyContent={'center'}
        data-test-id="create-product-page"
      >
        <Grid item xs={12} md={6} lg={4}>
          <form onSubmit={form.handleSubmitDefault}>
            <Field title={i18n.t('title')}>
              <Input
                {...form.register('title', { ...requiredRule() })}
                errorText={form.formState.errors.title?.message}
              />
            </Field>
            <Field title={i18n.t('product_type')}>
              <HookFormDropDownInput
                options={[
                  ProductType.Auto,
                  ProductType.Electronic,
                  ProductType.Other,
                ]}
                name={'productType'}
                control={form.control}
                errorText={form.formState.errors.productType?.message}
              />
            </Field>
            <Field title={i18n.t('lastStockUpdatedAt')}>
              <HookFormDatePicker
                name={'lastStockUpdatedAt'}
                control={form.control}
                errorText={form.formState.errors.lastStockUpdatedAt?.message}
              />
            </Field>
            <FormError>{form.overallError}</FormError>
            <Button type={'submit'} title={i18n.t('save_button')} />
          </form>
        </Grid>
      </Grid>
    </Loading>
  );
};
