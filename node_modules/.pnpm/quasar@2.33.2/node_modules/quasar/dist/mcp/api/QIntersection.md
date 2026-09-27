## QIntersection API

### Props

- `tag` (string, optional), default `'div'`
  HTML tag to use
  Examples: `'div'`, `'span'`, `'blockquote'`
- `once` (boolean, optional)
  Get triggered only once
- `ssr-prerender` (boolean, optional)
  Pre-render content on server side if using SSR/SSG (use it to pre-render above the fold content)
- `root` (Element, optional), default `null`
  [Intersection API root prop] Lets you define an alternative to the viewport as your root (through its DOM element); It is important to keep in mind that root needs to be an ancestor of the observed element
  Examples: `document.getElementById('myTable')`, `$refs.myTable.$el`
- `margin` (string, optional)
  [Intersection API rootMargin prop] Allows you to specify the margins for the root, effectively allowing you to either grow or shrink the area used for intersections
  Examples: `'-20px 0px'`, `'10px 20px 30px 40px'`
- `threshold` (any[] | number, optional)
  [Intersection API threshold prop] Threshold(s) at which to trigger, specified as a ratio, or list of ratios, of (visible area / total area) of the observed element
  Examples:
    - `[0, 0.25, 0.5, 0.75, 1]`
    - `1`
- `transition` (string, optional)
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `disable` (boolean, optional)
  Disable visibility observable (content will remain as it was, visible or hidden)

### Events

- `@visibility`
  Fires when visibility changes
  Params:
    - `isVisible` (boolean, optional)
      Visibility status (true/false)

### Slots

- `#default`
  Default slot in the devland unslotted content of the component
- `#hidden`
  Slot for content to render when component is not on screen; Example: a text that the user can search for with the browser's search function

