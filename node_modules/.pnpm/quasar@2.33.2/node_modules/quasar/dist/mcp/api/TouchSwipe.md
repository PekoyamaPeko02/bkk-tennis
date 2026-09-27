## TouchSwipe API

### Directive Value

- `value` (Function, optional)
  Handler for swipe (use undefined to disable)
  Function signature: `(details?: object) => void`
  Examples:
    - `({ evt, touch, mouse, direction, duration, distance }) => { /* ... */ }`
  Params:
    - `details` (object, optional)
      Event details
      Object shape:
        - `evt` (Event, optional)
          Original JS event Object
        - `touch` (boolean, optional)
          Triggered by a touch event
        - `mouse` (boolean, optional)
          Triggered by a mouse event
        - `direction` (string, optional)
          Direction of movement
          Accepts: `'up'`, `'right'`, `'down'`, `'left'`
        - `duration` (number, optional)
          How long it took to trigger the event (in milliseconds)
        - `distance` (object, optional)
          Absolute distance (in pixels) since movement started from initial point
          Object shape:
            - `x` (number, optional)
              Absolute distance horizontally
            - `y` (number, optional)
              Absolute distance vertically

### Directive Argument

- `arg` (string, optional), default `'6e-2:6:50'`
  x:y:z, where x is minimum velocity (dist/time; please use float without a dot, example: 6e-2 which is equivalent to 6 * 10^-2 = 0.06), y is minimum distance on first move on mobile, z is minimum distance on desktop until deciding if it's a swipe indeed
  Examples: `v-touch-swipe:7e-2:10:100="fnToCall"`

### Directive Modifiers

- `capture` (boolean, optional)
  Use capture for touchstart event
- `mouse` (boolean, optional)
  Listen for mouse events too
- `mouseCapture` (boolean, optional)
  Use capture for mousedown event
- `horizontal` (boolean, optional)
  Catch horizontal (left/right) movement
- `vertical` (boolean, optional)
  Catch vertical (up/down) movement
- `up` (boolean, optional)
  Catch swipe to up
- `right` (boolean, optional)
  Catch swipe to right
- `down` (boolean, optional)
  Catch swipe to down
- `left` (boolean, optional)
  Catch swipe to left

