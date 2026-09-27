## QTime API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `landscape` (boolean, optional)
  Display the component in landscape mode
- `mask` (string, optional), default `'HH:mm'`
  Mask (formatting string) used for parsing and formatting value
  Examples:
    - `'HH:mm:ss'`
    - `'YYYY-MM-DD HH:mm:ss'`
    - `'HH:mm MMMM Do, YYYY'`
- `locale` (object, optional)
  Locale formatting options
  Examples:
    - `{ monthsShort: [ 'Ian', 'Feb', 'Mar', '...' ] }`
  Object shape:
    - `days` (any[], optional)
      List of full day names (DDDD), starting with Sunday
      Examples:
        - `['Duminica', 'Luni', 'Marti', '...']`
    - `daysShort` (any[], optional)
      List of short day names (DDD), starting with Sunday
      Examples:
        - `['Dum', 'Lun', 'Mar', '...']`
    - `months` (any[], optional)
      List of full month names (MMMM), starting with January
      Examples:
        - `['Ianuarie', 'Februarie', 'Martie', '...']`
    - `monthsShort` (any[], optional)
      List of short month names (MMM), starting with January
      Examples:
        - `['Ian', 'Feb', 'Mar', '...']`
- `calendar` (string, optional), default `'gregorian'`
  Specify calendar type
  Accepts: `'gregorian'`, `'persian'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow)
- `bordered` (boolean, optional)
  Applies a default border to the component
- `readonly` (boolean, optional)
  Put component in readonly mode
- `disable` (boolean, optional)
  Put component in disabled mode
- `model-value` (string, required, syncable), default `null`
  Time of the component; Either use this property (along with a listener for 'update:modelValue' event) OR use v-model directive
  Examples: `v-model="currentTime"`
- `format24h` (boolean, optional), default `null`
  Forces 24 hour time display instead of AM/PM system; If prop is not set, then the default is based on Quasar lang language being used
- `default-date` (string, optional), default `current day`
  The default date to use (in YYYY/MM/DD format) when model is unfilled (undefined or null)
  Examples: `'1995/02/23'`
- `options` (Function, optional)
  Optionally configure what time is the user allowed to set; Overridden by 'hour-options', 'minute-options' and 'second-options' if those are set; For best performance, reference it from your scope and do not define it inline
  Function signature: `(hr?: number, min?: number, sec?: number) => boolean`
  Examples:
    - `(hr, min, sec) => hr <= 6`
  Params:
    - `hr` (number, optional)
      Hour
    - `min` (number, optional)
      Minutes
    - `sec` (number, optional)
      Seconds
  Returns: `boolean`
    Is the user allowed to set the specified time?
- `hour-options` (any[], optional)
  Optionally configure what hours is the user allowed to set; Overrides 'options' prop if that is also set
  Examples:
    - `[3, 6, 9]`
- `minute-options` (any[], optional)
  Optionally configure what minutes is the user allowed to set; Overrides 'options' prop if that is also set
  Examples:
    - `[0, 15, 30, 45]`
- `second-options` (any[], optional)
  Optionally configure what seconds is the user allowed to set; Overrides 'options' prop if that is also set
  Examples:
    - `[0, 7, 10, 23]`
- `with-seconds` (boolean, optional)
  Allow the time to be set with seconds
- `now-btn` (boolean, optional)
  Display a button that selects the current time

### Methods

- `setNow(): void`
  Change model to current moment

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (string, required)
      New model value
    - `details` (object, optional)
      Object of properties on the new model
      Object shape:
        - `year` (number, required)
          The year
        - `month` (number, required)
          The month
        - `day` (number, required)
          The day of the month
        - `hour` (number, required)
          The hour
        - `minute` (number, required)
          The minute
        - `second` (number, required)
          The second
        - `millisecond` (number, required)
          The millisecond
        - `changed` (boolean, required)
          Did the model change?

### Slots

- `#default`
  This is where additional buttons can go

