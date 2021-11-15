import { useAdvancedForm } from 'helpers/form/useAdvancedForm';
import React, { useCallback, useMemo } from 'react';
import { Field } from 'components/uikit/Field';
import { HookFormDropDownInput } from 'components/uikit/inputs/dropdown/HookFormDropDownInput';
import { HookFormMultiSelectDropDownInput } from 'components/uikit/inputs/dropdown/HookFormMultiSelectDropDownInput';
import { Button, ButtonColor } from 'components/uikit/buttons/Button';
import { HookFormDatePicker } from 'components/uikit/inputs/date-time/HookFormDatePicker';
import { Input } from 'components/uikit/inputs/Input';
import { HookFormTimePicker } from 'components/uikit/inputs/date-time/HookFormTimePicker';
import { requiredRule } from 'helpers/form/react-hook-form-helper';
import { Links } from 'application/constants/links';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ProductType } from 'services/api/api-client';
import { useScopedTranslation } from 'application/localization/useScopedTranslation';

const styles = require('./UiKitPage.module.scss');

type UiKitForm = {
  dropDown: ProductType;
  multiSelectDropDown: ProductType[];
  timeInMilliseconds: number;
  date: Date;
  input: string;
};

export const UiKitPage: React.FC = () => {
  const i18n = useScopedTranslation('Page.uikit');
  const form = useAdvancedForm<UiKitForm>(
    useCallback(async (data) => {
      console.log(data);
      alert(JSON.stringify(data));
    }, []),
  );
  const options = useMemo(() => {
    return [ProductType.Auto, ProductType.Electronic, ProductType.Other];
  }, []);

  return (
    <div>
      <AppLink color={ButtonColor.Primary} to={Links.Authorized.Products}>
        Back
      </AppLink>
      <form onSubmit={form.handleSubmitDefault} className={styles.main}>
        <Field title={i18n.t('input')}>
          <Input
            {...form.register('input', requiredRule())}
            errorText={form.formState.errors.input?.message}
          />
        </Field>
        <Field title={i18n.t('dropdown')}>
          <HookFormDropDownInput
            options={options}
            name={'dropDown'}
            control={form.control}
          />
        </Field>
        <Field title={i18n.t('multi_select_dropdown')}>
          <HookFormMultiSelectDropDownInput
            options={options}
            name={'multiSelectDropDown'}
            control={form.control}
          />
        </Field>

        <Field title={i18n.t('date')}>
          <HookFormDatePicker name={'date'} control={form.control} />
        </Field>
        <Field title={i18n.t('time')}>
          <HookFormTimePicker
            name={'timeInMilliseconds'}
            control={form.control}
          />
        </Field>
        <Button type={'submit'} title={i18n.t('submit_button_title')} />
      </form>
    </div>
  );
};
