import {
  Dropdown,
  ICalloutProps,
  IDropdownOption,
  IDropdownStyles,
} from '@fluentui/react';
import clsx from 'clsx';
import * as React from 'react';
import { CSSProperties, useCallback, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FieldsWithType } from '../../type-utils';

const styles = require('./DropDownInput.module.scss');
const arrowDownIcon = require('app/icons/arrow-down.svg');

export type DropDownInputProps<
  D extends Record<V, string | number>,
  V extends FieldsWithType<D, string | number>
> = {
  options: D[];
  customOptions?: Array<CustomOption>;
  valueField: V;
  labelField: FieldsWithType<D, string>;
  disabledField?: (item: D) => boolean;
  value?: D[V] | null;
  rootClassName?: string;
  disabled?: boolean;
  emptyLabel?: string;
  error?: boolean;
  errorText?: string;
  variant?: 'normal' | 'formInput';
  calloutMaxHeight?: number;
  noPlaceholder?: boolean;
  style?: CSSProperties;
  testId?: string;
} & (
  | {
      required: true;
      onSelectedOptionChanged: (newSelectedOption: D) => void;
    }
  | {
      required: false;
      onSelectedOptionChanged: (newSelectedOption: D | null) => void;
    }
);

export interface CustomOption {
  key: number | string;
  label: string;
  onClick: () => void;
}

type DropDownOption = StandardDropDownOption | CustomDropDownOption;

interface StandardDropDownOption extends IDropdownOption {
  __option_type: 'standard';
}

interface CustomDropDownOption extends IDropdownOption {
  __option_type: 'custom';
  onClick: () => void;
}

export function DropDownInput<
  D extends Record<V, string | number>,
  V extends FieldsWithType<D, string | number>
>(props: DropDownInputProps<D, V>) {
  const {
    rootClassName,
    options,
    valueField,
    labelField,
    disabled,
    onSelectedOptionChanged,
    value,
    required,
    emptyLabel,
    error,
    errorText,
    variant = 'normal',
    calloutMaxHeight,
    noPlaceholder,
    customOptions,
  } = props;

  const i18next = useTranslation();
  const [expanded, setExpanded] = useState(false);

  const getLabelForOption = useCallback(
    (option: D | null) =>
      option === null
        ? emptyLabel || i18next.t('uikit.inputs.nothing_selected')
        : labelField
        ? option[labelField]
        : option,
    [emptyLabel],
  );

  const getValueForOption = useCallback(
    (option: D) =>
      option &&
      ((valueField ? (option[valueField] as any) : option) as string | number),
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
      root: styles.dropdownInputContainer,
      dropdownItem: clsx(
        styles.dropdownItem,
        isFormInput ? styles.dropdownFormItem : undefined,
      ),
      dropdownItemDisabled: styles.dropdownItemDisabled,
      dropdownItemSelectedAndDisabled: styles.dropdownItemSelectedDisabled,
      dropdownOptionText: styles.optionValue,
      dropdownItemSelected: clsx(
        styles.dropdownItem,
        isFormInput ? styles.dropdownFormItem : undefined,
        styles.dropdownItemSelected,
      ),
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

  const optionList: Array<DropDownOption> = useMemo(() => {
    const result: DropDownOption[] = (customOptions || []).map(
      (customOption) => ({
        __option_type: 'custom',
        key: customOption.key,
        text: customOption.label,
        onClick: customOption.onClick,
      }),
    );
    const standardOptions: Array<StandardDropDownOption> = options.map(
      (option) => ({
        __option_type: 'standard',
        key: getValueForOption(option),
        text: getLabelForOption(option),
        disabled: props.disabledField ? props.disabledField(option) : false,
      }),
    );
    result.push(...standardOptions);
    if (
      !required &&
      value !== null &&
      value !== undefined &&
      result.length > 0
    ) {
      result.unshift({
        __option_type: 'standard',
        key: -1,
        text: getLabelForOption(null),
      });
    }
    return result;
  }, [options, required, value, customOptions]);

  const onChange = useCallback(
    (_e: React.FormEvent<HTMLDivElement>, selectedOption?: IDropdownOption) => {
      const castedOption = selectedOption as DropDownOption | undefined;
      if (castedOption?.__option_type === 'custom') {
        castedOption.onClick();
      } else if (selectedOption?.key !== value) {
        const foundOption =
          selectedOption &&
          options.find(
            (option) => getValueForOption(option) === selectedOption.key,
          );
        const newVar: D | null = foundOption || null;
        (onSelectedOptionChanged as (option: D | null) => void)(newVar);
      }
    },
    [options, onSelectedOptionChanged, value],
  );

  const calloutProps: ICalloutProps = useMemo(
    () => ({
      calloutMaxHeight,
    }),
    [],
  );

  const placeholder = useMemo(
    () => (!noPlaceholder ? getLabelForOption(null) : undefined),
    [noPlaceholder, getLabelForOption],
  );
  return (
    <div
      className={clsx(styles.rootContainer, rootClassName)}
      style={props.style}
    >
      <Dropdown
        data-test-id={props.testId}
        data-form-input={isFormInput}
        data-disabled={disabled}
        data-expanded={expanded}
        data-error={error}
        options={optionList}
        placeholder={placeholder}
        onChange={onChange}
        selectedKey={value}
        styles={dropdownStyles}
        onRenderCaretDown={caretDown}
        onClick={onDropdownClick}
        onDismiss={onOptionsDismiss}
        calloutProps={calloutProps}
      />
      {error && !!errorText && (
        <div className={styles.errorText}>{errorText}</div>
      )}
    </div>
  );
}
