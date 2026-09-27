## QTable API

### Props

- `fullscreen` (boolean, optional, syncable)
  Fullscreen mode
  Required to be used with v-model.
  Examples: `v-model:fullscreen="isFullscreen"`
- `no-route-fullscreen-exit` (boolean, optional)
  Changing route app won't exit fullscreen
- `rows` (any[], required)
  Rows of data to display
  Examples: `:rows="myData"`
- `row-key` (string | Function, optional), default `'id'`
  Property of each row that defines the unique key of each row (the result must be a primitive, not Object, Array, etc); The value of property must be string or a function taking a row and returning the desired (nested) key in the row; If supplying a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(row?: object) => any`
  Examples: `'name'`, `row => row.name`
  Params:
    - `row` (object, optional)
      The current row being processed
      Examples:
        - `{ name: 'Lorem Ipsum', price: 19 }`
  Returns: `any`
    Current row's key
    Examples: `'34f39dda-6206-4071-a9df-4393aabe49ac'`, `34`
- `virtual-scroll` (boolean, optional)
  Display data using QVirtualScroll (for non-grid mode only)
- `virtual-scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector, DOM element or Vue component reference (standing for its root element) to be used as a custom scroll container instead of the auto detected one
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`
- `virtual-scroll-slice-size` (number | string, optional), default `10`
  Minimum number of rows to render in the virtual list
- `virtual-scroll-slice-ratio-before` (number | string, optional), default `1`
  Ratio of number of rows in visible zone to render before it
- `virtual-scroll-slice-ratio-after` (number | string, optional), default `1`
  Ratio of number of rows in visible zone to render after it
- `virtual-scroll-item-size` (number | string, optional), default `48/24`
  Default size in pixels of a row; This value is used for rendering the initial table; Try to use a value close to the minimum size of a row; Default value: 48 (24 if dense)
- `virtual-scroll-sticky-size-start` (number | string, optional), default `0`
  Size in pixels of the sticky header (if using one); A correct value will improve scroll precision; Will be also used for non-virtual-scroll tables for fixing top alignment when using scrollTo method
- `virtual-scroll-sticky-size-end` (number | string, optional), default `0`
  Size in pixels of the sticky footer part (if using one); A correct value will improve scroll precision
- `table-colspan` (number | string, optional)
  The number of columns in the table (you need this if you use table-layout: fixed)
- `color` (string, optional), default `'grey-8'`
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `icon-first-page` (string, optional)
  Icon name following Quasar convention for stepping to first page; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-prev-page` (string, optional)
  Icon name following Quasar convention for stepping to previous page; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-next-page` (string, optional)
  Icon name following Quasar convention for stepping to next page; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `icon-last-page` (string, optional)
  Icon name following Quasar convention for stepping to last page; Make sure you have the icon library installed unless you are using 'img:' prefix
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `grid` (boolean, optional)
  Display data as a grid instead of the default table
- `grid-header` (boolean, optional)
  Display header for grid-mode also
- `dense` (boolean, optional)
  Dense mode; Connect with $q.screen for responsive behavior
- `columns` (any[], optional)
  The column definitions (Array of Objects)
  Examples: `:columns="tableColumns"`
  Object shape:
    - `name` (string, required)
      Unique id, identifies column, (used by pagination.sortBy, 'body-cell-[name]' slot, ...)
      Examples: `'desc'`
    - `label` (string, required)
      Label for header
      Examples: `'Dessert (100g serving)'`
    - `field` (string | Function, required)
      Row Object property to determine value for this column or function which maps to the required property
      Function signature: `(row: object) => any`
      Examples: `'name'`, `row => row.prices.active`
      Params:
        - `row` (object, required)
          The current row being processed
          Examples:
            - `{ name: 'Lorem Ipsum', prices: { active: 19, old: 25, list: 29 } }`
      Returns: `any`
        Value for this column
        Examples: `'19'`, `19`
    - `required` (boolean, optional)
      If we use visible-columns, this col will always be visible
    - `align` (string, optional), default `'right'`
      Horizontal alignment of cells in this column
      Accepts: `'left'`, `'right'`, `'center'`
    - `sortable` (boolean, optional), default `false`
      Tell QTable you want this column sortable
    - `sort` (Function, optional)
      Compare function if you have some custom data or want a specific way to compare two rows; rows with null/undefined values will get sorted without triggering this method (use 'rawSort' instead if you want to handle those values too)
      Function signature: `(a: any, b: any, rowA: object, rowB: object) => number`
      Examples:
        - `(a, b, _rowA, _rowB) => parseInt(a, 10) - parseInt(b, 10)`
      Params:
        - `a` (any, required)
          Value of the first comparison term
          Examples: `123`, `'abc'`
        - `b` (any, required)
          Value of the second comparison term
          Examples: `123`, `'abc'`
        - `rowA` (object, required)
          Full Row object in which is contained the first term
          Examples:
            - `{ name: 'Potassium', value: 'K' }`
        - `rowB` (object, required)
          Full Row object in which is contained the second term
          Examples:
            - `{ name: 'Fluorine', value: 'F' }`
      Returns: `number`
        Comparison result of term 'a' with term 'b'. Less than 0 when 'a' should come first; greater than 0 if 'b' should come first; equal to 0 if their position must not be changed with respect to each other
        Examples: `-1`, `0`, `1`
    - `rawSort` (Function, optional) *(added v2.13)*
      Compare function if you have some custom data or want a specific way to compare two rows; includes rows with null/undefined values (use 'sort' instead if you don't want that)
      Function signature: `(a: any, b: any, rowA: object, rowB: object) => number`
      Examples:
        - `(a, b, _rowA, _rowB) => parseInt(a, 10) - parseInt(b, 10)`
      Params:
        - `a` (any, required)
          Value of the first comparison term
          Examples: `123`, `'abc'`
        - `b` (any, required)
          Value of the second comparison term
          Examples: `123`, `'abc'`
        - `rowA` (object, required)
          Full Row object in which is contained the first term
          Examples:
            - `{ name: 'Potassium', value: 'K' }`
        - `rowB` (object, required)
          Full Row object in which is contained the second term
          Examples:
            - `{ name: 'Fluorine', value: 'F' }`
      Returns: `number`
        Comparison result of term 'a' with term 'b'. Less than 0 when 'a' should come first; greater than 0 if 'b' should come first; equal to 0 if their position must not be changed with respect to each other
        Examples: `-1`, `0`, `1`
    - `sortOrder` (string, optional), default `'ad'`
      Set column sort order: 'ad' (ascending-descending) or 'da' (descending-ascending); Overrides the 'column-sort-order' prop
      Accepts: `'ad'`, `'da'`
    - `format` (Function, optional)
      Function you can apply to format your data
      Function signature: `(val: any, row: object) => any`
      Examples:
        - `(val, _row) => `${ val }%``
        - `val => val ? /* Unicode checkmark checked */ '☑' : /* Unicode checkmark unchecked */ '☐'`
      Params:
        - `val` (any, required)
          Value of the cell
          Examples: `123`, `'abc'`
        - `row` (object, required)
          Full Row object in which the cell is contained
          Examples:
            - `{ name: 'Potassium', value: 'K' }`
      Returns: `any`
        The resulting formatted value
        Examples: `'20%'`
    - `style` (string | Function, optional)
      Style to apply on normal cells of the column
      Function signature: `(row: object) => string`
      Examples: `'width: 500px'`, `row => (row.calories % 2 === 0 ? 'width: 10px' : 'font-size: 2em; font-weight: bold')`
      Params:
        - `row` (object, required)
          The current row being processed
          Examples:
            - `{ name: 'Frozen Yogurt', calories: 159 }`
      Returns: `string`
    - `classes` (string | Function, optional)
      Classes to add on normal cells of the column
      Function signature: `(row: object) => string`
      Examples: `'my-special-class bg-primary'`, `row => (row.calories % 2 === 0 ? 'bg-green text-white' : 'bg-yellow')`
      Params:
        - `row` (object, required)
          The current row being processed
          Examples:
            - `{ name: 'Frozen Yogurt', calories: 159 }`
      Returns: `string`
    - `headerStyle` (string, optional)
      Style to apply on header cells of the column
      Examples: `'width: 500px'`
    - `headerClasses` (string, optional)
      Classes to add on header cells of the column
      Examples: `'my-special-class'`
    - `autoWidth` (boolean, optional) *(added v2.31)*
      Tries to shrink the column to the minimum width required by its content (header and body cells alike)
- `visible-columns` (any[], optional)
  Array of Strings defining column names ('name' property of each column from 'columns' prop definitions); Columns marked as 'required' are not affected by this property
  Examples:
    - `['desc', 'carbs', 'protein']`
    - `:visible-columns="myCols"`
- `loading` (boolean, optional)
  Put Table into 'loading' state; Notify the user something is happening behind the scenes
- `title` (string, optional)
  Table title
  Examples: `'Device list'`
- `hide-header` (boolean, optional)
  Hide table header layer
- `hide-bottom` (boolean, optional)
  Hide table bottom layer regardless of what it has to display
- `hide-selected-banner` (boolean, optional)
  Hide the selected rows banner (if any)
- `hide-no-data` (boolean, optional)
  Hide the default no data bottom layer
- `hide-pagination` (boolean, optional)
  Hide the pagination controls at the bottom
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow)
- `bordered` (boolean, optional)
  Applies a default border to the component
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `separator` (string, optional), default `'horizontal'`
  Use a separator/border between rows, columns or all cells
  Accepts: `'horizontal'`, `'vertical'`, `'cell'`, `'none'`
- `wrap-cells` (boolean, optional)
  Wrap text within table cells
- `binary-state-sort` (boolean, optional)
  Skip the third state (unsorted) when user toggles column sort direction
- `column-sort-order` (string, optional), default `'ad'`
  Set column sort order: 'ad' (ascending-descending) or 'da' (descending-ascending); It gets applied to all columns unless a column has its own sortOrder specified in the 'columns' definition prop
  Accepts: `'ad'`, `'da'`
- `no-data-label` (string, optional)
  Override default text to display when no data is available
  Examples: `'No devices available'`
- `no-results-label` (string, optional)
  Override default text to display when user filters the table and no matched results are found
  Examples: `'No matched records'`
- `loading-label` (string, optional)
  Override default text to display when table is in loading state (see 'loading' prop)
  Examples: `'Loading devices...'`
- `selected-rows-label` (Function, optional)
  Text to display when user selected at least one row; For best performance, reference it from your scope and do not define it inline
  Function signature: `(numberOfRows?: number) => string`
  Examples: `(numberOfRows) => `Selected: ${ numberOfRows } entries``
  Params:
    - `numberOfRows` (number, optional)
      Number of rows available
  Returns: `string`
    Label to display
    Examples: `'5 rows are selected'`
- `rows-per-page-label` (string, optional)
  Text to override default rows per page label at bottom of table; also used as the 'aria-label' of the rows per page selection field
  Examples: `'Records per page:'`
- `pagination-label` (Function, optional)
  Text to override default pagination label at bottom of table (unless 'pagination' scoped slot is used); For best performance, reference it from your scope and do not define it inline
  Function signature: `(firstRowIndex?: number, endRowIndex?: number, totalRowsNumber?: number) => string`
  Examples:
    - `(start, end, total) => `${ start }-${ end } of ${ total }``
  Params:
    - `firstRowIndex` (number, optional)
      Index of first displayed row
    - `endRowIndex` (number, optional)
      Index of last displayed row
    - `totalRowsNumber` (number, optional)
      Number of total rows available in data
  Returns: `string`
    Label to display
    Examples: `'1-10 of 132'`
- `table-style` (string | any[] | object, optional)
  CSS style to apply to native HTML <table> element's wrapper (which is a DIV)
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `table-class` (string | any[] | object, optional)
  CSS classes to apply to native HTML <table> element's wrapper (which is a DIV)
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `table-header-style` (string | any[] | object, optional)
  CSS style to apply to header of native HTML <table> (which is a TR)
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `table-header-class` (string | any[] | object, optional)
  CSS classes to apply to header of native HTML <table> (which is a TR)
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `table-row-style-fn` (Function, optional) *(added v2.18)*
  CSS style to apply to the table rows (which are TR elements); For best performance, reference it from your scope and do not define it inline
  Function signature: `(row?: object) => string`
  Params:
    - `row` (object, optional)
      The current row being processed
      Examples:
        - `{ name: 'Frozen Yogurt', calories: 159 }`
  Returns: `string`
    CSS style to apply to the row
    Examples: `'color: blue'`, `'background-color: #ff0000; color: green'`
- `table-row-class-fn` (Function, optional) *(added v2.18)*
  CSS class(es) to apply the table rows (which are TR elements); For best performance, reference it from your scope and do not define it inline
  Function signature: `(row?: object) => string`
  Params:
    - `row` (object, optional)
      The current row being processed
      Examples:
        - `{ name: 'Frozen Yogurt', calories: 159 }`
  Returns: `string`
    CSS class(es) to apply to the row, space separated
    Examples: `'my-special-class'`, `'my-class my-second-class'`
- `card-container-style` (string | any[] | object, optional)
  CSS style to apply to the cards container (when in grid mode)
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `card-container-class` (string | any[] | object, optional)
  CSS classes to apply to the cards container (when in grid mode)
  Examples: `'my-special-class'`, `'justify-center'`, `{ 'my-special-class': true }`
- `card-style` (string | any[] | object, optional)
  CSS style to apply to the card (when in grid mode) or container card (when not in grid mode)
  Examples: `'background-color: #ff0000'`, `{ backgroundColor: '#ff0000' }`
- `card-class` (string | any[] | object, optional)
  CSS classes to apply to the card (when in grid mode) or container card (when not in grid mode)
  Examples: `'my-special-class'`, `{ 'my-special-class': true }`
- `card-style-fn` (Function, optional) *(added v2.18)*
  (Grid mode only) CSS style to apply to the row/record card; Has no effect when the 'item' slot is used; For best performance, reference it from your scope and do not define it inline
  Function signature: `(row?: object) => string`
  Params:
    - `row` (object, optional)
      The current row/record being processed
      Examples:
        - `{ name: 'Frozen Yogurt', calories: 159 }`
  Returns: `string`
    CSS style to apply to the row/record
    Examples: `'color: blue'`, `'background-color: #ff0000; color: green'`
- `card-class-fn` (Function, optional) *(added v2.18)*
  (Grid mode only) CSS class(es) to apply the row/record card; Has no effect when the 'item' slot is used; For best performance, reference it from your scope and do not define it inline
  Function signature: `(row?: object) => string`
  Params:
    - `row` (object, optional)
      The current row/record being processed
      Examples:
        - `{ name: 'Frozen Yogurt', calories: 159 }`
  Returns: `string`
    CSS class(es) to apply to the row, space separated
    Examples: `'my-special-class'`, `'my-class my-second-class'`
- `title-class` (string | any[] | object, optional)
  CSS classes to apply to the title (if using 'title' prop)
  Examples: `'my-special-class'`, `'text-h1'`, `{ 'text-h1': true }`
- `filter` (string | object, optional)
  String/Object to filter table with; When using an Object it requires 'filter-method' to also be specified since it will be a custom filtering
  Examples: `'car'`
- `filter-method` (Function, optional)
  The actual filtering mechanism; For best performance, reference it from your scope and do not define it inline
  Function signature: `(rows?: any[], terms?: string | object, cols?: any[], getCellValue?: Function) => any[]`
  Examples: `see source code`
  Params:
    - `rows` (any[], optional)
      Array of rows
    - `terms` (string | object, optional)
      Terms to filter with (is essentially the 'filter' prop value)
    - `cols` (any[], optional)
      Column definitions
    - `getCellValue` (Function, optional)
      Optional function to get a cell value
      Function signature: `(col: object, row: object) => any`
      Params:
        - `col` (object, required)
          Column entry from column definitions
        - `row` (object, required)
          The row object
      Returns: `any`
        Parsed/Processed cell value
        Examples: `'Ice Cream Sandwich'`
  Returns: `any[]`
    Filtered rows
- `pagination` (object, optional, syncable)
  Pagination object; You can also use the 'v-model:pagination' for synching; When not synching it simply initializes the pagination on first render
  Examples: `:pagination="myInitialPagination"`, `v-model:pagination="myPagination"`
  Object shape:
    - `sortBy` (string, optional)
      Column name (from column definition)
      Examples: `'calories'`
    - `descending` (boolean, optional)
      Is sorting in descending order?
    - `page` (number, optional)
      Page number (1-based)
    - `rowsPerPage` (number, optional)
      How many rows per page? 0 means Infinite
    - `rowsNumber` (number, optional)
      For server-side fetching only. How many total database rows are there to be added to the table. If set, causes the QTable to emit @request when data is required.
- `rows-per-page-options` (any[], optional), default `[5, 7, 10, 15, 20, 25, 50, 0]`
  Options for user to pick (Numbers); Number 0 means 'Show all rows in one page'
  Examples:
    - `[10, 20]`
- `selection` (string, optional), default `'none'`
  Selection type
  Accepts: `'single'`, `'multiple'`, `'none'`
- `selected` (any[], optional, syncable), default `[]`
  Keeps the user selection array
  Examples: `v-model:selected="selection"`
- `expanded` (any[], optional, syncable)
  Keeps the array with expanded rows keys
  Examples: `v-model:expanded="expanded"`
- `sort-method` (Function, optional)
  The actual sort mechanism. Function (rows, sortBy, descending) => sorted rows; For best performance, reference it from your scope and do not define it inline
  Function signature: `(rows?: any[], sortBy?: string, descending?: boolean) => any[]`
  Examples: `see source code`
  Params:
    - `rows` (any[], optional)
      Array with rows
    - `sortBy` (string, optional)
      Column name (from column definition)
      Examples: `'calories'`
    - `descending` (boolean, optional)
      Is sorting in descending order?
  Returns: `any[]`
    Sorted rows

### Computed Props

- `filteredSortedRows` (any[], optional)
  The filtered and sorted rows (same as the rows prop if using server-side fetching)
  Examples:
    - `[{ name: 'Ice Cream Sandwich', calories: 237, fat: 9.0, carbs: 37, protein: 4.3, sodium: 129, calcium: 8, iron: 1 }, ...]`
- `computedRows` (any[], optional)
  Paginated, filtered, and sorted rows (same as the rows prop if using server-side fetching)
  Examples:
    - `[{ name: 'Ice Cream Sandwich', calories: 237, fat: 9.0, carbs: 37, protein: 4.3, sodium: 129, calcium: 8, iron: 1 }, ...]`
- `computedRowsNumber` (number, optional)
  The number of computed rows

### Methods

- `toggleFullscreen(): void`
  Toggles fullscreen mode
- `setFullscreen(): void`
  Enter the fullscreen view
- `exitFullscreen(): void`
  Leave the fullscreen view
- `requestServerInteraction(props?: object): void`
  Trigger a server request (emits 'request' event)
  Params:
    - `props` (object, optional)
      Request details
      Object shape:
        - `pagination` (object, optional)
          Optional pagination object
          Object shape:
            - `sortBy` (string, optional)
              Column name (from column definition)
              Examples: `'calories'`
            - `descending` (boolean, optional)
              Is sorting in descending order?
            - `page` (number, optional)
              Page number (1-based)
            - `rowsPerPage` (number, optional)
              How many rows per page? 0 means Infinite
            - `rowsNumber` (number, optional)
              For server-side fetching only. How many total database rows are there to be added to the table.
        - `filter` (Function, optional)
          Filtering method (the 'filter-method' prop)
          Function signature: `(rows: any[], terms: string | object, cols?: any[], getCellValue?: Function) => any[]`
          Params:
            - `rows` (any[], required)
              Array of rows
            - `terms` (string | object, required)
              Terms to filter with (is essentially the 'filter' prop value)
            - `cols` (any[], optional)
              Optional column definitions
            - `getCellValue` (Function, optional)
              Optional function to get a cell value
              Function signature: `(col: object, row: object) => any`
              Params:
                - `col` (object, required)
                  Column entry from column definitions
                - `row` (object, required)
                  The row object
              Returns: `any`
                Parsed/Processed cell value
                Examples: `'Ice Cream Sandwich'`
          Returns: `any[]`
            Filtered rows
- `setPagination(pagination: object, forceServerRequest?: boolean): void`
  Unless using an external pagination Object (through 'v-model:pagination' prop), you can use this method and force the internal pagination to change
  Params:
    - `pagination` (object, required)
      Pagination object
      Object shape:
        - `sortBy` (string, optional)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, optional)
          Is sorting in descending order?
        - `page` (number, optional)
          Page number (1-based)
        - `rowsPerPage` (number, optional)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `forceServerRequest` (boolean, optional)
      Also force a server request
- `firstPage(): void`
  Navigates to first page
- `prevPage(): void`
  Navigates to previous page, if available
- `nextPage(): void`
  Navigates to next page, if available
- `lastPage(): void`
  Navigates to last page
- `isRowSelected(key: any): boolean`
  Determine if a row has been selected by user
  Params:
    - `key` (any, required)
      Row key value
      Examples: `'calories'`
  Returns: `boolean`
    Is row selected or not?
- `clearSelection(): void`
  Clears user selection (emits 'update:selected' with empty array)
- `isRowExpanded(key: any): boolean`
  Determine if a row is expanded or not
  Params:
    - `key` (any, required)
      Row key value
      Examples: `'calories'`
  Returns: `boolean`
    Is row expanded or not?
- `setExpanded(expanded: any[]): void`
  Sets the expanded rows keys array; Especially useful if not using an external 'expanded' state otherwise just emits 'update:expanded' with the value
  Params:
    - `expanded` (any[], required)
      Array containing keys of the expanded rows
      Examples:
        - `['row-a', 'row-b']`
- `sort(col: string | object): void`
  Trigger a table sort
  Params:
    - `col` (string | object, required)
      Column name or column definition object
      Examples: `'calories'`
- `resetVirtualScroll(): void`
  Resets the virtual scroll (if using it) computations; Needed for custom edge-cases
- `scrollTo(index: number | string, edge?: string): void`
  Scroll the table to the row with the specified index in page (0 based)
  Params:
    - `index` (number | string, required)
      The index of the row in page (0 based)
    - `edge` (string, optional), default `end/start`
      Only for virtual scroll - the edge to align to if the row is not visible already; If the '-force' version is used then it always aligns; Default value: end (if scrolling towards the end) / start (if scrolling towards the start)
      Accepts: `'start'`, `'center'`, `'end'`, `'start-force'`, `'center-force'`, `'end-force'`
- `getCellValue(colName: string, row: object): any`
  Method to get a cell value
  Function signature: `(colName: string, row: object) => any`
  Params:
    - `colName` (string, required)
      Column name
    - `row` (object, required)
      The row object
  Returns: `any`
    Parsed/Processed cell value
    Examples: `'Ice Cream Sandwich'`

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
- `@row-click`
  Emitted when user clicks/taps on a row; Is not emitted when using body/row/item scoped slots
  Params:
    - `evt` (Event, optional)
      JS event object
    - `row` (object, optional)
      The row upon which user has clicked/tapped
    - `index` (number, optional)
      Index of the row in the current page
- `@row-dblclick`
  Emitted when user quickly double clicks/taps on a row; Is not emitted when using body/row/item scoped slots; Please check JS dblclick event support before using
  Params:
    - `evt` (Event, optional)
      JS event object
    - `row` (object, optional)
      The row upon which user has double clicked/tapped
    - `index` (number, optional)
      Index of the row in the current page
- `@row-contextmenu`
  Emitted when user right clicks/long taps on a row; Is not emitted when using body/row/item scoped slots
  Params:
    - `evt` (Event, optional)
      JS event object
    - `row` (object, optional)
      The row upon which user has right clicked/long tapped
    - `index` (number, optional)
      Index of the row in the current page
- `@request`
  Emitted when a server request is triggered
  Params:
    - `requestProp` (object, optional)
      Props of the request
      Object shape:
        - `pagination` (object, required)
          Pagination object
          Object shape:
            - `sortBy` (string, required)
              Column name (from column definition)
              Examples: `'calories'`
            - `descending` (boolean, required)
              Is sorting in descending order?
            - `page` (number, required)
              Page number (1-based)
            - `rowsPerPage` (number, required)
              How many rows per page? 0 means Infinite
            - `rowsNumber` (number, optional)
              For server-side fetching only. How many total database rows are there to be added to the table.
        - `filter` (string | object, optional)
          String/Object to filter table with (the 'filter' prop)
        - `getCellValue` (Function, required)
          Function to get a cell value
          Function signature: `(col: object, row: object) => any`
          Params:
            - `col` (object, required)
              Column entry from column definitions
            - `row` (object, required)
              The row object
          Returns: `any`
            Parsed/Processed cell value
            Examples: `'Ice Cream Sandwich'`
- `@selection`
  Emitted when user selects/unselects row(s)
  Params:
    - `details` (object, optional)
      Selection details
      Object shape:
        - `rows` (any[], required)
          Array of row objects that were selected/unselected
        - `keys` (any[], required)
          Array of the keys of rows that were selected/unselected
        - `added` (boolean, required)
          Were the rows added to selection (true) or removed from selection (false)
        - `evt` (Event, required)
          JS event object
- `@update:pagination`
  Used by Vue on 'v-model:pagination' for updating its value
  Params:
    - `newPagination` (object, optional)
      The updated pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
- `@update:selected`
  Used by Vue on 'v-model:selected' prop for updating its value
  Params:
    - `newSelected` (any[], optional)
      The updated selected array
      Examples:
        - `[{ name: 'Frozen Yogurt', calories: 159, fat: 6 }]`
- `@update:expanded`
  Used by Vue on 'v-model:expanded' prop for updating its value
  Params:
    - `newExpanded` (any[], optional)
      The updated expanded array
      Examples:
        - `['row-a', 'row-b']`
- `@virtual-scroll`
  Emitted when the virtual scroll occurs, if using virtual scroll
  Params:
    - `details` (object, optional)
      Object of properties on the new scroll position
      Object shape:
        - `index` (number, required)
          Index of the list item that was scrolled into view (0 based)
        - `from` (number, required)
          The index of the first list item that is rendered (0 based)
        - `to` (number, required)
          The index of the last list item that is rendered (0 based)
        - `direction` (string, required)
          Direction of change
          Accepts: `'increase'`, `'decrease'`
        - `ref` (ComponentInstance, required)
          Vue reference to the underlying QVirtualScroll instance

### Slots

- `#loading`
  Override default effect when table is in loading state; Suggestion: QInnerLoading

### Scoped Slots

- `#item`
  Slot to use for defining an item when in 'grid' mode; Suggestion: QCard
  Scope:
    - `key` (any, optional)
      Row/Item's key
    - `row` (object, optional)
      Row/Item object
    - `rowIndex` (number, optional)
      Row/Item's index (0 based) in the filtered and sorted table
    - `pageIndex` (number, optional)
      Row/Item's index (0 based) in the current page of the filtered and sorted table
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row/item selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row/item expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#body`
  Slot to define how a body row looks like; Suggestion: QTr + Td
  Scope:
    - `key` (any, optional)
      Row's key
    - `row` (object, optional)
      Row object
    - `rowIndex` (number, optional)
      Row's index (0 based) in the filtered and sorted table
    - `pageIndex` (number, optional)
      Row's index (0 based) in the current page of the filtered and sorted table
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
    - `__trClass` (string, optional)
      Internal prop passed down to QTr (if used)
    - `__trStyle` (string, optional) *(added v2.18)*
      Internal prop passed down to QTr (if used)
- `#body-cell`
  Slot to define how all body cells look like; Suggestion: QTd
  Scope:
    - `col` (object, optional)
      Column definition for column associated with table cell
    - `value` (any, optional)
      Parsed/Formatted value of table cell
    - `key` (any, optional)
      Row's key
    - `row` (object, optional)
      Row object
    - `rowIndex` (number, optional)
      Row's index (0 based) in the filtered and sorted table
    - `pageIndex` (number, optional)
      Row's index (0 based) in the current page of the filtered and sorted table
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#body-cell-[name]`
  Slot to define how a specific column cell looks like; replace '[name]' with column name (from columns definition object)
  Scope:
    - `col` (object, optional)
      Column definition for column associated with table cell
    - `value` (any, optional)
      Parsed/Formatted value of table cell
    - `key` (any, optional)
      Row's key
    - `row` (object, optional)
      Row object
    - `rowIndex` (number, optional)
      Row's index (0 based) in the filtered and sorted table
    - `pageIndex` (number, optional)
      Row's index (0 based) in the current page of the filtered and sorted table
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#header`
  Slot to define how header looks like; Suggestion: QTr + QTh
  Scope:
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
    - `__trClass` (string, optional)
      Internal prop passed down to QTr (if used)
    - `header` (boolean, optional)
      Internal prop passed down to QTh (if used); Always 'true'
- `#header-cell`
  Slot to define how each header cell looks like; Suggestion: QTh
  Scope:
    - `col` (object, optional)
      Column definition associated to header cell
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#header-cell-[name]`
  Slot to define how a specific header cell looks like; replace '[name]' with column name (from columns definition object)
  Scope:
    - `col` (object, optional)
      Column definition associated to header cell
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#body-selection`
  Slot to define how body selection column looks like; Suggestion: QCheckbox
  Scope:
    - `key` (any, optional)
      Row's key
    - `row` (object, optional)
      Row object
    - `rowIndex` (number, optional)
      Row's index (0 based) in the filtered and sorted table
    - `pageIndex` (number, optional)
      Row's index (0 based) in the current page of the filtered and sorted table
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#header-selection`
  Slot to define how header selection column looks like (available only for multiple selection mode); Suggestion: QCheckbox
  Scope:
    - `cols` (object, optional)
      Column definitions
    - `colsMap` (object, optional)
      Column mapping (key is column name, value is column object)
    - `sort` (Function, optional)
      Trigger a table sort
      Function signature: `(col: string | object) => void`
      Params:
        - `col` (string | object, required)
          Column name or column definition object
          Examples: `'calories'`
    - `selected` (boolean, optional, reactive)
      (Only if using selection) Is row selected? Can directly be assigned new Boolean value which changes selection state
    - `expand` (boolean, optional, reactive)
      Is row expanded? Can directly be assigned new Boolean value which changes expanded state
    - `color` (string, optional)
      Color name for component from the Quasar Color Palette
      Examples: `'primary'`, `'teal'`, `'teal-10'`
    - `dark` (boolean, optional), default `null`
      Notify the component that the background is a dark color
    - `dense` (boolean, optional)
      Dense mode; occupies less space
- `#top-row`
  Slot to define how top extra row looks like
  Scope:
    - `cols` (object, optional)
      Column definitions
- `#bottom-row`
  Slot to define how bottom extra row looks like
  Scope:
    - `cols` (object, optional)
      Column definitions
- `#footer`
  Slot to define the table footer (gets rendered in a <tfoot> element); not applied when in 'grid' mode; example: a totals row (which can be made sticky through CSS)
  Scope:
    - `cols` (object, optional)
      Column definitions
- `#top`
  Slot to define how table top looks like
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#bottom`
  Slot to define how table bottom looks like
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#pagination`
  Slot to override default pagination label and buttons
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#top-left`
  Slot to define how left part of the table top looks like
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#top-right`
  Slot to define how right part of the table top looks like
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#top-selection`
  Slot to define how top table section looks like when user has selected at least one row
  Scope:
    - `pagination` (object, optional)
      Pagination object
      Object shape:
        - `sortBy` (string, required)
          Column name (from column definition)
          Examples: `'calories'`
        - `descending` (boolean, required)
          Is sorting in descending order?
        - `page` (number, required)
          Page number (1-based)
        - `rowsPerPage` (number, required)
          How many rows per page? 0 means Infinite
        - `rowsNumber` (number, optional)
          For server-side fetching only. How many total database rows are there to be added to the table.
    - `pagesNumber` (number, optional)
      Number of pages available
    - `isFirstPage` (boolean, optional)
      Are we on first page?
    - `isLastPage` (boolean, optional)
      Are we on last page?
    - `firstPage` (Function, optional)
      Navigates to first page
    - `prevPage` (Function, optional)
      Navigates to previous page, if available
    - `nextPage` (Function, optional)
      Navigates to next page, if available
    - `lastPage` (Function, optional)
      Navigates to last page
    - `inFullscreen` (boolean, optional)
      Is table in fullscreen mode?
    - `toggleFullscreen` (Function, optional)
      Toggles fullscreen mode
- `#no-data`
  Slot to define how the bottom will look like when is nothing to display
  Scope:
    - `message` (string, optional)
      The suggested message
      Examples: `'No data available'`
    - `icon` (string, optional)
      The suggested icon name (following Quasar convention)
      Examples: `'warning'`
    - `filter` (string | object, optional)
      String/Object to filter table with (the 'filter' prop)

