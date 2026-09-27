## QBadge API

### Props

- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `floating` (boolean, optional)
  Tell QBadge if it should float to the top right side of the relative positioned parent element or not
- `transparent` (boolean, optional)
  Applies a 0.8 opacity; Useful especially for floating QBadge
- `multi-line` (boolean, optional)
  Content can wrap to multiple lines
- `label` (string | number, optional)
  Badge's content as string; overrides default slot if specified
  Examples: `'John Doe'`, `22`
- `align` (string, optional)
  Sets vertical-align CSS prop
  Accepts: `'top'`, `'middle'`, `'bottom'`
- `outline` (boolean, optional)
  Use 'outline' design (colored text and borders only)
- `rounded` (boolean, optional)
  Makes a rounded shaped badge

### Slots

- `#default`
  This is where QBadge content goes, if not using 'label' property

