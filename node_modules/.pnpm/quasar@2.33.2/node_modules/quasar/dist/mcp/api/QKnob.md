## QKnob API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `model-value` (number, required, syncable)
  Any number to indicate the given value of the knob. Either use this property (along with a listener for 'update:modelValue' event) OR use the v-model directive
  Examples: `v-model="myValue"`
- `min` (number, optional), default `0`
  The minimum value that the model (the knob value) should start at
- `max` (number, optional), default `100`
  The maximum value that the model (the knob value) should go to
- `inner-min` (number, optional)
  Inner minimum value of the model; Use in case you need the model value to be inside of the track's min-max values; Needs to be higher or equal to 'min' prop; Defaults to 'min' prop
- `inner-max` (number, optional)
  Inner maximum value of the model; Use in case you need the model value to be inside of the track's min-max values; Needs to be lower or equal to 'max' prop; Defaults to 'max' prop
- `step` (number, optional), default `1`
  A number representing steps in the value of the model, while adjusting the knob
- `reverse` (boolean, optional)
  Reverses the direction of progress
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `tabindex` (number | string, optional), default `0`
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `disable` (boolean, optional)
  Put component in disabled mode
- `readonly` (boolean, optional)
  Put component in readonly mode
- `instant-feedback` (boolean, optional)
  No animation when model changes
- `center-color` (string, optional)
  Color name for the center part of the component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `track-color` (string, optional)
  Color name for the track of the component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `font-size` (string, optional)
  Size of text in CSS units, including unit name. Suggestion: use 'em' units to sync with component size
  Examples: `'1em'`, `'16px'`, `'2rem'`
- `rounded` (boolean, optional)
  Rounding the arc of progress
- `thickness` (number, optional), default `0.2`
  Thickness of progress arc as a ratio (0.0 < x < 1.0) of component size
- `angle` (number, optional), default `0`
  Angle to rotate progress arc by
- `show-value` (boolean, optional)
  Enables the default slot and uses it (if available), otherwise it displays the 'value' prop as text; Make sure the text has enough space to be displayed inside the component

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (number, required)
      New model value
- `@change`
  Fires at the end of a knob's adjustment and offers the value of the model
  Params:
    - `value` (number, optional)
      New model value
- `@drag-value`
  The value of the model while dragging is still in progress
  Params:
    - `value` (number, optional)
      New model value

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

