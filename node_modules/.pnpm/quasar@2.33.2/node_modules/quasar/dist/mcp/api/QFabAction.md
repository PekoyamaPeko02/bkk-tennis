## QFabAction API

### Props

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
- `hide-label` (boolean, optional)
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
- `icon` (string, optional), default `''`
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `anchor` (string, optional)
  How to align the Fab Action relative to Fab expand side; By default it uses the align specified in QFab
  Accepts: `'start'`, `'center'`, `'end'`
- `to` (string | object, optional)
  Equivalent to Vue Router <router-link> 'to' property
  Examples: `'/home/dashboard'`, `{ name: 'my-route-name' }`
- `replace` (boolean, optional)
  Equivalent to Vue Router <router-link> 'replace' property

### Methods

- `click(evt?: Event): void`
  Emulate click on QFabAction
  Params:
    - `evt` (Event, optional)
      JS event object

### Events

- `@click`
  Emitted when user clicks/taps on the component
  Params:
    - `evt` (Event, required)
      JS event object

### Slots

- `#default`
  Suggestion for this slot: QTooltip
- `#icon`
  Slot for icon; Suggestion: QIcon
- `#label`
  Slot for label

