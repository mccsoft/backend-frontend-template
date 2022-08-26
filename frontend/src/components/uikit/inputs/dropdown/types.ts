import { AutocompleteProps } from '@mui/material/Autocomplete/Autocomplete';
import { AutocompleteFreeSoloValueMapping } from '@mui/base/AutocompleteUnstyled/useAutocomplete';
import * as React from 'react';

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
   * 'normal' - input will have minimal width
   * 'formInput' - input will have the standard width (as all form elements)
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
};

export type PropertyAccessor<T> = ((option: T) => string | number) | keyof T;
