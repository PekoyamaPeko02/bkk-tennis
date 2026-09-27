## QTab API

### Props

- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `label` (number | string, optional)
  A number or string to label the tab
  Examples: `'Home'`
- `alert` (boolean | string, optional)
  Adds an alert symbol to the tab, notifying the user there are some updates; If its value is not a Boolean, then you can specify a color
  Examples: `'purple'`
- `alert-icon` (string, optional)
  Adds a floating icon to the tab, notifying the user there are some updates; It's displayed only if 'alert' is set; Can use the color specified by 'alert' prop
  Examples: `'alarm_on'`
- `name` (number | string, optional), default `a random UUID`
  Panel name
  Examples: `'home'`, `1`
- `no-caps` (boolean, optional)
  Turns off capitalizing all letters within the tab (which is the default)
- `content-class` (string, optional)
  Class definitions to be attributed to the content wrapper
  Examples: `'my-special-class'`
- `ripple` (boolean | object, optional), default `true`
  Configure material ripple (disable it by setting it to 'false' or supply a config object)
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `disable` (boolean, optional)
  Put component in disabled mode

### Slots

- `#default`
  Suggestion: QMenu, QTooltip

