## QTree API

### Props

- `nodes` (any[], required)
  The array of nodes that designates the tree structure
  Examples:
    - `[{}, {}]`
- `node-key` (string, required)
  The property name of each node object that holds a unique node id
  Examples: `'key'`, `'id'`
- `label-key` (string, optional), default `'label'`
  The property name of each node object that holds the label of the node
  Examples: `'name'`, `'description'`
- `children-key` (string, optional), default `'children'`
  The property name of each node object that holds the list of children of the node
  Examples: `'roles'`, `'relatives'`
- `no-connectors` (boolean, optional)
  Do not display the connector lines between nodes
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `control-color` (string, optional)
  Color name for controls (like checkboxes) from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `selected-color` (string, optional)
  Color name for selected nodes (from the Quasar Color Palette)
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `dense` (boolean, optional)
  Dense mode; occupies less space
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `icon` (string, optional)
  Icon name following Quasar convention; Make sure you have the icon library installed unless you are using 'img:' prefix; If 'none' (String) is used as value then no icon is rendered (but screen real estate will still be used for it)
  Examples: `'map'`, `'ion-add'`, `'img:https://cdn.quasar.dev/logo-v2/svg/logo.svg'`, `'img:path/to/some_image.png'`
- `tick-strategy` (string, optional), default `'none'`
  The type of strategy to use for the selection of the nodes
  Accepts: `'none'`, `'strict'`, `'leaf'`, `'leaf-filtered'`
- `ticked` (any[], optional, syncable)
  Keys of nodes that are ticked
  Required to be used with v-model.
  Examples: `v-model:ticked="tickedKeys"`
- `expanded` (any[], optional, syncable)
  Keys of nodes that are expanded
  Required to be used with v-model.
  Examples: `v-model:expanded="expandedKeys"`
- `selected` (any, optional, syncable)
  Key of node currently selected
  Required to be used with v-model.
  Examples: `v-model:selected="selectedKey"`
- `no-selection-unset` (boolean, optional)
  Do not allow un-selection when clicking currently selected node
- `default-expand-all` (boolean, optional)
  Allow the tree to have all its branches expanded, when first rendered
- `accordion` (boolean, optional)
  Allows the tree to be set in accordion mode
- `no-transition` (boolean, optional)
  Turn off transition effects when expanding/collapsing nodes; Also enhances perf by a lot as a side-effect; Recommended for big trees
- `virtual-scroll` (boolean, optional) *(added v2.25)*
  Render the visible nodes as a flat virtualized list, keeping only the rows around the scrolling viewport in the DOM; Recommended for big trees; The tree itself becomes the scrolling container (give it a height through CSS) unless 'virtual-scroll-target' is used; Expanding/collapsing is instant in this mode (no slide transition, so 'duration', 'no-transition' and the '@after-show'/'@after-hide' events do not apply)
- `virtual-scroll-target` (Element | string | ComponentInstance, optional) *(added v2.25)*
  CSS selector or DOM element to be used as the scrolling container (instead of the tree itself) when 'virtual-scroll' is set
  Examples:
    - `.scroll-target-class`
    - `#scroll-target-id`
    - `$refs.scrollTarget`
    - `$refs.scrollAreaComponent`
    - `document.body`
- `virtual-scroll-item-size` (number | string, optional) *(added v2.25)*
  Default size in pixels of a row; This value is used for rendering the initial list; Try to use a value close to the minimum size of a row; Defaults to 35 (23 for dense mode)
- `virtual-scroll-slice-size` (number | string, optional), default `10` *(added v2.25)*
  Minimum number of rows to render in the virtual list
  Examples: `60`, `'60'`
- `virtual-scroll-slice-ratio-before` (number | string, optional), default `1` *(added v2.25)*
  Ratio of number of rows in visible zone to render before it
- `virtual-scroll-slice-ratio-after` (number | string, optional), default `1` *(added v2.25)*
  Ratio of number of rows in visible zone to render after it
- `virtual-scroll-sticky-size-start` (number | string, optional), default `0` *(added v2.25)*
  Size in pixels of the sticky part (if using one) at the start of the scrolling container; A correct value will improve scroll precision
- `virtual-scroll-sticky-size-end` (number | string, optional), default `0` *(added v2.25)*
  Size in pixels of the sticky part (if using one) at the end of the scrolling container; A correct value will improve scroll precision
- `filter` (string, optional)
  The text value to be used for filtering nodes
  Examples: `'car'`
- `filter-method` (Function, optional)
  The function to use to filter the tree nodes; For best performance, reference it from your scope and do not define it inline
  Function signature: `(node?: object, filter?: string) => boolean`
  Examples:
    - `(node, filter) => node.label.toLowerCase().includes(filter.toLowerCase())`
  Params:
    - `node` (object, optional)
      Node currently being filtered
    - `filter` (string, optional)
      Filter text to match against
  Returns: `boolean`
    Matches or not
- `duration` (number, optional), default `300`
  Toggle animation duration (in milliseconds); use 0 to disable the animation and toggle instantly
- `no-nodes-label` (string, optional)
  Override default such label for when no nodes are available
  Examples: `'No nodes to show!'`
- `no-results-label` (string, optional)
  Override default such label for when no nodes are available due to filtering
  Examples: `'No results'`

### Methods

- `scrollTo(key: any, edge?: string): void`
  (Only when 'virtual-scroll' is set) Scroll the tree to the row of the node with the given key (if the node is currently visible -- its ancestors are expanded and it passes filtering)
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
    - `edge` (string, optional)
      The edge to align to if the row is not visible already (by default it aligns to end if scrolling towards the end and to start otherwise); If the '-force' version is used then it always aligns
      Accepts: `'start'`, `'center'`, `'end'`, `'start-force'`, `'center-force'`, `'end-force'`
- `getNodeByKey(key: any): object`
  Get the node with the given key
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `object`
    Requested node
- `getParentNode(key: any): object`
  Get the parent of the node with the given key
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `object`
    The parent node, or 'undefined' if the node is a root one or the key is not in the nodes model
- `getTickedNodes(): any[]`
  Get array of nodes that are ticked
  Returns: `any[]`
    Ticked node objects
- `getIndeterminateNodes(): any[]`
  Get array of nodes that are partially ticked (some, but not all, of their tickable children are ticked); Always empty unless the 'leaf' or 'leaf-filtered' tick strategy is used
  Returns: `any[]`
    Partially ticked node objects, in the order of the nodes model
- `getExpandedNodes(): any[]`
  Get array of nodes that are expanded
  Returns: `any[]`
    Expanded node objects
- `isExpanded(key: any): boolean`
  Determine if a node is expanded
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `boolean`
    Is specified node expanded?
- `expandAll(): void`
  Use to expand all branches of the tree
- `collapseAll(): void`
  Use to collapse all branches of the tree
- `setExpanded(key: any, state: boolean): void`
  Expands the tree at the point of the node with the key given
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
    - `state` (boolean, required)
      Set to 'true' to expand the branch of the tree, otherwise 'false' collapses it
- `isTicked(key: any): boolean`
  Method to check if a node's checkbox is selected or not
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `boolean`
    Is specified node ticked?
- `isIndeterminate(key: any): boolean`
  Method to check if a node's checkbox is partially ticked (some, but not all, of its tickable children are ticked); Always 'false' unless the 'leaf' or 'leaf-filtered' tick strategy is used
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `boolean`
    Is specified node partially ticked?
- `getTickState(key: any): boolean`
  Method to get a node's tick state in the tri-state form that a QCheckbox takes as its model ('true' ticked, 'null' partially ticked, 'false' unticked); It is the value that the node's own tickbox gets
  Params:
    - `key` (any, required)
      The key of a node
      Examples: `'Node 1'`
  Returns: `boolean`
    The tick state of the specified node ('false' if the key is not in the nodes model)
- `setTicked(keys: any[], state: boolean): void`
  Method to set a node's checkbox programmatically
  Params:
    - `keys` (any[], required)
      The keys of nodes to tick/untick
      Examples:
        - `['Node 1', 'Node 2']`
    - `state` (boolean, required)
      Set to 'true' to tick the checkbox of nodes, otherwise 'false' unticks them

### Events

- `@update:expanded`
  Triggered when nodes are expanded or collapsed; Used by Vue on 'v-model:update' to update its value
  Params:
    - `expanded` (any[], optional)
      The expanded node keys
      Examples:
        - `['Node 1', 'Node 2']`
- `@lazy-load`
  Emitted when the lazy loading of nodes is finished
  Params:
    - `details` (object, optional)
      Lazy loading details
      Object shape:
        - `node` (object, required)
          The node to which the new nodes (the children) will be appended
        - `key` (string, required)
          The key of the node getting the newly loaded child nodes
          Examples: `'New Node'`
        - `done` (Function, required)
          The callback to be carried out when the loading is successful
          Function signature: `(children?: any[]) => void`
          Params:
            - `children` (any[], optional), default `[]`
              Array of nodes
        - `fail` (Function, required)
          The callback to be carried out should the loading fails
- `@update:ticked`
  Emitted when nodes are ticked/unticked via the checkbox; Used by Vue on 'v-model:ticked' to update its value
  Params:
    - `target` (any[], optional)
      The ticked node keys
      Examples:
        - `['Node 1', 'Node 2']`
- `@update:selected`
  Emitted when selected node changes; Used by Vue on 'v-model:selected' to update its value
  Params:
    - `target` (any, optional)
      The selected node key
      Examples: `'Node 1'`
- `@after-show`
  Emitted when component show animation is finished
- `@after-hide`
  Emitted when component hide animation is finished
- `@virtual-scroll`
  Emitted when the virtual scroll occurs (only when 'virtual-scroll' is set)
  Params:
    - `details` (object, optional)
      Object of properties on the new scroll position
      Object shape:
        - `index` (number, required)
          Index of the row that was scrolled into view (0 based)
        - `from` (number, required)
          The index of the first rendered row (0 based)
        - `to` (number, required)
          The index of the last rendered row (0 based)
        - `direction` (string, required)
          Direction of change
          Accepts: `'increase'`, `'decrease'`
        - `ref` (ComponentInstance, required)
          Vue reference to the QTree

### Scoped Slots

- `#default-header`
  Slot to use for defining the header of a node
  Scope:
    - `expanded` (boolean, optional, reactive)
      Is node expanded? Can directly be assigned new Boolean value which changes expanded state
    - `ticked` (boolean, optional, reactive)
      Is node ticked? Can directly be assigned new Boolean value which changes ticked state
    - `indeterminate` (boolean, optional, reactive) *(added v2.25)*
      Is node partially ticked (some, but not all, of its tickable children are ticked)? Read-only -- tick its children to change it
    - `tree` (ComponentInstance, optional)
      QTree instance
    - `node` (object, optional)
      Node object
    - `key` (any, optional)
      Node's key
    - `color` (string, optional)
      QTree instance 'color' supplied prop value
      Examples: `'primary'`
    - `dark` (boolean, optional)
      QTree instance 'dark' supplied prop value
- `#header-[name]`
  Header template slot for describing node header; Used by nodes which have their 'header' prop set to '[name]', where '[name]' can be any string
  Scope:
    - `expanded` (boolean, optional, reactive)
      Is node expanded? Can directly be assigned new Boolean value which changes expanded state
    - `ticked` (boolean, optional, reactive)
      Is node ticked? Can directly be assigned new Boolean value which changes ticked state
    - `indeterminate` (boolean, optional, reactive) *(added v2.25)*
      Is node partially ticked (some, but not all, of its tickable children are ticked)? Read-only -- tick its children to change it
    - `tree` (ComponentInstance, optional)
      QTree instance
    - `node` (object, optional)
      Node object
    - `key` (any, optional)
      Node's key
    - `color` (string, optional)
      QTree instance 'color' supplied prop value
      Examples: `'primary'`
    - `dark` (boolean, optional)
      QTree instance 'dark' supplied prop value
- `#default-body`
  Slot to use for defining the body of a node
  Scope:
    - `expanded` (boolean, optional, reactive)
      Is node expanded? Can directly be assigned new Boolean value which changes expanded state
    - `ticked` (boolean, optional, reactive)
      Is node ticked? Can directly be assigned new Boolean value which changes ticked state
    - `indeterminate` (boolean, optional, reactive) *(added v2.25)*
      Is node partially ticked (some, but not all, of its tickable children are ticked)? Read-only -- tick its children to change it
    - `tree` (ComponentInstance, optional)
      QTree instance
    - `node` (object, optional)
      Node object
    - `key` (any, optional)
      Node's key
    - `color` (string, optional)
      QTree instance 'color' supplied prop value
      Examples: `'primary'`
    - `dark` (boolean, optional)
      QTree instance 'dark' supplied prop value
- `#body-[name]`
  Body template slot for describing node body; Used by nodes which have their 'body' prop set to '[name]', where '[name]' can be any string
  Scope:
    - `expanded` (boolean, optional, reactive)
      Is node expanded? Can directly be assigned new Boolean value which changes expanded state
    - `ticked` (boolean, optional, reactive)
      Is node ticked? Can directly be assigned new Boolean value which changes ticked state
    - `indeterminate` (boolean, optional, reactive) *(added v2.25)*
      Is node partially ticked (some, but not all, of its tickable children are ticked)? Read-only -- tick its children to change it
    - `tree` (ComponentInstance, optional)
      QTree instance
    - `node` (object, optional)
      Node object
    - `key` (any, optional)
      Node's key
    - `color` (string, optional)
      QTree instance 'color' supplied prop value
      Examples: `'primary'`
    - `dark` (boolean, optional)
      QTree instance 'dark' supplied prop value

