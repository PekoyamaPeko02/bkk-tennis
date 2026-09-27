## QBtn API

### Props

- `size` (string, optional)
  Size in CSS units, including unit name or standard size name (xs|sm|md|lg|xl)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `type` (string, optional), default `'button'`
  1) Define the button native type attribute (submit, reset, button) or 2) render component with <a> tag so you can access events even if disable or 3) Use 'href' prop and specify 'type' as a media tag
  Examples:
    - `'a'`
    - `'submit'`
    - `'button'`
    - `'reset'`
    - `'image/png'`
    - `href="https://quasar.dev" target="_blank"`
- `to` (string | object, optional)
  Equivalent to Vue Router <router-link> 'to' property; Superseded by 'href' prop if used
  Examples: `'/home/dashboard'`, `{ name: 'my-route-name' }`
- `replace` (boolean, optional)
  Equivalent to Vue Router <router-link> 'replace' property; Superseded by 'href' prop if used
- `href` (string, optional)
  Native <a> link href attribute; Has priority over the 'to' and 'replace' props
  Examples: `'https://quasar.dev'`, `href="https://quasar.dev" target="_blank"`
- `target` (string, optional)
  Native <a> link target attribute; Use it only with 'to' or 'href' props
  Examples: `'_blank'`, `'_self'`, `'_parent'`, `'_top'`
- `label` (string | number, optional)
  The text that will be shown on the button
  Examples: `'Button Label'`
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-right` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `outline` (boolean, optional)
  Use 'outline' design
- `flat` (boolean, optional)
  Use 'flat' design
- `unelevated` (boolean, optional)
  Remove shadow
- `rounded` (boolean, optional)
  Applies a more prominent border-radius for a squared shape button
- `push` (boolean, optional)
  Use 'push' design
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `glossy` (boolean, optional)
  Applies a glossy effect
- `fab` (boolean, optional)
  Makes button size and shape to fit a Floating Action Button
- `fab-mini` (boolean, optional)
  Makes button size and shape to fit a small Floating Action Button
- `padding` (string, optional)
  Apply custom padding (vertical [horizontal]); Size in CSS units, including unit name or standard size name (none|xs|sm|md|lg|xl); Also removes the min width and height when set
  Examples:
    - `'16px'`
    - `'10px 5px'`
    - `'2rem'`
    - `'xs'`
    - `'md lg'`
    - `'2px 2px 5px 7px'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `no-caps` (boolean, optional)
  Avoid turning label text into caps (which happens by default)
- `no-wrap` (boolean, optional)
  Avoid label text wrapping
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `ripple` (boolean | object, optional), default `true`
  Configure material ripple (disable it by setting it to 'false' or supply a config object)
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `align` (string, optional), default `'center'`
  Label or content alignment
  Accepts: `'left'`, `'right'`, `'center'`, `'around'`, `'between'`, `'evenly'`
- `stack` (boolean, optional)
  Stack icon and label vertically instead of on same line (like it is by default)
- `stretch` (boolean, optional)
  When used on flexbox parent, button will stretch to parent's height
- `loading` (boolean, optional), default `null`
  Put button into loading state (displays a QSpinner -- can be overridden by using a 'loading' slot)
- `disable` (boolean, optional)
  Put component in disabled mode
- `round` (boolean, optional)
  Makes a circle shaped button
- `percentage` (number, optional)
  Percentage (0.0 < x < 100.0); To be used along 'loading' prop; Display a progress bar on the background
- `dark-percentage` (boolean, optional)
  Progress bar on the background should have dark color; To be used along with 'percentage' and 'loading' props

### Methods

- `click(evt?: Event): void`
  Emulate click on QBtn
  Params:
    - `evt` (Event, optional)
      JS event object

### Events

- `@click`
  Emitted when the component is clicked
  Params:
    - `evt` (Event, optional)
      JS event object; If you are using route navigation ('to'/'replace' props) and you want to cancel navigation then call evt.preventDefault() synchronously in your event handler
    - `go` (Function, optional)
      Available ONLY if you are using route navigation ('to'/'replace' props); When you need to control the time at which the component should trigger the route navigation then call evt.preventDefault() synchronously and then call this function at your convenience; Useful if you have async work to be done before the actual route navigation or if you want to redirect somewhere else
      Function signature: `(opts?: object) => Promise<any>`
      Params:
        - `opts` (object, optional)
          Optional options
          Object shape:
            - `to` (string | object, optional)
              Equivalent to Vue Router <router-link> 'to' property; Specify it explicitly otherwise it will be set with same value as component's 'to' prop
              Examples: `'/home/dashboard'`, `{ name: 'my-route-name' }`
            - `replace` (boolean, optional)
              Equivalent to Vue Router <router-link> 'replace' property; Specify it explicitly otherwise it will be set with same value as component's 'replace' prop
            - `returnRouterError` (boolean, optional)
              Return the router error, if any; Otherwise the returned Promise will always fulfill
      Returns: `Promise<any>`
        Returns the router's navigation promise

### Slots

- `#default`
  Use for custom content, instead of relying on 'icon' and 'label' props
- `#loading`
  Override the default QSpinner when in 'loading' state

