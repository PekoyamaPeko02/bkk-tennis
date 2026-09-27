## Scroll API

### Directive Value

- `value` (Function, optional)
  Function to call when scrolling occurs (use undefined to disable)
  Function signature: `(verticalScrollPosition?: number, horizontalScrollPosition?: number) => void`
  Examples:
    - `(verticalScrollPosition, horizontalScrollPosition) => { /* ... */ }`
  Params:
    - `verticalScrollPosition` (number, optional)
      Scroll offset from top
    - `horizontalScrollPosition` (number, optional)
      Scroll offset from left

