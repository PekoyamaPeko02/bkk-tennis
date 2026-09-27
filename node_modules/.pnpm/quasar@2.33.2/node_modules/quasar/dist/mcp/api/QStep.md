## QStep API

### Props

- `done-icon` (string, optional)
  Icon name following Quasar convention; If 'none' (String) is used as value, then it will defer to prefix or the regular icon for this state; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `done-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `active-icon` (string, optional)
  Icon name following Quasar convention; If 'none' (String) is used as value, then it will defer to prefix or the regular icon for this state; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `active-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `error-icon` (string, optional)
  Icon name following Quasar convention; If 'none' (String) is used as value, then it will defer to prefix or the regular icon for this state; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `error-color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `header-nav` (boolean, optional), default `true`
  Allow navigation through the header
- `name` (any, required)
  Panel name
  Examples: `'accounts'`, `'firstPanel'`, `1`
- `disable` (boolean, optional)
  Put component in disabled mode
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `title` (string, required)
  Step title
  Examples: `'Ad Groups'`, `'Payment'`
- `caption` (string, optional)
  Step’s additional information that appears beneath the title
  Examples: `'Create an account'`, `'Payment details'`
- `prefix` (string | number, optional)
  Step's prefix (max 2 characters) which replaces the icon if step does not has error, is being edited or is marked as done
  Examples: `'1'`, `2`, `'A'`, `'B'`
- `done` (boolean, optional)
  Mark the step as 'done'
- `error` (boolean, optional)
  Mark the step as having an error

### Slots

- `#default`
  The content of the step; Can also contain a QStepperNavigation if you want to handle step navigation and don't have a global navigation in place

