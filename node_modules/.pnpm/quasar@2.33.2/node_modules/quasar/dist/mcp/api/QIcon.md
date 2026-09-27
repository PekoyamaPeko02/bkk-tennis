## QIcon API

### Props

- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `tag` (string, optional), default `'i'`
  HTML tag to render, unless no icon is supplied or it's an svg icon
  Examples: `'div'`, `'i'`
- `name` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `left` (boolean, optional)
  Useful if icon is on the left side of something: applies a standard margin on the right side of Icon
- `right` (boolean, optional)
  Useful if icon is on the right side of something: applies a standard margin on the left side of Icon

### Slots

- `#default`
  Suggestions: QTooltip or QMenu

