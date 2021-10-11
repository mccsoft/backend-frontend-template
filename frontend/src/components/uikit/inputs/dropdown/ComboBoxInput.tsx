import {
  ComboBox,
  VirtualizedComboBox,
  IDropdownOption,
  IComboBoxStyles,
  ICalloutProps,
  IComboBoxOption,
  IComboBox,
} from '@fluentui/react';
import * as React from 'react';
import { CSSProperties, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import clsx from 'clsx';
import arrowDownIcon from 'assets/icons/arrow-down.svg';

const styles = require('./ComboBoxInput.module.scss');

type Props<D extends unknown = unknown> = {
  options: D[];
  labelFunction?: (item: D) => string;
  idFunction?: (item: D) => string;
  label?: string;
  value?: D | null;
  rootClassName?: string;
  disabled?: boolean;
  emptyLabel?: string;
  error?: boolean;
  errorText?: string;
  variant?: 'normal' | 'formInput';
  calloutMaxHeight?: number;
  noPlaceholder?: boolean;
  onValueChanged: (value: string | null, option: D | null) => void;
  style?: CSSProperties;
  virtualized?: boolean;
};

export function ComboBoxInput<D extends unknown = unknown>(props: Props<D>) {
  const {
    rootClassName,
    options,
    disabled,
    onValueChanged,
    label,
    value,
    emptyLabel,
    error,
    errorText,
    variant = 'normal',
    calloutMaxHeight,
    noPlaceholder,
    style,
  } = props;
  const labelFunction =
    props.labelFunction ??
    useCallback(
      (item: D) => (item as { toString: () => string }).toString(),
      [],
    );

  const i18next = useTranslation();

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

  const isFormInput = variant === 'formInput';
  const dropdownStyles: Partial<IComboBoxStyles> = useMemo(
    () => ({
      container: styles.comboboxInputContainer,
      root: clsx(
        styles.combobox,
        isFormInput ? styles.comboboxFormInput : undefined,
        disabled ? styles.comboboxDisabled : undefined,
        error ? styles.comboboxError : undefined,
      ),
      input: clsx(styles.input, styles.value),
      callout: styles.comboboxCallout,
    }),
    [styles, isFormInput, disabled, error],
  );

  const optionList = useMemo(
    () =>
      options.map(
        (option) =>
          ({
            key: getValueForOption(option) ?? '',
            text: getLabelForOption(option),
            data: option,
          } as IComboBoxOption),
      ),
    [options],
  );

  const onChange = useCallback(
    (e: React.FormEvent<IComboBox>, selectedOption?: IDropdownOption) => {
      onValueChanged((e.target as any).value, selectedOption?.data);
    },
    [options, onValueChanged, value],
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

  const knownKey = useMemo(
    () => options.find((option) => option === value),
    [options, value],
  );

  const caretDownButtonStyles = useMemo(
    () => ({
      root: styles.expandButton,
      iconChecked: styles.expanded,
      icon: styles.expandIconContainer,
    }),
    [styles],
  );

  const comboBoxOptionStyles = useMemo(
    () => ({
      root: clsx(
        styles.comboboxItem,
        isFormInput ? styles.comboboxFormItem : undefined,
      ),
      optionText: styles.value,
    }),
    [styles, isFormInput],
  );

  const iconButtonProps = useMemo(
    () => ({
      iconProps: {
        imageProps: {
          src: arrowDownIcon,
        },
      },
    }),
    [arrowDownIcon],
  );
  const Component = props.virtualized ? VirtualizedComboBox : ComboBox;
  return (
    <div className={clsx(styles.rootContainer, rootClassName)} style={style}>
      <Component
        autoComplete={'on'}
        allowFreeform={true}
        options={optionList}
        placeholder={placeholder}
        onChange={onChange}
        text={label || ''}
        selectedKey={knownKey ? getValueForOption(value) : undefined}
        styles={dropdownStyles}
        caretDownButtonStyles={caretDownButtonStyles}
        useComboBoxAsMenuWidth={true}
        comboBoxOptionStyles={comboBoxOptionStyles}
        iconButtonProps={iconButtonProps}
        calloutProps={calloutProps}
      />
      {error && !!errorText && (
        <div className={styles.errorText}>{errorText}</div>
      )}
    </div>
  );
}
