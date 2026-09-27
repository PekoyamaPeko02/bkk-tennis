## QMenu API

### Props

- `hover` (boolean, optional) *(added v2.26)*
  Also opens the menu when the pointer hovers its target and closes it when the pointer leaves both the target and the menu; Click/tap and keyboard interactions keep toggling as usual (touch devices fall back to them), so activating the target closes a hover-shown menu, unless it is still animating into view (which keeps a single move-and-click gesture from closing what it just opened); A hover-opened menu does not switch focus on itself and ignores 'touch-position'; Mutually exclusive with 'context-menu', which takes precedence
- `hover-delay` (number, optional), default `0` *(added v2.26)*
  Delay (in milliseconds) between the pointer entering the target and the menu showing up; Requires the 'hover' prop
- `hover-hide-delay` (number, optional), default `150` *(added v2.26)*
  Grace period (in milliseconds) in which the pointer can re-enter the target or the menu before it gets closed; Requires the 'hover' prop
- `transition-show` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `target` (boolean | string | Element, optional), default `true`
  Configure a target element to trigger component toggle; 'true' means it enables the parent DOM element, 'false' means it disables attaching events to any DOM elements; By using a String (CSS selector) or a DOM element it attaches the events to the specified DOM element (if it exists)
  Examples: `false`, `.my-parent`, `#target-id`, `$refs.target`
- `no-parent-event` (boolean, optional)
  Skips attaching events to the target DOM element (that trigger the element to get shown)
- `context-menu` (boolean, optional)
  Allows the component to behave like a context menu, which opens with a right mouse click (or a long press on touch-capable devices)
- `model-value` (boolean, optional), default `null`
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `fit` (boolean, optional)
  Allows the menu to match at least the full width of its target
- `cover` (boolean, optional)
  Allows the menu to cover its target. When used, the 'self' and 'fit' props are no longer effective
- `anchor` (string, optional)
  Two values setting the starting position or anchor point of the menu relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `self` (string, optional)
  Two values setting the menu's own position relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `offset` (any[], optional)
  An array of two numbers (in pixels) which expands the anchor element's bounding box outward horizontally and vertically; the menu is then positioned against the expanded box, so the visible effect depends on the 'anchor'/'self' points in use
  Examples:
    - `[8, 8]`
    - `[5, 10]`
- `touch-position` (boolean, optional)
  Allows for the target position to be set by the mouse position, when the target of the menu is either clicked or touched
- `persistent` (boolean, optional)
  Allows the menu to not be dismissed by a click/tap outside of the menu or by hitting the ESC key; Also, an app route change won't dismiss it
- `no-esc-dismiss` (boolean, optional) *(added v2.18)*
  User cannot dismiss the popup by hitting ESC key; No need to set it if 'persistent' prop is also set
- `no-route-dismiss` (boolean, optional)
  Changing route app won't dismiss the popup; No need to set it if 'persistent' prop is also set
- `auto-close` (boolean, optional)
  Allows any click/tap in the menu to close it; Useful instead of attaching events to each menu item that should close the menu on click/tap
- `separate-close-popup` (boolean, optional)
  Separate from parent menu, marking it as a separate closing point for v-close-popup (without this, chained menus close all together)
- `square` (boolean, optional)
  Forces content to have squared borders
- `no-refocus` (boolean, optional)
  (Accessibility) When Menu gets hidden, do not refocus on the DOM element that previously had focus
- `no-focus` (boolean, optional)
  (Accessibility) When Menu gets shown, do not switch focus on it
- `max-height` (string, optional), default `null`
  The maximum height of the menu; Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `max-width` (string, optional), default `null`
  The maximum width of the menu; Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`

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
- `updatePosition(): void`
  The menu follows its target automatically (through any scrolling container) and this method only re-checks whether the intended placement still fits the viewport (which side the menu opens to and its maximum size); call it for the scenarios Quasar cannot detect, e.g. after replacing the menu's content while it is shown
- `focus(): void`
  Focus menu; if you have content with autofocus attribute, it will directly focus it

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
- `@escape-key`
  Emitted when ESC key is pressed; Does not get emitted if Menu is 'persistent' or it has 'no-esc-dismiss' set

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

