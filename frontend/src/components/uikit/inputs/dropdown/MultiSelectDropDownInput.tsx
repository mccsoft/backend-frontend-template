import { Dropdown, IDropdownOption, IDropdownStyles } from '@fluentui/react';
import { ISelectableOption } from 'office-ui-fabric-react/src/utilities/selectableOption/SelectableOption.types';
import * as React from 'react';
import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import clsx from 'clsx';
import { PickFieldsWithType } from '../../type-utils';
import { CheckBox } from '../../CheckBox';

const styles = require('./DropDownInput.module.scss');
const arrowDownIcon = require('app/icons/arrow-down.svg');

export type MultiSelectDropDownInputProps<
  D,
  V extends keyof PickFieldsWithType<D, string | number>
> = {
  options: D[];
  valueField: V;
  labelField: keyof PickFieldsWithType<D, string>;
  values: Array<D[V]> | null;
  onSelectedOptionsChanged: (newValues: D[]) => void;
  rootClassName?: string;
  disabled?: boolean;
  allSelectedLabel?: string;
  emptyLabel?: string;
  error?: boolean;
  errorText?: string;
  variant?: 'normal' | 'formInput';
  customPostfixRenderer?: (option: D) => React.ReactElement<unknown>;
};

type DropdownItemWithOption<D> = {
  optionObject: D;
} & IDropdownOption;

export function MultiSelectDropDownInput<
  D,
  V extends keyof PickFieldsWithType<D, string | number>
>(props: MultiSelectDropDownInputProps<D, V>) {
  const {
    rootClassName,
    options,
    valueField,
    labelField,
    disabled,
    values,
    onSelectedOptionsChanged,
    emptyLabel,
    allSelectedLabel,
    error,
    errorText,
    variant = 'normal',
    customPostfixRenderer,
  } = props;

  const i18next = useTranslation();
  const [expanded, setExpanded] = useState(false);

  const getLabelForOption = useCallback(
    (option: D | undefined | null) =>
      option === undefined || option === null
        ? emptyLabel || i18next.t('uikit.inputs.nothing_selected')
        : option[labelField],
    [emptyLabel],
  );

  const getValueForOption: (option: D | null) => string | number = useCallback(
    (option) =>
      ((option && option[valueField]) as string | number | null) || '',
    [],
  );

  const caretDown = useCallback(
    () => (
      <img
        src={arrowDownIcon}
        className={styles.expandIcon}
        data-expanded={expanded}
        data-disabled={disabled}
      />
    ),
    [expanded, disabled],
  );
  const isFormInput = variant === 'formInput';
  const dropdownStyles: Partial<IDropdownStyles> = useMemo(
    () => ({
      dropdown: styles.dropdown,
      title: styles.optionValue,
      label: styles.optionValue,
      root: styles.dropdownInputContainer,
      caretDownWrapper: styles.expandIconWrapper,
      callout: styles.dropdownCallout,
    }),
    [styles, isFormInput],
  );
  const onDropdownClick = useCallback(
    () => setExpanded((currentState) => !currentState),
    [],
  );
  const onOptionsDismiss = useCallback(() => setExpanded(false), []);

  const optionList: Array<DropdownItemWithOption<D>> = useMemo(
    () =>
      options.map((option) => ({
        key: getValueForOption(option) as string | number,
        text: getLabelForOption(option),
        optionObject: option,
        selected: !!values?.find((value) => value === option[valueField]),
      })),
    [options, values],
  );

  const onChange = useCallback(
    (selectedItem: IDropdownOption) => {
      const newSelectedOptionsValues = !selectedItem.selected
        ? [selectedItem.key, ...(values || [])]
        : (values || []).filter(
            (value) => ((value as any) as string | number) !== selectedItem.key,
          );

      const newSelectedOptions = newSelectedOptionsValues
        .map((selectedValue) =>
          options.find((option) => option[valueField] === selectedValue),
        )
        .filter((option) => option) as D[];
      onSelectedOptionsChanged(newSelectedOptions);
    },
    [options, onSelectedOptionsChanged, values],
  );

  const getTitle = useCallback(
    (selectedOptions: unknown[] | undefined) =>
      selectedOptions === undefined || selectedOptions.length === 0
        ? emptyLabel || i18next.t('uikit.inputs.nothing_selected')
        : selectedOptions.length === options.length
        ? allSelectedLabel || i18next.t('uikit.inputs.all_selected')
        : i18next.t('uikit.inputs.some_selected', {
            count: selectedOptions.length,
          }),
    [emptyLabel, allSelectedLabel, options],
  );

  const renderTitle = useCallback(
    (
      items: unknown[] | undefined,
      defaultRender?: (
        items: IDropdownOption[] | undefined,
      ) => JSX.Element | null,
    ) => {
      const text = getTitle(items);
      return defaultRender?.([{ key: text, text }]) || null;
    },
    [getTitle],
  );

  const renderItem = useCallback(
    (item: ISelectableOption | undefined) =>
      item ? (
        <div
          key={item.key}
          className={styles.dropdownItem}
          onClick={() => onChange(item)}
        >
          <CheckBox checked={item.selected} title={item.text} />
          {customPostfixRenderer?.(
            (item as DropdownItemWithOption<D>).optionObject,
          )}
        </div>
      ) : null,
    [onChange, values],
  );

  return (
    <div className={clsx(styles.rootContainer, rootClassName)}>
      <Dropdown
        multiSelect
        data-form-input={isFormInput}
        data-disabled={disabled}
        data-expanded={expanded}
        data-error={error}
        options={optionList}
        placeholder={getLabelForOption(null)}
        selectedKeys={(values as any) as string[] | number[]}
        styles={dropdownStyles}
        onRenderCaretDown={caretDown}
        onClick={onDropdownClick}
        onDismiss={onOptionsDismiss}
        onRenderTitle={renderTitle}
        onRenderItem={renderItem}
      />
      {error && !!errorText && (
        <div className={styles.errorText}>{errorText}</div>
      )}
    </div>
  );
}
