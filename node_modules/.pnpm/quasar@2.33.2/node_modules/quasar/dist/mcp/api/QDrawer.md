## QDrawer API

### Props

- `model-value` (boolean, optional), default `null`
  Model of the component defining shown/hidden state; Either use this property (along with a listener for 'update:model-value' event) OR use v-model directive
  Examples: `v-model="state"`
- `side` (string, optional), default `'left'`
  Side to attach to
  Accepts: `'left'`, `'right'`
- `overlay` (boolean, optional)
  Puts drawer into overlay mode (does not occupy space on screen, narrowing the page)
- `width` (number, optional), default `300`
  Width of drawer (in pixels)
- `mini` (boolean, optional)
  Puts drawer into mini mode
- `mini-width` (number, optional), default `57`
  Width of drawer (in pixels) when in mini mode
- `mini-to-overlay` (boolean, optional)
  Mini mode will expand as an overlay
- `no-mini-animation` (boolean, optional) *(added v2.12)*
  Disables animation of the drawer when toggling mini mode
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `breakpoint` (number, optional), default `1023`
  Breakpoint (in pixels) of layout width up to which mobile mode is used
  Examples: `1200`
- `behavior` (string, optional), default `'default'`
  Overrides the default dynamic mode into which the drawer is put on
  Accepts: `'default'`, `'desktop'`, `'mobile'`
- `bordered` (boolean, optional)
  Applies a default border to the component
- `elevated` (boolean, optional)
  Adds a default shadow to the drawer
- `persistent` (boolean, optional)
  Prevents drawer from auto-closing when app's route changes or when ESC key is pressed while in a dismissible state (below breakpoint or shown in overlay mode)
- `show-if-above` (boolean, optional)
  Forces drawer to be shown on screen if the layout width is above breakpoint, regardless of v-model; Applies on initial render and also whenever the layout width goes back above the breakpoint; This is the default behavior when SSR/SSG is taken over by client on initial render
- `no-swipe-open` (boolean, optional)
  Disables the default behavior where drawer can be swiped into view; Useful for iOS platforms where it might interfere with Safari's 'swipe to go to previous/next page' feature
- `no-swipe-close` (boolean, optional)
  Disables the default behavior where drawer can be swiped out of view (applies to drawer content only); Useful for iOS platforms where it might interfere with Safari's 'swipe to go to previous/next page' feature
- `no-swipe-backdrop` (boolean, optional)
  Disables the default behavior where drawer backdrop can be swiped

### Methods

- `show(evt?: Event): void`
  Triggers component to show
  Params:
    - `evt` (Event, optional)
      JS event object
- `hide(evt?: Event): void`
  Triggers component to hide
  Params:
    - `evt` (Event, optional)
      JS event object
- `toggle(evt?: Event): void`
  Triggers component to toggle between show/hide
  Params:
    - `evt` (Event, optional)
      JS event object

### Events

- `@update:model-value`
  Emitted when showing/hidden state changes; Is also used by v-model
  Params:
    - `value` (boolean, optional)
      New state (showing/hidden)
- `@show`
  Emitted after component has triggered show()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-show`
  Emitted when component triggers show() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@hide`
  Emitted after component has triggered hide()
  Params:
    - `evt` (Event, required)
      JS event object
- `@before-hide`
  Emitted when component triggers hide() but before it finishes doing it
  Params:
    - `evt` (Event, required)
      JS event object
- `@escape-key`
  Emitted when ESC key is pressed while the drawer is in a dismissible state (below breakpoint or shown in overlay mode); Does not get emitted if the drawer is 'persistent'
- `@on-layout`
  Emitted when drawer toggles between occupying space on page or not
  Params:
    - `state` (boolean, optional)
      New state
- `@click`
  Emitted when user clicks/taps on the component; Useful for when taking a decision to toggle mini mode
  Params:
    - `evt` (Event, required)
      JS event object
- `@mouseover`
  Emitted when user moves mouse cursor over the component; Useful for when taking a decision to toggle mini mode
  Params:
    - `evt` (Event, required)
      JS event object
- `@mouseout`
  Emitted when user moves mouse cursor out of the component; Useful for when taking a decision to toggle mini mode
  Params:
    - `evt` (Event, required)
      JS event object
- `@mini-state`
  Emitted when drawer changes the mini-mode state (sometimes it is forced to do so)
  Params:
    - `state` (boolean, optional)
      New state
- `@pan`
  Emitted when the drawer is being panned/swiped
  Params:
    - `state` (object, optional)
      Contains information about the pan gesture
      Object shape:
        - `type` (string, optional)
          Type of pan gesture
          Accepts: `'open'`, `'close'`
        - `stage` (string, optional)
          Stage of the pan gesture
          Accepts: `'start'`, `'end'`, `'cancel'`

### Slots

- `#default`
  Default slot in the devland unslotted content of the component (overridden by 'mini' slot if used and drawer is in mini mode)
- `#mini`
  Content to show when in mini mode (overrides 'default' slot)

