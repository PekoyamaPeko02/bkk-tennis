## TouchRepeat API

### Directive Value

- `value` (Function, optional)
  Handler for touch-repeat (use undefined to disable)
  Function signature: `(details?: object) => void`
  Examples:
    - `({ evt, touch, mouse, keyboard, position, keyCode, duration, repeatCount, startTime }) => { /* ... */ }`
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
        - `keyboard` (boolean, optional)
          Triggered by a keyboard event
        - `position` (object, optional)
          Event Position Object; Supplied ONLY if it's a touch or mouse event
          Object shape:
            - `top` (number, optional)
              Vertical offset from top of window
            - `left` (number, optional)
              Horizontal offset from left of window
        - `keyCode` (number, optional)
          Keycode; Supplied ONLY if it's a keyboard event
        - `duration` (number, optional)
          How long it took to trigger the event (in milliseconds)
        - `repeatCount` (number, optional)
          Handler called for nth time
        - `startTime` (number, optional)
          Unix timestamp of the moment when event started; Equivalent to Date.now()

### Directive Argument

- `arg` (string, optional), default `'0:600:300'`
  String of numbers (at least one number) separated by ':' which defines the amount of time to wait for 1st handler call, 2nd, 3rd and so on; All subsequent calls will use last value as time to wait until triggering
  Examples: `v-touch-repeat:0:400="fnToCall"`

### Directive Modifiers

- `capture` (boolean, optional)
  Use capture for touchstart event
- `mouse` (boolean, optional)
  Listen for mouse events too
- `mouseCapture` (boolean, optional)
  Use capture for mousedown event
- `keyCapture` (boolean, optional)
  Use capture for keydown event
- `esc` (boolean, optional)
  Catch ESC key
- `tab` (boolean, optional)
  Catch TAB key
- `enter` (boolean, optional)
  Catch ENTER key
- `space` (boolean, optional)
  Catch SPACE key
- `up` (boolean, optional)
  Catch UP arrow key
- `left` (boolean, optional)
  Catch LEFT arrow key
- `right` (boolean, optional)
  Catch RIGHT arrow key
- `down` (boolean, optional)
  Catch DOWN key
- `delete` (boolean, optional)
  Catch DELETE key
- `[keycode]` (number, optional)
  Key code to catch
  Examples: `v-touch-repeat.68="fnToCall"`

