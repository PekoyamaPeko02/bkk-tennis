## QTabPanels API

### Props

- `model-value` (any, required)
  Model of the component defining the current panel's name; If a Number is used, it does not define the panel's index, but rather the panel's name which can also be an Integer; Either use this property (along with a listener for 'update:model-value' event) OR use the v-model directive.
  Examples: `v-model="panelName"`
- `keep-alive` (boolean, optional)
  Equivalent to using Vue's native <keep-alive> component on the content
- `keep-alive-include` (string | any[] | RegExp, optional)
  Equivalent to using Vue's native include prop for <keep-alive>; Values must be valid Vue component names
  Examples:
    - `'a,b'`
    - `/a|b/`
    - `['a', 'b']`
- `keep-alive-exclude` (string | any[] | RegExp, optional)
  Equivalent to using Vue's native exclude prop for <keep-alive>; Values must be valid Vue component names
  Examples:
    - `'a,b'`
    - `/a|b/`
    - `['a', 'b']`
- `keep-alive-max` (number, optional)
  Equivalent to using Vue's native max prop for <keep-alive>
- `animated` (boolean, optional)
  Enable transitions between panel (also see 'transition-prev' and 'transition-next' props)
- `infinite` (boolean, optional)
  Makes component appear as infinite (when reaching last panel, next one will become the first one)
- `swipeable` (boolean, optional)
  Enable swipe events (may interfere with content's touch/mouse events)
- `vertical` (boolean, optional)
  Default transitions and swipe actions will be on the vertical axis
- `transition-prev` (string, optional), default `slide-right/slide-down`
  One of Quasar's embedded transitions (has effect only if 'animated' prop is set)
  Examples: `'fade'`, `'slide-down'`
- `transition-next` (string, optional), default `slide-left/slide-up`
  One of Quasar's embedded transitions (has effect only if 'animated' prop is set)
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Methods

- `next(): void`
  Go to next panel
- `previous(): void`
  Go to previous panel
- `goTo(panelName: string | number): void`
  Go to specific panel
  Params:
    - `panelName` (string | number, required)
      Panel's name, which may be a String or Number; Number does not refers to panel index, but to its name, which may be an Integer
      Examples: `'dashboard'`

### Events

- `@update:model-value`
  Emitted when the component changes the model; This event _isn't_ fired if the model is changed externally; Is also used by v-model
  Params:
    - `value` (string | number, optional)
      New current panel name
      Examples: `'dashboard'`
- `@before-transition`
  Emitted before transitioning to a new panel
  Params:
    - `newVal` (string | number, optional)
      Panel name towards transition is going
      Examples: `'dashboard'`
    - `oldVal` (string | number, optional)
      Panel name from which transition is happening
      Examples: `'dashboard'`
- `@transition`
  Emitted after component transitioned to a new panel
  Params:
    - `newVal` (string | number, optional)
      Panel name towards transition has occurred
      Examples: `'dashboard'`
    - `oldVal` (string | number, optional)
      Panel name from which transition has happened
      Examples: `'dashboard'`

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

