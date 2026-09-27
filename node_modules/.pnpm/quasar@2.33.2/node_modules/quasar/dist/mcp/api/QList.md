## QList API

### Props

- `bordered` (boolean, optional)
  Applies a default border to the component
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `separator` (boolean, optional)
  Applies a separator between contained items
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `padding` (boolean, optional)
  Applies a material design-like padding on top and bottom
- `tag` (string, optional), default `'div'`
  HTML tag to use
  Examples: `'div'`, `'ul'`, `'ol'`
- `role` (string, optional) *(added v2.25)*
  Overrides the default 'list' ARIA role; Contained QItems derive their own default role from it: with 'menu'/'menubar' the actionable items become 'menuitem's, while any other value stops the non-actionable items from claiming 'listitem'
  Examples: `'menu'`, `'listbox'`, `'none'`

### Slots

- `#default`
  This is where the content goes; Suggestion: QItem, QExpansionItem, ...

