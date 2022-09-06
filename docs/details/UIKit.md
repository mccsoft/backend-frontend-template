[TOC]

# UI Kit
Every project has his own UI Kit. UI Kit includes standard controls that are used across the app (e.g. Inputs, Checkboxes, DropDowns, Alerts, Tooltips, Popups, etc.).

Having a UI Kit ensures that all controls throughout the App look the same. Also, if Designer decides to change the look&feel of these controls, it allows to change it in a single place (not in every place in the app).

You should always use these standard controls (not default html `<input>` or `<select>`), whenever you need one of these elements.
Moreover, if you plan to add some new control, that is likely to be used in several places across the app, do not hesitate to create it in `ui-kit` folder.

When you set up your app from template, you will need to adjust the UIKit (or hopefully just the colors in `variables.sccs`) to make it look like designed for you particular app (your designer will probably send you a Figma link to a UIKit).

# Elements
## DropDown, ComboBox, MultiSelect
Our DropDown, ComboBox and MultiSelect are tiny wrappers over MUI [Autocomplete](https://mui.com/material-ui/react-autocomplete/).
These controls accept all underlying props of Autocomplete, so you could consult [official docs](https://mui.com/material-ui/api/autocomplete/) for details.
We also add some additional properties on top of existing:

#### getOptionLabel : ((option: T) => string) | PropertyAccessor<T>
Same as in MUI Autocomplete, but also allows to define a property name (e.g. if Option is `{value: string;}` object, we could put 'value' in `getOptionLabel` )

#### idFunction : ((option: T) => string) | PropertyAccessor<T>
Could be specified instead of `isOptionEqualToValue`. Defines an identifier for the Option.

#### required : boolean
If false the special `Not selected` element is added to the list of options

#### enableSearch : boolean
If true, it's possible to type right into the input to filter results.

If false, user could only select from a list

#### useVirtualization : boolean;
If true, uses react-window to render elements in drop-down list.
Optimizes performance when there are a lot of Options.

#### customOptions : {label: string; onClick: () => void;}[]
You could add more options to Autocomplete beside the standard `menuItems`.
It's useful when for example you have a dropdown with categories and want to add a `Add New Category' item
Custom options are added to the top of the list.

#### postfixRenderer?: (option: T) => React.ReactElement<unknown>;
Allows to render something custom at the right of each Option.

#### autosizeInputWidth : boolean
If true, the size of an input is determined by the currently selected element


#### maxPopupWidth?: number;
Specifies maximum width of the popup.
Default to '450px'

#### additionalWidth?: CSSProperties['width'];
Makes sense to use when `useVirtualization` is true and `popupWidth` is auto.
Specifies the width that is added to automatically calculated item width.
Could be used for paddings and/or postfix.
Defaults to '40px'

#### useIdFunctionAsValue?: boolean;
If true, we assume that `value` field contains the result of `idFunction` of the option.
HookFormDropDownInput has this enabled by default.

## HookFormDropDown, HookFormComboBox, HookFormMultiSelect
These are convenience wrappers to use DropDown, ComboBox and MultiSelect in a form controlled by react-hook-form (so that you don't need to wrap it in `<Controller>` every time).
Check out example usage on [UIKit page](/frontend/src/pages/authorized/uikit/UiKitPage.tsx).
