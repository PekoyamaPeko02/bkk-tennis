## QScrollObserver API

### Props

- `debounce` (string | number, optional)
  Debounce amount (in milliseconds)
  Examples: `0`, `'530'`
- `axis` (string, optional), default `'vertical'`
  Axis on which to detect changes
  Accepts: `'both'`, `'vertical'`, `'horizontal'`
- `scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector, DOM element or Vue component reference (standing for its root element) to be used as a custom scroll container instead of the auto detected one
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`

### Methods

- `trigger(immediately?: boolean): void`
  Emit a 'scroll' event
  Params:
    - `immediately` (boolean, optional)
      Skip over the debounce amount
- `getPosition(): void`
  Get current scroll details under the form of an Object: { position, direction, directionChanged, inflectionPoint }

### Events

- `@scroll`
  Emitted when scroll position changes
  Params:
    - `details` (object, optional)
      Scroll details
      Object shape:
        - `position` (object, required)
          Scroll offset (from top and left)
          Object shape:
            - `top` (number, required)
              Scroll offset from top (vertical)
            - `left` (number, required)
              Scroll offset from left (horizontal)
        - `direction` (string, required)
          Direction of scroll
          Accepts: `'up'`, `'down'`, `'left'`, `'right'`
        - `directionChanged` (boolean, required)
          Has scroll direction changed since event was last emitted?
        - `delta` (object, required)
          Delta of distance (in pixels) since event was last emitted
          Object shape:
            - `top` (number, required)
              Vertical delta distance since event was last emitted
            - `left` (number, required)
              Horizontal delta distance since event was last emitted
        - `inflectionPoint` (object, required)
          Last scroll offset where scroll direction has changed
          Object shape:
            - `top` (number, required)
              Scroll offset from top (vertical)
            - `left` (number, required)
              Scroll offset from left (horizontal)

