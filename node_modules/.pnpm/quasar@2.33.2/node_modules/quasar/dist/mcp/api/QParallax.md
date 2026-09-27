## QParallax API

### Props

- `src` (string, optional)
  Path to image (unless a 'media' slot is used)
  Examples: `(public folder) src="img/something.png"`, `(assets folder) src="~@/assets/my-img.png"`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) src="https://some-site.net/some-img.jpg"`
- `height` (number, optional), default `500`
  Height of component (in pixels)
- `speed` (number, optional), default `1`
  Speed of parallax effect (0.0 < x < 1.0)
- `scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector or DOM element to be used as the container box the scroll percentage is computed against, instead of the auto detected one; the scrolling itself is detected in any scrolling container
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`

### Methods

- `refresh(): void`
  Re-evaluates what is settled at mount: the scrolling container and the way the media is moved along it, the media size and the position; call it after a change the component cannot observe on its own, like an ancestor changing its overflow, a media swap without a load event or a 'scroll-target' that appeared later

### Events

- `@scroll`
  Emitted when scrolling occurs
  Params:
    - `percentage` (number, optional)
      Number between 0.0 and 1.0 defining the scrolled percentage of the component

### Slots

- `#default`
  Default slot can be used for content that gets displayed on top of the component
- `#media`
  Slot for describing <img> or <video> tags

### Scoped Slots

- `#content`
  Scoped slot for describing content that gets displayed on top of the component; If specified, it overrides the default slot
  Scope:
    - `percentScrolled` (number, optional)
      Percentage (0.0 < x < 1.0) of scroll in regards to QParallax

