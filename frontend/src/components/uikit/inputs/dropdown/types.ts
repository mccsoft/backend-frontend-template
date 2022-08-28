import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteFreeSoloValueMapping } from '@mui/base/AutocompleteUnstyled/useAutocomplete';
import * as React from 'react';
import { CSSProperties } from 'react';

export interface CustomOption {
  label: string;
  onClick: () => void;
}

export type StyledAutocompleteProps<
  T,
  Multiple extends boolean | undefined = undefined,
  Required extends boolean | undefined = undefined,
  FreeSolo extends boolean | undefined = undefined,
> = Omit<
  AutocompleteProps<T, Multiple, Required, FreeSolo>,
  'disableClearable' | 'renderInput' | 'getOptionLabel'
> & {
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
   * if you are using custom
   */
  itemSize?: number;

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
};

export type PropertyAccessor<T> = ((option: T) => string | number) | keyof T;
