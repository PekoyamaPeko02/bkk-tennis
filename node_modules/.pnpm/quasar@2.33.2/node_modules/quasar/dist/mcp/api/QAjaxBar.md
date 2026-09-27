## QAjaxBar API

### Props

- `position` (string, optional), default `'top'`
  Position within window of where QAjaxBar should be displayed
  Accepts: `'top'`, `'right'`, `'bottom'`, `'left'`
- `size` (string, optional), default `'2px'`
  Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `reverse` (boolean, optional)
  Reverse direction of progress
- `skip-hijack` (boolean, optional)
  Skip Ajax hijacking (not a reactive prop)
- `hijack-filter` (Function, optional)
  Filter which URL should trigger start() + stop()
  Function signature: `(url?: string) => boolean`
  Params:
    - `url` (string, optional)
      The URL being triggered
      Examples: `'https://some.url/path'`
  Returns: `boolean`
    Should the URL received as param trigger start() + stop()?

### Methods

- `start(speed?: number): number`
  Notify bar you are waiting for a new process to finish
  Params:
    - `speed` (number, optional), default `300`
      Delay (in milliseconds) between progress auto-increments; If delay is 0 then it disables auto-incrementing
  Returns: `number`
    Number of active simultaneous sessions
- `increment(amount?: number): number`
  Manually trigger a bar progress increment
  Params:
    - `amount` (number, optional)
      Amount (0 < x <= 100) to increment with
  Returns: `number`
    Number of active simultaneous sessions
- `stop(): number`
  Notify bar that one process you were waiting has finished
  Returns: `number`
    Number of active simultaneous sessions

### Events

- `@start`
  Emitted when bar is triggered to appear
- `@stop`
  Emitted when bar has finished its job

