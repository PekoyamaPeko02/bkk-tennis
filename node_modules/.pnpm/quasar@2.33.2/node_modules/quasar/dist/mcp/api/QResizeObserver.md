## QResizeObserver API

### Props

- `debounce` (string | number, optional), default `100`
  Debounce amount (in milliseconds)
  Examples: `0`, `'530'`

### Methods

- `trigger(immediately?: boolean): void`
  Emit a 'resize' event
  Params:
    - `immediately` (boolean, optional)
      Skip over the debounce amount

### Events

- `@resize`
  Parent element has resized (outer width or height changed; padding and border included)
  Params:
    - `size` (object, optional)
      New size
      Object shape:
        - `height` (number, required)
          Layout height
        - `width` (number, required)
          Layout width

