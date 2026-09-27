## QItem API

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
- `active` (boolean, optional), default `null`
  Put item into 'active' state
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `clickable` (boolean, optional), default `null`
  Is QItem clickable? If it's the case, then it will add hover effects and emit 'click' events; when not set, the item is clickable if a 'click' listener is attached (a link or a 'label' tag always make it clickable)
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `inset-level` (number, optional)
  Apply an inset; Useful when avatar/left side is missing but you want to align content with other items that do have a left side, or when you're building a menu
  Examples: `1`
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`
- `role` (string, optional) *(added v2.25)*
  Overrides the ARIA role that the item derives from its context: 'menuitem' when the wrapping QList has a 'menu'/'menubar' role (actionable items only), 'button' when clickable, 'listitem' when non-interactive inside a default QList, none otherwise
  Examples: `'menuitem'`, `'menuitemcheckbox'`, `'option'`
- `tag` (string, optional), default `'div'`
  HTML tag to render; Suggestion: use 'label' when encapsulating a QCheckbox/QRadio/QToggle so that when user clicks/taps on the whole item it will trigger a model change for the mentioned components
  Examples: `'a'`, `'label'`, `'div'`
- `manual-focus` (boolean, optional)
  Put item into a manual focus state; Enables 'focused' prop which will determine if item is focused or not, rather than relying on native hover/focus states
- `focused` (boolean, optional)
  Determines focus state, ONLY if 'manual-focus' is enabled / set to true

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
  This is where QItem's content goes

