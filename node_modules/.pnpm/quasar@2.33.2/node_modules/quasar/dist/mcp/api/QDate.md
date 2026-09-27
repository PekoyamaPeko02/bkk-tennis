## QDate API

### Props

- `name` (string, optional)
  Used to specify the name of the control; Useful if dealing with forms submitted directly to a URL
  Examples: `'car_id'`
- `landscape` (boolean, optional)
  Display the component in landscape mode
- `mask` (string, optional), default `'YYYY/MM/DD'`
  Mask (formatting string) used for parsing and formatting value
  Examples:
    - `'YYYY-MM-DD'`
    - `'MMMM Do, YYYY'`
    - `'YYYY-MM-DD HH:mm:ss'`
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
- `model-value` (string | any[] | object, required, syncable), default `null`
  Date(s) of the component; Must be Array if using 'multiple' prop; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples:
    - `v-model="myDate"`
    - `v-model="[myDate1, myDate2]"`
    - `v-model="[{ from: myDateFrom, to: myDateTo }]"`
    - `v-model="[myDate1, { from: myDateFrom, to: myDateTo }, myDate2]"`
- `title` (string, optional)
  When specified, it overrides the default header title; Makes sense when not in 'minimal' mode
  Examples: `'Birthday'`
- `subtitle` (string, optional)
  When specified, it overrides the default header subtitle; Makes sense when not in 'minimal' mode
  Examples: `'John Doe'`
- `default-year-month` (string, optional)
  The default year and month to display (in YYYY/MM format) when model is unfilled (undefined or null); Please ensure it is within the navigation min/max year-month (if using them)
  Examples: `'1986/02'`
- `default-view` (string, optional), default `'Calendar'`
  The view which will be displayed by default
  Accepts: `'Calendar'`, `'Months'`, `'Years'`
- `years-in-month-view` (boolean, optional)
  Show the years selector in months view
- `events` (any[] | Function, optional)
  A list of events to highlight on the calendar; If using an Array, its entries must be in YYYY/MM/DD format, regardless of the 'mask' in use; If using a function, it receives the date as a String (in YYYY/MM/DD format) and must return a Boolean (matches or not); If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(date?: string) => boolean`
  Examples:
    - `['2018/11/05', '2018/11/06', '2018/11/09', '2018/11/23']`
    - `date => (date[ 9 ] % 3 === 0)`
  Params:
    - `date` (string, optional)
      The current date being processed.
      Examples: `'2018/11/05'`, `'2021/10/25'`
  Returns: `boolean`
    If true, the current date will be highlighted
- `event-color` (string | Function, optional)
  Color name (from the Quasar Color Palette); If using a function, it receives the date as a String and must return a String (color for the received date); If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(date?: string) => string`
  Examples: `'teal-10'`, `date => (date[ 9 ] % 2 === 0 ? 'teal' : 'orange')`
  Params:
    - `date` (string, optional)
      The current date being processed.
      Examples: `'2018/11/05'`, `'2021/10/25'`
  Returns: `string`
    Color for the current date.
    Examples: `'teal'`, `'orange'`
- `options` (any[] | Function, optional)
  Optionally configure the days that are selectable; If using an Array, its entries must be in YYYY/MM/DD format, regardless of the 'mask' in use; If using a function, it receives the date as a String (in YYYY/MM/DD format) and must return a Boolean (is date acceptable or not); If using a function then for best performance, reference it from your scope and do not define it inline; Incompatible with 'range' prop
  Function signature: `(date?: string) => boolean`
  Examples:
    - `['2018/11/05', '2018/11/12', '2018/11/19', '2018/11/26']`
    - `date => (date[ 9 ] % 3 === 0)`
    - `date => (date >= '2018/11/03' && date <= '2018/11/15')`
  Params:
    - `date` (string, optional)
      The current date being processed.
      Examples: `'2018/11/05'`, `'2021/10/25'`
  Returns: `boolean`
    If true, the current date will be made available for selection
- `navigation-min-year-month` (string, optional)
  Lock user from navigating below a specific year+month (in YYYY/MM format); This prop is not used to correct the model; You might want to also use 'default-year-month' prop
  Examples: `'2020/07'`
- `navigation-max-year-month` (string, optional)
  Lock user from navigating above a specific year+month (in YYYY/MM format); This prop is not used to correct the model; You might want to also use 'default-year-month' prop
  Examples: `'2020/10'`
- `no-unset` (boolean, optional)
  Remove ability to unselect a date; It does not apply to selecting a range over already selected dates
- `first-day-of-week` (string | number, optional), default `based on configured Quasar lang language`
  Sets the day of the week that is considered the first day (0 - Sunday, 1 - Monday, ...); This day will show in the left-most column of the calendar
  Examples: `1`, `first-day-of-week="1"`, `:first-day-of-week="selectedFirstDayOfTheWeek"`
- `today-btn` (boolean, optional)
  Display a button that selects the current day
- `minimal` (boolean, optional)
  Don’t display the header
- `multiple` (boolean, optional)
  Allow multiple selection; Model must be Array
- `range` (boolean, optional)
  Allow range selection; Partial compatibility with 'options' prop: selected ranges might also include 'unselectable' days
- `emit-immediately` (boolean, optional)
  Emit model when user browses month and year too; ONLY for single selection (non-multiple, non-range)

### Methods

- `setToday(): void`
  Change model to today
- `setView(view: string): void`
  Change current view
  Params:
    - `view` (string, required)
      QDate view name
      Accepts: `'Calendar'`, `'Months'`, `'Years'`
- `offsetCalendar(type: string, descending?: boolean): void`
  Increment or decrement calendar view's month or year
  Params:
    - `type` (string, required)
      What to increment/decrement
      Accepts: `'month'`, `'year'`
    - `descending` (boolean, optional)
      Decrement?
- `setCalendarTo(year?: number, month?: number): void`
  Change current year and month of the Calendar view; It gets corrected if using navigation-min/max-year-month and sets the current view to Calendar
  Params:
    - `year` (number, optional)
      The year
    - `month` (number, optional)
      The month
- `setEditingRange(from?: object, to?: object): void`
  Configure the current editing range
  Params:
    - `from` (object, optional)
      Definition of date from where the range begins
      Object shape:
        - `year` (number, optional)
          The year
        - `month` (number, optional)
          The month
        - `day` (number, optional)
          The day of month
    - `to` (object, optional)
      Definition of date to where the range ends
      Object shape:
        - `year` (number, optional)
          The year
        - `month` (number, optional)
          The month
        - `day` (number, optional)
          The day of month

### Events

- `@update:model-value`
  Emitted when the component needs to change the model; Is also used by v-model
  Params:
    - `value` (string | any[] | object, required)
      New model value
    - `reason` (string, optional)
      Reason of the user interaction (what was picked)
      Accepts: `'add-day'`, `'remove-day'`, `'add-range'`, `'remove-range'`, `'mask'`, `'locale'`, `'year'`, `'month'`
    - `details` (object, optional)
      Object of properties on the new model
      Object shape:
        - `year` (number, required)
          The year of the date that the user has clicked/tapped on
        - `month` (number, required)
          The month of the date that the user has clicked/tapped on
        - `day` (number, required)
          The day of the month that the user has clicked/tapped on
        - `from` (object, optional)
          Object of properties of the range starting point (only if range)
          Object shape:
            - `year` (number, required)
              The year
            - `month` (number, required)
              The month
            - `day` (number, required)
              The day of month
        - `to` (object, optional)
          Object of properties of the range ending point (only if range)
          Object shape:
            - `year` (number, required)
              The year
            - `month` (number, required)
              The month
            - `day` (number, required)
              The day of month
- `@navigation`
  Emitted when user navigates to a different month or year (and even when the model changes from an outside source)
  Params:
    - `view` (object, optional)
      Definition of the current view (year, month)
      Object shape:
        - `year` (number, required)
          The year
        - `month` (number, required)
          The month
- `@range-start`
  User has started a range selection
  Params:
    - `from` (object, optional)
      Definition of date from where the range begins
      Object shape:
        - `year` (number, required)
          The year
        - `month` (number, required)
          The month
        - `day` (number, required)
          The day of month
- `@range-end`
  User has ended a range selection
  Params:
    - `range` (object, optional)
      Definition of the range
      Object shape:
        - `from` (object, required)
          Definition of date from where the range begins
          Object shape:
            - `year` (number, required)
              The year
            - `month` (number, required)
              The month
            - `day` (number, required)
              The day of month
        - `to` (object, required)
          Definition of date to where the range ends
          Object shape:
            - `year` (number, required)
              The year
            - `month` (number, required)
              The month
            - `day` (number, required)
              The day of month

### Slots

- `#default`
  This is where additional buttons can go

