## QPageScroller API

### Props

- `position` (string, optional), default `'bottom-right'`
  Page side/corner to stick to
  Accepts: `'top-right'`, `'top-left'`, `'bottom-right'`, `'bottom-left'`, `'top'`, `'right'`, `'bottom'`, `'left'`
- `offset` (any[], optional), default `[18, 18]`
  An array of two numbers to offset the component horizontally and vertically in pixels
  Examples:
    - `[8, 8]`
    - `[5, 10]`
- `expand` (boolean, optional)
  By default the component shrinks to content's size; By using this prop you make the component fully expand horizontally or vertically, based on 'position' prop
- `scroll-offset` (number, optional), default `1000`
  Scroll offset (in pixels) from which point the component is shown on page; Measured from the top of the page (or from the bottom if in 'reverse' mode)
- `reverse` (boolean, optional)
  Work in reverse (shows when scrolling to the top of the page and scrolls to bottom when triggered)
- `duration` (number, optional), default `300`
  Duration (in milliseconds) of the scrolling until it reaches its target

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

