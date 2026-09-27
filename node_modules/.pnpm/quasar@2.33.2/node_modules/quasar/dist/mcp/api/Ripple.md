## Ripple API

### quasar.config.js Options

Configuration key: `framework.config.ripple` (boolean | object)

- `early` (boolean, optional)
  Trigger early/immediately on user interaction; the ripple auto-cancels if the gesture turns into a scroll/pan or the pressed pointer is dragged off the element
- `stop` (boolean, optional)
  Stop click/touch event propagation
- `center` (boolean, optional)
  Ripple starts from the absolute center
- `color` (string, optional)
  Color name from Quasar Color Palette; Overrides default dynamic color
  Examples: `'orange-5'`
- `keyCodes` (any[] | number, optional)
  List of keyCode that should trigger the ripple
  Examples:
    - `[]`
    - `[13, 32]`

### Directive Value

- `value` (boolean | object, optional)
  Boolean (if just wanting to enable/disable) or Object for configuring more options
  Examples:
    - `v-ripple="booleanState"`
    - `v-ripple="{ center: true, color: 'primary', keyCodes: [] }"`
  Object shape:
    - `early` (boolean, optional)
      Trigger early/immediately on user interaction; the ripple auto-cancels if the gesture turns into a scroll/pan or the pressed pointer is dragged off the element
    - `stop` (boolean, optional)
      Stop click/touch event propagation
    - `center` (boolean, optional)
      Ripple starts from the absolute center
    - `color` (string, optional)
      Color name from Quasar Color Palette; Overrides default dynamic color
      Examples: `'orange-5'`
    - `keyCodes` (any[] | number, optional)
      List of keyCode that should trigger the ripple
      Examples:
        - `[]`
        - `[13, 32]`

### Directive Argument

- `arg` (string, optional)
  Color name from Quasar Color Palette; Overrides default dynamic color
  Examples: `v-ripple:orange-5`

### Directive Modifiers

- `early` (boolean, optional)
  Trigger early/immediately on user interaction; the ripple auto-cancels if the gesture turns into a scroll/pan or the pressed pointer is dragged off the element
- `stop` (boolean, optional)
  Stop click/touch event propagation
  Examples: `v-ripple.stop`
- `center` (boolean, optional)
  Ripple starts from the absolute center
  Examples: `v-ripple.center`

