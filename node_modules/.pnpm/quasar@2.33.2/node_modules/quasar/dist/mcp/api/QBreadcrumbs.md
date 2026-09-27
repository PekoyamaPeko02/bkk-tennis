## QBreadcrumbs API

### Props

- `separator` (string, optional), default `'/'`
  The string used to separate the breadcrumbs
  Examples: `'-'`, `'|'`, `'>'`
- `active-color` (string, optional), default `'primary'`
  The color of the active breadcrumb, which can be any color from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `gutter` (string, optional), default `'sm'`
  The gutter value allows you control over the space between the breadcrumb elements.
  Accepts: `'none'`, `'xs'`, `'sm'`, `'md'`, `'lg'`, `'xl'`
- `separator-color` (string, optional)
  The color used to color the separator, which can be any color from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `align` (string, optional), default `'left'`
  Specify how to align the breadcrumbs horizontally
  Accepts: `'left'`, `'center'`, `'right'`, `'between'`, `'around'`, `'evenly'`

### Slots

- `#default`
  Default slot in the devland unslotted content of the component
- `#separator`
  HTML or component you can slot in to separate the breadcrumbs

