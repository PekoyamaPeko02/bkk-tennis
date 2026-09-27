## Loading API

### Props

- `isActive` (boolean, optional, reactive)
  Is Loading active?

### Methods

- `show(opts?: object): Function`
  Activate and show
  Params:
    - `opts` (object, optional)
      All props are optional
      Object shape:
        - `delay` (number, optional)
          Wait a number of millisecond before showing; Not worth showing for 100ms for example then hiding it, so wait until you're sure it's a process that will take some considerable amount of time
        - `message` (string, optional)
          Message to display
          Examples: `'Processing your request'`
        - `group` (string, optional)
          Loading group name
          Examples: `'some-api-call'`
        - `html` (boolean, optional)
          Render the message as HTML; This can lead to XSS attacks so make sure that you sanitize the message first
        - `boxClass` (string, optional)
          Content wrapped element custom classes
          Examples: `'bg-amber text-black'`, `'q-pa-xl'`
        - `spinnerSize` (number, optional)
          Spinner size (in pixels)
        - `spinnerColor` (string, optional)
          Color name for spinner from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `messageColor` (string, optional)
          Color name for text from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `backgroundColor` (string, optional)
          Color name for background from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `spinner` (Component, optional)
          One of the QSpinners
        - `customClass` (string, optional)
          Add a CSS class to easily customize the component
          Examples: `'my-class'`
        - `ignoreDefaults` (boolean, optional)
          Ignore the default configuration (set by setDefaults()) for this instance only
  Returns: `Function`
    Calling this function with no parameters hides the group; When called with one Object parameter then it updates the Loading group (specified properties are shallow merged with the group ones; note that group cannot be changed while updating and it is ignored)
    Function signature: `(props?: object) => void`
    Params:
      - `props` (object, optional)
        Loading properties that will be shallow merged to the group ones; (See 'opts' param of 'show()' for object properties, except 'group')
- `hide(group?: string): void`
  Hide it
  Params:
    - `group` (string, optional)
      Optional Loading group name to hide instead of hiding all groups
      Examples: `'some-api-call'`
- `setDefaults(opts: object): void`
  Merge options into the default ones
  Params:
    - `opts` (object, required)
      Pick the subprop you want to define
      Object shape:
        - `delay` (number, optional)
          Wait a number of millisecond before showing; Not worth showing for 100ms for example then hiding it, so wait until you're sure it's a process that will take some considerable amount of time
        - `message` (string, optional)
          Message to display
          Examples: `'Processing your request'`
        - `group` (string, optional), default `'__default_quasar_group__'`
          Default Loading group name
          Examples: `'default-group-name'`
        - `spinnerSize` (number, optional)
          Spinner size (in pixels)
        - `spinnerColor` (string, optional)
          Color name for spinner from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `messageColor` (string, optional)
          Color name for text from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `backgroundColor` (string, optional)
          Color name for background from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `spinner` (Component, optional)
          One of the QSpinners
        - `customClass` (string, optional)
          Add a CSS class to easily customize the component
          Examples: `'my-class'`

### Vue Injection

Accessible via `$q.loading` (e.g., `this.$q.loading` in Options API or `useQuasar().loading` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.loading` (object)

- `delay` (number, optional)
  Wait a number of millisecond before showing; Not worth showing for 100ms for example then hiding it, so wait until you're sure it's a process that will take some considerable amount of time
  Examples: `400`
- `message` (string, optional)
  Message to display
  Examples: `'Processing your request'`
- `group` (string, optional), default `'__default_quasar_group__'`
  Default Loading group name
  Examples: `'default-group-name'`
- `html` (boolean, optional)
  Force render the message as HTML; This can lead to XSS attacks so make sure that you sanitize the content
- `boxClass` (string, optional)
  Content wrapped element custom classes
  Examples: `'bg-amber text-black'`, `'q-pa-xl'`
- `spinnerSize` (number, optional)
  Spinner size (in pixels)
- `spinnerColor` (string, optional)
  Color name for spinner from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `messageColor` (string, optional)
  Color name for text from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `backgroundColor` (string, optional)
  Color name for background from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `spinner` (Component, optional)
  One of the QSpinners
  quasar.config file type: `string`
  Examples: `QSpinnerAudio`
- `customClass` (string, optional)
  Add a CSS class to the container element to easily customize the component
  Examples: `'my-class'`

