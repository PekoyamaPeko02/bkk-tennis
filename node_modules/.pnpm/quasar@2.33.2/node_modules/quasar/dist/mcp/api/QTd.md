## QTd API

### Props

- `props` (object, optional)
  QTable's column scoped slot property
  Examples: `:props="props"`
- `col-name` (string, optional) *(added v2.27)*
  The 'name' of the column this cell belongs to; Overrides relying on the Vue 'key' attribute, which cannot cross a wrapper component; Only meaningful when used in QTable's 'body' slot
  Examples: `col-name="desc"`
- `auto-width` (boolean, optional)
  Tries to shrink column width size; Useful for columns with a checkbox/radio/toggle
- `no-hover` (boolean, optional)
  Disable hover effect

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

