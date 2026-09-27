## AppFullscreen API

### Props

- `isCapable` (boolean, optional)
  Does browser support it?
- `isActive` (boolean, optional, reactive)
  Is Fullscreen active?
- `activeEl` (Element, optional, reactive)
  The DOM element used as root for fullscreen, otherwise 'null'
  Examples: `document.fullscreenElement`, `null`

### Methods

- `request(target?: Element): Promise<void>`
  Request going into Fullscreen (with optional target)
  Params:
    - `target` (Element, optional)
      Optional Element of target to request Fullscreen on
      Examples: `document.getElementById('example')`
  Returns: `Promise<void>`
    A Promise which is resolved when transitioned to fullscreen mode. It gets rejected with 'Not capable' if the browser is not capable, and with an Error object if something else went wrong.
    Examples: `request().then(response => { ... }).catch(err => { ... })`
- `exit(): Promise<void>`
  Request exiting out of Fullscreen mode
  Returns: `Promise<void>`
    A Promise which is resolved when exited out of fullscreen mode. It gets rejected with 'Not capable' if the browser is not capable, and with an Error object if something else went wrong.
    Examples: `exit().then(response => { ... }).catch(err => { ... })`
- `toggle(target?: Element): Promise<void>`
  Request toggling Fullscreen mode (with optional target if requesting going into Fullscreen only)
  Params:
    - `target` (Element, optional)
      Optional Element of target to request Fullscreen on
      Examples: `document.getElementById('example')`
  Returns: `Promise<void>`
    A Promise which is resolved when transitioned to / exited out of fullscreen mode. It gets rejected with 'Not capable' if the browser is not capable, and with an Error object if something else went wrong.
    Examples: `toggle().then(response => { ... }).catch(err => { ... })`

### Vue Injection

Accessible via `$q.fullscreen` (e.g., `this.$q.fullscreen` in Options API or `useQuasar().fullscreen` in Composition API).

