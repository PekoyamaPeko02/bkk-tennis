## QPopupEdit API

### Props

- `model-value` (any, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="myValue"`
- `title` (string, optional)
  Optional title (unless 'title' slot is used)
  Examples: `'Calories'`
- `buttons` (boolean, optional)
  Show Set and Cancel buttons
- `label-set` (string, optional)
  Override Set button label
  Examples: `'OK'`
- `label-cancel` (string, optional)
  Override Cancel button label
  Examples: `'Cancel'`
- `auto-save` (boolean, optional)
  Automatically save the model (if changed) when user clicks/taps outside of the popup; It does not apply to ESC key
- `color` (string, optional), default `'primary'`
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `validate` (Function, optional), default `() => true`
  Validates model then triggers 'save' and closes Popup; Returns a Boolean ('true' means valid, 'false' means abort); Syntax: validate(value); For best performance, reference it from your scope and do not define it inline
  Function signature: `(value?: any) => boolean`
  Examples: `value => value !== 0`
  Params:
    - `value` (any, optional)
      Model to validate
      Examples: `'My car'`
  Returns: `boolean`
    Is the model valid or not?
- `disable` (boolean, optional)
  Put component in disabled mode
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
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `fit` (boolean, optional)
  Allows the menu to match at least the full width of its target
- `cover` (boolean, optional), default `true`
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

### Methods

- `set(): void`
  Trigger a model update; Validates model (and emits 'save' event if it's the case) then closes Popup
- `cancel(): void`
  Triggers a model reset to its initial value ('cancel' event is emitted) then closes Popup
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
- `updatePosition(): void`
  There are some custom scenarios for which Quasar cannot automatically reposition the component without significant performance drawbacks so the optimal solution is for you to call this method when you need it

### Events

- `@update:model-value`
  Emitted when Popup gets cancelled in order to reset model to its initial value; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value
- `@before-show`
  Emitted right before Popup gets shown
- `@show`
  Emitted right after Popup gets shown
- `@before-hide`
  Emitted right before Popup gets dismissed
- `@hide`
  Emitted right after Popup gets dismissed
- `@save`
  Emitted when value has been successfully validated and it should be saved
  Params:
    - `value` (any, optional)
      Validated value to be saved
    - `initialValue` (any, optional)
      Initial value, before changes
- `@cancel`
  Emitted when user cancelled the change (hit ESC key or clicking outside of Popup or hit 'Cancel' button)
  Params:
    - `value` (any, optional)
      Edited value
    - `initialValue` (any, optional)
      Initial value, before changes
- `@escape-key`
  Emitted when ESC key is pressed; Does not get emitted if Menu is 'persistent' or it has 'no-esc-dismiss' set

### Scoped Slots

- `#default`
  Used for injecting the form component; Do NOT destructure it
  Scope:
    - `initialValue` (any, optional)
      Initial value
      Examples: `0.241`, `'Text'`
    - `value` (any, optional)
      Current value
      Examples: `0.241`, `'Text'`
    - `validate` (Function, optional)
      Function that checks if the value is valid
      Function signature: `(value: any) => boolean`
      Params:
        - `value` (any, required)
          Value to be checked
          Examples: `0`, `'Changed text'`
      Returns: `boolean`
        Checked value is valid or not
    - `set` (Function, optional)
      Function that sets the value and closes the popup
    - `cancel` (Function, optional)
      Function that cancels the editing and reverts the value to the initialValue
    - `updatePosition` (Function, optional)
      There are some custom scenarios for which Quasar cannot automatically reposition the component without significant performance drawbacks so the optimal solution is for you to call this method when you need it

