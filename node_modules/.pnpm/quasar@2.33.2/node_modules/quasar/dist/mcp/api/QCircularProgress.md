## QCircularProgress API

### Props

- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `value` (number, optional), default `0`
  Current progress (must be between min/max)
- `min` (number, optional), default `0`
  Minimum value defining 'no progress' (must be lower than 'max')
- `max` (number, optional), default `100`
  Maximum value defining 100% progress made (must be higher than 'min')
- `color` (string, optional)
  Color name for the arc progress from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `center-color` (string, optional)
  Color name for the center part of the component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `track-color` (string, optional)
  Color name for the track of the component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `font-size` (string, optional)
  Size of text in CSS units, including unit name. Suggestion: use 'em' units to sync with component size
  Examples: `'1em'`, `'16px'`, `'2rem'`
- `rounded` (boolean, optional)
  Rounding the arc of progress
- `thickness` (number, optional), default `0.2`
  Thickness of progress arc as a ratio (0.0 < x < 1.0) of component size
- `angle` (number, optional), default `0`
  Angle to rotate progress arc by
- `indeterminate` (boolean, optional)
  Put component into 'indeterminate' state; Ignores 'value' prop
- `show-value` (boolean, optional)
  Enables the default slot and uses it (if available), otherwise it displays the 'value' prop as text; Make sure the text has enough space to be displayed inside the component
- `reverse` (boolean, optional)
  Reverses the direction of progress; Only for determined state
- `instant-feedback` (boolean, optional)
  No animation when model changes
- `animation-speed` (string | number, optional), default `600`
  Animation speed (in milliseconds, without unit)
  Examples: `500`, `'1200'`

### Slots

- `#default`
  Used for component content only if 'show-value' prop is set; Make sure the content has enough space to be displayed inside the component

