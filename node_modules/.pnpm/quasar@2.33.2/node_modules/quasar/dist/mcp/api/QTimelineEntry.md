## QTimelineEntry API

### Props

- `heading` (boolean, optional)
  Defines a heading timeline item
- `tag` (string, optional), default `'h3'`
  Tag to use, if of type 'heading' only
  Examples: `'h1'`
- `side` (string, optional), default `'right'`
  Side to place the timeline entry; Works only if QTimeline layout is loose.
  Accepts: `'left'`, `'right'`
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `avatar` (string, optional)
  URL to the avatar image; Icon takes precedence if used, so it replaces avatar
  Examples: `(public folder) src="img/my-bg.png"`, `(assets folder) src="~@/assets/my-img.png"`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) src="https://picsum.photos/500/300"`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `title` (string, optional)
  Title of timeline entry; Is overridden if using 'title' slot
  Examples: `'December party'`
- `subtitle` (string, optional)
  Subtitle of timeline entry; Is overridden if using 'subtitle' slot
  Examples: `'All invited'`
- `body` (string, optional)
  Body content of timeline entry; Use this prop or the default slot
  Examples:
    - `'Lorem ipsum dolor sit amet, consectetur adipisicing elit.'`

### Slots

- `#default`
  Timeline entry content (body)
- `#title`
  Optional slot for title; When used, it overrides 'title' prop
- `#subtitle`
  Optional slot for subtitle; When used, it overrides 'subtitle' prop

