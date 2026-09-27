## QBtnToggle API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `model-value` (any, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
  Examples: `v-model="selected"`
- `options` (any[], required)
  Array of Objects defining each option
  Examples:
    - `[{ label: 'One', value: 'one' }, { label: 'Two', value: 'two' }]`
  Object shape:
    - `attrs` (object, optional)
      Key-value for attributes to be set on the button
      Examples: `{ 'aria-label': 'Button label' }`
    - `label` (string, optional)
      Label of option button; Use this prop and/or 'icon', but at least one is required
      Examples: `'Option 1'`
    - `icon` (string, optional)
      Icon of option button; Use this prop and/or 'label', but at least one is required
      Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
    - `value` (any, required)
      Value of the option that will be used by component model
    - `slot` (string, optional)
      Slot name to use for this button content; Useful for customizing content or even add tooltips
      Examples: `'mySlot'`
    - `...props` (any, optional)
      Any other QBtn props (including class and style)
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `toggle-color` (string, optional), default `'primary'`
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `toggle-text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `spread` (boolean, optional)
  Spread horizontally to all available space
- `size` (string, optional)
  Button size name or a CSS unit including unit name
  Examples:
    - `'xs'`
    - `'sm'`
    - `'md'`
    - `'lg'`
    - `'xl'`
    - `'25px'`
    - `'2rem'`
- `ripple` (boolean | object, optional), default `true`
  Configure material ripple (disable it by setting it to 'false' or supply a config object)
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `readonly` (boolean, optional)
  Put component in readonly mode
- `disable` (boolean, optional)
  Put component in disabled mode
- `clearable` (boolean, optional)
  Clears model on click of the already selected button
- `outline` (boolean, optional)
  Use 'outline' design
- `flat` (boolean, optional)
  Use 'flat' design
- `unelevated` (boolean, optional)
  Remove shadow
- `rounded` (boolean, optional)
  Applies a more prominent border-radius for a squared shape button
- `push` (boolean, optional)
  Use 'push' design
- `glossy` (boolean, optional)
  Applies a glossy effect
- `padding` (string, optional)
  Apply custom padding (vertical [horizontal]); Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl); Also removes the min width and height when set
  Examples:
    - `'16px'`
    - `'10px 5px'`
    - `'2rem'`
    - `'xs'`
    - `'md lg'`
    - `'2px 2px 5px 7px'`
- `no-caps` (boolean, optional)
  Avoid turning label text into caps (which happens by default)
- `no-wrap` (boolean, optional)
  Avoid label text wrapping
- `stack` (boolean, optional)
  Stack icon and label vertically instead of on same line (like it is by default)
- `stretch` (boolean, optional)
  When used on flexbox parent, button will stretch to parent's height

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value
- `@clear`
  When using the 'clearable' property, this event is emitted when the already selected button is clicked

### Slots

- `#default`
  Suggestions: QTooltip, QBadge
- `#...`
  Any other dynamic slots to be used with 'slot' property of the 'options' prop

