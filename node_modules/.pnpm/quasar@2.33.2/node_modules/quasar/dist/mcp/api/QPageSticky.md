## QPageSticky API

### Props

- `position` (string, optional), default `'bottom-right'`
  Page side/corner to stick to
  Accepts: `'top-right'`, `'top-left'`, `'bottom-right'`, `'bottom-left'`, `'top'`, `'right'`, `'bottom'`, `'left'`
- `offset` (any[], optional)
  An array of two numbers to offset the component horizontally and vertically in pixels
  Examples:
    - `[8, 8]`
    - `[5, 10]`
- `expand` (boolean, optional)
  By default the component shrinks to content's size; By using this prop you make the component fully expand horizontally or vertically, based on 'position' prop

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

