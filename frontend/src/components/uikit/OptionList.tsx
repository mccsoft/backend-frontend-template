import clsx from 'clsx';
import React from 'react';
import { PickFieldsWithType } from './type-utils';

import styles from './OptionList.module.scss';

interface OptionListProps<D> {
  visible: boolean;
  options: D[];
  onClick: (option: D) => void;
  labelField: keyof PickFieldsWithType<D, string>;
  className?: string;
  elementRef?: React.RefObject<HTMLDivElement>;
  testId?: string;
}

export function OptionList<D>(props: OptionListProps<D>) {
  const { visible, labelField, options, onClick, className, elementRef } =
    props;

  if (!visible) {
    return null;
  }
  if (options.length === 0) {
    return (
      <div
        ref={elementRef}
        className={clsx(styles.optionListContainer, className)}
        data-test-id={props.testId}
      >
        <div className={styles.optionContainer}>
          <div className={styles.selectedValue}>{'No data'}</div>
        </div>
      </div>
    );
  }

  return (
    <div
      ref={elementRef}
      className={clsx(styles.optionListContainer, className)}
      data-test-id={props.testId}
    >
      {options.map((option) => (
        <div
          key={`${option[labelField]}`}
          className={styles.optionContainer}
          onClick={() => onClick(option)}
        >
          <div className={styles.selectedValue}>{option[labelField]}</div>
        </div>
      ))}
    </div>
  );
}
