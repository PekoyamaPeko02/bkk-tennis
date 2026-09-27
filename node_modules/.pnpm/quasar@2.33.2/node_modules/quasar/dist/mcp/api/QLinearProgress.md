## QLinearProgress API

### Props

- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `value` (number, optional), default `0`
  Progress value (0.0 < x < 1.0)
- `buffer` (number, optional)
  Optional buffer value (0.0 < x < 1.0)
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `track-color` (string, optional)
  Color name for component's track from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `reverse` (boolean, optional)
  Reverse direction of progress (has no effect for query behaviour)
- `stripe` (boolean, optional)
  Draw stripes; For determinate state only (for performance reasons)
- `indeterminate` (boolean, optional)
  Put component into indeterminate mode
- `query` (boolean, optional)
  Put component into query mode
- `rounded` (boolean, optional)
  Applies a small standard border-radius for a squared shape of the component
- `instant-feedback` (boolean, optional)
  No transition when model changes
- `animation-speed` (string | number, optional), default `2100`
  Animation speed (in milliseconds, without unit)
  Examples: `500`, `'1200'`

### Slots

- `#default`
  Suggestion: QTooltip

