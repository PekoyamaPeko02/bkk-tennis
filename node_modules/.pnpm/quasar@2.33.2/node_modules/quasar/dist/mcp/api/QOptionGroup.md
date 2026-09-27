## QOptionGroup API

### Props

- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `model-value` (any, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="group"`
- `options` (any[], optional), default `[]`
  Array of objects that the binary components will be created from. For best performance reference a variable in your scope. Canonical form of each object is with 'label' (String), 'value' (Any) and optional 'disable' (Boolean) props (can be customized with options-value/option-label/option-disable props) along with any other props from QToggle, QCheckbox, or QRadio.
  Examples:
    - `[{ label: 'Option 1', value: 'op1' }, { label: 'Option 2', value: 'op2' }, { label: 'Option 3', value: 'op3', disable: true }]`
  Object shape:
    - `...props` (any, optional)
      Any other props from QToggle, QCheckbox, or QRadio
      Examples: `val="car"`, `:true-value="trueValue"`, `checked-icon="visibility"`
- `option-value` (Function | string, optional), default `'value'` *(added v2.17)*
  Property of option which holds the 'value'; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(option?: string | object) => any`
  Examples: `'modelNumber'`, `item => (item === null ? null : item.modelNumber)`
  Params:
    - `option` (string | object, optional)
      The current option being processed
      Examples:
        - `'Tesla'`
        - `'iPhone'`
        - `{ label: 'Tesla', value: 'car', cannotSelect: true }`
  Returns: `any`
    Value of the current option
    Examples: `'car'`, `34`
- `option-label` (Function | string, optional), default `'label'` *(added v2.17)*
  Property of option which holds the 'label'; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(option?: string | object) => string`
  Examples: `'itemName'`, `item => (item === null ? 'Null value' : item.itemName)`
  Params:
    - `option` (string | object, optional)
      The current option being processed
      Examples:
        - `'Tesla'`
        - `'iPhone'`
        - `{ label: 'Tesla', value: 'car', cannotSelect: true }`
  Returns: `string`
    Label of the current option
    Examples: `'Tesla'`, `'iPhone'`
- `option-disable` (Function | string, optional), default `'disable'` *(added v2.17)*
  Property of option which tells it's disabled; The value of the property must be a Boolean; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(option?: string | object) => boolean`
  Examples: `item => (item === null ? true : item.cannotSelect)`, `option-disable="cannotSelect"`
  Params:
    - `option` (string | object, optional)
      The current option being processed
      Examples:
        - `'Tesla'`
        - `'iPhone'`
        - `{ label: 'Tesla', value: 'car', cannotSelect: true }`
  Returns: `boolean`
    If true, the current option will be disabled
- `name` (string, optional)
  Used to specify the name of the controls; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `type` (string, optional), default `'radio'`
  The type of input component to be used
  Accepts: `'radio'`, `'checkbox'`, `'toggle'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `keep-color` (boolean, optional)
  Should the color (if specified any) be kept when input components are unticked?
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `left-label` (boolean, optional)
  Label (if any specified) should be displayed on the left side of the input components
- `inline` (boolean, optional)
  Show input components as inline-block rather than each having their own row
- `disable` (boolean, optional)
  Put component in disabled mode

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value

### Scoped Slots

- `#label`
  Generic slot for all labels
  Scope:
    - `...self` (object, optional)
      The corresponding option entry from the 'options' prop
      Object shape:
        - `label` (string, required)
          Label to display along the component
          Examples: `'Option 1'`, `'Option 2'`, `'Option 3'`
        - `value` (any, required)
          Value of the option that will be used by the component model
          Examples: `'op1'`, `'op2'`, `'op3'`
        - `disable` (boolean, optional)
          If true, the option will be disabled
        - `...props` (any, optional)
          Any other props from QToggle, QCheckbox, or QRadio
          Examples: `val="car"`, `:true-value="trueValue"`, `checked-icon="visibility"`
- `#label-[name]`
  Slot to define the specific label for the option at '[name]' where name is a 0-based index; Overrides the generic 'label' slot if used
  Scope:
    - `...self` (object, optional)
      The corresponding option entry from the 'options' prop
      Object shape:
        - `label` (string, required)
          Label to display along the component
          Examples: `'Option 1'`, `'Option 2'`, `'Option 3'`
        - `value` (any, required)
          Value of the option that will be used by the component model
          Examples: `'op1'`, `'op2'`, `'op3'`
        - `disable` (boolean, optional)
          If true, the option will be disabled
        - `...props` (any, optional)
          Any other props from QToggle, QCheckbox, or QRadio
          Examples: `val="car"`, `:true-value="trueValue"`, `checked-icon="visibility"`

