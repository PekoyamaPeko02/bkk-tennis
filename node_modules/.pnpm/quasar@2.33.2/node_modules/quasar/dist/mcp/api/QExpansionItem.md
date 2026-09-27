## QExpansionItem API

### Props

- `to` (string | object, optional)
  Equivalent to Vue Router <router-link> 'to' property; Superseded by 'href' prop if used
  Examples: `'/home/dashboard'`, `{ name: 'my-route-name' }`
- `exact` (boolean, optional)
  Equivalent to Vue Router <router-link> 'exact' property; Superseded by 'href' prop if used
- `replace` (boolean, optional)
  Equivalent to Vue Router <router-link> 'replace' property; Superseded by 'href' prop if used
- `active-class` (string, optional), default `'q-router-link--active'`
  Equivalent to Vue Router <router-link> 'active-class' property; Superseded by 'href' prop if used
  Examples: `'my-active-class'`
- `exact-active-class` (string, optional), default `'q-router-link--exact-active'`
  Equivalent to Vue Router <router-link> 'active-class' property; Superseded by 'href' prop if used
  Examples: `'my-exact-active-class'`
- `href` (string, optional)
  Native <a> link href attribute; Has priority over the 'to'/'exact'/'replace'/'active-class'/'exact-active-class' props
  Examples: `'https://quasar.dev'`
- `target` (string, optional)
  Native <a> link target attribute; Use it only along with 'href' prop; Has priority over the 'to'/'exact'/'replace'/'active-class'/'exact-active-class' props
  Examples: `'_blank'`, `'_self'`, `'_parent'`, `'_top'`
- `disable` (boolean, optional)
  Put component in disabled mode
- `model-value` (boolean, optional), default `null`
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `expand-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `expanded-icon` (string, optional)
  Expand icon name (following Quasar convention) for when QExpansionItem is expanded; When used, it also disables the rotation animation of the expand icon; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `expand-icon-class` (string | any[] | object, optional)
  Apply custom class(es) to the expand icon item section
  Examples: `'text-purple'`
- `toggle-aria-label` (string, optional)
  aria-label to be used on the expansion toggle element
  Examples: `'Open details'`
- `label` (string, optional)
  Header label (unless using 'header' slot)
  Examples: `'My expansion item'`
- `label-lines` (number | string, optional)
  Apply ellipsis when there's not enough space to render on the specified number of lines; If more than one line specified, then it will only work on webkit browsers because it uses the '-webkit-line-clamp' CSS property!
  Examples: `1`, `'3'`
- `caption` (string, optional)
  Header sub-label (unless using 'header' slot)
  Examples: `'Unread message: 5'`
- `caption-lines` (number | string, optional)
  Apply ellipsis when there's not enough space to render on the specified number of lines; If more than one line specified, then it will only work on webkit browsers because it uses the '-webkit-line-clamp' CSS property!
  Examples: `1`, `'3'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `duration` (number, optional), default `300`
  Animation duration (in milliseconds)
- `header-inset-level` (number, optional)
  Apply an inset to header (unless using 'header' slot); Useful when header avatar/left side is missing but you want to align content with other items that do have a left side, or when you're building a menu
  Examples: `1`
- `content-inset-level` (number, optional)
  Apply an inset to content (changes content padding)
  Examples: `1`
- `expand-separator` (boolean, optional)
  Apply a top and bottom separator when expansion item is opened
- `default-opened` (boolean, optional)
  Puts expansion item into open state on initial render; Overridden by v-model if used
- `hide-expand-icon` (boolean, optional)
  Do not show the expand icon
- `expand-icon-toggle` (boolean, optional)
  Applies the expansion events to the expand icon only and not to the whole header
- `switch-toggle-side` (boolean, optional)
  Switch expand icon side (from default 'right' to 'left')
- `dense-toggle` (boolean, optional)
  Use dense mode for expand icon
- `group` (string, optional)
  Register expansion item into a group (unique name that must be applied to all expansion items in that group) for coordinated open/close state within the group a.k.a. 'accordion mode'
  Examples: `'my-emails'`
- `popup` (boolean, optional)
  Put expansion list into 'popup' mode
- `header-style` (string | any[] | object, optional)
  Apply custom style to the header
  Examples: `'background: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `header-class` (string | any[] | object, optional)
  Apply custom class(es) to the header
  Examples: `'my-custom-class'`, `{ 'my-custom-class': true }`

### Methods

- `show(evt?: Event): void`
  Triggers component to show
  Params:
    - `evt` (Event, optional)
      JS event object
- `hide(evt?: Event): void`
  Triggers component to hide
  Params:
    - `evt` (Event, optional)
      JS event object
- `toggle(evt?: Event): void`
  Triggers component to toggle between show/hide
  Params:
    - `evt` (Event, optional)
      JS event object

### Events

- `@update:model-value`
  Emitted when showing/hidden state changes; Is also used by v-model
  Params:
    - `value` (boolean, optional)
      New state (showing/hidden)
- `@show`
  Emitted after component has triggered show()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-show`
  Emitted when component triggers show() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@hide`
  Emitted after component has triggered hide()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-hide`
  Emitted when component triggers hide() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@after-show`
  Emitted when component show animation is finished
- `@after-hide`
  Emitted when component hide animation is finished

### Slots

- `#default`
  Slot used for expansion item's content

### Scoped Slots

- `#header`
  Slot used for overriding default header
  Scope:
    - `expanded` (boolean, optional)
      QExpansionItem expanded status
    - `detailsId` (string, optional)
      QExpansionItem details panel id (for use in aria-controls)
    - `show` (Function, optional)
      Triggers component to show
      Function signature: `(evt?: object) => void`
      Params:
        - `evt` (object, optional)
          JS event object
    - `hide` (Function, optional)
      Triggers component to hide
      Function signature: `(evt?: object) => void`
      Params:
        - `evt` (object, optional)
          JS event object
    - `toggle` (Function, optional)
      Triggers component to toggle between show/hide
      Function signature: `(evt?: object) => void`
      Params:
        - `evt` (object, optional)
          JS event object

