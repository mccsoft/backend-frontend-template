import * as React from 'react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { mergeRefs } from 'react-merge-refs';
import {
  convertPropertyAccessorToFunction,
  StyledAutocomplete,
} from './StyledAutocomplete';
import type {
  DropDownInputProps,
  StyledAutocompleteControl,
  StyledAutocompleteProps,
} from './types';

export function DropDownInput<
  T,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
>(props: DropDownInputProps<T, Required, UseIdAsValue>) {
  const { onValueChanged, ...rest } = props;
  const onValueChanged_ValueRef = useRef(props.value);

  const idFunction = props.idFunction
    ? convertPropertyAccessorToFunction<T, false, Required, false>(
        props.idFunction,
      )
    : (v: any) => v;
  let value = props.value;
  if (props.useIdFunctionAsValue && idFunction && value) {
    value =
      (props.options.find((x) => idFunction(x as any) == value) as any) ??
      value;
  }
  const onChange: StyledAutocompleteProps<
    T,
    false,
    Required,
    false
  >['onChange'] = useMemo(
    () => (e, item) => {
      const result = onValueChanged(item as any, {
        cancel: () => {
          actionsRef.current?.blur();
        },
      });
      if (!props.disableAutomaticResetAfterOnValueChanged) {
        Promise.resolve(result)
          .then(() => {
            onValueChanged_ValueRef.current = item as any;
            setResetValueIfUnchangedAfterCallingOnValueChanged(
              new Date().getTime(),
            );
          })
          .catch((ex: any) => console.error(ex));
      }
    },
    [onValueChanged],
  );

  const [
    resetValueIfUnchangedAfterCallingOnValueChanged,
    setResetValueIfUnchangedAfterCallingOnValueChanged,
  ] = useState(0);
  useEffect(
    function resetValueIfUnchangedAfterCallingOnValueChanged() {
      /*
       * Use case: user selected something in DropDown, we've shown a confirmation like 'Are you sure you want to change the value?' and user said No.
       * In this case we need to revert the value in DropDown.
       * But technically `value` in the state wasn't yet changed, so there's nothing to revert.
       *
       * To overcome this, we verify that if `props.value` wasn't changed after calling `onValueChanged`, we reset the value in DropDown.
       */
      if (props.value != idFunction(onValueChanged_ValueRef.current as any)) {
        actionsRef.current?.blur();
      }
    },
    [resetValueIfUnchangedAfterCallingOnValueChanged],
  );

  const actionsRef = useRef<StyledAutocompleteControl>(null!);
  return (
    <StyledAutocomplete<T, false, Required, false>
      {...rest}
      multiple={false}
      value={value as any}
      onChange={onChange}
      actions={actionsRef}
    />
  );
}
