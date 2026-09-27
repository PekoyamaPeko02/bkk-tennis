## QInfiniteScroll API

### Props

- `offset` (number, optional), default `500`
  Distance (pixels) from the end of the content (the top of it in reverse mode) to the visible area of the scroll target at which loading more content starts, in advance
- `debounce` (string | number, optional), default `100`
  Debounce amount (in milliseconds)
- `initial-index` (number, optional), default `0`
  Initialize the pagination index (used for the @load event)
- `scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector, DOM element or Vue component reference (standing for its root element) to be used as a custom scroll container instead of the auto detected one
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`
- `disable` (boolean, optional)
  Put component in disabled mode
- `reverse` (boolean, optional)
  Scroll area should behave like a messenger - starting scrolled to bottom and loading when reaching the top

### Methods

- `poll(): void`
  Checks whether the end of the content is within offset of the visible area and loads more content if necessary
- `trigger(): void`
  Tells Infinite Scroll to load more content, regardless of the scroll position
- `reset(): void`
  Resets calling index to 0
- `stop(): void`
  Stops working, regardless of scroll position
- `resume(): void`
  Starts working. Checks scroll position upon call and if trigger is hit, it loads more content
- `setIndex(newIndex: number): void`
  Overwrite the current pagination index
  Params:
    - `newIndex` (number, required)
      New pagination index
- `updateScrollTarget(): void`
  Updates the scroll target; Useful when the parent elements change so that the scrolling target also changes

### Events

- `@load`
  Emitted when Infinite Scroll needs to load more data
  Params:
    - `index` (number, optional)
      The index parameter can be used to make some sort of pagination on the content you load. It takes numeric values starting with 1 and incrementing with each call
    - `done` (Function, optional)
      Function to call when you made all necessary updates. DO NOT forget to call it otherwise your loading message will continue to be displayed
      Function signature: `(stop?: boolean) => void`
      Params:
        - `stop` (boolean, optional)
          Stops QInfiniteScroll if it's Boolean 'true'; Specify it in case there's nothing more to load

### Slots

- `#default`
  Default slot in the devland unslotted content of the component
- `#loading`
  Slot displaying something while loading content; Example: QSpinner

