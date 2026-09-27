## QCheckbox API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `model-value` (any | any[], required, syncable), default `null`
  Model of the component; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples:
    - `false`
    - `['car', 'building']`
- `val` (any, optional)
  Works when model ('value') is Array. It tells the component which value should add/remove when ticked/unticked
  Examples: `'car'`
- `true-value` (any, optional), default `true`
  What model value should be considered as checked/ticked/on?
  Examples: `'Agreed'`
- `false-value` (any, optional), default `false`
  What model value should be considered as unchecked/unticked/off?
  Examples: `'Disagree'`
- `indeterminate-value` (any, optional), default `null`
  What model value should be considered as 'indeterminate'?
  Examples: `0`, `'not_answered'`
- `toggle-order` (string, optional)
  Determines toggle order of the two states ('t' stands for state of true, 'f' for state of false); If 'toggle-indeterminate' is true, then the order is: indet -> first state -> second state -> indet (and repeat), otherwise: indet -> first state -> second state -> first state -> second state -> ...
  Accepts: `'tf'`, `'ft'`
- `toggle-indeterminate` (boolean, optional)
  When user clicks/taps on the component, should we toggle through the indeterminate state too?
- `label` (string, optional)
  Label to display along the component (or use the default slot instead of this prop)
  Examples: `'I agree with the Terms and Conditions'`
- `left-label` (boolean, optional)
  Label (if any specified) should be displayed on the left side of the component
- `checked-icon` (string, optional)
  The icon to be used when the model is truthy (instead of the default design)
  Examples: `'visibility'`
- `unchecked-icon` (string, optional)
  The icon to be used when the toggle is falsy (instead of the default design)
  Examples: `'visibility_off'`
- `indeterminate-icon` (string, optional)
  The icon to be used when the model is indeterminate (instead of the default design)
  Examples: `'help'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `keep-color` (boolean, optional)
  Should the color (if specified any) be kept when the component is unticked/ off?
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `disable` (boolean, optional)
  Put component in disabled mode
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`

### Methods

- `toggle(): void`
  Toggle the state (of the model)

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value
    - `evt` (Event, required)
      JS event object

### Slots

- `#default`
  Default slot can be used as label, unless 'label' prop is specified; Suggestion: string

