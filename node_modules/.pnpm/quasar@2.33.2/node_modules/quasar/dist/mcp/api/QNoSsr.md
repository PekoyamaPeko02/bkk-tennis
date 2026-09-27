## QNoSsr API

### Props

- `tag` (string, optional), default `'div'`
  HTML tag to use
  Examples: `'div'`, `'span'`, `'blockquote'`
- `placeholder` (string, optional)
  Text to display on server-side render (unless using 'placeholder' slot)
  Examples: `'This is server-side only'`

### Slots

- `#default`
  Default slot is used to render content on client-side
- `#placeholder`
  Slot used as placeholder on server-side render, which gets replaced by the default slot on client-side; overrides 'placeholder' prop

