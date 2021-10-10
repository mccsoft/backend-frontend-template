import {
  ComboBox,
  VirtualizedComboBox,
  IDropdownOption,
  IComboBoxStyles,
  ICalloutProps,
  IComboBox,
} from '@fluentui/react';
import * as React from 'react';
import { CSSProperties, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import clsx from 'clsx';
import { PickFieldsWithType } from 'components/uikit/type-utils';

const styles = require('components/uikit/inputs/dropdown/ComboBoxInput.module.css');
const arrowDownIcon = require('assets/icons/arrow-down.svg');

type Props<D, V extends keyof PickFieldsWithType<D, string | number | null>> = {
  options: D[];
  valueField: V;
  labelField: keyof PickFieldsWithType<D, string>;
  label?: string;
  value?: D[V] | null;
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

export function ComboBoxInput<
  D,
  V extends keyof PickFieldsWithType<D, string | number | null>,
>(props: Props<D, V>) {
  const {
    rootClassName,
    options,
    valueField,
    labelField,
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

  const i18next = useTranslation();

  const getLabelForOption = useCallback(
    (option: D | null) =>
      option === null
        ? emptyLabel || i18next.t('uikit.inputs.nothing_selected')
        : option[labelField],
    [emptyLabel],
  );

  const getValueForOption = useCallback(
    (option: D) => option && (option[valueField] as any as string | number),
    [],
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
      options.map((option) => ({
        key: getValueForOption(option),
        text: getLabelForOption(option),
      })),
    [options, value],
  );

  const onChange = useCallback(
    (e: React.FormEvent<IComboBox>, selectedOption?: IDropdownOption) => {
      let foundOption: D | null = null;
      if (selectedOption?.key !== value) {
        foundOption =
          (selectedOption &&
            options.find(
              (option) =>
                (option[valueField] as any as string | number) ===
                selectedOption.key,
            )) ||
          null;
      }
      onValueChanged((e.target as any).value, foundOption);
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
    () =>
      options.find((option) => getValueForOption(option) === (value as any)),
    [options, value, getValueForOption],
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
        selectedKey={knownKey ? value : undefined}
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
