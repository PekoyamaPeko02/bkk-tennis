## Lang API

### Props

- `props` (object, optional)
  Quasar language pack
  Object shape:
    - `isoName` (string, optional)
      The ISO name of the Quasar language pack
      Examples: `'en-US'`
    - `nativeName` (string, optional)
      The native name of the Quasar language pack
      Examples: `'English (US)'`
    - `rtl` (boolean, optional)
      Whether the language is RTL (right-to-left)
      Examples: `false`
    - `formatNumber` (Function, optional)
      Renders a number displayed by a Quasar component (QDate, QTime, QPagination) in the locale's digits; receives the ASCII digit string, zero-padded where the component pads it
      Function signature: `(value?: string) => string`
      Examples:
        - `value => value.replace(/\d/g, digit => '۰۱۲۳۴۵۶۷۸۹'[digit])`
      Params:
        - `value` (string, optional)
          The ASCII digit string the component would display
          Examples: `'05'`, `'1403'`
      Returns: `string`
        The localized digit string
        Examples: `'۰۵'`, `'۱۴۰۳'`
    - `label` (object, optional)
      Generic labels
      Object shape:
        - `clear` (string, optional)
          Label
          Examples: `'Clear'`
        - `ok` (string, optional)
          Label
          Examples: `'OK'`
        - `cancel` (string, optional)
          Label
          Examples: `'Cancel'`
        - `close` (string, optional)
          Label
          Examples: `'Close'`
        - `set` (string, optional)
          Label
          Examples: `'Set'`
        - `select` (string, optional)
          Label
          Examples: `'Select'`
        - `reset` (string, optional)
          Label
          Examples: `'Reset'`
        - `remove` (string, optional)
          Label
          Examples: `'Remove'`
        - `update` (string, optional)
          Label
          Examples: `'Update'`
        - `create` (string, optional)
          Label
          Examples: `'Create'`
        - `search` (string, optional)
          Label
          Examples: `'Search'`
        - `filter` (string, optional)
          Label
          Examples: `'Filter'`
        - `refresh` (string, optional)
          Label
          Examples: `'Refresh'`
        - `minimum` (string, optional)
          Aria-label for the QRange minimum-value thumb
          Examples: `'Minimum'`
        - `maximum` (string, optional)
          Aria-label for the QRange maximum-value thumb
          Examples: `'Maximum'`
        - `range` (string, optional)
          Aria-label for the QRange track when it drags the whole selected window (drag-range/drag-only-range)
          Examples: `'Range'`
        - `noValue` (string, optional)
          Aria-valuetext for a QSlider/QRange thumb whose model side is null
          Examples: `'No value'`
        - `resize` (string, optional)
          Aria-label for the QSplitter separator
          Examples: `'Resize'`
        - `expand` (Function, optional)
          Label function
          Function signature: `(label?: string) => string`
          Examples: `label => (label ? `Expand '${ label }'` : 'Expand')`
          Params:
            - `label` (string, optional)
              Item to expand
          Returns: `string`
            Label
            Examples: `'Expand'`
        - `collapse` (Function, optional)
          Label function
          Function signature: `(label?: string) => string`
          Examples: `label => (label ? `Collapse '${ label }'` : 'Collapse')`
          Params:
            - `label` (string, optional)
              Item to collapse
          Returns: `string`
            Label
            Examples: `'Collapse'`
    - `date` (object, optional)
      QDate/QTime labels
      Object shape:
        - `days` (any[], optional)
          Label
          Examples:
            - `['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']`
        - `daysShort` (any[], optional)
          Label
          Examples:
            - `['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']`
        - `months` (any[], optional)
          Label
          Examples:
            - `['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']`
        - `monthsShort` (any[], optional)
          Label
          Examples:
            - `['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']`
        - `firstDayOfWeek` (number, optional)
          0-6, 0 - Sunday, 1 Monday, ...
          Examples: `0`
        - `format24h` (boolean, optional)
          Uses 24-hour format
        - `pluralDay` (string, optional)
          Label
          Examples: `'days'`
        - `prevMonth` (string, optional)
          Aria-label for the QDate previous month button
          Examples: `'Previous month'`
        - `nextMonth` (string, optional)
          Aria-label for the QDate next month button
          Examples: `'Next month'`
        - `prevYear` (string, optional)
          Aria-label for the QDate previous year button
          Examples: `'Previous year'`
        - `nextYear` (string, optional)
          Aria-label for the QDate next year button
          Examples: `'Next year'`
        - `today` (string, optional)
          Aria-label for the QDate today button
          Examples: `'Today'`
        - `prevRangeYears` (Function, optional)
          Aria-label for the QDate previous years range button
          Function signature: `(range?: number) => string`
          Examples: `range => `Previous ${ range } years``
          Params:
            - `range` (number, optional)
              Number of years in the range
              Examples: `20`
          Returns: `string`
            The label
        - `nextRangeYears` (Function, optional)
          Aria-label for the QDate next years range button
          Function signature: `(range?: number) => string`
          Examples: `range => `Next ${ range } years``
          Params:
            - `range` (number, optional)
              Number of years in the range
              Examples: `20`
          Returns: `string`
            The label
        - `hour` (string, optional)
          Aria-label for the QTime hour control
          Examples: `'Hour'`
        - `minute` (string, optional)
          Aria-label for the QTime minute control
          Examples: `'Minute'`
        - `second` (string, optional)
          Aria-label for the QTime second control
          Examples: `'Second'`
        - `now` (string, optional)
          Aria-label for the QTime 'now' button
          Examples: `'Current time'`
    - `table` (object, optional)
      QTable labels
      Object shape:
        - `noData` (string, optional)
          Label
          Examples: `'No data available'`
        - `noResults` (string, optional)
          Label
          Examples: `'No matching records found'`
        - `loading` (string, optional)
          Label
          Examples: `'Loading...'`
        - `selectedRecords` (Function, optional)
          Label function
          Function signature: `(rows: number) => string`
          Examples: `rows => `${ rows } records selected``
          Params:
            - `rows` (number, required)
              Number of selected rows
              Examples: `5`
          Returns: `string`
            Label
            Examples: `'5 records selected'`
        - `recordsPerPage` (string, optional)
          Label
          Examples: `'Records per page:'`
        - `allRows` (string, optional)
          Label
          Examples: `'All'`
        - `pagination` (Function, optional)
          Label function
          Function signature: `(start: number, end: number, total: number) => string`
          Examples:
            - `(start, end, total) => start + '–' + end + ' of ' + total`
          Params:
            - `start` (number, required)
              Page start index
              Examples: `5`
            - `end` (number, required)
              Page end index
              Examples: `10`
            - `total` (number, required)
              Total number of rows
              Examples: `50`
          Returns: `string`
            Label
            Examples: `'5–10 of 50'`
        - `columns` (string, optional)
          Label
          Examples: `'Columns'`
        - `selectAllRows` (string, optional)
          Accessible name (aria-label) of the select-all-rows checkbox
          Examples: `'Select all rows'`
        - `selectRow` (string, optional)
          Accessible name (aria-label) of a row selection checkbox
          Examples: `'Select row'`
    - `carousel` (object, optional)
      QCarousel labels
      Object shape:
        - `prevSlide` (string, optional)
          Accessible name (aria-label) of the previous-slide arrow
          Examples: `'Previous slide'`
        - `nextSlide` (string, optional)
          Accessible name (aria-label) of the next-slide arrow
          Examples: `'Next slide'`
    - `colorPicker` (object, optional)
      QColor labels
      Object shape:
        - `spectrum` (string, optional)
          Accessible name (aria-label) of the spectrum tab
          Examples: `'Spectrum'`
        - `tune` (string, optional)
          Accessible name (aria-label) of the tune tab
          Examples: `'Tune'`
        - `palette` (string, optional)
          Accessible name (aria-label) of the palette tab
          Examples: `'Palette'`
        - `value` (string, optional)
          Accessible name (aria-label) of the color value input
          Examples: `'Color value'`
        - `hue` (string, optional)
          Accessible name (aria-label) of the hue slider
          Examples: `'Hue'`
        - `alpha` (string, optional)
          Accessible name (aria-label) of the opacity slider
          Examples: `'Opacity'`
        - `saturation` (string, optional)
          Label of the saturation axis in the spectrum panel's accessible value (aria-valuetext)
          Examples: `'Saturation'`
        - `brightness` (string, optional)
          Label of the brightness axis in the spectrum panel's accessible value (aria-valuetext)
          Examples: `'Brightness'`
    - `uploader` (object, optional)
      QUploader labels
      Object shape:
        - `addFiles` (string, optional)
          Accessible name (aria-label) of the pick-files button
          Examples: `'Pick files'`
        - `upload` (string, optional)
          Accessible name (aria-label) of the upload button
          Examples: `'Upload files'`
        - `abort` (string, optional)
          Accessible name (aria-label) of the abort-upload button
          Examples: `'Abort upload'`
        - `removeQueued` (string, optional)
          Accessible name (aria-label) of the remove-queued-files button
          Examples: `'Remove queued files'`
        - `removeUploaded` (string, optional)
          Accessible name (aria-label) of the remove-uploaded-files button
          Examples: `'Remove uploaded files'`
        - `removeFile` (string, optional)
          Accessible name (aria-label) of a single file removal button
          Examples: `'Remove file'`
    - `editor` (object, optional)
      QEditor labels
      Object shape:
        - `toolbar` (string, optional)
          Accessible name (aria-label) of the toolbar
          Examples: `'Editor toolbar'`
        - `url` (string, optional)
          Label
          Examples: `'URL'`
        - `bold` (string, optional)
          Label
          Examples: `'Bold'`
        - `italic` (string, optional)
          Label
          Examples: `'Italic'`
        - `strikethrough` (string, optional)
          Label
          Examples: `'Strikethrough'`
        - `underline` (string, optional)
          Label
          Examples: `'Underline'`
        - `unorderedList` (string, optional)
          Label
          Examples: `'Unordered List'`
        - `orderedList` (string, optional)
          Label
          Examples: `'Ordered List'`
        - `subscript` (string, optional)
          Label
          Examples: `'Subscript'`
        - `superscript` (string, optional)
          Label
          Examples: `'Superscript'`
        - `hyperlink` (string, optional)
          Label
          Examples: `'Hyperlink'`
        - `toggleFullscreen` (string, optional)
          Label
          Examples: `'Toggle Fullscreen'`
        - `quote` (string, optional)
          Label
          Examples: `'Quote'`
        - `left` (string, optional)
          Label
          Examples: `'Left align'`
        - `center` (string, optional)
          Label
          Examples: `'Center align'`
        - `right` (string, optional)
          Label
          Examples: `'Right align'`
        - `justify` (string, optional)
          Label
          Examples: `'Justify align'`
        - `print` (string, optional)
          Label
          Examples: `'Print'`
        - `outdent` (string, optional)
          Label
          Examples: `'Decrease indentation'`
        - `indent` (string, optional)
          Label
          Examples: `'Increase indentation'`
        - `removeFormat` (string, optional)
          Label
          Examples: `'Remove formatting'`
        - `formatting` (string, optional)
          Label
          Examples: `'Formatting'`
        - `fontSize` (string, optional)
          Label
          Examples: `'Font Size'`
        - `align` (string, optional)
          Label
          Examples: `'Align'`
        - `hr` (string, optional)
          Label
          Examples: `'Insert Horizontal Rule'`
        - `undo` (string, optional)
          Label
          Examples: `'Undo'`
        - `redo` (string, optional)
          Label
          Examples: `'Redo'`
        - `heading1` (string, optional)
          Label
          Examples: `'Heading 1'`
        - `heading2` (string, optional)
          Label
          Examples: `'Heading 2'`
        - `heading3` (string, optional)
          Label
          Examples: `'Heading 3'`
        - `heading4` (string, optional)
          Label
          Examples: `'Heading 4'`
        - `heading5` (string, optional)
          Label
          Examples: `'Heading 5'`
        - `heading6` (string, optional)
          Label
          Examples: `'Heading 6'`
        - `paragraph` (string, optional)
          Label
          Examples: `'Paragraph'`
        - `code` (string, optional)
          Label
          Examples: `'Code'`
        - `size1` (string, optional)
          Label
          Examples: `'Very small'`
        - `size2` (string, optional)
          Label
          Examples: `'A bit small'`
        - `size3` (string, optional)
          Label
          Examples: `'Normal'`
        - `size4` (string, optional)
          Label
          Examples: `'Medium-large'`
        - `size5` (string, optional)
          Label
          Examples: `'Big'`
        - `size6` (string, optional)
          Label
          Examples: `'Very big'`
        - `size7` (string, optional)
          Label
          Examples: `'Maximum'`
        - `defaultFont` (string, optional)
          Label
          Examples: `'Default Font'`
        - `viewSource` (string, optional)
          Label
          Examples: `'View Source'`
    - `tree` (object, optional)
      QTree labels
      Object shape:
        - `noNodes` (string, optional)
          Label
          Examples: `'No nodes available'`
        - `noResults` (string, optional)
          Label
          Examples: `'No matching nodes found'`

### Methods

- `set(quasarLanguagePack: object, ssrContent?: object): void`
  Set another Quasar Language Pack
  Params:
    - `quasarLanguagePack` (object, required)
      Usually you will import such an object directly from quasar (eg: import qIconSet from 'quasar/lang/<lang-name>')
      Object shape:
        - `isoName` (string, required)
          The ISO name of the Quasar language pack
          Examples: `'en-US'`
        - `nativeName` (string, required)
          The native name of the Quasar language pack
          Examples: `'English (US)'`
        - `rtl` (boolean, optional), default `true`
          Whether the language is RTL (right-to-left)
          Examples: `false`
        - `formatNumber` (Function, optional)
          Renders a number displayed by a Quasar component (QDate, QTime, QPagination) in the locale's digits; receives the ASCII digit string, zero-padded where the component pads it
          Function signature: `(value?: string) => string`
          Examples:
            - `value => value.replace(/\d/g, digit => '۰۱۲۳۴۵۶۷۸۹'[digit])`
          Params:
            - `value` (string, optional)
              The ASCII digit string the component would display
              Examples: `'05'`, `'1403'`
          Returns: `string`
            The localized digit string
            Examples: `'۰۵'`, `'۱۴۰۳'`
        - `label` (object, required)
          Generic labels
          Object shape:
            - `clear` (string, required)
              Label
              Examples: `'Clear'`
            - `ok` (string, required)
              Label
              Examples: `'OK'`
            - `cancel` (string, required)
              Label
              Examples: `'Cancel'`
            - `close` (string, required)
              Label
              Examples: `'Close'`
            - `set` (string, required)
              Label
              Examples: `'Set'`
            - `select` (string, required)
              Label
              Examples: `'Select'`
            - `reset` (string, required)
              Label
              Examples: `'Reset'`
            - `remove` (string, required)
              Label
              Examples: `'Remove'`
            - `update` (string, required)
              Label
              Examples: `'Update'`
            - `create` (string, required)
              Label
              Examples: `'Create'`
            - `search` (string, required)
              Label
              Examples: `'Search'`
            - `filter` (string, required)
              Label
              Examples: `'Filter'`
            - `refresh` (string, required)
              Label
              Examples: `'Refresh'`
            - `minimum` (string, optional)
              Aria-label for the QRange minimum-value thumb
              Examples: `'Minimum'`
            - `maximum` (string, optional)
              Aria-label for the QRange maximum-value thumb
              Examples: `'Maximum'`
            - `range` (string, optional)
              Aria-label for the QRange track when it drags the whole selected window (drag-range/drag-only-range)
              Examples: `'Range'`
            - `noValue` (string, optional)
              Aria-valuetext for a QSlider/QRange thumb whose model side is null
              Examples: `'No value'`
            - `resize` (string, optional)
              Aria-label for the QSplitter separator
              Examples: `'Resize'`
            - `expand` (Function, required)
              Label function
              Function signature: `(label?: string) => string`
              Examples: `label => (label ? `Expand '${ label }'` : 'Expand')`
              Params:
                - `label` (string, optional)
                  Item to expand
              Returns: `string`
                Label
                Examples: `'Expand'`
            - `collapse` (Function, required)
              Label function
              Function signature: `(label?: string) => string`
              Examples: `label => (label ? `Collapse '${ label }'` : 'Collapse')`
              Params:
                - `label` (string, optional)
                  Item to collapse
              Returns: `string`
                Label
                Examples: `'Collapse'`
        - `date` (object, required)
          QDate/QTime labels
          Object shape:
            - `days` (any[], required)
              Label
              Examples:
                - `['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']`
            - `daysShort` (any[], required)
              Label
              Examples:
                - `['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']`
            - `months` (any[], required)
              Label
              Examples:
                - `['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']`
            - `monthsShort` (any[], required)
              Label
              Examples:
                - `['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']`
            - `firstDayOfWeek` (number, required)
              0-6, 0 - Sunday, 1 Monday, ...
              Examples: `0`
            - `format24h` (boolean, required)
              Uses 24-hour format
            - `pluralDay` (string, required)
              Label
              Examples: `'days'`
            - `prevMonth` (string, optional)
              Aria-label for the QDate previous month button
              Examples: `'Previous month'`
            - `nextMonth` (string, optional)
              Aria-label for the QDate next month button
              Examples: `'Next month'`
            - `prevYear` (string, optional)
              Aria-label for the QDate previous year button
              Examples: `'Previous year'`
            - `nextYear` (string, optional)
              Aria-label for the QDate next year button
              Examples: `'Next year'`
            - `today` (string, optional)
              Aria-label for the QDate today button
              Examples: `'Today'`
            - `prevRangeYears` (Function, optional)
              Aria-label for the QDate previous years range button
              Function signature: `(range?: number) => string`
              Examples: `range => `Previous ${ range } years``
              Params:
                - `range` (number, optional)
                  Number of years in the range
                  Examples: `20`
              Returns: `string`
                The label
            - `nextRangeYears` (Function, optional)
              Aria-label for the QDate next years range button
              Function signature: `(range?: number) => string`
              Examples: `range => `Next ${ range } years``
              Params:
                - `range` (number, optional)
                  Number of years in the range
                  Examples: `20`
              Returns: `string`
                The label
            - `hour` (string, optional)
              Aria-label for the QTime hour control
              Examples: `'Hour'`
            - `minute` (string, optional)
              Aria-label for the QTime minute control
              Examples: `'Minute'`
            - `second` (string, optional)
              Aria-label for the QTime second control
              Examples: `'Second'`
            - `now` (string, optional)
              Aria-label for the QTime 'now' button
              Examples: `'Current time'`
        - `table` (object, required)
          QTable labels
          Object shape:
            - `noData` (string, required)
              Label
              Examples: `'No data available'`
            - `noResults` (string, required)
              Label
              Examples: `'No matching records found'`
            - `loading` (string, required)
              Label
              Examples: `'Loading...'`
            - `selectedRecords` (Function, required)
              Label function
              Function signature: `(rows: number) => string`
              Examples: `rows => `${ rows } records selected``
              Params:
                - `rows` (number, required)
                  Number of selected rows
                  Examples: `5`
              Returns: `string`
                Label
                Examples: `'5 records selected'`
            - `recordsPerPage` (string, required)
              Label
              Examples: `'Records per page:'`
            - `allRows` (string, required)
              Label
              Examples: `'All'`
            - `pagination` (Function, required)
              Label function
              Function signature: `(start: number, end: number, total: number) => string`
              Examples:
                - `(start, end, total) => start + '–' + end + ' of ' + total`
              Params:
                - `start` (number, required)
                  Page start index
                  Examples: `5`
                - `end` (number, required)
                  Page end index
                  Examples: `10`
                - `total` (number, required)
                  Total number of rows
                  Examples: `50`
              Returns: `string`
                Label
                Examples: `'5–10 of 50'`
            - `columns` (string, required)
              Label
              Examples: `'Columns'`
            - `selectAllRows` (string, required)
              Accessible name (aria-label) of the select-all-rows checkbox
              Examples: `'Select all rows'`
            - `selectRow` (string, required)
              Accessible name (aria-label) of a row selection checkbox
              Examples: `'Select row'`
        - `carousel` (object, optional)
          QCarousel labels
          Object shape:
            - `prevSlide` (string, optional)
              Accessible name (aria-label) of the previous-slide arrow
              Examples: `'Previous slide'`
            - `nextSlide` (string, optional)
              Accessible name (aria-label) of the next-slide arrow
              Examples: `'Next slide'`
        - `colorPicker` (object, optional)
          QColor labels
          Object shape:
            - `spectrum` (string, required)
              Accessible name (aria-label) of the spectrum tab
              Examples: `'Spectrum'`
            - `tune` (string, required)
              Accessible name (aria-label) of the tune tab
              Examples: `'Tune'`
            - `palette` (string, required)
              Accessible name (aria-label) of the palette tab
              Examples: `'Palette'`
            - `value` (string, required)
              Accessible name (aria-label) of the color value input
              Examples: `'Color value'`
            - `hue` (string, required)
              Accessible name (aria-label) of the hue slider
              Examples: `'Hue'`
            - `alpha` (string, required)
              Accessible name (aria-label) of the opacity slider
              Examples: `'Opacity'`
            - `saturation` (string, required)
              Label of the saturation axis in the spectrum panel's accessible value (aria-valuetext)
              Examples: `'Saturation'`
            - `brightness` (string, required)
              Label of the brightness axis in the spectrum panel's accessible value (aria-valuetext)
              Examples: `'Brightness'`
        - `uploader` (object, optional)
          QUploader labels
          Object shape:
            - `addFiles` (string, required)
              Accessible name (aria-label) of the pick-files button
              Examples: `'Pick files'`
            - `upload` (string, required)
              Accessible name (aria-label) of the upload button
              Examples: `'Upload files'`
            - `abort` (string, required)
              Accessible name (aria-label) of the abort-upload button
              Examples: `'Abort upload'`
            - `removeQueued` (string, required)
              Accessible name (aria-label) of the remove-queued-files button
              Examples: `'Remove queued files'`
            - `removeUploaded` (string, required)
              Accessible name (aria-label) of the remove-uploaded-files button
              Examples: `'Remove uploaded files'`
            - `removeFile` (string, required)
              Accessible name (aria-label) of a single file removal button
              Examples: `'Remove file'`
        - `editor` (object, required)
          QEditor labels
          Object shape:
            - `toolbar` (string, optional)
              Accessible name (aria-label) of the toolbar
              Examples: `'Editor toolbar'`
            - `url` (string, required)
              Label
              Examples: `'URL'`
            - `bold` (string, required)
              Label
              Examples: `'Bold'`
            - `italic` (string, required)
              Label
              Examples: `'Italic'`
            - `strikethrough` (string, required)
              Label
              Examples: `'Strikethrough'`
            - `underline` (string, required)
              Label
              Examples: `'Underline'`
            - `unorderedList` (string, required)
              Label
              Examples: `'Unordered List'`
            - `orderedList` (string, required)
              Label
              Examples: `'Ordered List'`
            - `subscript` (string, required)
              Label
              Examples: `'Subscript'`
            - `superscript` (string, required)
              Label
              Examples: `'Superscript'`
            - `hyperlink` (string, required)
              Label
              Examples: `'Hyperlink'`
            - `toggleFullscreen` (string, required)
              Label
              Examples: `'Toggle Fullscreen'`
            - `quote` (string, required)
              Label
              Examples: `'Quote'`
            - `left` (string, required)
              Label
              Examples: `'Left align'`
            - `center` (string, required)
              Label
              Examples: `'Center align'`
            - `right` (string, required)
              Label
              Examples: `'Right align'`
            - `justify` (string, required)
              Label
              Examples: `'Justify align'`
            - `print` (string, required)
              Label
              Examples: `'Print'`
            - `outdent` (string, required)
              Label
              Examples: `'Decrease indentation'`
            - `indent` (string, required)
              Label
              Examples: `'Increase indentation'`
            - `removeFormat` (string, required)
              Label
              Examples: `'Remove formatting'`
            - `formatting` (string, required)
              Label
              Examples: `'Formatting'`
            - `fontSize` (string, required)
              Label
              Examples: `'Font Size'`
            - `align` (string, required)
              Label
              Examples: `'Align'`
            - `hr` (string, required)
              Label
              Examples: `'Insert Horizontal Rule'`
            - `undo` (string, required)
              Label
              Examples: `'Undo'`
            - `redo` (string, required)
              Label
              Examples: `'Redo'`
            - `heading1` (string, required)
              Label
              Examples: `'Heading 1'`
            - `heading2` (string, required)
              Label
              Examples: `'Heading 2'`
            - `heading3` (string, required)
              Label
              Examples: `'Heading 3'`
            - `heading4` (string, required)
              Label
              Examples: `'Heading 4'`
            - `heading5` (string, required)
              Label
              Examples: `'Heading 5'`
            - `heading6` (string, required)
              Label
              Examples: `'Heading 6'`
            - `paragraph` (string, required)
              Label
              Examples: `'Paragraph'`
            - `code` (string, required)
              Label
              Examples: `'Code'`
            - `size1` (string, required)
              Label
              Examples: `'Very small'`
            - `size2` (string, required)
              Label
              Examples: `'A bit small'`
            - `size3` (string, required)
              Label
              Examples: `'Normal'`
            - `size4` (string, required)
              Label
              Examples: `'Medium-large'`
            - `size5` (string, required)
              Label
              Examples: `'Big'`
            - `size6` (string, required)
              Label
              Examples: `'Very big'`
            - `size7` (string, required)
              Label
              Examples: `'Maximum'`
            - `defaultFont` (string, required)
              Label
              Examples: `'Default Font'`
            - `viewSource` (string, required)
              Label
              Examples: `'View Source'`
        - `tree` (object, required)
          QTree labels
          Object shape:
            - `noNodes` (string, required)
              Label
              Examples: `'No nodes available'`
            - `noResults` (string, required)
              Label
              Examples: `'No matching nodes found'`
    - `ssrContent` (object, optional)
      Required for SSR/SSG only
- `getLocale(): string`
  Get the browser locale ISO name; Returns undefined when it cannot determine current browser locale or when running on server in SSR/SSG mode
  Returns: `string`
    Browser locale ISO name
    Examples: `'en-US'`
- `getClosestIsoName(locale: string, isoNames: any[]): string`
  Pick, from a list of language pack names, the one that best matches a locale: an exact match, then progressively less specific tags (the script outranks the region), then the pack whose likely script and region agree best; use it to map a browser or user locale to one of the packs your app ships
  Params:
    - `locale` (string, required)
      BCP 47 language tag (an underscore separator is accepted too); undefined yields undefined, so the result of getLocale() can be passed directly
      Examples: `'es-MX'`, `'sr-Cyrl-RS'`, `void 0`
    - `isoNames` (any[], required)
      Language pack names (their isoName) to choose from; the entries are returned as given
      Examples:
        - `['de', 'en-US', 'es']`
  Returns: `string`
    The best matching entry of the list; undefined when no entry shares the locale's language
    Examples: `'es'`

### Vue Injection

Accessible via `$q.lang` (e.g., `this.$q.lang` in Options API or `useQuasar().lang` in Composition API).

### quasar.config.js Options

Configuration key: `framework.config.lang` (object)

- `noHtmlAttrs` (boolean, optional)
  Whether to disable 'dir' and 'lang' HTML attributes getting added to the '<html>' tag. The 'dir' attribute is crucial when using RTL support. Disable this only if you need to handle these yourself for some reason.

