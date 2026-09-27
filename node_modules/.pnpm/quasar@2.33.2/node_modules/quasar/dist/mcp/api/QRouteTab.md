## QRouteTab API

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
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `label` (number | string, optional)
  A number or string to label the tab
  Examples: `'Home'`
- `alert` (boolean | string, optional)
  Adds an alert symbol to the tab, notifying the user there are some updates; If its value is not a Boolean, then you can specify a color
  Examples: `'purple'`
- `alert-icon` (string, optional)
  Adds a floating icon to the tab, notifying the user there are some updates; It's displayed only if 'alert' is set; Can use the color specified by 'alert' prop
  Examples: `'alarm_on'`
- `name` (number | string, optional), default `a random UUID`
  Panel name
  Examples: `'home'`, `1`
- `no-caps` (boolean, optional)
  Turns off capitalizing all letters within the tab (which is the default)
- `content-class` (string, optional)
  Class definitions to be attributed to the content wrapper
  Examples: `'my-special-class'`
- `ripple` (boolean | object, optional), default `true`
  Configure material ripple (disable it by setting it to 'false' or supply a config object)
  Examples:
    - `false`
    - `{ early: true, center: true, color: 'teal', keyCodes: [] }`
- `tabindex` (number | string, optional)
  Tabindex HTML attribute value
  Examples: `100`, `'0'`

### Events

- `@click`
  Emitted when the component is clicked
  Params:
    - `evt` (Event, optional)
      JS event object; If you want to cancel navigation then call evt.preventDefault() synchronously in your event handler
    - `go` (Function, optional)
      When you need to control the time at which the component should trigger the route navigation then call evt.preventDefault() synchronously and then call this function at your convenience; Useful if you have async work to be done before the actual route navigation or if you want to redirect somewhere else
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
  Suggestion: QMenu, QTooltip

