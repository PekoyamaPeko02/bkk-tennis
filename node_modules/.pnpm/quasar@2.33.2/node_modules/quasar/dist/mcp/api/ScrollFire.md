## ScrollFire API

### Directive Value

- `value` (Function, optional)
  Function to call (once) when the element comes into view; the element is watched through a shared IntersectionObserver, so it also fires when it becomes visible without scrolling (use undefined to disable; re-assign a function after disabling to arm it again)
  Function signature: `(el?: Element) => void`
  Examples:
    - `el => { console.log('Element:', el) }`
  Params:
    - `el` (Element, optional)
      DOM element that scroll-fire is applied to

### Directive Argument

- `arg` (number, optional), default `0` *(added v2.30)*
  Fraction of the element (0 to 1) that must be visible before firing; 0 (the default) fires as soon as any part of it is visible, 1 only once it is fully visible
  Examples: `v-scroll-fire:0.5`, `v-scroll-fire:1`

