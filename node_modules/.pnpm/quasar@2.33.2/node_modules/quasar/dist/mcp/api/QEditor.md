## QEditor API

### Props

- `fullscreen` (boolean, optional, syncable)
  Fullscreen mode
  Required to be used with v-model.
  Examples: `v-model:fullscreen="isFullscreen"`
- `no-route-fullscreen-exit` (boolean, optional)
  Changing route app won't exit fullscreen
- `model-value` (string, required, syncable)
  Model of the component; Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
  Examples: `v-model="content"`
- `readonly` (boolean, optional)
  Put component in readonly mode
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `flat` (boolean, optional)
  Applies a 'flat' design (no borders)
- `dense` (boolean, optional)
  Dense mode; toolbar buttons are shown on one-line only
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `disable` (boolean, optional)
  Put component in disabled mode
- `min-height` (string, optional), default `'10rem'`
  CSS unit for the minimum height of the editable area
  Examples: `'15rem'`, `'50vh'`
- `max-height` (string, optional)
  CSS unit for maximum height of the input area
  Examples: `'1000px'`, `'90vh'`
- `height` (string, optional)
  CSS value to set the height of the editable area
  Examples: `'100px'`, `'50vh'`
- `definitions` (object, optional)
  Definition of commands and their buttons to be included in the 'toolbar' prop
  Examples:
    - `{ save: { tip: 'Save your work', icon: 'save', label: 'Save', handler: saveWork }, upload: { tip: 'Upload to cloud', icon: 'cloud_upload', label: 'Upload', handler: uploadIt } }`
  Object shape:
    - `...commandName` (object, optional)
      Command definition
      Object shape:
        - `label` (string, optional)
          Label of the button
          Examples: `'Addresses'`
        - `tip` (string, optional)
          Text to be displayed as a tooltip on hover
          Examples: `'Add a contact from the Address Book'`
        - `htmlTip` (string, optional)
          HTML formatted text to be displayed within a tooltip on hover
          Examples: `'Add a <span class="red">user</span> from the address book'`
        - `icon` (string, optional)
          Icon of the button
          Examples: `'fas fa-address-book'`
        - `key` (number, optional)
          Keycode of a key to be used together with the <ctrl> key for use as a shortcut to trigger this element
          Examples: `12`, `36`
        - `handler` (Function, optional)
          Either this or "cmd" is required. Function for when button gets clicked/tapped.
          Examples: `() => this.uploadFile()`
        - `cmd` (string, optional)
          Either this or "handler" is required. This must be a valid execCommand method according to the designMode API.
          Examples: `'insertHTML'`, `'justifyFull'`
        - `param` (string, optional)
          Only set a param if using a "cmd". This is commonly text or HTML to inject, but is highly dependent upon the specific cmd being called.
          Examples: `'<img src="://uploads/001.jpg" alt="nice pic" />'`
        - `disable` (boolean | Function, optional)
          Is button disabled?
          Function signature: `() => boolean`
          Examples: `true`, `() => !checkIfUserIsActive()`
          Returns: `boolean`
            If true, the button will be disabled
        - `type` (string, optional)
          Pass the value "no-state" if the button should not have an "active" state
          Accepts: `null`, `'no-state'`
          Examples: `'no-state'`
        - `fixedLabel` (boolean, optional)
          Lock the button label, so it doesn't change based on the child option selected.
        - `fixedIcon` (boolean, optional)
          Lock the button icon, so it doesn't change based on the child option selected.
        - `highlight` (boolean, optional)
          Highlight the toolbar button, when a child option has been selected.
- `fonts` (object, optional)
  Object with definitions of fonts
  Examples:
    - `{ arial: 'Arial', arial_black: 'Arial Black', comic_sans: 'Comic Sans MS' }`
- `toolbar` (any[], optional), default `[['left', 'center', 'right', 'justify'], ['bold', 'italic', 'underline', 'strike'], ['undo', 'redo']]`
  An array of arrays of Objects/Strings that you use to define the construction of the elements and commands available in the toolbar
  Examples:
    - `['left', 'center', 'right', 'justify']`
- `toolbar-color` (string, optional)
  Font color (from the Quasar Palette) of buttons and text in the toolbar
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `toolbar-text-color` (string, optional)
  Text color (from the Quasar Palette) of toolbar commands
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `toolbar-toggle-color` (string, optional), default `'primary'`
  Choose the active color (from the Quasar Palette) of toolbar commands button
  Examples: `'secondary'`, `'blue-3'`
- `toolbar-bg` (string, optional), default `'grey-3'`
  Toolbar background color (from Quasar Palette)
  Examples: `'secondary'`, `'blue-3'`
- `toolbar-outline` (boolean, optional)
  Toolbar buttons are rendered "outlined"
- `toolbar-push` (boolean, optional)
  Toolbar buttons are rendered as a "push-button" type
- `toolbar-rounded` (boolean, optional)
  Toolbar buttons are rendered "rounded"
- `paragraph-tag` (string, optional), default `'div'`
  Paragraph tag to be used
  Accepts: `'div'`, `'p'`
- `content-style` (object, optional)
  Object with CSS properties and values for styling the container of QEditor
  Examples: `{ backgroundColor: '#C0C0C0' }`
- `content-class` (string | any[] | object, optional)
  CSS classes for the input area
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `placeholder` (string, optional)
  Text to display as placeholder
  Examples: `'Type your story here ...'`
- `dropdown-hover` (boolean, optional) *(added v2.30)*
  Toolbar dropdowns also open when the pointer hovers their button and close when it leaves both the button and the menu; Click/tap and keyboard interactions keep toggling as usual (touch devices fall back to them)
- `dropdown-hover-delay` (number, optional), default `0` *(added v2.30)*
  Delay (in milliseconds) between the pointer entering a toolbar dropdown's button and the dropdown showing up; Requires the 'dropdown-hover' prop
- `dropdown-hover-hide-delay` (number, optional), default `150` *(added v2.30)*
  Grace period (in milliseconds) in which the pointer can re-enter a toolbar dropdown's button or its menu before the dropdown gets closed; Requires the 'dropdown-hover' prop

### Computed Props

- `caret` (object, optional)
  The current caret state

### Methods

- `toggleFullscreen(): void`
  Toggle the view to be fullscreen or not fullscreen
- `setFullscreen(): void`
  Enter the fullscreen view
- `exitFullscreen(): void`
  Leave the fullscreen view
- `runCmd(cmd: string, param?: string, update?: boolean): void`
  Run contentEditable command at caret position and range
  Params:
    - `cmd` (string, required)
      Must be a valid execCommand method according to the designMode API
      Examples: `'copy'`, `'cut'`, `'paste'`
    - `param` (string, optional)
      The argument to pass to the command
      Examples: `'<small>Small Text</small>'`
    - `update` (boolean, optional), default `true`
      Refresh the toolbar
- `refreshToolbar(): void`
  Hide the link editor if visible and force the instance to re-render
- `focus(): void`
  Focus on the contentEditable at saved cursor position
- `getContentEl(): Element`
  Retrieve the content of the Editor
  Returns: `Element`
    Provides the pure HTML within the editable area

### Events

- `@fullscreen`
  Emitted when fullscreen state changes
  Params:
    - `value` (boolean, optional)
      Fullscreen state (showing/hidden)
- `@update:fullscreen`
  Used by Vue on 'v-model:fullscreen' prop for updating its value
  Params:
    - `value` (boolean, optional)
      Fullscreen state (showing/hidden)
- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (string, required)
      The pure HTML of the content
- `@dropdown-show`
  Emitted after a dropdown in the toolbar has triggered show()
  Params:
    - `evt` (Event, optional)
      JS event object
- `@dropdown-before-show`
  Emitted when a dropdown in the toolbar triggers show() but before it finishes doing it
  Params:
    - `evt` (Event, optional)
      JS event object
- `@dropdown-hide`
  Emitted after a dropdown in the toolbar has triggered hide()
  Params:
    - `evt` (Event, optional)
      JS event object
- `@dropdown-before-hide`
  Emitted when a dropdown in the toolbar triggers hide() but before it finishes doing it
  Params:
    - `evt` (Event, optional)
      JS event object
- `@link-show`
  Emitted when the toolbar for editing a link is shown
- `@link-hide`
  Emitted when the toolbar for editing a link is hidden

### Slots

- `#[command]`
  Content for the given command in the toolbar

