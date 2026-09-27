## QFab API

### Props

- `hover` (boolean, optional) *(added v2.27)*
  Also opens the FAB when the pointer hovers it and closes it when the pointer leaves both the main button and the actions; Click/tap and keyboard interactions keep toggling as usual (touch devices fall back to them), so activating the main button closes a hover-shown FAB, unless the actions are still animating into view (which keeps a single move-and-click gesture from closing what it just opened)
- `hover-delay` (number, optional), default `0` *(added v2.27)*
  Delay (in milliseconds) between the pointer entering the FAB and the actions showing up; Requires the 'hover' prop
- `hover-hide-delay` (number, optional), default `150` *(added v2.27)*
  Grace period (in milliseconds) in which the pointer can re-enter the FAB (main button or actions) before it gets closed; Requires the 'hover' prop
- `type` (string, optional), default `'a'`
  Define the button HTML DOM type
  Accepts: `'a'`, `'submit'`, `'button'`, `'reset'`
- `outline` (boolean, optional)
  Use 'outline' design for Fab button
- `push` (boolean, optional)
  Use 'push' design for Fab button
- `flat` (boolean, optional)
  Use 'flat' design for Fab button
- `unelevated` (boolean, optional)
  Remove shadow
- `padding` (string, optional)
  Apply custom padding (vertical [horizontal]); Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl); Also removes the min width and height when set
  Examples:
    - `'16px'`
    - `'10px 5px'`
    - `'2rem'`
    - `'xs'`
    - `'md lg'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `glossy` (boolean, optional)
  Apply the glossy effect over the button
- `external-label` (boolean, optional)
  Display label besides the FABs, as external content
- `label` (string | number, optional), default `''`
  The label that will be shown when Fab is extended
  Examples: `'Button Label'`
- `label-position` (string, optional), default `'right'`
  Position of the label around the icon
  Accepts: `'top'`, `'right'`, `'bottom'`, `'left'`
- `hide-label` (boolean, optional), default `null`
  Hide the label; Useful for animation purposes where you toggle the visibility of the label
- `label-class` (string | any[] | object, optional)
  Class definitions to be attributed to the label container
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `label-style` (string | any[] | object, optional)
  Style definitions to be attributed to the label container
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `square` (boolean, optional)
  Apply a rectangle aspect to the FAB
- `disable` (boolean, optional)
  Put component in disabled mode
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `model-value` (boolean, optional), default `null`
  Controls state of fab actions (showing/hidden); Works best with v-model directive, otherwise use along listening to 'update:modelValue' event
  Examples: `v-model="state"`
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `active-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `hide-icon` (boolean, optional)
  Hide the icon (don't use any)
- `direction` (string, optional), default `'right'`
  Direction to expand Fab Actions to
  Accepts: `'up'`, `'right'`, `'down'`, `'left'`
- `vertical-actions-align` (string, optional), default `'center'`
  The side of the Fab where Fab Actions will expand (only when direction is 'up' or 'down')
  Accepts: `'left'`, `'center'`, `'right'`
- `stagger` (number, optional), default `40` *(added v2.30)*
  Number of milliseconds between the show/hide animation of one Fab Action and the next one; The cascade runs outwards from the main button when opening and back towards it when closing; Use 0 to animate all Fab Actions at once
- `persistent` (boolean, optional)
  By default, Fab Actions are hidden when user navigates to another route and this prop disables this behavior

### Methods

- `show(evt?: Event): void`
  Expands fab actions list
  Params:
    - `evt` (Event, optional)
      JS event object
- `hide(evt?: Event): void`
  Collapses fab actions list
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
  Emitted when fab actions are shown/hidden; Captured by v-model directive
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
  This is where QFabActions may go into
- `#tooltip`
  Slot specifically designed for a QTooltip

### Scoped Slots

- `#icon`
  Slot for icon shown when FAB is closed; Suggestion: QIcon
  Scope:
    - `opened` (boolean, optional)
      FAB is opened
- `#active-icon`
  Slot for icon shown when FAB is opened; Suggestion: QIcon
  Scope:
    - `opened` (boolean, optional)
      FAB is opened
- `#label`
  Slot for label
  Scope:
    - `opened` (boolean, optional)
      FAB is opened

