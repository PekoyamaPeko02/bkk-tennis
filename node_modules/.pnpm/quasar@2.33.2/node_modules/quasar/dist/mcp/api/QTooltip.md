## QTooltip API

### Props

- `transition-show` (string, optional), default `'jump-down'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional), default `'jump-up'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `target` (boolean | string | Element, optional), default `true`
  Configure a target element to trigger component toggle; 'true' means it enables the parent DOM element, 'false' means it disables attaching events to any DOM elements; By using a String (CSS selector) or a DOM element it attaches the events to the specified DOM element (if it exists)
  Examples: `false`, `.my-parent`, `#target-id`, `$refs.target`
- `no-parent-event` (boolean, optional)
  Skips attaching events to the target DOM element (that trigger the element to get shown)
- `model-value` (boolean, optional), default `null`
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
- `max-height` (string, optional), default `null`
  The maximum height of the Tooltip; Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `max-width` (string, optional), default `null`
  The maximum width of the Tooltip; Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `anchor` (string, optional), default `'bottom middle'`
  Two values setting the starting position or anchor point of the Tooltip relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `self` (string, optional), default `'top middle'`
  Two values setting the Tooltip's own position relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `offset` (any[], optional), default `[14, 14]`
  An array of two numbers (in pixels) which expands the anchor element's bounding box outward horizontally and vertically; the Tooltip is then positioned against the expanded box, so the visible effect depends on the 'anchor'/'self' points in use
  Examples:
    - `[8, 8]`
    - `[5, 10]`
- `cursor-position` (boolean, optional) *(added v2.30)*
  Position the Tooltip at the pointer instead of relative to its target; the pointer must settle for a moment first, and the position is then frozen for as long as the Tooltip stays shown. Ignored when the Tooltip is shown through keyboard focus or its model, since neither reports a pointer position
- `delay` (number, optional), default `0`
  Configure Tooltip to appear with delay
- `hide-delay` (number, optional), default `0`
  Configure Tooltip to disappear with delay
- `persistent` (boolean, optional)
  Prevents Tooltip from auto-closing when app's route changes

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
  The Tooltip follows its target automatically (through any scrolling container) and this method only re-checks whether the intended placement still fits the viewport (which side the Tooltip opens to and its maximum size); call it for the scenarios Quasar cannot detect

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

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

