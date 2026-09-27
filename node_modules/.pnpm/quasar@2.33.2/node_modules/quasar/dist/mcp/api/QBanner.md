## QBanner API

### Props

- `inline-actions` (boolean, optional)
  Display actions on same row as content
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `rounded` (boolean, optional)
  Applies a small standard border-radius for a squared shape of the component
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Slots

- `#default`
  This is where Banner content goes
- `#avatar`
  Slot for displaying an avatar (suggestions: QIcon, QAvatar)
- `#action`
  Slot for Banner action (suggestions: QBtn)

