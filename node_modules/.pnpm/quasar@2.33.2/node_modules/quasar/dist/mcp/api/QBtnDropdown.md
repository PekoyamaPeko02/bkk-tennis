## QBtnDropdown API

### Props

- `hover` (boolean, optional) *(added v2.26)*
  Also opens the dropdown when the pointer hovers the button (in split mode: either one of the two buttons) and closes it when the pointer leaves both the button and the menu; Click/tap and keyboard interactions keep toggling as usual (touch devices fall back to them), so activating the toggle closes a hover-shown dropdown, unless it is still animating into view (which keeps a single move-and-click gesture from closing what it just opened); A hover-opened dropdown does not switch focus on itself
- `hover-delay` (number, optional), default `0` *(added v2.26)*
  Delay (in milliseconds) between the pointer entering the button and the dropdown showing up; Requires the 'hover' prop
- `hover-hide-delay` (number, optional), default `150` *(added v2.26)*
  Grace period (in milliseconds) in which the pointer can re-enter the button or the menu before the dropdown gets closed; Requires the 'hover' prop
- `transition-show` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `model-value` (boolean, optional)
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
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
- `split` (boolean, optional)
  Split dropdown icon into its own button
- `dropdown-icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `disable-main-btn` (boolean, optional)
  Disable main button (useful along with 'split' prop)
- `disable-dropdown` (boolean, optional)
  Disables dropdown (dropdown button if using along 'split' prop)
- `no-icon-animation` (boolean, optional)
  Disables the rotation of the dropdown icon when state is toggled
- `content-style` (string | any[] | object, optional)
  Style definitions to be attributed to the menu
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `content-class` (string | any[] | object, optional)
  Class definitions to be attributed to the menu
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `toggle-aria-label` (string, optional)
  aria-label to be used on the dropdown toggle element
  Examples: `'Open menu'`
- `toggle-aria-haspopup` (string, optional) *(added v2.25)*
  aria-haspopup to be used on the dropdown toggle element; Must name the ARIA role of the dropdown content ('menu', 'listbox', 'tree', 'grid' or 'dialog'), so only set it once that content declares a matching role
  Examples: `'menu'`, `'listbox'`
- `cover` (boolean, optional)
  Allows the menu to cover the button. When used, the 'menu-self' prop is no longer effective
- `persistent` (boolean, optional)
  Allows the menu to not be dismissed by a click/tap outside of the menu or by hitting the ESC key; Also, an app route change won't dismiss it
- `no-esc-dismiss` (boolean, optional) *(added v2.18)*
  User cannot dismiss the popup by hitting ESC key; No need to set it if 'persistent' prop is also set
- `no-route-dismiss` (boolean, optional)
  Changing route app won't dismiss the popup; No need to set it if 'persistent' prop is also set
- `auto-close` (boolean, optional)
  Allows any click/tap in the menu to close it; Useful instead of attaching events to each menu item that should close the menu on click/tap
- `no-refocus` (boolean, optional) *(added v2.18)*
  (Accessibility) When the dropdown gets hidden, do not refocus on the DOM element that previously had focus
- `no-focus` (boolean, optional) *(added v2.18)*
  (Accessibility) When the dropdown gets shown, do not switch focus on it
- `menu-anchor` (string, optional), default `'bottom end'`
  Two values setting the starting position or anchor point of the menu relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `menu-self` (string, optional), default `'top end'`
  Two values setting the menu's own position relative to its target
  Accepts: `'top left'`, `'top middle'`, `'top right'`, `'top start'`, `'top end'`, `'center left'`, `'center middle'`, `'center right'`, `'center start'`, `'center end'`, `'bottom left'`, `'bottom middle'`, `'bottom right'`, `'bottom start'`, `'bottom end'`
- `menu-offset` (any[], optional)
  An array of two numbers (in pixels) which expands the anchor element's bounding box outward horizontally and vertically; the menu is then positioned against the expanded box, so the visible effect depends on the 'anchor'/'self' points in use
  Examples:
    - `[8, 8]`
    - `[5, 10]`

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
- `@click`
  Emitted when user clicks/taps on the main button (not the icon one, if using 'split')
  Params:
    - `evt` (Event, required)
      JS event object

### Slots

- `#default`
  Default slot in the devland unslotted content of the component
- `#label`
  Customize main button's content through this slot, unless you're using the 'icon' and 'label' props
- `#toggle`
  Additional content rendered inside the dropdown toggle, next to the arrow icon; With the 'split' prop it is the only way to reach the toggle button (e.g. to attach a QTooltip to it); Does not replace the arrow icon - use the 'dropdown-icon' prop for that
- `#loading`
  Override the default QSpinner when in 'loading' state

