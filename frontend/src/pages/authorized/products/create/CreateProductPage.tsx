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
import { CreateProductDto, ProductType } from 'services/api/api-client';
import { HookFormDropDownInput } from 'components/uikit/inputs/dropdown/HookFormDropDownInput';
import { Location, useNavigate } from 'react-router';
import { HookFormDatePicker } from 'components/uikit/inputs/date-time/HookFormDatePicker';
import { Grid } from '@mui/material';
import { useModal } from 'components/uikit/modal/useModal';
import { useBlockNavigation } from 'helpers/router/useBlockNavigation';

export const CreateProductPage: React.FC = () => {
  const i18n = useScopedTranslation('Page.Products.Create');
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const form = useAdvancedForm<CreateProductDto>(async (data) => {
    await QueryFactory.ProductQuery.Client.create(data);
    await queryClient.invalidateQueries({
      queryKey: QueryFactory.ProductQuery.searchQueryKey(),
    });

    // we need to `.reset` to prevent blocking the navigation (otherwise the form would think it's dirty)
    form.reset();
    void navigate(Links.Authorized.Products.link());
  });
  const modals = useModal();

  // we need to access `dirtyFields` during rendering, otherwise the value inside useBlockNavigation won't be updated
  // we need to use `.dirtyFields` instead of `.isDirty` because latter only works if we set `defaultValues`
  const dirtyFields = form.formState.dirtyFields;
  useBlockNavigation(async () => {
    if (Object.keys(dirtyFields).length === 0) return false;
    const confirmResult = await modals.showConfirm({
      title: i18n.t('unsaved_changes_prompt.title'),
      text: i18n.t('unsaved_changes_prompt.text'),
    });
    return !confirmResult;
  });
  return (
    <Loading loading={form.formState.isSubmitting}>
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
                required={true}
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
            <Button type={'submit'} title={i18n.t('create_button')} />
          </form>
        </Grid>
      </Grid>
    </Loading>
  );
};
