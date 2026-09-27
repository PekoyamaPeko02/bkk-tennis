## QInnerLoading API

### Props

- `transition-show` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-hide` (string, optional), default `'fade'`
  One of Quasar's embedded transitions
  Examples: `'fade'`, `'slide-down'`
- `transition-duration` (string | number, optional), default `300`
  Transition duration (in milliseconds, without unit)
- `size` (string | number, optional), default `'42px'`
  Size in CSS units, including unit name, or standard size name (xs|sm|md|lg|xl), for the inner Spinner (unless using the default slot)
  Examples: `'16px'`, `'2rem'`, `'xs'`, `'md'`
- `showing` (boolean, optional)
  State - loading or not
- `color` (string, optional)
  Color name for component from the Quasar Color Palette for the inner Spinner (unless using the default slot)
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `label` (string, optional)
  Add a label; Gets overriden when using the default slot
  Examples: `'Please wait...'`
- `label-class` (string, optional)
  Add CSS class(es) to the label; Works along the 'label' prop only
  Examples: `'text-red q-mt-xl'`
- `label-style` (string | any[] | object, optional)
  Apply custom style to the label; Works along the 'label' prop only
  Examples: `'font-size: 28px'`, `{ color: '#ff0000' }`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color

### Slots

- `#default`
  Default slot is used for replacing default Spinner; Suggestions: a spinner or text

