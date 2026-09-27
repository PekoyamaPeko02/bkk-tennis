## Screen API

### Props

- `width` (number, optional, reactive)
  Screen width (in pixels)
  Examples: `452`
- `height` (number, optional, reactive)
  Screen height (in pixels)
  Examples: `721`
- `name` (string, optional, reactive)
  Tells current window breakpoint
  Accepts: `'xs'`, `'sm'`, `'md'`, `'lg'`, `'xl'`
- `sizes` (object, optional, reactive)
  Breakpoints (in pixels)
  Examples:
    - `{ sm: 600, md: 1024, lg: 1440, xl: 1920 }`
  Object shape:
    - `sm` (number, optional)
      Breakpoint width size (minimum size)
    - `md` (number, optional)
      Breakpoint width size (minimum size)
    - `lg` (number, optional)
      Breakpoint width size (minimum size)
    - `xl` (number, optional)
      Breakpoint width size (minimum size)
- `lt` (object, optional, reactive)
  Tells if current screen width is lower than breakpoint-name
  Examples:
    - `{ sm: false, md: true, lg: true, xl: true }`
  Object shape:
    - `sm` (boolean, optional)
      Is current screen width lower than this breakpoint's lowest limit?
    - `md` (boolean, optional)
      Is current screen width lower than this breakpoint's lowest limit?
    - `lg` (boolean, optional)
      Is current screen width lower than this breakpoint's lowest limit?
    - `xl` (boolean, optional)
      Is current screen width lower than this breakpoint's lowest limit?
- `gt` (object, optional, reactive)
  Tells if current screen width is greater than breakpoint-name
  Examples:
    - `{ xs: true, sm: true, md: false, lg: false, xl: false }`
  Object shape:
    - `xs` (boolean, optional)
      Is current screen width greater than this breakpoint's max limit?
    - `sm` (boolean, optional)
      Is current screen width greater than this breakpoint's max limit?
    - `md` (boolean, optional)
      Is current screen width greater than this breakpoint's max limit?
    - `lg` (boolean, optional)
      Is current screen width greater than this breakpoint's max limit?
- `xs` (boolean, optional, reactive)
  Current screen width fits exactly 'xs' breakpoint
- `sm` (boolean, optional, reactive)
  Current screen width fits exactly 'sm' breakpoint
- `md` (boolean, optional, reactive)
  Current screen width fits exactly 'md' breakpoint
- `lg` (boolean, optional, reactive)
  Current screen width fits exactly 'lg' breakpoint
- `xl` (boolean, optional, reactive)
  Current screen width fits exactly 'xl' breakpoint

### Methods

- `setSizes(breakpoints: object): void`
  Override default breakpoint sizes
  Params:
    - `breakpoints` (object, required)
      Pick what you want to override
      Object shape:
        - `sm` (number, optional)
          Breakpoint width size (minimum size)
        - `md` (number, optional)
          Breakpoint width size (minimum size)
        - `lg` (number, optional)
          Breakpoint width size (minimum size)
        - `xl` (number, optional)
          Breakpoint width size (minimum size)
- `setDebounce(amount: number): void`
  Debounce update of all props when screen width/height changes
  Params:
    - `amount` (number, required)
      Amount in milliseconds

### Vue Injection

Accessible via `$q.screen` (e.g., `this.$q.screen` in Options API or `useQuasar().screen` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.screen` (object)

- `bodyClasses` (boolean, optional)
  Whether to apply CSS classes for the current window breakpoint to the body element

