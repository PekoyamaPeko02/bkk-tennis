## Morph API

### Directive Value

- `value` (object | any, optional)
  Configuration object or trigger value
  Examples:
    - `v-morph:element2:group1="groupModel"`
    - `v-morph="{ name: 'element2', group: 'group1', model: 'element1' }"`
  Object shape:
    - `group` (string, optional)
      Name of the morph group the element belongs to
      Examples: `'dialogGroup'`
    - `name` (string, optional)
      Name of the morph inside the group that the element belongs to
      Examples: `'btn'`
    - `model` (string, optional)
      Current value of the group model; when it becomes the same as the 'name' it triggers the morphing
      Examples: `'btn'`
    - `duration` (number, optional), default `300`
      Duration of the animation (in milliseconds)
    - `delay` (number, optional), default `0`
      Delay for the animation (in milliseconds)
    - `easing` (string, optional), default `'ease-in-out'`
      Timing function for the animation (CSS easing format)
      Examples: `'ease-out'`
    - `fill` (string, optional), default `'none'`
      Fill mode for the animation
      Examples: `'forward'`
    - `classes` (string, optional)
      Class names to be added to the destination element during the animation
      Examples: `'bg-grey-2'`
    - `style` (string | object, optional)
      Styles to be added to the destination element during the animation
      Examples: `'border-radius: 20px'`
    - `resize` (boolean, optional)
      Use resize instead of scaling during animation
    - `useCSS` (boolean, optional)
      Use CSS animations instead of the Animation API
    - `hideFromClone` (boolean, optional)
      Hide the spacer for the initial element during animation; Use it if the initial element is not removed or resizing of the space occupied by the initial element is not desired
    - `keepToClone` (boolean, optional)
      Keep a clone of the final element visible during animation
    - `tween` (boolean, optional)
      Use an opacity tween between the initial and final elements
    - `tweenFromOpacity` (number, optional), default `0.6`
      If using tween it is the initial opacity of the initial element (will be animated to 0) - the initial element is placed on top of the final element
    - `tweenToOpacity` (number, optional), default `0.5`
      If using tween it is the initial opacity of the final element (will be animated to 1)
    - `waitFor` (number | string | Promise<void>, optional), default `0`
      Delay animation start for that number of milliseconds, or until a 'transitionend' event is emitted by the destination element, or until the promise is resolved (if the promise is rejected the morphing will abort, but the 'toggle function' was already called)
      Examples: `300`, `'200'`, `'transitionend'`
    - `onEnd` (Function, optional)
      A function that will be called once the morphing is finished; Not called if morphing is aborted
      Function signature: `(direction?: string, aborted?: boolean) => void`
      Examples:
        - `(direction, _aborted) => { if (direction !== 'to') { /* revertLogic() */ } }`
      Params:
        - `direction` (string, optional)
          'to' if the morphing was finished in the final state or 'from' if it was finished in the initial state
          Accepts: `'to'`, `'from'`
        - `aborted` (boolean, optional)
          Was the morphing aborted?

### Directive Argument

- `arg` (string, optional)
  x:x2:y:z, where x is the morph element name, x2 is the morph group, y is the animation duration (in milliseconds) and z is the amount of time to wait (in milliseconds) or the 'transitionend' string
  Examples:
    - `v-morph:name="options"`
    - `v-morph:name:groupName="options"`
    - `v-morph:name:groupName:400="options"`
    - `v-morph:name:groupName:400:100="options"`
    - `v-morph:name:groupName:400:transitionend="options"`

### Directive Modifiers

- `resize` (boolean, optional)
  Use resize instead of scale transform for morph (forceResize option of the morph function)
- `useCSS` (boolean, optional)
  Use CSS animations for morph (forceCssAnimation option of the morph function)
- `hideFromClone` (boolean, optional)
  Hide the spacer for the initial element (hideFromClone option of the morph function)
- `keepToClone` (boolean, optional)
  Keep the final element visible while morphing (keepToClone option of the morph function)
- `tween` (boolean, optional)
  Use opacity tween morphing between initial and final elements (tween option of the morph function)

