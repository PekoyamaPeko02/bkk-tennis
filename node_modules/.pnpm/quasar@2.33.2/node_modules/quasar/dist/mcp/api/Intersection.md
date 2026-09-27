## Intersection API

### Directive Value

- `value` (boolean | object | Function, optional)
  Boolean false (or undefined) to disable the directive (the element stops being observed until a handler is supplied again); Function to call when scrolling occurs (identical to description of 'handler' prop of the Object form); Elements sharing the same 'cfg' share one Intersection Observer, and the Object form is compared by its option values, so an inline Object literal costs nothing extra
  Function signature: `(entry?: object) => boolean`
  Examples:
    - `v-intersection="myFunction"`
    - `v-intersection="isEnabled ? myFunction : false"`
    - `v-intersection="{ handler: myFunction, cfg: { root: myScrollingParentEl, rootMargin: '10px 20px 30px 40px', threshold: [0, 0.25, 0.5, 0.75, 1] } }"`
  Params:
    - `entry` (object, optional)
      The IntersectionObserverEntry object
  Returns: `boolean`
    If you return Boolean false from the handler, the observer stops
  Object shape:
    - `handler` (Function, optional)
      The handler function to be called
      Function signature: `(entry?: object) => boolean`
      Params:
        - `entry` (object, optional)
          The IntersectionObserverEntry object
          Object shape:
            - `boundingClientRect` (object, optional)
              Object containing the client rect information
              Object shape:
                - `bottom` (number, optional)
                  The bottom of the client rect
                - `height` (number, optional)
                  The height of the client rect
                - `left` (number, optional)
                  The left of the client rect
                - `right` (number, optional)
                  The right of the client rect
                - `top` (number, optional)
                  The top of the client rect
                - `width` (number, optional)
                  The width of the client rect
                - `x` (number, optional)
                  The x position of the client rect
                - `y` (number, optional)
                  The y position of the client rect
            - `intersectionRatio` (number, optional)
              The ratio of the observed objects visibility
            - `intersectionRect` (object, optional)
              Object containing the client rect information
              Object shape:
                - `bottom` (number, optional)
                  The bottom of the client rect
                - `height` (number, optional)
                  The height of the client rect
                - `left` (number, optional)
                  The left of the client rect
                - `right` (number, optional)
                  The right of the client rect
                - `top` (number, optional)
                  The top of the client rect
                - `width` (number, optional)
                  The width of the client rect
                - `x` (number, optional)
                  The x position of the client rect
                - `y` (number, optional)
                  The y position of the client rect
            - `isIntersecting` (boolean, optional)
              It is Boolean true if intersecting the scrollable area
            - `rootBounds` (object, optional)
              Object containing the client rect information
              Object shape:
                - `bottom` (number, optional)
                  The bottom of the client rect
                - `height` (number, optional)
                  The height of the client rect
                - `left` (number, optional)
                  The left of the client rect
                - `right` (number, optional)
                  The right of the client rect
                - `top` (number, optional)
                  The top of the client rect
                - `width` (number, optional)
                  The width of the client rect
                - `x` (number, optional)
                  The x position of the client rect
                - `y` (number, optional)
                  The y position of the client rect
            - `target` (Element, optional)
              The target element
            - `time` (number, optional)
              The timestamp of the event
              Examples: `6316.879999998491`
      Returns: `boolean`
        If you return Boolean false from the handler, the observer stops
    - `cfg` (object, optional)
      Intersection observer options (can be omitted and all its props are optional)
      Object shape:
        - `root` (Element, optional)
          Lets you define an alternative to the viewport as your root (through its DOM element); It is important to keep in mind that root needs to be an ancestor of the observed element
          Examples: `myScrollingParentEl`
        - `rootMargin` (string, optional)
          Allows you to specify the margins for the root, effectively allowing you to either grow or shrink the area used for intersections
          Examples: `'10px 20px 30px 40px'`
        - `threshold` (any[], optional)
          Threshold(s) at which to trigger callback, specified as a ratio, or list of ratios, of (visible area / total area) of the observed element
          Examples:
            - `[0, 0.25, 0.5, 0.75, 1]`

### Directive Modifiers

- `once` (boolean, optional)
  Call handler only once, when the conditions are first met
  Examples: `v-intersection.once`

