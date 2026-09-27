## LoadingBar API

### Props

- `isActive` (boolean, optional, reactive)
  Is LoadingBar active?

### Methods

- `setDefaults(props: object): void`
  Set the inner QAjaxBar's props
  Params:
    - `props` (object, required)
      QAjaxBar component props
      Examples:
        - `{ position: 'bottom', reverse: true }`
- `start(speed?: number): void`
  Notify bar you are waiting for a new process to finish
  Params:
    - `speed` (number, optional), default `300`
      Delay (in milliseconds) between progress auto-increments; If delay is 0 then it disables auto-incrementing
- `stop(): void`
  Notify bar that one process you were waiting has finished
- `increment(amount?: number): void`
  Manually trigger a bar progress increment
  Params:
    - `amount` (number, optional)
      Amount (0 < x <= 100) to increment with

### Vue Injection

Accessible via `$q.loadingBar` (e.g., `this.$q.loadingBar` in Options API or `useQuasar().loadingBar` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.loadingBar` (object)

QAjaxBar component props, EXCEPT for 'hijack-filter' in quasar.config file (if using Quasar CLI)

Examples:
  - `{ position: 'bottom', reverse: true }`

