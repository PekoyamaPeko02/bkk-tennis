## QMarkupTable API

### Props

- `dense` (boolean, optional)
  Dense mode; occupies less space
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow)
- `bordered` (boolean, optional)
  Applies a default border to the component
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `separator` (string, optional), default `'horizontal'`
  Use a separator/border between rows, columns or all cells
  Accepts: `'horizontal'`, `'vertical'`, `'cell'`, `'none'`
  Examples: `'cell'`
- `wrap-cells` (boolean, optional)
  Wrap text within table cells

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

