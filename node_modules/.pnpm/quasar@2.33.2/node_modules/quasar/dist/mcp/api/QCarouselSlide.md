## QCarouselSlide API

### Props

- `name` (any, required)
  Slide name
  Examples: `'accounts'`, `'firstPanel'`, `1`
- `disable` (boolean, optional)
  Put component in disabled mode
- `img-src` (string, optional)
  URL pointing to a slide background image (use public folder)
  Examples: `(public folder) src="img/my-bg.png"`, `(assets folder) src="~@/assets/my-img.png"`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) src="https://picsum.photos/500/300"`

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

