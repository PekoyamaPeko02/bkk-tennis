## BottomSheet API

### Methods

- `create(opts: object): object`
  Creates an ad-hoc Bottom Sheet; Same as calling $q.bottomSheet(...)
  Params:
    - `opts` (object, required)
      Bottom Sheet options
      Object shape:
        - `class` (string | any[] | object, optional)
          CSS Class name to apply to the Dialog's QCard
          Examples: `'my-class'`
        - `style` (string | any[] | object, optional)
          CSS style to apply to the Dialog's QCard
          Examples: `'border: 2px solid black'`
        - `title` (string, optional)
          Title
          Examples: `'Share'`
        - `message` (string, optional)
          Message
          Examples: `'Please select how to share'`
        - `actions` (any[], optional)
          Array of Objects, each Object defining an action
          Object shape:
            - `classes` (string | any[] | object, optional)
              CSS classes for this action
              Examples: `'my-class'`
            - `style` (string | any[] | object, optional)
              Style definitions to be attributed to this action element
              Examples: `{ padding: '2px' }`
            - `icon` (string, optional)
              Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
              Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
            - `img` (string, optional)
              Path to an image for this action
              Examples: `(public folder) 'img/something.png'`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) https://some-site.net/some-img.gif`
            - `avatar` (string, optional)
              Path to an avatar image for this action
              Examples: `(public folder) 'img/avatar.png'`, `(relative path format) :src="require('./my_img.jpg')"`, `(URL) https://some-site.net/some-img.gif`
            - `label` (string | number, optional)
              Action label
              Examples: `'Facebook'`
            - `...` (any, optional)
              Any other custom props
        - `grid` (boolean, optional)
          Display actions as a grid instead of as a list
        - `dark` (boolean, optional), default `null`
          Apply dark mode
        - `seamless` (boolean, optional)
          Put Bottom Sheet into seamless mode; Does not use a backdrop so user is able to interact with the rest of the page too
        - `persistent` (boolean, optional)
          User cannot dismiss Bottom Sheet if clicking outside of it or hitting ESC key; Also, an app route change won't dismiss it
  Returns: `object`
    Chainable Object
    Object shape:
      - `onOk` (Function, required)
        Receives a Function param to tell what to do when OK is pressed / option is selected
        Function signature: `(callbackFn: Function) => object`
        Params:
          - `callbackFn` (Function, required)
            Tell what to do
            Function signature: `(payload?: any) => void`
            Examples:
              - `() => console.log('OK!')`
              - `payload => Notify.create({ type: 'positive', message: `Successfully saved '${payload.book.name}' book!` })`
            Params:
              - `payload` (any, optional)
                The payload if called onDialogOK with the parameter or emitted one with the 'ok' event
                Examples:
                  - `'Quasar Framework'`
                  - `[1, 2, 6, 3]`
                  - `{ book: { id: 1, name: 'Lorem Ipsum' }, user: { name: 'Lorem J. Ipsum', role: 'admin' } }`
        Returns: `object`
          Chained Object
      - `onCancel` (Function, required)
        Receives a Function as param to tell what to do when Cancel is pressed / dialog is dismissed
        Function signature: `(callbackFn: Function) => object`
        Params:
          - `callbackFn` (Function, required)
            Tell what to do
            Function signature: `(reason?: string) => void`
            Examples: `() => console.log('Cancelled')`, `reason => { if (reason === 'cancel') { console.log('Cancel button was clicked') } }`
            Params:
              - `reason` (string, optional) *(added v2.28)*
                Why the dialog got dismissed: Cancel button, backdrop click, ESC key, or hidden through code (which includes an app route change); With a custom component, it mirrors the payload of the component's 'hide' event
                Accepts: `'cancel'`, `'backdrop'`, `'escape'`, `'programmatic'`
        Returns: `object`
          Chained Object
      - `onDismiss` (Function, required)
        Receives a Function param to tell what to do when the dialog is closed
        Function signature: `(callbackFn: Function) => object`
        Params:
          - `callbackFn` (Function, required)
            Tell what to do
            Function signature: `(payload?: any) => void`
            Params:
              - `payload` (any, optional) *(added v2.28)*
                When closed through OK, the same payload the onOk callback receives; Otherwise the dismissal reason ('cancel', 'backdrop', 'escape' or 'programmatic')
        Returns: `object`
          Chained Object
      - `hide` (Function, required)
        Hides the dialog when called
        Function signature: `() => object`
        Returns: `object`
          Chained Object
      - `update` (Function, required)
        Updates the initial properties (given as create() param) except for 'component'
        Function signature: `(opts: object) => object`
        Params:
          - `opts` (object, required)
            If using with 'component' prop then the props to update the current 'componentProps' (will be shallowly merged on top of the previous 'componentProps'); Otherwise the props to be shallowly merged with the previous create() param Object
        Returns: `object`
          Chained Object

### Vue Injection

Accessible via `$q.bottomSheet` (e.g., `this.$q.bottomSheet` in Options API or `useQuasar().bottomSheet` in Composition API).

