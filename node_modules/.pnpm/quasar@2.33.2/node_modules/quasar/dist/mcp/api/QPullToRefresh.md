## QPullToRefresh API

### Props

- `color` (string, optional)
  Color name for the icon from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `bg-color` (string, optional)
  Color name for background of the icon container from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `icon` (string, optional)
  Icon to display when refreshing the content
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `no-mouse` (boolean, optional)
  Don't listen for mouse events
- `side` (string, optional), default `'top'` *(added v2.30)*
  Side of the content the pull starts from; the refresh is triggered by pulling from that edge towards the inside of the content, while the scroll target is scrolled to that edge; 'bottom' suits messenger-styled content, where the newest entries sit at the bottom
  Accepts: `'top'`, `'bottom'`, `'left'`, `'right'`
- `disable` (boolean, optional)
  Put component in disabled mode
- `scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector, DOM element or Vue component reference (standing for its root element) to be used as a custom scroll container instead of the auto detected one
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`

### Methods

- `trigger(): void`
  Triggers a refresh
- `updateScrollTarget(): void`
  Updates the scroll target; Useful when the parent elements change so that the scrolling target also changes

### Events

- `@refresh`
  Called whenever a refresh is triggered; at this time, your function should load more data
  Params:
    - `done` (Function, optional)
      Call the done() function when your data has been refreshed

### Slots

- `#default`
  Content (area controlled by the component) goes here

