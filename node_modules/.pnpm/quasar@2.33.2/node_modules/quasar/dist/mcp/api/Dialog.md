## Dialog API

### Methods

- `create(opts: object): object`
  Creates an ad-hoc Dialog; Same as calling $q.dialog(...)
  Params:
    - `opts` (object, required)
      Dialog options
      Object shape:
        - `class` (string | any[] | object, optional)
          CSS Class name to apply to the Dialog's QCard
          Examples: `'my-class'`
        - `style` (string | any[] | object, optional)
          CSS style to apply to the Dialog's QCard
          Examples: `'border: 2px solid black'`
        - `title` (string, optional)
          A text for the heading title of the dialog
          Examples: `'Continue?'`
        - `message` (string, optional)
          A text with more information about what needs to be input, selected or confirmed.
          Examples: `'Are you certain you want to continue?'`
        - `html` (boolean, optional)
          Render title and message as HTML; This can lead to XSS attacks, so make sure that you sanitize the message first
        - `position` (string, optional), default `'standard'`
          Position of the Dialog on screen. Standard is centered.
          Accepts: `'top'`, `'right'`, `'bottom'`, `'left'`, `'standard'`
        - `prompt` (object, optional)
          An object definition of the input field for the prompting question.
          Examples:
            - `{ model: 'initial-value', type: 'number' }`
          Object shape:
            - `model` (string, required)
              The initial value of the input
            - `type` (string, optional), default `'text'`
              Optional property to determine the input field type
              Examples: `'text'`, `'number'`, `'textarea'`
            - `isValid` (Function, optional)
              Is typed content valid?
              Function signature: `(val: string) => boolean`
              Params:
                - `val` (string, required)
                  The value of the input
              Returns: `boolean`
                The text passed validation or not
            - `...QInputProps` (any, optional)
              Any QInput props, like color, label, stackLabel, filled, outlined, rounded, prefix etc
              Examples: `label: 'My Label'`, `standout: true`, `counter: true`, `maxlength: 12`
            - `...nativeAttributes` (object, optional)
              Any native attributes to pass to the prompt control
              Examples: `autocomplete: 'off'`
        - `options` (object, optional)
          An object definition for creating the selection form content
          Examples:
            - `{ model: null, type: 'radio', items: [ /* ...listOfItems */ ] }`
          Object shape:
            - `model` (string | any[], required)
              The value of the selection (String if it's of type radio or Array otherwise)
              Examples: `[]`
            - `type` (string, optional), default `'radio'`
              The type of selection
              Accepts: `'radio'`, `'checkbox'`, `'toggle'`
            - `items` (any[], optional)
              The list of options to interact with; Equivalent to options prop of the QOptionGroup component
              Examples:
                - `[{ label: 'Option 1', value: 'op1' }, { label: 'Option 2', value: 'op2' }, { label: 'Option 3', value: 'op3' }]`
            - `isValid` (Function, optional)
              Is the model valid?
              Function signature: `(model: string | any[]) => boolean`
              Params:
                - `model` (string | any[], required)
                  The current model (String if it's of type radio or Array otherwise)
                  Examples:
                    - `'opt2'`
                    - `['opt1']`
                    - `[]`
                    - `['opt1', 'opt3']`
              Returns: `boolean`
                The selection passed validation or not
            - `...QOptionGroupProps` (any, optional)
              Any QOptionGroup props
              Examples: `color: 'deep-purple-4'`, `inline: true`, `dense: true`, `leftLabel: true`
            - `...nativeAttributes` (object, optional)
              Any native attributes to pass to the inner QOptionGroup
        - `progress` (boolean | object, optional)
          Display a Quasar spinner (if value is true, then the defaults are used); Useful for conveying the idea that something is happening behind the covers; Tip: use along with persistent, ok: false and update() method
          Object shape:
            - `spinner` (Component, optional)
              One of the QSpinners
            - `color` (string, optional)
              Color name for component from the Quasar Color Palette
              Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `ok` (string | object | boolean, optional)
          Props for an 'OK' button
          Object shape:
            - `...props` (any, optional)
              See QBtn for available props
        - `cancel` (string | object | boolean, optional)
          Props for a 'CANCEL' button
          Object shape:
            - `...props` (any, optional)
              See QBtn for available props
        - `focus` (string, optional), default `'ok'`
          What button to focus, unless you also have 'prompt' or 'options'
          Accepts: `'ok'`, `'cancel'`, `'none'`
        - `stackButtons` (boolean, optional)
          Makes buttons be stacked instead of vertically aligned
        - `color` (string, optional)
          Color name for component from the Quasar Color Palette
          Examples: `'primary'`, `'teal'`, `'teal-10'`
        - `dark` (boolean, optional), default `null`
          Apply dark mode
        - `persistent` (boolean, optional)
          User cannot dismiss Dialog if clicking outside of it or hitting ESC key; Also, an app route change won't dismiss it
        - `noEscDismiss` (boolean, optional)
          User cannot dismiss Dialog by hitting ESC key; No need to set it if 'persistent' prop is also set
        - `noBackdropDismiss` (boolean, optional)
          User cannot dismiss Dialog by clicking outside of it; No need to set it if 'persistent' prop is also set
        - `noRouteDismiss` (boolean, optional)
          Changing route app won't dismiss Dialog; No need to set it if 'persistent' prop is also set
        - `seamless` (boolean, optional)
          Put Dialog into seamless mode; Does not use a backdrop so user is able to interact with the rest of the page too
        - `maximized` (boolean, optional)
          Put Dialog into maximized mode
        - `fullWidth` (boolean, optional)
          Dialog will try to render with same width as the window
        - `fullHeight` (boolean, optional)
          Dialog will try to render with same height as the window
        - `transitionShow` (string, optional), default `'scale'`
          One of Quasar's embedded transitions
          Examples: `'fade'`, `'slide-down'`
        - `transitionHide` (string, optional), default `'scale'`
          One of Quasar's embedded transitions
          Examples: `'fade'`, `'slide-down'`
        - `component` (Component | string, optional)
          Use custom dialog component; use along with 'componentProps' prop where possible
          Examples: `CustomComponent`, `'custom-component'`
        - `componentProps` (object, optional)
          User defined props which will be forwarded to underlying custom component if 'component' prop is used; May also include any built-in QDialog option such as 'persistent' or 'seamless'
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

Accessible via `$q.dialog` (e.g., `this.$q.dialog` in Options API or `useQuasar().dialog` in Composition API).

