## QImg API

### Props

- `ratio` (string | number, optional)
  Force the component to maintain an aspect ratio
  Examples:
    - `1`
    - `'1.7778'`
    - `:ratio="4/3"`
    - `:ratio="16/9"`
    - `(Number format) :ratio="16/9"`
    - `(String format) ratio="1"`
- `src` (string, optional)
  Path to image
  Examples: `(public folder) src="img/something.png"`, `(assets folder) src="~@/assets/my-img.gif"`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) src="https://picsum.photos/500/300"`
- `srcset` (string, optional)
  Same syntax as <img> srcset attribute
  Examples:
    - `'elva-fairy-320w.jpg 320w, elva-fairy-480w.jpg 480w'`
- `sizes` (string, optional)
  Same syntax as <img> sizes attribute
  Examples:
    - `'(max-width: 320px) 280px, (max-width: 480px) 440px, 800px'`
- `placeholder-src` (string, optional)
  While waiting for your image to load, you can use a placeholder image
  Examples: `(public folder) placeholder-src="img/some-placeholder.png"`, `(assets folder) placeholder-src="~@/assets/my-placeholder.gif"`, `(relative path format) :placeholder-src="require('./placeholder.jpg')"`, `(URL) placeholder-src="https://picsum.photos/500/300"`
- `error-src` (string, optional) *(added v2.15)*
  In case your image fails to load, you can use an error image
  Examples: `(public folder) error-src="img/some-placeholder.png"`, `(assets folder) error-src="~@/assets/my-placeholder.gif"`, `(relative path format) :error-src="require('./placeholder.jpg')"`, `(URL) error-src="https://picsum.photos/500/300"`
- `initial-ratio` (string | number, optional), default `1.7778`
  Use it when not specifying 'ratio' but still wanting an initial aspect ratio
  Examples: `(Number format) :initial-ratio="16/9"`, `(String format) initial-ratio="1"`
- `width` (string, optional)
  Forces image width; Must also include the unit (px or %)
  Examples: `'280px'`, `'70%'`
- `height` (string, optional)
  Forces image height; Must also include the unit (px or %)
  Examples: `'280px'`, `'70%'`
- `loading` (string, optional), default `'lazy'`
  Lazy or immediate load; Same syntax as <img> loading attribute
  Accepts: `'lazy'`, `'eager'`
- `loading-show-delay` (number | string, optional), default `0` *(added v2.14.6)*
  Delay showing the spinner when image changes; Gives time for the browser to load the image from cache to prevent flashing the spinner unnecessarily; Value should represent milliseconds
  Examples: `500`, `'700'`
- `crossorigin` (string, optional)
  Same syntax as <img> crossorigin attribute
  Accepts: `'anonymous'`, `'use-credentials'`
- `decoding` (string, optional)
  Same syntax as <img> decoding attribute
  Accepts: `'sync'`, `'async'`, `'auto'`
- `referrerpolicy` (string, optional)
  Same syntax as <img> referrerpolicy attribute
  Accepts: `'no-referrer'`, `'no-referrer-when-downgrade'`, `'origin'`, `'origin-when-cross-origin'`, `'same-origin'`, `'strict-origin'`, `'strict-origin-when-cross-origin'`, `'unsafe-url'`
- `fetchpriority` (string, optional), default `'auto'`
  Provides a hint of the relative priority to use when fetching the image
  Accepts: `'high'`, `'low'`, `'auto'`
- `fit` (string, optional), default `'cover'`
  How the image will fit into the container; Equivalent of the object-fit prop; Can be coordinated with 'position' prop
  Accepts: `'cover'`, `'fill'`, `'contain'`, `'none'`, `'scale-down'`
- `position` (string, optional), default `'50% 50%'`
  The alignment of the image into the container; Equivalent of the object-position CSS prop
  Examples: `'0 0'`, `'20px 50px'`
- `alt` (string, optional)
  Specifies an alternate text for the image, if the image cannot be displayed
  Examples: `'Two cats'`
- `draggable` (boolean, optional)
  Adds the native 'draggable' attribute
- `img-class` (string, optional)
  CSS classes to be attributed to the native img element
  Examples: `'my-special-class'`
- `img-style` (object, optional)
  Apply CSS to the native img element
  Examples: `{ transform: 'rotate(45deg)' }`
- `spinner-color` (string, optional)
  Color name for default Spinner (unless using a 'loading' slot)
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `spinner-size` (string, optional)
  Size in CSS units, including unit name, for default Spinner (unless using a 'loading' slot)
  Examples: `'16px'`, `'2rem'`
- `no-spinner` (boolean, optional)
  Do not display the default spinner while waiting for the image to be loaded; It is overriden by the 'loading' slot when one is present
- `no-native-menu` (boolean, optional)
  Disables the native context menu for the image
- `no-transition` (boolean, optional)
  Disable default transition when switching between old and new image
- `ssr-prerender` (boolean, optional) *(added v2.32)*
  By default the image is rendered in the server-side (SSR/SSG) HTML; When none of 'ratio', 'initial-ratio' or 'height' is specified the shape of the box cannot be known until the image loads, so the image is rendered client-side after hydration instead; Set this prop to still render it server-side, accepting that the box changes to the natural ratio of the image once hydrated

### Events

- `@load`
  Emitted when image has been loaded by the browser
  Params:
    - `src` (string, optional)
      URL of image that has been loaded; useful when using 'srcset' and/or 'sizes'
      Examples: `'https://some-site.net/some-img.gif'`
- `@error`
  Emitted when browser could not load the image
  Params:
    - `evt` (Event, optional)
      JS Event object (same as the browser's native 'error' event)

### Slots

- `#default`
  Default slot can be used for captions. See examples
- `#loading`
  While image is loading, this slot is being displayed on top of the component; Suggestions: a spinner or text
- `#error`
  Optional slot to be used when image could not be loaded; make sure you assign a min-height and min-width to the component through CSS

