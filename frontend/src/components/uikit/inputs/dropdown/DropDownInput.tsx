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
import arrowDownIcon from 'assets/icons/arrow-down.svg';

import styles from './DropDownInput.module.scss';

export type DropDownInputProps<D> = {
  options: D[];
  customOptions?: Array<CustomOption>;
  disabledField?: (item: D) => boolean;
  labelFunction?: (item: D) => string;
  idFunction?: (item: D) => string;
  value?: D | null;
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
      required?: false | undefined;
      onSelectedOptionChanged: (newSelectedOption: D | null) => void;
    }
);

export interface CustomOption {
  key: number | string;
  label: string;
  onClick: () => void;
}

type DropDownOption = IDropdownOption &
  (
    | {
        type: 'standard';
      }
    | {
        type: 'custom';
        onClick: () => void;
      }
  );

export function DropDownInput<D>(props: DropDownInputProps<D>) {
  const {
    rootClassName,
    options,
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
        type: 'custom',
        key: customOption.key,
        text: customOption.label,
        onClick: customOption.onClick,
      }),
    );
    result.push(
      ...options.map(
        (option): DropDownOption => ({
          type: 'standard',
          key: getValueForOption(option) ?? '',
          text: getLabelForOption(option),
          disabled: props.disabledField ? props.disabledField(option) : false,
          data: option,
        }),
      ),
    );

    if (
      !required &&
      value !== null &&
      value !== undefined &&
      result.length > 0
    ) {
      result.unshift({
        type: 'standard',
        key: '-1',
        text: getLabelForOption(null),
      } as DropDownOption);
    }
    return result;
  }, [options, required, value, customOptions]);

  const onChange = useCallback(
    (_e: React.FormEvent<HTMLDivElement>, selectedOption?: IDropdownOption) => {
      const castedOption = selectedOption as DropDownOption | undefined;
      if (castedOption?.type === 'custom') {
        castedOption.onClick();
      } else if (selectedOption?.key !== value) {
        (onSelectedOptionChanged as (option: D | null) => void)(
          castedOption?.data,
        );
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
        selectedKey={getValueForOption(value)}
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
