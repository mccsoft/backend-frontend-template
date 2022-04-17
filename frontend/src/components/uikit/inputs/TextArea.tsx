import React, { Ref } from 'react';
import clsx from 'clsx';
import TextareaAutosize, {
  TextareaAutosizeProps,
} from 'react-textarea-autosize';

import styles from './TextArea.module.scss';

type Props = TextareaAutosizeProps & {
  hasFixedSize?: boolean;
  helperText?: string;
  errorText?: string;
};

export const TextArea = React.forwardRef(function TextArea(
  props: Props,
  ref: Ref<HTMLTextAreaElement>,
) {
  const { hasFixedSize, helperText, errorText, className, ...rest } = props;

  return (
    <div>
      <TextareaAutosize
        data-error={!!props.errorText}
        ref={ref}
        {...rest}
        className={clsx(styles.area, className)}
        maxRows={hasFixedSize ? 1 : undefined}
      />
      {!!(helperText || errorText) && (
        <div data-error={!!props.errorText} className={styles.helperText}>
          {helperText || errorText}
        </div>
      )}
    </div>
  );
});
