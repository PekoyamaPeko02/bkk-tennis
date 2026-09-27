## QPage API

### Props

- `padding` (boolean, optional)
  Applies a default responsive page padding
- `style-fn` (Function, optional)
  Override default CSS style applied to the component (sets minHeight); Function(offset: Number) => CSS props/value: Object; For best performance, reference it from your scope and do not define it inline
  Function signature: `(offset?: number, height?: number) => object`
  Examples:
    - `(offset, height) => ({ minHeight: offset + 'px' })`
  Params:
    - `offset` (number, optional)
      Header + Footer height (in pixels)
    - `height` (number, optional)
      Value in pixels of container height (if containerized) or window height otherwise
  Returns: `object`
    Object with CSS properties to apply to Page DOM element

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

