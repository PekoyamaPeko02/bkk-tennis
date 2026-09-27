## QSkeleton API

### Props

- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `type` (string, optional), default `'rect'`
  Type of skeleton placeholder
  Accepts: `'text'`, `'rect'`, `'circle'`, `'QBtn'`, `'QBadge'`, `'QChip'`, `'QToolbar'`, `'QCheckbox'`, `'QRadio'`, `'QToggle'`, `'QSlider'`, `'QRange'`, `'QInput'`, `'QAvatar'`
- `animation` (string, optional), default `'wave'`
  The animation effect of the skeleton placeholder
  Accepts: `'wave'`, `'pulse'`, `'pulse-x'`, `'pulse-y'`, `'fade'`, `'blink'`, `'none'`
- `animation-speed` (string | number, optional), default `1500`
  Animation speed (in milliseconds, without unit)
  Examples: `500`, `'1200'`
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `bordered` (boolean, optional)
  Applies a default border to the component
- `size` (string, optional)
  Size in CSS units, including unit name; Overrides 'height' and 'width' props and applies the value to both height and width
  Examples: `'16px'`, `'2rem'`
- `width` (string, optional)
  Width in CSS units, including unit name; Apply custom width; Use this prop or through CSS; Overridden by 'size' prop if used
  Examples: `'16px'`, `'2rem'`
- `height` (string, optional)
  Height in CSS units, including unit name; Apply custom height; Use this prop or through CSS; Overridden by 'size' prop if used
  Examples: `'16px'`, `'2rem'`
- `tag` (string, optional), default `'div'`
  HTML tag to use
  Examples: `'div'`, `'span'`

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

