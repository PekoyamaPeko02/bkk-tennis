## QLayout API

### Props

- `view` (string, optional), default `'hhh lpr fff'`
  Defines how your layout components (header/footer/drawer) should be placed on screen; See docs examples
  Examples: `'hHh lpR fFf'`
- `container` (boolean, optional)
  Containerize the layout which means it changes the default behavior of expanding to the whole window; Useful (but not limited to) for when using on a QDialog

### Events

- `@resize`
  Emitted when layout size (height, width) changes
  Params:
    - `size` (object, optional)
      New size
      Object shape:
        - `height` (number, required)
          Layout height
        - `width` (number, required)
          Layout width
- `@scroll`
  Emitted when user scrolls within layout
  Params:
    - `details` (object, optional)
      Scroll details
      Object shape:
        - `position` (number, required)
          Scroll offset from top (vertical)
        - `direction` (string, required)
          Direction of scroll
          Accepts: `'up'`, `'down'`
        - `directionChanged` (boolean, required)
          Has scroll direction changed since event was last emitted?
        - `delta` (number, required)
          Vertical delta distance since event was last emitted
        - `inflectionPoint` (number, required)
          Scroll offset from top (vertical)
- `@scroll-height`
  Emitted when the scroll size of layout changes
  Params:
    - `height` (number, optional)
      New scroll height of layout

### Slots

- `#default`
  Suggestion: QHeader, QFooter, QDrawer, QPageContainer

