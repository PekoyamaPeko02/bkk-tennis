## Mutation API

### Directive Value

- `value` (boolean | Function, optional)
  Boolean false (or undefined) to disable the directive (the observer is dropped and recreated when a handler is supplied again); Function to call when mutation occurs
  Function signature: `(mutationList?: any[]) => boolean`
  Examples: `v-mutation="myHandler"`, `v-mutation="isEnabled ? myHandler : false"`
  Params:
    - `mutationList` (any[], optional)
      Array of MutationRecord items
      Object shape:
        - `type` (string, optional)
          Type of mutation
          Accepts: `'childList'`, `'attributes'`, `'characterData'`
        - `target` (Element, optional)
          The DOM element that the mutation affected, depending on the mutation type
        - `addedNodes` (any[], optional)
          The NodeList of the nodes that have been added
        - `removedNodes` (any[], optional)
          The NodeList of the nodes that have been removed
        - `previousSibling` (object, optional)
          The previous sibling of the added or removed nodes, or null
        - `nextSibling` (object, optional)
          The next sibling of the added or removed nodes, or null
        - `attributeName` (string, optional)
          The local name of the changed attribute, or null
          Examples: `'some-attribute'`
        - `attributeNamespace` (string, optional)
          The namespace of the changed attribute, or null
        - `oldValue` (string, optional)
          Value depends on the mutation type; For attributes, it is the value of the changed attribute before the change; For characterData it is data of the changed node before the change; For childList it is null; Note that for this to work as expected, attributeOldValue or characterDataOldValue must be set
  Returns: `boolean`
    If you return Boolean false from the handler, the observer stops

### Directive Modifiers

- `once` (boolean, optional)
  Call handler only once, when the first mutation was triggered, then stop monitoring
  Examples: `v-mutation.once`
- `childList` (boolean, optional)
  Monitor the target node (and, if 'subtree' is also set, its descendants) for the addition of new child nodes or removal of existing child nodes
  Examples: `v-mutation.childList`
- `subtree` (boolean, optional)
  Extend monitoring to the entire subtree of nodes rooted at target
  Examples: `v-mutation.subtree`
- `attributes` (boolean, optional)
  Watch for changes to the value of attributes on the node or nodes being monitored
  Examples: `v-mutation.attributes`
- `characterData` (boolean, optional)
  Monitor the specified target node or subtree for changes to the character data contained within the node or nodes
  Examples: `v-mutation.characterData`
- `attributeOldValue` (boolean, optional)
  Record the previous value of any attribute that changes when monitoring the node or nodes for attribute changes
  Examples: `v-mutation.attributeOldValue`
- `characterDataOldValue` (boolean, optional)
  Record the previous value of a node's text whenever the text changes on nodes being monitored
  Examples: `v-mutation.characterDataOldValue`

