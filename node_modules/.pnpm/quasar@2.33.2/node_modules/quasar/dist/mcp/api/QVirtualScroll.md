## QVirtualScroll API

### Props

- `virtual-scroll-horizontal` (boolean, optional)
  Make virtual list work in horizontal mode
- `virtual-scroll-slice-size` (number | string, optional), default `10`
  Minimum number of items to render in the virtual list
  Examples: `60`, `'60'`
- `virtual-scroll-slice-ratio-before` (number | string, optional), default `1`
  Ratio of number of items in visible zone to render before it
- `virtual-scroll-slice-ratio-after` (number | string, optional), default `1`
  Ratio of number of items in visible zone to render after it
- `virtual-scroll-item-size` (number | string, optional), default `24`
  Default size in pixels (height if vertical, width if horizontal) of an item; This value is used for rendering the initial list; Try to use a value close to the minimum size of an item
- `virtual-scroll-sticky-size-start` (number | string, optional), default `0`
  Size in pixels (height if vertical, width if horizontal) of the sticky part (if using one) at the start of the list; A correct value will improve scroll precision
- `virtual-scroll-sticky-size-end` (number | string, optional), default `0`
  Size in pixels (height if vertical, width if horizontal) of the sticky part (if using one) at the end of the list; A correct value will improve scroll precision
- `table-colspan` (number | string, optional)
  The number of columns in the table (you need this if you use table-layout: fixed)
- `type` (string, optional), default `'list'`
  The type of content: list (default) or table
  Accepts: `'list'`, `'table'`
- `items` (any[], optional), default `[]`
  Available list items that will be passed to the scoped slot; For best performance freeze the list of items; Required if 'itemsFn' is not supplied
  Examples:
    - `['Tesla', 'iPhone']`
    - `[{ label: 'Tesla', value: 'car' }, { label: 'iPhone', value: 'phone' }]`
- `items-size` (number, optional)
  Number of available items in the list; Required and used only if 'itemsFn' is provided
  Examples: `100000`
- `items-fn` (Function, optional)
  Function to return the scope for the items to be displayed; Should return an array for items starting from 'from' index for size length; For best performance, reference it from your scope and do not define it inline
  Function signature: `(from?: number, size?: number) => any[]`
  Examples:
    - `(from, size) => { const items = []; for (let i = 0; i < size; i++) { items.push('Item ' + i) }; return items }`
  Params:
    - `from` (number, optional)
      Index of the first item (0 based)
    - `size` (number, optional)
      Number of items to return
  Returns: `any[]`
    List of scope for items to be displayed
- `scroll-target` (Element | string | ComponentInstance, optional)
  CSS selector, DOM element or Vue component reference (standing for its root element) to be used as a custom scroll container instead of the auto detected one
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`
- `separator` (boolean | string, optional)
  When a QList is used (see 'type'), a Boolean applying a separator between contained items; When a QMarkupTable is used, a String ('horizontal', 'vertical', 'cell' or 'none'; default is 'horizontal') using a separator/border between rows, columns or all cells
  Examples: `true`, `'cell'`
- `bordered` (boolean, optional)
  Applies a default border to the component
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `padding` (boolean, optional)
  Applies a material design-like padding on top and bottom; Only applies when a QList is used (see 'type')
- `tag` (string, optional), default `'div'`
  HTML tag to use; Only applies when a QList is used (see 'type')
  Examples: `'div'`, `'ul'`, `'ol'`
- `role` (string, optional) *(added v2.25)*
  Overrides the default 'list' ARIA role; Contained QItems derive their own default role from it: with 'menu'/'menubar' the actionable items become 'menuitem's, while any other value stops the non-actionable items from claiming 'listitem'; Only applies when a QList is used (see 'type')
  Examples: `'menu'`, `'listbox'`, `'none'`
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow); Only applies when a QMarkupTable is used (see 'type')
- `square` (boolean, optional)
  Removes border-radius so borders are squared; Only applies when a QMarkupTable is used (see 'type')
- `wrap-cells` (boolean, optional)
  Wrap text within table cells; Only applies when a QMarkupTable is used (see 'type')

### Methods

- `scrollTo(index: number | string, edge?: string): void`
  Scroll the virtual scroll list to the item with the specified index (0 based)
  Params:
    - `index` (number | string, required)
      The index of the list item (0 based)
    - `edge` (string, optional)
      The edge to align to if the item is not visible already (by default it aligns to end if scrolling towards the end and to start otherwise); If the '-force' version is used then it always aligns
      Accepts: `'start'`, `'center'`, `'end'`, `'start-force'`, `'center-force'`, `'end-force'`
- `reset(): void`
  Resets the virtual scroll computations; Needed for custom edge-cases
- `refresh(index?: string | number): void`
  Refreshes the virtual scroll list; Use it after appending items
  Params:
    - `index` (string | number, optional)
      The index of the list item to scroll to after refresh (0 based); If it's not specified the scroll position is not changed; Use a negative value to keep scroll position
      Examples: `5`

### Events

- `@virtual-scroll`
  Emitted when the virtual scroll occurs
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
          Vue reference to the QVirtualScroll

### Slots

- `#before`
  Template slot for the elements that should be rendered before the list; Suggestion: thead before a table
- `#after`
  Template slot for the elements that should be rendered after the list; Suggestion: tfoot after a table

### Scoped Slots

- `#default`
  Template slot for defining the list item; Suggestion: QItem
  Scope:
    - `index` (number, optional)
      Item index in the items list
    - `item` (any, optional)
      Item data -- its value is taken from 'items' prop

