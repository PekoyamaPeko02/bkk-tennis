## QSlideItem API

### Props

- `left-color` (string, optional)
  Color name for left-side background from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `right-color` (string, optional)
  Color name for right-side background from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `top-color` (string, optional)
  Color name for top-side background from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `bottom-color` (string, optional)
  Color name for bottom-side background from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Methods

- `reset(): void`
  Reset to initial state (not swiped to any side)

### Events

- `@left`
  Emitted when user finished sliding the item to the left
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `reset` (Function, required)
          When called, it resets the component to its initial non-slided state
- `@right`
  Emitted when user finished sliding the item to the right
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `reset` (Function, required)
          When called, it resets the component to its initial non-slided state
- `@top`
  Emitted when user finished sliding the item up
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `reset` (Function, required)
          When called, it resets the component to its initial non-slided state
- `@bottom`
  Emitted when user finished sliding the item down
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `reset` (Function, required)
          When called, it resets the component to its initial non-slided state
- `@slide`
  Emitted while user is sliding the item to one of the available sides
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `side` (string, required)
          Side to which sliding is taking effect
          Accepts: `'left'`, `'right'`, `'top'`, `'bottom'`
        - `ratio` (number, required)
          Ratio of how much of the required slide was performed (0 <= x <= 1)
        - `isReset` (boolean, required)
          Ratio has been reset
- `@action`
  Emitted when user finished sliding the item to either sides
  Params:
    - `details` (object, optional)
      Details
      Object shape:
        - `side` (string, required)
          Side to which sliding has taken effect
          Accepts: `'left'`, `'right'`, `'top'`, `'bottom'`
        - `reset` (Function, required)
          When called, it resets the component to its initial non-slided state

### Slots

- `#default`
  This is where item's sections go; Suggestion: QItemSection
- `#left`
  Left side content when sliding
- `#right`
  Right side content when sliding
- `#top`
  Top side content when sliding
- `#bottom`
  Bottom side content when sliding

