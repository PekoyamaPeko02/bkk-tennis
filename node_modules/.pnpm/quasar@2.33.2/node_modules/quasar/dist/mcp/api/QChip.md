## QChip API

### Props

- `dense` (boolean, optional)
  Dense mode; occupies less space
- `size` (string, optional)
  QChip size name or a CSS unit including unit name
  Examples:
    - `'xs'`
    - `'sm'`
    - `'md'`
    - `'lg'`
    - `'xl'`
    - `'25px'`
    - `'2rem'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-right` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-remove` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-selected` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `label` (string | number, optional)
  Chip's content as string; overrides default slot if specified
  Examples: `'John Doe'`, `'Book'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `model-value` (boolean, optional, syncable), default `true`
  Model of the component determining if QChip should be rendered or not
- `selected` (boolean, optional, syncable), default `null`
  Model for QChip if it's selected or not
  Required to be used with v-model.
  Examples: `v-model:selected="myState"`
- `square` (boolean, optional)
  Sets a low value for border-radius instead of the default one, making it close to a square
- `outline` (boolean, optional)
  Display using the 'outline' design
- `clickable` (boolean, optional), default `null`
  Is QChip clickable? If it's the case, then it will add hover effects and emit 'click' events; when not set, the chip is clickable if a 'click' listener is attached (a 'selected' model always makes it clickable)
- `removable` (boolean, optional)
  If set, then it displays a 'remove' icon that when clicked the QChip emits 'remove' event
- `ripple` (boolean | object, optional), default `true`
  Configure material ripple (disable it by setting it to 'false' or supply a config object)
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`
- `remove-aria-label` (string, optional)
  aria-label to be used on the remove icon
  Examples: `'Remove item'`
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `disable` (boolean, optional)
  Put component in disabled mode

### Events

- `@click`
  Emitted on QChip click when the chip is clickable
  Params:
    - `evt` (Event, optional)
      JS event object
- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (any, required)
      New model value
- `@update:selected`
  Used by Vue on 'v-model:selected' for updating its value
  Params:
    - `state` (boolean, optional)
      Selected state
- `@remove`
  Works along with 'value' and 'removable' prop. Emitted when toggling rendering state of the QChip
  Params:
    - `state` (boolean, optional)
      Render state (render or not)

### Slots

- `#default`
  This is where QChip content goes, if not using 'label' property

