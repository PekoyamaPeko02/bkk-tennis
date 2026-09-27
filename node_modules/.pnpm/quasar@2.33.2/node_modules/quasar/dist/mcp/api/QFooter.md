## QFooter API

### Props

- `model-value` (boolean, optional, syncable), default `true`
  Model of the component defining if it is shown or hidden to the user; Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
  Examples: `v-model="footerState"`
- `bordered` (boolean, optional)
  Applies a default border to the component
- `reveal` (boolean, optional)
  Enable 'reveal' mode; Takes into account user scroll to temporarily show/hide footer
- `elevated` (boolean, optional)
  Adds a default shadow to the footer
- `height-hint` (number | string, optional), default `50`
  When using SSR/SSG, you can optionally hint of the height (in pixels) of the QFooter

### Events

- `@reveal`
  Emitted when 'reveal' state gets changed
  Params:
    - `value` (boolean, optional)
      New 'reveal' state

### Slots

- `#default`
  Default slot in the devland unslotted content of the component; Suggestion: QToolbar

