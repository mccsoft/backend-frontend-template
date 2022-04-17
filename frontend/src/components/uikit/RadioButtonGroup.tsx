import clsx from 'clsx';
import * as React from 'react';
import { ChangeEventHandler, useCallback, useMemo } from 'react';
import { PickFieldsWithType } from './type-utils';
import { RadioButton } from './RadioButton';
import { Input } from './inputs/Input';
import { createId } from './type-utils';

import styles from './RadioButtonGroup.module.scss';

type Props<D, V extends keyof PickFieldsWithType<D, string | number | null>> = {
  options: D[];
  valueField: V;
  labelField: keyof PickFieldsWithType<D, string>;
  disabledField?: (item: D) => boolean;
  value?: D[V] | null;
  rootClassName?: string;
  disabled?: boolean;
  error?: boolean;
  errorText?: string;
  editableFieldValue?: string;
  onEditableFieldChange?: ChangeEventHandler<HTMLInputElement>;
  onSelectedOptionChanged: (newSelectedOption: D) => void;
  radioButtonClassName?: string;
};

type RadioButtonGroupOption = {
  key: string | number;
  text: string;
  disabled: boolean;
};

export function RadioButtonGroup<
  D,
  V extends keyof PickFieldsWithType<D, string | number | null>,
>(props: Props<D, V>) {
  const {
    rootClassName,
    options,
    valueField,
    labelField,
    disabled,
    onSelectedOptionChanged,
    value,
    error,
    errorText,
    editableFieldValue,
    onEditableFieldChange,
    radioButtonClassName,
  } = props;

  const getLabelForOption = useCallback(
    (option: D) => option[labelField] as any as string,
    [],
  );

  const getValueForOption = useCallback(
    (option: D) => option && (option[valueField] as any as string | number),
    [],
  );

  const optionList: Array<RadioButtonGroupOption> = useMemo(() => {
    return options.map((option) => ({
      key: getValueForOption(option),
      text: getLabelForOption(option),
      disabled: props.disabledField ? props.disabledField(option) : false,
    }));
  }, [options, value]);

  const onChange = useCallback(
    (
      _e: React.FormEvent<HTMLDivElement>,
      selectedOption?: RadioButtonGroupOption,
    ) => {
      if (selectedOption?.key !== value) {
        const foundOption =
          selectedOption &&
          options.find(
            (option) =>
              (option[valueField] as any as string | number) ===
              selectedOption.key,
          );
        const newVar: D | null = foundOption || null;
        (onSelectedOptionChanged as (option: D | null) => void)(newVar);
      }
    },
    [options, onSelectedOptionChanged, value],
  );

  return (
    <div className={clsx(styles.rootContainer, rootClassName)}>
      {optionList.map((option) => (
        <div key={option.key} className={styles.fieldContainer}>
          <RadioButton
            title={option.key != editableFieldValue ? option.text : ''}
            key={option.key}
            checked={(value as any) === option.key}
            value={option.key}
            name={createId() + option.text}
            disabled={disabled}
            onChange={(e) => {
              onChange(e, option);
            }}
            className={clsx(styles.radioButton, radioButtonClassName)}
          />
          {option.key === editableFieldValue && (
            <Input value={option.text} onChange={onEditableFieldChange} />
          )}
        </div>
      ))}
      {error && !!errorText && (
        <div className={styles.errorText}>{errorText}</div>
      )}
    </div>
  );
}
