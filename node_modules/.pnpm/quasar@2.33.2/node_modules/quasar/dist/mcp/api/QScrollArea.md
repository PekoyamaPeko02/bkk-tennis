## QScrollArea API

### Props

- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `vertical-offset` (any[], optional), default `[0, 0]` *(added v2.17)*
  Adds [top, bottom] offset to vertical thumb
- `horizontal-offset` (any[], optional), default `[0, 0]` *(added v2.17)*
  Adds [left, right] offset to horizontal thumb
- `bar-style` (string | any[] | object, optional)
  Object with CSS properties and values for custom styling the scrollbars (both vertical and horizontal)
  Examples:
    - `{ borderRadius: '5px', background: 'red', opacity: 1 }`
- `vertical-bar-style` (string | any[] | object, optional)
  Object with CSS properties and values for custom styling the vertical scrollbar; Is applied on top of 'bar-style' prop
  Examples:
    - `{ right: '4px', borderRadius: '5px', background: 'red', width: '10px', opacity: 1 }`
- `horizontal-bar-style` (string | any[] | object, optional)
  Object with CSS properties and values for custom styling the horizontal scrollbar; Is applied on top of 'bar-style' prop
  Examples:
    - `{ bottom: '4px', borderRadius: '5px', background: 'red', height: '10px', opacity: 1 }`
- `thumb-style` (object, optional)
  Object with CSS properties and values for custom styling the thumb of scrollbars (both vertical and horizontal)
  Examples:
    - `{ right: '4px', borderRadius: '5px', background: 'red', width: '10px', opacity: 1 }`
- `vertical-thumb-style` (object, optional)
  Object with CSS properties and values for custom styling the thumb of the vertical scrollbar; Is applied on top of 'thumb-style' prop
  Examples:
    - `{ right: '4px', borderRadius: '5px', background: 'red', width: '10px', opacity: 1 }`
- `horizontal-thumb-style` (object, optional)
  Object with CSS properties and values for custom styling the thumb of the horizontal scrollbar; Is applied on top of 'thumb-style' prop
  Examples:
    - `{ bottom: '4px', borderRadius: '5px', background: 'red', height: '10px', opacity: 1 }`
- `content-style` (string | any[] | object, optional)
  Object with CSS properties and values for styling the container of QScrollArea
  Examples: `{ backgroundColor: '#C0C0C0' }`
- `content-active-style` (string | any[] | object, optional)
  Object with CSS properties and values for styling the container of QScrollArea when scroll area becomes active (is mouse hovered)
  Examples: `{ backgroundColor: 'white' }`
- `visible` (boolean, optional), default `null`
  Manually control the visibility of the scrollbar; Overrides default mouse over/leave behavior
- `delay` (number | string, optional), default `1000`
  When content changes, the scrollbar appears; this delay defines the amount of time (in milliseconds) before scrollbars disappear again (if component is not hovered)
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`

### Methods

- `getScrollTarget(): Element`
  Get the scrolling DOM element target
  Returns: `Element`
    DOM element upon which scrolling takes place
- `getScroll(): object`
  Get the current scroll information
  Returns: `object`
    Scroll information
    Object shape:
      - `verticalPosition` (number, required)
        Vertical scroll position (in px)
      - `verticalPercentage` (number, required)
        Vertical scroll percentage (0.0 <= x <= 1.0)
      - `verticalSize` (number, required)
        Vertical scroll size (in px)
      - `verticalContainerSize` (number, required)
        Height of the container (in px)
      - `verticalContainerInnerSize` (number, required) *(added v2.17)*
        Height of the container without the vertical offset (in px)
      - `horizontalPosition` (number, required)
        Horizontal scroll position (in px)
      - `horizontalPercentage` (number, required)
        Horizontal scroll percentage (0.0 <= x <= 1.0)
      - `horizontalSize` (number, required)
        Horizontal scroll size (in px)
      - `horizontalContainerSize` (number, required)
        Width of the container (in px)
      - `horizontalContainerInnerSize` (number, required) *(added v2.17)*
        Width of the container without the horizontal offset (in px)
- `getScrollPosition(): object`
  Get current scroll position
  Returns: `object`
    An object containing scroll position information
    Examples:
      - `{ top: 10, left: 0 }`
    Object shape:
      - `top` (number, required)
        Scroll offset from top (vertical)
      - `left` (number, required)
        Scroll offset from left (horizontal)
- `getScrollPercentage(): object`
  Get current scroll position in percentage (0.0 <= x <= 1.0)
  Returns: `object`
    An object containing scroll position information in percentage
    Examples:
      - `{ top: 0.212, left: 0 }`
    Object shape:
      - `top` (number, required)
        Scroll percentage (0.0 <= x <= 1.0) offset from top (vertical)
      - `left` (number, required)
        Scroll percentage (0.0 <= x <= 1.0) offset from left (horizontal)
- `setScrollPosition(axis: string, offset: number, duration?: number): void`
  Set scroll position to an offset; If a duration (in milliseconds) is specified then the scroll is animated
  Params:
    - `axis` (string, required)
      Scroll axis
      Accepts: `'vertical'`, `'horizontal'`
    - `offset` (number, required)
      Scroll position offset from top (in pixels)
    - `duration` (number, optional)
      Duration (in milliseconds) enabling animated scroll
- `setScrollPercentage(axis: string, offset: number, duration?: number): void`
  Set scroll position to a percentage (0.0 <= x <= 1.0) of the total scrolling size; If a duration (in milliseconds) is specified then the scroll is animated
  Params:
    - `axis` (string, required)
      Scroll axis
      Accepts: `'vertical'`, `'horizontal'`
    - `offset` (number, required)
      Scroll percentage (0.0 <= x <= 1.0) of the total scrolling size
    - `duration` (number, optional)
      Duration (in milliseconds) enabling animated scroll

### Events

- `@scroll`
  Emitted when scroll information changes (and listener is configured)
  Params:
    - `info` (object, optional)
      An object containing scroll information
      Object shape:
        - `ref` (ComponentInstance, required)
          Vue reference to the QScrollArea which triggered the event
        - `verticalPosition` (number, required)
          Vertical scroll position (in px)
        - `verticalPercentage` (number, required)
          Vertical scroll percentage (0.0 <= x <= 1.0)
        - `verticalSize` (number, required)
          Vertical scroll size (in px)
        - `verticalContainerSize` (number, required)
          Height of the container (in px)
        - `verticalContainerInnerSize` (number, required) *(added v2.17)*
          Height of the container without the vertical offset (in px)
        - `horizontalPosition` (number, required)
          Horizontal scroll position (in px)
        - `horizontalPercentage` (number, required)
          Horizontal scroll percentage (0.0 <= x <= 1.0)
        - `horizontalSize` (number, required)
          Horizontal scroll size (in px)
        - `horizontalContainerSize` (number, required)
          Width of the container (in px)
        - `horizontalContainerInnerSize` (number, required) *(added v2.17)*
          Width of the container without the horizontal offset (in px)

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

