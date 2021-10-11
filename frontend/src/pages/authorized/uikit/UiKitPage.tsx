import { useAdvancedForm } from '../../../helpers/form/advanced-form';
import React, { useCallback, useMemo } from 'react';
import { Field } from 'components/uikit/Field';
import { useTranslation } from 'react-i18next';
import { HookFormDropDownInput } from '../../../components/uikit/inputs/dropdown/HookFormDropDownInput';
import { ProductType } from '../../../services/api/api-client';
import { HookFormMultiSelectDropDownInput } from '../../../components/uikit/inputs/dropdown/HookFormMultiSelectDropDownInput';
import { Button } from '../../../components/uikit/buttons/Button';
import { HookFormDatePicker } from '../../../components/uikit/inputs/date-time/HookFormDatePicker';
import { Input } from '../../../components/uikit/inputs/Input';
import { HookFormTimePicker } from '../../../components/uikit/inputs/date-time/HookFormTimePicker';
import { requiredRule } from '../../../helpers/form/react-hook-form-helper';

const styles = require('./UiKitPage.module.scss');

type UiKitForm = {
  dropDown: ProductType;
  multiSelectDropDown: ProductType[];
  timeInMilliseconds: number;
  date: Date;
  input: string;
};

export const UiKitPage: React.FC = () => {
  const i18n = useTranslation();
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
      <form onSubmit={form.handleSubmitDefault} className={styles.main}>
        <Field title={i18n.t('Page.uikit.input')}>
          <Input
            {...form.register('input', requiredRule())}
            errorText={form.formState.errors.input?.message}
          />
        </Field>
        <Field title={i18n.t('Page.uikit.dropdown')}>
          <HookFormDropDownInput
            options={options}
            name={'dropDown'}
            control={form.control}
          />
        </Field>
        <Field title={i18n.t('Page.uikit.multi_select_dropdown')}>
          <HookFormMultiSelectDropDownInput
            options={options}
            name={'multiSelectDropDown'}
            control={form.control}
          />
        </Field>

        <Field title={i18n.t('Page.uikit.date')}>
          <HookFormDatePicker name={'date'} control={form.control} />
        </Field>
        <Field title={i18n.t('Page.uikit.time')}>
          <HookFormTimePicker
            name={'timeInMilliseconds'}
            control={form.control}
          />
        </Field>
        <Button
          type={'submit'}
          title={i18n.t('Page.uikit.submit_button_title')}
        />
      </form>
    </div>
  );
};
