## QBreadcrumbsEl API

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
- `label` (string, optional)
  The label text for the breadcrumb
  Examples: `'Home'`, `'Index'`
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `tag` (string, optional), default `'span'`
  HTML tag to use
  Examples: `'div'`, `'span'`

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
  This is where custom content goes, unless 'icon' and 'label' props are not enough

