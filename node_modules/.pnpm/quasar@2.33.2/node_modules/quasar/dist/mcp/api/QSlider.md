## QSlider API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `min` (number, optional), default `0`
  Minimum value of the model; Set track's minimum value
- `max` (number, optional), default `100`
  Maximum value of the model; Set track's maximum value
- `inner-min` (number, optional)
  Inner minimum value of the model; Use in case you need the model value to be inside of the track's min-max values; Needs to be higher or equal to 'min' prop; Defaults to 'min' prop
- `inner-max` (number, optional)
  Inner maximum value of the model; Use in case you need the model value to be inside of the track's min-max values; Needs to be lower or equal to 'max' prop; Defaults to 'max' prop
- `step` (number, optional), default `1`
  Specify step amount between valid values (> 0.0); When step equals to 0 it defines infinite granularity
- `snap` (boolean, optional)
  Snap on valid values, rather than sliding freely; Suggestion: use with 'step' prop
- `reverse` (boolean, optional)
  Work in reverse (changes direction)
- `vertical` (boolean, optional)
  Display in vertical direction
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `track-color` (string, optional)
  Color name for the track (can be 'transparent' too) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `track-img` (string, optional)
  Apply a pattern image on the track
  Examples: `'~@/assets/my-pattern.png'`
- `inner-track-color` (string, optional)
  Color name for the inner track (can be 'transparent' too) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `inner-track-img` (string, optional)
  Apply a pattern image on the inner track
  Examples: `'~@/assets/my-pattern.png'`
- `selection-color` (string, optional)
  Color name for the selection bar (can be 'transparent' too) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `selection-img` (string, optional)
  Apply a pattern image on the selection bar
  Examples: `'~@/assets/my-pattern.png'`
- `label` (boolean, optional)
  Popup a label when user clicks/taps on the slider thumb and moves it
- `label-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `label-text-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `switch-label-side` (boolean, optional)
  Switch the position of the label (top <-> bottom or left <-> right)
- `label-always` (boolean, optional)
  Always display the label
- `markers` (boolean | number, optional)
  Display markers on the track, one for each possible value for the model or using a custom step (when specifying a Number)
  Examples: `5`, `true`
- `marker-labels` (boolean | any[] | object | Function, optional)
  Configure the marker labels (or show the default ones if 'true'); Array of definition Objects or Object with key-value where key is the model and the value is the marker label definition
  Function signature: `(value: number) => string | object`
  Examples:
    - `true`
    - `[{ value: 0, label: '0%' }, { value: 5, classes: 'my-class', style: { width: '24px' } }]`
    - `{ 0: '0%', 5: { label: '5%', classes: 'my-class', style: { width: '24px' } } }`
    - `val => (10 * val) + '%'`
    - `val => ({ label: (10 * val) + '%', classes: 'my-class', style: { width: '24px' } })`
  Params:
    - `value` (number, required)
      The marker value to transform
  Returns: `string | object`
    Marker definition Object or directly a String for the label of the marker
    Object shape:
      - `value` (number, optional)
        Value of equivalent model where to position the marker
      - `label` (number | string, optional)
        Label to use
      - `classes` (string | any[] | object, optional)
        CSS classes to be attributed to the marker label
        Examples: `'my-class-name'`
      - `style` (object, optional)
        Style definitions to be attributed to the marker label
        Examples: `{ height: '24px' }`
  Object shape:
    - `value` (number, required)
      Value of equivalent model where to position the marker
    - `label` (number | string, optional)
      Label to use
    - `classes` (string | any[] | object, optional)
      CSS classes to be attributed to the marker label
      Examples: `'my-class-name'`
    - `style` (object, optional)
      Style definitions to be attributed to the marker label
      Examples: `{ height: '24px' }`
- `marker-labels-class` (string, optional)
  CSS class(es) to apply to the marker labels container
  Examples: `'text-orange'`
- `switch-marker-labels-side` (boolean, optional)
  Switch the position of the marker labels (top <-> bottom or left <-> right)
- `track-size` (string, optional), default `'4px'`
  Track size (including CSS unit)
  Examples: `'35px'`
- `thumb-size` (string, optional), default `'20px'`
  Thumb size (including CSS unit)
  Examples: `'20px'`
- `thumb-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `thumb-path` (string, optional), default `'M 4, 10 a 6,6 0 1,0 12,0 a 6,6 0 1,0 -12,0'`
  Set custom thumb svg path
  Examples: `'M5 5 h10 v10 h-10 v-10'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `disable` (boolean, optional)
  Put component in disabled mode
- `readonly` (boolean, optional)
  Put component in readonly mode
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `model-value` (number, required, syncable), default `null`
  Model of the component (must be between min/max); Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
  Examples: `v-model="positionModel"`
- `label-value` (string | number, optional)
  Override default label value
  Examples: `:label-value="model + 'px'"`

### Events

- `@change`
  Emitted on lazy model value change (after user slides then releases the thumb)
  Params:
    - `value` (any, required)
      New model value
- `@pan`
  Triggered when user starts panning on the component
  Params:
    - `phase` (string, optional)
      Phase of panning
      Accepts: `'start'`, `'end'`
- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (number, required)
      New model value

### Scoped Slots

- `#marker-label`
  What should the menu display after filtering options and none are left to be displayed; Suggestion: <div>
  Scope:
    - `marker` (object, optional)
      Config for current marker label
      Object shape:
        - `index` (number, optional)
          Index of the marker label (0-based)
        - `value` (number, optional)
          Equivalent model value for the marker label
        - `label` (number | string, optional)
          Configured label for the marker
        - `classes` (string, optional)
          Required CSS classes to be applied to the marker element
        - `style` (object, optional)
          Style definitions to be attributed to the marker label
          Examples: `{ height: '24px' }`
    - `markerList` (any[], optional)
      Array of marker label configs
      Object shape:
        - `index` (number, optional)
          Index of the marker label (0-based)
        - `value` (number, optional)
          Equivalent model value for the marker label
        - `label` (number | string, optional)
          Configured label for the marker
        - `classes` (string, optional)
          Required CSS classes to be applied to the marker element
        - `style` (object, optional)
          Style definitions to be attributed to the marker label
          Examples: `{ height: '24px' }`
    - `markerMap` (object, optional)
      Object with key-value where key is the model and the value is the marker label config
      Object shape:
        - `...key` (object, optional)
          Marker label config
          Object shape:
            - `index` (number, optional)
              Index of the marker label (0-based)
            - `value` (number, optional)
              Equivalent model value for the marker label
            - `label` (number | string, optional)
              Configured label for the marker
            - `classes` (string, optional)
              Required CSS classes to be applied to the marker element
            - `style` (object, optional)
              Style definitions to be attributed to the marker label
              Examples: `{ height: '24px' }`
    - `classes` (string, optional)
      Required CSS classes to be applied to the marker element
    - `getStyle` (Function, optional)
      Get CSS style Object to apply to a marker element at respective model value; For perf reasons, use only if requested model value is not already part of markerMap
      Function signature: `(value: number) => object`
      Params:
        - `value` (number, required)
          The marker label equivalent model value
      Returns: `object`
        CSS style Object to apply to a marker element at respective model value
- `#marker-label-group`
  What should the menu display after filtering options and none are left to be displayed; Suggestion: <div>
  Scope:
    - `markerList` (any[], optional)
      Array of marker label configs
      Object shape:
        - `index` (number, optional)
          Index of the marker label (0-based)
        - `value` (number, optional)
          Equivalent model value for the marker label
        - `label` (number | string, optional)
          Configured label for the marker
        - `classes` (string, optional)
          Required CSS classes to be applied to the marker element
        - `style` (object, optional)
          Style definitions to be attributed to the marker label
          Examples: `{ height: '24px' }`
    - `markerMap` (object, optional)
      Object with key-value where key is the model and the value is the marker label config
      Object shape:
        - `...key` (object, optional)
          Marker label config
          Object shape:
            - `index` (number, optional)
              Index of the marker label (0-based)
            - `value` (number, optional)
              Equivalent model value for the marker label
            - `label` (number | string, optional)
              Configured label for the marker
            - `classes` (string, optional)
              Required CSS classes to be applied to the marker element
            - `style` (object, optional)
              Style definitions to be attributed to the marker label
              Examples: `{ height: '24px' }`
    - `classes` (string, optional)
      Required CSS classes to be applied to the marker element
    - `getStyle` (Function, optional)
      Get CSS style Object to apply to a marker element at respective model value; For perf reasons, use only if requested model value is not already part of markerMap
      Function signature: `(value: number) => object`
      Params:
        - `value` (number, required)
          The marker label equivalent model value
      Returns: `object`
        CSS style Object to apply to a marker element at respective model value

