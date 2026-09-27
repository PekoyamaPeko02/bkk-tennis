## TouchPan API

### Directive Value

- `value` (Function, optional)
  Handler for panning (use undefined to disable)
  Function signature: `(details?: object) => void`
  Examples:
    - `({ evt, touch, mouse, position, direction, isFirst, isFinal, distance, offset, delta }) => { /* ... */ }`
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
        - `position` (object, optional)
          Event Position Object
          Object shape:
            - `top` (number, optional)
              Vertical offset from top of window
            - `left` (number, optional)
              Horizontal offset from left of window
        - `direction` (string, optional)
          Direction of movement
          Accepts: `'up'`, `'right'`, `'down'`, `'left'`
        - `isFirst` (boolean, optional)
          Is first time the handler is called since movement started
        - `isFinal` (boolean, optional)
          Is last time the handler is called since movement ended
        - `duration` (number, optional)
          How long it took to trigger the event (in milliseconds)
        - `distance` (object, optional)
          Absolute distance (in pixels) since movement started from initial point
          Object shape:
            - `x` (number, optional)
              Absolute distance horizontally
            - `y` (number, optional)
              Absolute distance vertically
        - `offset` (object, optional)
          Distance (in pixels) since movement started from initial point
          Object shape:
            - `x` (number, optional)
              Distance horizontally
              Examples: `-231`, `110`
            - `y` (number, optional)
              Distance vertically
              Examples: `-231`, `110`
        - `delta` (object, optional)
          Delta of distance (in pixels) since handler was called last time
          Object shape:
            - `x` (number, optional)
              Distance horizontally
            - `y` (number, optional)
              Distance vertically

### Directive Modifiers

- `stop` (boolean, optional)
  Stop event propagation for touch events
- `prevent` (boolean, optional)
  Calls event.preventDefault() for touch events
- `capture` (boolean, optional)
  Use capture for touchstart event
- `mouse` (boolean, optional)
  Listen for mouse events too
- `mouseCapture` (boolean, optional)
  Use capture for mousedown event
- `mouseAllDir` (boolean, optional)
  Ignore initial mouse move direction (do not abort if the first mouse move is in an unaccepted direction)
- `preserveCursor` (boolean, optional)
  Prevent the mouse cursor from automatically displaying as grabbing when panning
- `horizontal` (boolean, optional)
  Catch horizontal (left/right) movement
- `vertical` (boolean, optional)
  Catch vertical (up/down) movement
- `up` (boolean, optional)
  Catch panning to up
- `right` (boolean, optional)
  Catch panning to right
- `down` (boolean, optional)
  Catch panning to down
- `left` (boolean, optional)
  Catch panning to left

