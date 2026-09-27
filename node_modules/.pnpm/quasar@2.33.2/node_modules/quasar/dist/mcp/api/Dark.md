## Dark API

### Props

- `isActive` (boolean, optional, reactive)
  Is Dark mode active?
- `mode` (boolean | string, optional, reactive)
  Dark mode configuration (not status)
  Accepts: `'auto'`, `true`, `false`

### Methods

- `set(status: boolean | string): void`
  Set dark mode status
  Params:
    - `status` (boolean | string, required)
      Dark mode status
      Accepts: `true`, `false`, `'auto'`
- `toggle(): void`
  Toggle dark mode status

### Vue Injection

Accessible via `$q.dark` (e.g., `this.$q.dark` in Options API or `useQuasar().dark` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.dark` (boolean | string)

"'auto'" uses the OS/browser preference (on SSR/SSG builds it is resolved by the client once it takes over, the server rendering in light mode). "true" forces dark mode. "false" forces light mode.

Accepts: `'auto'`, `true`, `false`

