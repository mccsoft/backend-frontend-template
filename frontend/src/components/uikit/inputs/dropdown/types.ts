import type { Input } from '../Input';
import { AutocompleteFreeSoloValueMapping } from '@mui/base/useAutocomplete/useAutocomplete';
import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import * as React from 'react';
import { CSSProperties } from 'react';

export interface CustomOption {
  label: string;
  onClick: () => void;
}
export type StyledAutocompleteControl = {
  blur: () => void;
};
export type StyledAutocompleteProps<
  T,
  Multiple extends boolean | undefined = undefined,
  Required extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined,
> = Omit<
  AutocompleteProps<T, Multiple, Required, FreeSolo>,
  'disableClearable' | 'renderInput' | 'getOptionLabel'
> & {
  placeholder?: string;
  actions?: React.RefObject<StyledAutocompleteControl>;
  /*
   * Defines the text for the option.
   * Could be either a function (as in default Autocomplete) or a property name.
   */
  getOptionLabel?:
    | ((
        option: T | AutocompleteFreeSoloValueMapping<FreeSolo>,
      ) => string | number)
    | keyof T
    | undefined;
  /*
   * Could be specified instead of `isOptionEqualToValue`
   */
  idFunction?: PropertyAccessor<T>;
  rootClassName?: string;
  required?: Required;
  testId?: string;
  errorText?: string;
  renderInput?: React.ComponentType<
    React.ComponentProps<typeof Input> & { selectedValue?: T }
  >;

  /*
   * Makes it possible to type right into the input to filter results
   */
  enableSearch?: boolean;
  /*
   * 'normal' - input will have minimal width and height
   * 'formInput' - input will have the standard width and height (as all form elements)
   */
  variant?: 'normal' | 'formInput';
  /*
   * You could add more options to Autocomplete beside the standard `menuItems`.
   * It's useful when for example you have a dropdown with categories and want to add a `Add New Category' item
   * Custom options are added to the top of the list.
   */
  customOptions?: CustomOption[];
  /*
   * Header of drop-down popup
   */
  popupHeader?: React.ReactNode;
  /*
   * Footer of drop-down popup
   */
  popupFooter?: React.ReactNode;
  /*
   * Component that renders after each option (on the same row)
   */
  postfixRenderer?: (option: T) => React.ReactElement<unknown>;

  /*
   * If true, the size of an input is determined by the currently selected element
   */
  autosizeInputWidth?: boolean;

  /*
   * If true, uses react-window to render elements in drop-down list
   * True by default.
   */
  useVirtualization?: boolean;

  /*
   * You need to specify this, if `useVirtualization` is true and default heights of `normal` or `form-input` (32px and 40px) are not ok
   */
  itemSize?: number;

  /*
   * Could be used to specify popupWidth.
   * Makes sense to use when `useVirtualization` is true, otherwise popupWidth will be equal to Input width
   * (if `useVirtualization` is false, autosize is enabled by default)
   */
  popupWidth?: number | 'autosize';

  /*
   * Specifies maximum width of the popup.
   * Default to '450px'
   */
  maxPopupWidth?: number;

  /*
   * Makes sense to use when `useVirtualization` is true and `popupWidth` is auto.
   * Specifies the width that is added to automatically calculated item width.
   * Could be used for paddings and/or postfix.
   * Defaults to '40px'
   */
  additionalWidth?: CSSProperties['width'];

  /*
   * If true renders <SearchInput> instead of <Input />
   */
  isSearch?: boolean | undefined;

  /*
   * Overrides input's endAdornment
   */
  endAdornment?: React.ReactNode;

  /*
   * Overrides input's endAdornment's style class name
   */
  endAdornmentClassname?: string;

  /*
   * Input's ref
   */
  inputRef?: React.RefObject<HTMLInputElement>;
};

export type DropDownInputProps<
  T,
  Required extends boolean | undefined = undefined,
  UseIdAsValue extends boolean | undefined = undefined,
> = Omit<
  StyledAutocompleteProps<T, false, Required, false>,
  'onChange' | 'value'
> & {
  onValueChanged: Required extends true
    ? (newSelectedOption: T, event: { cancel: () => void }) => unknown
    : (newSelectedOption: T | null, event: { cancel: () => void }) => unknown;

  /*
   * If true, we assume that `value` field contains the result of `idFunction` of the option.
   * HookFormDropDownInput has this enabled by default.
   */
  useIdFunctionAsValue?: UseIdAsValue;

  value: UseIdAsValue extends true
    ? string | number | undefined | null
    : StyledAutocompleteProps<T, false, Required, false>['value'];

  /*
   * By default after calling `onValueChanged` we check if the value in `props` was set to a changed value or not.
   * If it's not set, we reset the value in DropDown to the value it had before.
   *
   * If you do not want this feature, you could set this flag to true.
   */
  disableAutomaticResetAfterOnValueChanged?: boolean;
};

export type PropertyAccessor<T> = ((option: T) => string | number) | keyof T;
