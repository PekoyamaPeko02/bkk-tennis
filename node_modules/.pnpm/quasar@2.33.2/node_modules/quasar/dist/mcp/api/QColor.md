## QColor API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `model-value` (string, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="myColor"`
- `default-value` (string, optional)
  The default value to show when the model doesn't have one
  Examples: `'#c0c0c0'`
- `default-view` (string, optional), default `'spectrum'`
  The default view of the picker
  Accepts: `'spectrum'`, `'tune'`, `'palette'`
- `format-model` (string, optional), default `'auto'`
  Forces a certain model format upon the model
  Accepts: `'auto'`, `'hex'`, `'rgb'`, `'hexa'`, `'rgba'`
- `palette` (any[], optional), default `hard-coded palette`
  Use a custom palette of colors for the palette tab
  Examples:
    - `['#019A9D', '#D9B801', 'rgb(23,120,0)', '#B2028A']`
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow)
- `bordered` (boolean, optional)
  Applies a default border to the component
- `no-header` (boolean, optional)
  Do not render header
- `no-header-tabs` (boolean, optional)
  Do not render header tabs (only the input)
- `no-footer` (boolean, optional)
  Do not render footer; Useful when you want a specific view ('default-view' prop) and don't want the user to be able to switch it
- `disable` (boolean, optional)
  Put component in disabled mode
- `readonly` (boolean, optional)
  Put component in readonly mode
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (string, required)
      New model value
- `@change`
  Emitted on lazy model value change (after user finishes selecting a color)
  Params:
    - `value` (any, required)
      New model value

### Scoped Slots

- `#palette`
  Override the default swatches of the palette view; you decide the layout, labels and accessibility of the swatches
  Scope:
    - `palette` (any[], optional)
      The colors of the palette (the 'palette' prop, or the built-in list when the prop is not set)
      Examples:
        - `['#019A9D', '#D9B801', 'rgb(23,120,0)', '#B2028A']`
    - `select` (Function, optional)
      Set the model to a color; does nothing while the component is disabled or readonly
      Function signature: `(color: string) => void`
      Params:
        - `color` (string, required)
          The color to select (hex, hexa, rgb or rgba string)
          Examples:
            - `'#019A9D'`
            - `'rgb(23,120,0)'`
    - `editable` (boolean, optional)
      Whether the user can currently change the color (false while disabled or readonly)

