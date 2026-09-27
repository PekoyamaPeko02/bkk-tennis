## QTimeline API

### Props

- `color` (string, optional), default `'primary'`
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `side` (string, optional), default `'right'`
  Side to place the timeline entries in dense and comfortable layout; For loose layout it gets overridden by QTimelineEntry side prop
  Accepts: `'left'`, `'right'`
- `layout` (string, optional), default `'dense'`
  Layout of the timeline. Dense keeps content and labels on one side. Comfortable keeps content on one side and labels on the opposite side. Loose puts content on both sides.
  Accepts: `'dense'`, `'comfortable'`, `'loose'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Slots

- `#default`
  Used for content of component

