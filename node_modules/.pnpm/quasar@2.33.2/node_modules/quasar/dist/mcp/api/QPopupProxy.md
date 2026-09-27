## QPopupProxy API

### Props

- `target` (boolean | string | Element, optional), default `true`
  Configure a target element to trigger component toggle; 'true' means it enables the parent DOM element, 'false' means it disables attaching events to any DOM elements; By using a String (CSS selector) or a DOM element it attaches the events to the specified DOM element (if it exists)
  Examples: `false`, `.my-parent`, `#target-id`, `$refs.target`
- `no-parent-event` (boolean, optional)
  Skips attaching events to the target DOM element (that trigger the element to get shown)
- `context-menu` (boolean, optional)
  Allows the component to behave like a context menu, which opens with a right mouse click (or a long press on touch-capable devices)
- `model-value` (boolean, optional, syncable)
  Defines the state of the component (shown/hidden); Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
- `breakpoint` (number | string, optional), default `450`
  Breakpoint (in pixels) of window width/height (whichever is smaller) from where a Menu will get to be used instead of a Dialog
- `hover` (boolean, optional) *(added v2.26)*
  Also opens the menu when the pointer hovers its target and closes it when the pointer leaves both the target and the menu; Click/tap and keyboard interactions keep toggling as usual (touch devices fall back to them), so activating the target closes a hover-shown menu, unless it is still animating into view (which keeps a single move-and-click gesture from closing what it just opened); A hover-opened menu does not switch focus on itself and ignores 'touch-position'; Mutually exclusive with 'context-menu', which takes precedence; Only applies when a Menu is used (see 'breakpoint')
- `hover-delay` (number, optional), default `0` *(added v2.26)*
  Delay (in milliseconds) between the pointer entering the target and the menu showing up; Requires the 'hover' prop; Only applies when a Menu is used (see 'breakpoint')
- `hover-hide-delay` (number, optional), default `150` *(added v2.26)*
  Grace period (in milliseconds) in which the pointer can re-enter the target or the menu before it gets closed; Requires the 'hover' prop; Only applies when a Menu is used (see 'breakpoint')
- `transition-show` (string, optional)
  One of Quasar's embedded transitions; When a Menu is used it defaults to 'fade'; When a Dialog is used the default depends on 'position' (see 'breakpoint')
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional)
  One of Quasar's embedded transitions; When a Menu is used it defaults to 'fade'; When a Dialog is used the default depends on 'position' (see 'breakpoint')
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color; Only applies when a Menu is used (see 'breakpoint')
- `fit` (boolean, optional)
  Allows the menu to match at least the full width of its target; Only applies when a Menu is used (see 'breakpoint')
- `cover` (boolean, optional)
  Allows the menu to cover its target. When used, the 'self' and 'fit' props are no longer effective; Only applies when a Menu is used (see 'breakpoint')
- `anchor` (string, optional)
  Two values setting the starting position or anchor point of the menu relative to its target; Only applies when a Menu is used (see 'breakpoint')
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `self` (string, optional)
  Two values setting the menu's own position relative to its target; Only applies when a Menu is used (see 'breakpoint')
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `offset` (any[], optional)
  An array of two numbers (in pixels) which expands the anchor element's bounding box outward horizontally and vertically; the menu is then positioned against the expanded box, so the visible effect depends on the 'anchor'/'self' points in use; Only applies when a Menu is used (see 'breakpoint')
  Examples:
    - `[8, 8]`
    - `[5, 10]`
- `touch-position` (boolean, optional)
  Allows for the target position to be set by the mouse position, when the target of the menu is either clicked or touched; Only applies when a Menu is used (see 'breakpoint')
- `persistent` (boolean, optional)
  Allows the popup to not be dismissed by a click/tap outside of it or by hitting the ESC key; Also, an app route change won't dismiss it
- `no-esc-dismiss` (boolean, optional)
  User cannot dismiss the popup by hitting ESC key; No need to set it if 'persistent' prop is also set
- `no-route-dismiss` (boolean, optional)
  Changing route app won't dismiss the popup; No need to set it if 'persistent' prop is also set
- `auto-close` (boolean, optional)
  Any click/tap inside of the popup will close it
- `square` (boolean, optional)
  Forces content to have squared borders
- `no-refocus` (boolean, optional)
  (Accessibility) When the popup gets hidden, do not refocus on the DOM element that previously had focus
- `no-focus` (boolean, optional)
  (Accessibility) When the popup gets shown, do not switch focus on it
- `max-height` (string, optional), default `'99vh'`
  The maximum height of the menu; Size in CSS units, including unit name; Only applies when a Menu is used (see 'breakpoint')
  Examples: `'16px'`, `'2rem'`
- `max-width` (string, optional), default `null`
  The maximum width of the menu; Size in CSS units, including unit name; Only applies when a Menu is used (see 'breakpoint')
  Examples: `'16px'`, `'2rem'`
- `no-backdrop-dismiss` (boolean, optional)
  User cannot dismiss Dialog by clicking outside of it; No need to set it if 'persistent' prop is also set; Only applies when a Dialog is used (see 'breakpoint')
- `seamless` (boolean, optional)
  Put Dialog into seamless mode; Does not use a backdrop so user is able to interact with the rest of the page too; Only applies when a Dialog is used (see 'breakpoint')
- `backdrop-filter` (string, optional) *(added v2.15)*
  Apply a backdrop filter; The value needs to be the same as in the CSS specs for backdrop-filter; The examples are not an exhaustive list; Only applies when a Dialog is used (see 'breakpoint')
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
  Put Dialog into maximized mode; Only applies when a Dialog is used (see 'breakpoint')
- `full-width` (boolean, optional)
  Dialog will try to render with same width as the window; Only applies when a Dialog is used (see 'breakpoint')
- `full-height` (boolean, optional)
  Dialog will try to render with same height as the window; Only applies when a Dialog is used (see 'breakpoint')
- `position` (string, optional), default `'standard'`
  Stick dialog to one of the sides (top, right, bottom or left); Only applies when a Dialog is used (see 'breakpoint')
  Accepts: `'standard'`, `'top'`, `'right'`, `'bottom'`, `'left'`
- `no-shake` (boolean, optional)
  Do not shake up the Dialog to catch user's attention; Only applies when a Dialog is used (see 'breakpoint')
- `allow-focus-outside` (boolean, optional)
  Allow elements outside of the Dialog to be focusable; By default, for accessibility reasons, QDialog does not allow outer focus; Only applies when a Dialog is used (see 'breakpoint')

### Computed Props

- `currentComponent` (object, optional)
  Access current underlying component (QMenu or QDialog)
  Object shape:
    - `type` (string, optional)
      Component type
      Accepts: `'dialog'`, `'menu'`
    - `ref` (ComponentInstance, optional)
      The actual component (QMenu or QDialog); Access it directly, without '.value'

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

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value
- `@before-show`
  Emitted when component triggers show() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@show`
  Emitted after component has triggered show()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-hide`
  Emitted when component triggers hide() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@hide`
  Emitted after component has triggered hide()
  Params:
    - `evt` (Event, required)
      JS event object
- `@escape-key`
  Emitted when ESC key is pressed; Does not get emitted if the popup is 'persistent' or it has 'no-esc-dismiss' set
- `@shake`
  Emitted when the Dialog shakes in order to catch user's attention, unless the 'no-shake' property is set; Only applies when a Dialog is used (see 'breakpoint')

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

