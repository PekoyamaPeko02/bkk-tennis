## QTh API

### Props

- `props` (object, optional)
  QTable's header column scoped slot property
  Examples: `:props="props"`
- `col-name` (string, optional) *(added v2.27)*
  The 'name' of the column this header cell belongs to; Overrides relying on the Vue 'key' attribute, which cannot cross a wrapper component; Only meaningful when used in QTable's 'header' slot
  Examples: `col-name="desc"`
- `auto-width` (boolean, optional)
  Tries to shrink header column width size; Useful for columns with a checkbox/radio/toggle

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

