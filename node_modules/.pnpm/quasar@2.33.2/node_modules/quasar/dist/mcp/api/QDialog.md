## QDialog API

### Props

- `transition-show` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `model-value` (boolean, optional), default `null`
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
- `persistent` (boolean, optional)
  User cannot dismiss Dialog if clicking outside of it or hitting ESC key; Also, an app route change won't dismiss it
- `no-esc-dismiss` (boolean, optional)
  User cannot dismiss Dialog by hitting ESC key; No need to set it if 'persistent' prop is also set
- `no-backdrop-dismiss` (boolean, optional)
  User cannot dismiss Dialog by clicking outside of it; No need to set it if 'persistent' prop is also set
- `no-route-dismiss` (boolean, optional)
  Changing route app won't dismiss Dialog; No need to set it if 'persistent' prop is also set
- `auto-close` (boolean, optional)
  Any click/tap inside of the dialog will close it
- `seamless` (boolean, optional)
  Put Dialog into seamless mode; Does not use a backdrop so user is able to interact with the rest of the page too
- `backdrop-filter` (string, optional) *(added v2.15)*
  Apply a backdrop filter; The value needs to be the same as in the CSS specs for backdrop-filter; The examples are not an exhaustive list
  Examples:
    - `'blur(4px)'`
    - `'blur(4px) saturate(150%)'`
    - `'brightness(60%)'`
    - `'invert(70%)'`
    - `'grayscale(100%)'`
    - `'contrast(40%)'`
    - `'hue-rotate(120deg)'`
    - `'sepia(90%)'`
    - `'saturate(80%)'`
    - `'none'`
- `maximized` (boolean, optional)
  Put Dialog into maximized mode
- `full-width` (boolean, optional)
  Dialog will try to render with same width as the window
- `full-height` (boolean, optional)
  Dialog will try to render with same height as the window
- `position` (string, optional), default `'standard'`
  Stick dialog to one of the sides (top, right, bottom or left)
  Accepts: `'standard'`, `'top'`, `'right'`, `'bottom'`, `'left'`
- `square` (boolean, optional)
  Forces content to have squared borders
- `no-refocus` (boolean, optional)
  (Accessibility) When Dialog gets hidden, do not refocus on the DOM element that previously had focus
- `no-focus` (boolean, optional)
  (Accessibility) When Dialog gets shown, do not switch focus on it
- `no-shake` (boolean, optional)
  Do not shake up the Dialog to catch user's attention
- `allow-focus-outside` (boolean, optional)
  Allow elements outside of the Dialog to be focusable; By default, for accessibility reasons, QDialog does not allow outer focus

### Computed Props

- `contentEl` (Element, optional)
  The DOM Element of the rendered content

### Methods

- `show(evt?: Event): void`
  Triggers component to show
  Params:
    - `evt` (Event, optional)
      JS event object
- `hide(evt?: Event): void`
  Triggers component to hide
  Params:
    - `evt` (Event, optional)
      JS event object
- `toggle(evt?: Event): void`
  Triggers component to toggle between show/hide
  Params:
    - `evt` (Event, optional)
      JS event object
- `focus(selector?: string): void`
  Focus dialog; if you have content with autofocus attribute, it will directly focus it
  Params:
    - `selector` (string, optional)
      Optional CSS selector to override default focusable element
      Examples: `'[tabindex]:not([tabindex="-1"])'`
- `shake(focusTarget?: Element): void`
  Shakes dialog
  Params:
    - `focusTarget` (Element, optional)
      Optional DOM Element to be focused after shake
      Examples: `document.getElementById('example')`

### Events

- `@update:model-value`
  Emitted when showing/hidden state changes; Is also used by v-model
  Params:
    - `value` (boolean, optional)
      New state (showing/hidden)
- `@show`
  Emitted after component has triggered show()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-show`
  Emitted when component triggers show() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@hide`
  Emitted after component has triggered hide()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-hide`
  Emitted when component triggers hide() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@shake`
  Emitted when the Dialog shakes in order to catch user's attention, unless the 'no-shake' property is set
- `@escape-key`
  Emitted when ESC key is pressed; Does not get emitted if Dialog is 'persistent' or it has 'no-esc-dismiss' set

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

