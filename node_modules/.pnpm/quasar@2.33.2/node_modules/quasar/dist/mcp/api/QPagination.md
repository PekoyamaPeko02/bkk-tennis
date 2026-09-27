## QPagination API

### Props

- `model-value` (number, required, syncable)
  Current page (must be between min/max)
- `min` (number | string, optional), default `1`
  Minimum page (must be lower than 'max')
- `max` (number | string, required)
  Number of last page (must be higher than 'min')
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color (useful when you are using it along with the 'input' prop)
- `size` (string, optional)
  Button size in CSS units, including unit name
  Examples: `'20px'`
- `disable` (boolean, optional)
  Put component in disabled mode
- `input` (boolean, optional)
  Use an input instead of buttons
- `icon-prev` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-next` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-first` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-last` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `to-fn` (Function, optional)
  Generate link for page buttons; For best performance, reference it from your scope and do not define it inline
  Function signature: `(page?: number) => object | string`
  Examples: `page => ({ query: { page } })`
  Params:
    - `page` (number, optional)
      Page number to navigate to
  Returns: `object | string`
    Object or String that can be passed to a <router-link> as 'to' parameter
- `boundary-links` (boolean, optional), default `null`
  Show boundary button links
- `boundary-numbers` (boolean, optional), default `null`
  Always show first and last page buttons (if not using 'input')
- `direction-links` (boolean, optional), default `null`
  Show direction buttons
- `ellipses` (boolean, optional), default `null`
  Show ellipses (...) when pages are available
- `max-pages` (number | string, optional), default `0`
  Maximum number of page links to display at a time; 0 means Infinite
- `flat` (boolean, optional)
  Use 'flat' design for non-active buttons (it's the default option)
- `outline` (boolean, optional)
  Use 'outline' design for non-active buttons
- `unelevated` (boolean, optional)
  Remove shadow for non-active buttons
- `push` (boolean, optional)
  Use 'push' design for non-active buttons
- `color` (string, optional), default `'primary'`
  Color name from the Quasar Color Palette for the non-active buttons
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Text color name from the Quasar Color Palette for the ACTIVE buttons
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `active-design` (string, optional), default `''`
  The design of the ACTIVE button, similar to the flat/outline/push/unelevated props (but those are used for non-active buttons)
  Accepts: `'flat'`, `'outline'`, `'push'`, `'unelevated'`, `''`
- `active-color` (string, optional), default `'primary'`
  Color name from the Quasar Color Palette for the ACTIVE button
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `active-text-color` (string, optional)
  Text color name from the Quasar Color Palette for the ACTIVE button
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `round` (boolean, optional)
  Makes a circle shaped button for all buttons
- `rounded` (boolean, optional)
  Applies a more prominent border-radius for a squared shape button for all buttons
- `glossy` (boolean, optional)
  Applies a glossy effect for all buttons
- `gutter` (string, optional), default `'2px'`
  Apply custom gutter; Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl)
  Examples:
    - `'16px'`
    - `'10px 5px'`
    - `'2rem'`
    - `'xs'`
    - `'md lg'`
    - `'2px 2px 5px 7px'`
- `padding` (string, optional), default `'3px 2px'`
  Apply custom padding (vertical [horizontal]); Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl); Also removes the min width and height when set
  Examples:
    - `'16px'`
    - `'10px 5px'`
    - `'2rem'`
    - `'xs'`
    - `'md lg'`
    - `'2px 2px 5px 7px'`
- `input-style` (string | any[] | object, optional)
  Style definitions to be attributed to the input (if using one)
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `input-class` (string | any[] | object, optional)
  Class definitions to be attributed to the input (if using one)
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `ripple` (boolean | object, optional), default `true`
  Configure buttons material ripple (disable it by setting it to 'false' or supply a config object); Does not applies to boundary and ellipsis buttons
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`

### Methods

- `set(pageNumber?: number): void`
  Go directly to the specified page
  Params:
    - `pageNumber` (number, optional)
      Page number to go to
- `setByOffset(offset?: number): void`
  Increment/Decrement current page by offset
  Params:
    - `offset` (number, optional)
      Offset page, can be negative or positive

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (number, required)
      New model value

### Scoped Slots

- `#ellipsis`
  Replaces an ellipsis (...) button; Suggestion: QBtn (spread 'btnProps' onto it)
  Scope:
    - `side` (string, optional)
      Which ellipsis is being rendered
      Accepts: `'start'`, `'end'`
    - `page` (number, optional)
      The page the default ellipsis button navigates to (the first hidden page on that side)
    - `btnProps` (object, optional)
      Default QBtn props (design, color, size, label, disable, aria-label, ...) that can be binded to your own QBtn; deliberately does not contain the click handler or the 'to' prop
    - `onClick` (Function, optional)
      The default click handler (navigates to 'page'); bind it to your own QBtn to keep the default behavior
    - `to` (any, optional)
      The router link target the default ellipsis button would use; only present when the 'to-fn' prop is set
      Examples:
        - `'/page/4'`
        - `{ name: 'list', query: { page: 4 } }`

