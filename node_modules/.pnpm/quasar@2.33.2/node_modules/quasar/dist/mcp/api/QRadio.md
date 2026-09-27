## QRadio API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `model-value` (any, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="option"`
- `val` (any, required)
  The actual value of the option with which model value is changed
  Examples: `'opt1'`, `50`
- `label` (string, optional)
  Label to display along the radio control (or use the default slot instead of this prop)
  Examples: `'Option 1'`
- `left-label` (boolean, optional)
  Label (if any specified) should be displayed on the left side of the checkbox
- `checked-icon` (string, optional)
  The icon to be used when selected (instead of the default design)
  Examples: `'visibility'`
- `unchecked-icon` (string, optional)
  The icon to be used when un-selected (instead of the default design)
  Examples: `'visibility_off'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `keep-color` (boolean, optional)
  Should the color (if specified any) be kept when checkbox is unticked?
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

- `set(): void`
  Sets the Radio's v-model to equal the val

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

