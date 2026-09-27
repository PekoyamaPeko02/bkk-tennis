## TouchHold API

### Directive Value

- `value` (Function, optional)
  Function to call after user has hold touch/click for the specified amount of time (use undefined to disable)
  Function signature: `(details?: object) => void`
  Examples:
    - `({ evt, touch, mouse, position, duration }) => { /* ... */ }`
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
        - `duration` (number, optional)
          How long it took to trigger the event (in milliseconds)

### Directive Argument

- `arg` (string, optional), default `'600:5:7'`
  x:y:z, where x is the amount of time to wait (in milliseconds), y is the touch event sensitivity (in pixels) and z is the mouse event sensitivity (in pixels)
  Examples: `v-touch-hold:400="fnToCall"`, `v-touch-hold:400:15="fnToCall"`, `v-touch-hold:400:10:10="fnToCall"`

### Directive Modifiers

- `capture` (boolean, optional)
  Use capture for touchstart event
- `mouse` (boolean, optional)
  Listen for mouse events too
- `mouseCapture` (boolean, optional)
  Use capture for mousedown event

