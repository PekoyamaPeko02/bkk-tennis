## Notify API

### Methods

- `create(opts: object | string): Function`
  Creates a notification; Same as calling $q.notify(...)
  Params:
    - `opts` (object | string, required)
      Notification options
      Object shape:
        - `type` (string, optional)
          Optional type (that has been previously registered) or one of the out of the box ones ('positive', 'negative', 'warning', 'info', 'ongoing')
          Examples: `'negative'`, `'custom-type'`
        - `color` (string, optional)
          Color name for component from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `textColor` (string, optional)
          Overrides text color (if needed); Color name from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `message` (string, optional)
          The content of your message
          Examples: `'John Doe pinged you'`
        - `caption` (string, optional)
          The content of your optional caption
          Examples: `'5 minutes ago'`
        - `html` (boolean, optional)
          Render the message as HTML; This can lead to XSS attacks, so make sure that you sanitize the message first
        - `icon` (string, optional)
          Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
          Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
        - `iconColor` (string, optional)
          Color name for component from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `iconSize` (string, optional)
          Size in CSS units, including unit name
          Examples: `'16px'`, `'2rem'`
        - `avatar` (string, optional)
          URL to an avatar/image; Suggestion: use public folder
          Examples: `(public folder) 'img/something.png'`, `(relative path format) require('./my_img.jpg')`, `(URL) https://some-site.net/some-img.gif`
        - `spinner` (boolean | Component, optional)
          Useful for notifications that are updated; Displays a Quasar spinner instead of an avatar or icon; If value is Boolean 'true' then the default QSpinner is shown
          Examples: `true`, `QSpinnerBars`
        - `spinnerColor` (string, optional)
          Color name for component from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `spinnerSize` (string, optional)
          Size in CSS units, including unit name
          Examples: `'16px'`, `'2rem'`
        - `position` (string, optional), default `'bottom'`
          Window side/corner to stick to
          Accepts: `'top-left'`, `'top-right'`, `'bottom-left'`, `'bottom-right'`, `'top'`, `'bottom'`, `'left'`, `'right'`, `'center'`
        - `group` (boolean | string | number, optional), default `message + caption + multiline + actions labels + position`
          Override the auto generated group with custom one; Grouped notifications cannot be updated; String or number value inform this is part of a specific group, regardless of its options; When a new notification is triggered with same group name, it replaces the old one and shows a badge with how many times the notification was triggered
          Examples: `'my-group'`
        - `badgeColor` (string, optional)
          Color name for the badge from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `badgeTextColor` (string, optional)
          Color name for the badge text from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `badgePosition` (string, optional), default `top-left/top-right`
          Notification corner to stick badge to; If notification is on the left side then default is top-right otherwise it is top-left
          Accepts: `'top-left'`, `'top-right'`, `'bottom-left'`, `'bottom-right'`
        - `badgeStyle` (string | any[] | object, optional)
          Style definitions to be attributed to the badge
          Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
        - `badgeClass` (string | any[] | object, optional)
          Class definitions to be attributed to the badge
          Examples: `'my-special-class'`, `{ 'my-special-class': true }`
        - `progress` (boolean, optional)
          Show progress bar to detail when notification will disappear automatically (unless timeout is 0)
        - `progressClass` (string | any[] | object, optional)
          Class definitions to be attributed to the progress bar
          Examples: `'my-special-class'`, `{ 'my-special-class': true }`
        - `classes` (string, optional)
          Add CSS class(es) to the notification for easier customization
          Examples: `'my-notif-class'`
        - `attrs` (object, optional)
          Key-value for attributes to be set on the notification
          Examples: `{ role: 'alertdialog' }`
        - `timeout` (number, optional), default `5000`
          Amount of time to display (in milliseconds). Set to 0 to never dismiss automatically.
          Examples: `2500`
        - `actions` (any[], optional)
          Notification actions (buttons); Unless 'noDismiss' is true, clicking/tapping on the button will close the notification; Also check 'closeBtn' convenience prop
          Examples:
            - `[{ label: 'Show', handler: () => {}, 'aria-label': 'Button label' }, { icon: 'map', handler: () => {}, color: 'yellow' }, { label: 'Learn more', noDismiss: true, handler: () => {} }]`
          Object shape:
            - `handler` (Function, optional)
              Function to be executed when the button is clicked/tapped
              Examples: `() => { console.log('button clicked') }`
            - `noDismiss` (boolean, optional)
              Do not dismiss the notification when the button is clicked/tapped
            - `...` (any, optional)
              Any other QBtn prop except 'onClick' (use 'handler' instead)
              Examples: `label: 'Learn more'`, `color: 'primary'`
        - `onDismiss` (Function, optional)
          Function to call when notification gets dismissed
          Examples: `() => { console.log('Dismissed') }`
        - `closeBtn` (boolean | string, optional)
          Convenient way to add a dismiss button with a specific label, without using the 'actions' prop; If set to true, it uses a label according to the current Quasar language
          Examples: `'Close me'`
        - `multiLine` (boolean, optional)
          Put notification into multi-line mode; If this prop isn't used and more than one 'action' is specified then notification goes into multi-line mode by default
        - `ignoreDefaults` (boolean, optional)
          Ignore the default configuration (set by setDefaults()) for this instance only
  Returns: `Function`
    Calling this function with no parameters hides the notification; When called with one Object parameter (the original notification must NOT be grouped), it updates the notification (specified properties are shallow merged with previous ones; note that group and position cannot be changed while updating and so they are ignored)
    Function signature: `(props?: object) => void`
    Params:
      - `props` (object, optional)
        Notification properties that will be shallow merged to previous ones in order to update the non-grouped notification; (See 'opts' param of 'create()' for object properties, except 'group' and 'position')
- `setDefaults(opts: object): void`
  Merge options into the default ones
  Params:
    - `opts` (object, required)
      Notification options except 'ignoreDefaults' (See 'opts' param of 'create()' for object properties)
- `registerType(typeName: string, typeOpts: object): void`
  Register a new type of notification (or override an existing one)
  Params:
    - `typeName` (string, required)
      Name of the type (to be used as 'type' prop later on)
      Examples: `'my-type'`
    - `typeOpts` (object, required)
      Notification options except 'ignoreDefaults' (See 'opts' param of 'create()' for object properties)

### Vue Injection

Accessible via `$q.notify` (e.g., `this.$q.notify` in Options API or `useQuasar().notify` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.notify` (object)

- `type` (string, optional)
  Optional type (that has been previously registered) or one of the out of the box ones ('positive', 'negative', 'warning', 'info', 'ongoing')
  Examples: `'negative'`, `'custom-type'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `textColor` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `message` (string, optional)
  The content of your message
  Examples: `'John Doe pinged you'`
- `caption` (string, optional)
  The content of your optional caption
  Examples: `'5 minutes ago'`
- `html` (boolean, optional)
  Render the message as HTML; This can lead to XSS attacks, so make sure that you sanitize the message first
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `iconColor` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `iconSize` (string, optional)
  Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `avatar` (string, optional)
  URL to an avatar/image; Suggestion: use public folder
  Examples: `(public folder) 'img/something.png'`, `(relative path format) require('./my_img.jpg')`, `(URL) https://some-site.net/some-img.gif`
- `spinner` (boolean | Component, optional)
  Useful for notifications that are updated; Displays a Quasar spinner instead of an avatar or icon; If value is Boolean 'true' then the default QSpinner is shown
  quasar.config file type: `boolean | string`
  Examples: `true`, `QSpinnerBars`
- `spinnerColor` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `spinnerSize` (string, optional)
  Size in CSS units, including unit name
  Examples: `'16px'`, `'2rem'`
- `position` (string, optional), default `'bottom'`
  Window side/corner to stick to
  Accepts: `'top-left'`, `'top-right'`, `'bottom-left'`, `'bottom-right'`, `'top'`, `'bottom'`, `'left'`, `'right'`, `'center'`
- `group` (boolean | string | number, optional), default `message + caption + multiline + actions labels + position`
  Override the auto generated group with custom one; Grouped notifications cannot be updated; String or number value inform this is part of a specific group, regardless of its options; When a new notification is triggered with same group name, it replaces the old one and shows a badge with how many times the notification was triggered
  Examples: `'my-group'`
- `badgeColor` (string, optional)
  Color name for the badge from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `badgeTextColor` (string, optional)
  Color name for the badge text from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `badgePosition` (string, optional), default `top-left/top-right`
  Notification corner to stick badge to; If notification is on the left side then default is top-right otherwise it is top-left
  Accepts: `'top-left'`, `'top-right'`, `'bottom-left'`, `'bottom-right'`
- `badgeStyle` (string | any[] | object, optional)
  Style definitions to be attributed to the badge
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `badgeClass` (string | any[] | object, optional)
  Class definitions to be attributed to the badge
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `progress` (boolean, optional)
  Show progress bar to detail when notification will disappear automatically (unless timeout is 0)
- `progressClass` (string | any[] | object, optional)
  Class definitions to be attributed to the progress bar
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `classes` (string, optional)
  Add CSS class(es) to the notification for easier customization
  Examples: `'my-notif-class'`
- `attrs` (object, optional)
  Key-value for attributes to be set on the notification
  Examples: `{ role: 'alertdialog' }`
- `timeout` (number, optional), default `5000`
  Amount of time to display (in milliseconds). Set to 0 to never dismiss automatically.
- `actions` (any[], optional)
  Notification actions (buttons); Unless 'noDismiss' is true, clicking/tapping on the button will close the notification; Also check 'closeBtn' convenience prop
  Examples:
    - `[{ label: 'Show', handler: () => {}, 'aria-label': 'Button label' }, { icon: 'map', handler: () => {}, color: 'yellow' }, { label: 'Learn more', noDismiss: true, handler: () => {} }]`
  Object shape:
    - `handler` (Function, optional)
      Function to be executed when the button is clicked/tapped
      UI config only; it cannot be set from the quasar.config file.
      Examples: `() => { console.log('button clicked') }`
    - `noDismiss` (boolean, optional)
      Do not dismiss the notification when the button is clicked/tapped
    - `...` (any, optional)
      Any other QBtn prop except 'onClick' (use 'handler' instead, only possible with UI config)
      Examples: `label: 'Learn more'`, `color: 'primary'`
- `onDismiss` (Function, optional)
  Function to call when notification gets dismissed
  UI config only; it cannot be set from the quasar.config file.
  Examples: `() => { console.log('Dismissed') }`
- `closeBtn` (boolean | string, optional)
  Convenient way to add a dismiss button with a specific label, without using the 'actions' prop; If set to true, it uses a label according to the current Quasar language
  Examples: `'Close me'`
- `multiLine` (boolean, optional)
  Put notification into multi-line mode; If this prop isn't used and more than one 'action' is specified then notification goes into multi-line mode by default

