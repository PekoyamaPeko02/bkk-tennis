## QSlideTransition API

### Props

- `appear` (boolean, optional)
  If set to true, the transition will be applied on the initial render.
- `duration` (number, optional), default `300`
  Duration (in milliseconds) of the slide animation; use 0 to disable the animation and toggle instantly.

### Events

- `@show`
  Emitted when component show animation is finished
- `@hide`
  Emitted when component hide animation is finished

### Slots

- `#default`
  This is where content goes

