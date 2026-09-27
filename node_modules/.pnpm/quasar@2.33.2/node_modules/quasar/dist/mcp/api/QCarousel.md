## QCarousel API

### Props

- `fullscreen` (boolean, optional, syncable)
  Fullscreen mode
  Required to be used with v-model.
  Examples: `v-model:fullscreen="isFullscreen"`
- `no-route-fullscreen-exit` (boolean, optional)
  Changing route app won't exit fullscreen
- `model-value` (any, required)
  Model of the component defining the current panel's name; If a Number is used, it does not define the panel's index, but rather the panel's name which can also be an Integer; Either use this property (along with a listener for 'update:model-value' event) OR use the v-model directive.
  Examples: `v-model="panelName"`
- `keep-alive` (boolean, optional)
  Equivalent to using Vue's native <keep-alive> component on the content
- `keep-alive-include` (string | any[] | RegExp, optional)
  Equivalent to using Vue's native include prop for <keep-alive>; Values must be valid Vue component names
  Examples:
    - `'a,b'`
    - `/a|b/`
    - `['a', 'b']`
- `keep-alive-exclude` (string | any[] | RegExp, optional)
  Equivalent to using Vue's native exclude prop for <keep-alive>; Values must be valid Vue component names
  Examples:
    - `'a,b'`
    - `/a|b/`
    - `['a', 'b']`
- `keep-alive-max` (number, optional)
  Equivalent to using Vue's native max prop for <keep-alive>
- `animated` (boolean, optional)
  Enable transitions between panel (also see 'transition-prev' and 'transition-next' props)
- `infinite` (boolean, optional)
  Makes component appear as infinite (when reaching last panel, next one will become the first one)
- `swipeable` (boolean, optional)
  Enable swipe events (may interfere with content's touch/mouse events)
- `vertical` (boolean, optional)
  Default transitions and swipe actions will be on the vertical axis
- `transition-prev` (string, optional), default `'fade'`
  One of Quasar's embedded transitions (has effect only if 'animated' prop is set)
  Examples: `'fade'`, `'slide-down'`
- `transition-next` (string, optional), default `'fade'`
  One of Quasar's embedded transitions (has effect only if 'animated' prop is set)
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `height` (string, optional)
  Height of Carousel in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `padding` (boolean, optional)
  Applies a default padding to each slide, according to the usage of 'arrows' and 'navigation' props
- `control-color` (string, optional)
  Color name for QCarousel button controls (arrows, navigation) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `control-text-color` (string, optional)
  Color name for text color of QCarousel button controls (arrows, navigation) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `control-type` (string, optional), default `'flat'`
  Type of button to use for controls (arrows, navigation)
  Accepts: `'regular'`, `'flat'`, `'outline'`, `'push'`, `'unelevated'`
- `autoplay` (number | boolean, optional)
  Jump to next slide (if 'true' or val > 0) or previous slide (if val < 0) at fixed time intervals (in milliseconds); 'false' disables autoplay, 'true' enables it for 5000ms intervals
  Examples: `true`, `false`, `2500`
- `arrows` (boolean, optional)
  Show navigation arrow buttons
- `prev-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `next-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `navigation` (boolean, optional)
  Show navigation dots
- `navigation-position` (string, optional), default `'bottom'/'right'`
  Side to stick navigation to
  Accepts: `'top'`, `'right'`, `'bottom'`, `'left'`
- `navigation-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `navigation-active-icon` (string, optional)
  Icon name following Quasar convention for the active (current slide) navigation icon; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `thumbnails` (boolean, optional)
  Show thumbnails

### Methods

- `toggleFullscreen(): void`
  Toggle the view to be fullscreen or not fullscreen
- `setFullscreen(): void`
  Enter the fullscreen view
- `exitFullscreen(): void`
  Leave the fullscreen view
- `next(): void`
  Go to next panel
- `previous(): void`
  Go to previous panel
- `goTo(panelName: string | number): void`
  Go to specific panel
  Params:
    - `panelName` (string | number, required)
      Panel's name, which may be a String or Number; Number does not refers to panel index, but to its name, which may be an Integer
      Examples: `'dashboard'`

### Events

- `@fullscreen`
  Emitted when fullscreen state changes
  Params:
    - `value` (boolean, optional)
      Fullscreen state (showing/hidden)
- `@update:fullscreen`
  Used by Vue on 'v-model:fullscreen' prop for updating its value
  Params:
    - `value` (boolean, optional)
      Fullscreen state (showing/hidden)
- `@update:model-value`
  Emitted when the component changes the model; This event _isn't_ fired if the model is changed externally; Is also used by v-model
  Params:
    - `value` (string | number, optional)
      New current panel name
      Examples: `'dashboard'`
- `@before-transition`
  Emitted before transitioning to a new panel
  Params:
    - `newVal` (string | number, optional)
      Panel name towards transition is going
      Examples: `'dashboard'`
    - `oldVal` (string | number, optional)
      Panel name from which transition is happening
      Examples: `'dashboard'`
- `@transition`
  Emitted after component transitioned to a new panel
  Params:
    - `newVal` (string | number, optional)
      Panel name towards transition has occurred
      Examples: `'dashboard'`
    - `oldVal` (string | number, optional)
      Panel name from which transition has happened
      Examples: `'dashboard'`

### Slots

- `#default`
  Suggestion: QCarouselSlide
- `#control`
  Slot specific for QCarouselControl

### Scoped Slots

- `#navigation-icon`
  Slot for navigation icon/btn; Suggestion: QBtn
  Scope:
    - `index` (number, optional)
      The 0-based index of corresponding slide
    - `maxIndex` (number, optional)
      The available number of slides
    - `name` (any, optional)
      The name of the corresponding slide
    - `active` (boolean, optional)
      Is this the current slide?
    - `btnProps` (object, optional)
      Default QBtn props that can be binded to your own QBtn
    - `onClick` (Function, optional)
      Default trigger when clicked/tapped on
      Function signature: `(evt: Event) => void`
      Params:
        - `evt` (Event, required)
          JS event object

