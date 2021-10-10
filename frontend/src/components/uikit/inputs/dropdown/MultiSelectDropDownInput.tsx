import {
  Dropdown,
  IDropdownOption,
  IDropdownStyles,
  IRenderFunction,
  ISelectableOption,
} from '@fluentui/react';
import * as React from 'react';
import { useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import clsx from 'clsx';
import { CheckBox } from '../../CheckBox';

const styles = require('./DropDownInput.module.scss');
const arrowDownIcon = require('assets/icons/arrow-down.svg');

export type MultiSelectDropDownInputProps<D extends unknown = unknown> = {
  options: D[];
  labelFunction?: (item: D) => string;
  idFunction?: (item: D) => string;
  values: Array<D> | null;
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

export function MultiSelectDropDownInput<D extends unknown = unknown>(
  props: MultiSelectDropDownInputProps<D>,
) {
  const {
    rootClassName,
    options,
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

  const labelFunction =
    props.labelFunction ??
    useCallback(
      (item: D) => (item as { toString: () => string }).toString(),
      [],
    );

  const i18next = useTranslation();
  const [expanded, setExpanded] = useState(false);

  const getLabelForOption = useCallback(
    (option: D | null): string =>
      option === null
        ? emptyLabel || i18next.t('uikit.inputs.nothing_selected')
        : labelFunction
        ? labelFunction(option)
        : (option as { toString: () => string }).toString(),
    [emptyLabel, labelFunction],
  );
  const getValueForOption: (option: D | null | undefined) => string | null =
    useCallback(
      (option: D | null | undefined) =>
        option === null || option === undefined
          ? null
          : props.idFunction
          ? props.idFunction(option)
          : getLabelForOption(option),
      [getLabelForOption, props.idFunction],
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
        key: getValueForOption(option) ?? '',
        text: getLabelForOption(option),
        data: option,
        optionObject: option,
        selected: !!values?.find((value) => value === option),
      })),
    [options, values],
  );

  const onChange = useCallback(
    (selectedItem: IDropdownOption) => {
      const newSelectedOptionsValues = !selectedItem.selected
        ? [selectedItem.key, ...(values || [])]
        : (values || []).filter(
            (value) => (value as any as string | number) !== selectedItem.key,
          );

      const newSelectedOptions = newSelectedOptionsValues
        .map((selectedValue) =>
          options.find((option) => option === selectedValue),
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

  const renderItem: IRenderFunction<ISelectableOption> = useCallback(
    (item: ISelectableOption | undefined) =>
      item ? (
        <div
          key={item.key}
          className={styles.dropdownItem}
          onClick={() => onChange(item)}
        >
          <CheckBox
            onChange={() => onChange(item)}
            checked={item.selected}
            title={item.text}
          />
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
        selectedKeys={values as any as string[] | number[]}
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
