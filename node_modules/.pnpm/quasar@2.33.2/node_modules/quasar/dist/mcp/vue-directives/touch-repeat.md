---
title: v-touch-repeat directive
related:
  - title: v-touch-swipe directive
    path: touch-swipe.md
  - title: v-touch-pan directive
    path: touch-pan.md
  - title: v-touch-hold directive
    path: touch-hold.md
---
Quasar offers full-featured Vue directives that can totally replace libraries like Hammerjs: `v-touch-pan`, `v-touch-swipe`, `v-touch-hold` and `v-touch-repeat`.

> **These directives also work with mouse events, not only touch events**, so you are able to build cool functionality for your App on desktops too.

We will be describing `v-touch-repeat` on the lines below.

## TouchRepeat API

Not inlined here: call the `get_api` tool with `name: "TouchRepeat"` for its definition, or add `part` (`value`, `arg`, `modifiers`) for one of them.

## Usage

Click and hold with your mouse on the area below to see it in action.
Notice that on touch capable devices the scrolling is not blocked.

> The default repeat pattern is 0:600:300 (ms).

Example "Basic":

```vue
<template>
  <div class="row justify-center">
    <q-card
      v-touch-repeat.mouse="handleRepeat"
      class="custom-area cursor-pointer bg-primary text-white shadow-2 relative-position row flex-center"
    >
      <div v-if="info" class="custom-info">
        <pre>{{ info }}</pre>
      </div>
      <div v-else class="text-center"> Click/touch and hold. </div>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const info = ref(null)

function handleRepeat({ evt, ...newInfo }) {
  info.value = newInfo

  // native Javascript event
  console.log(evt)
}
</script>

<style lang="sass" scoped>
.custom-area
  width: 96%
  height: 210px
  border-radius: 3px
  padding: 8px

.custom-info pre
  width: 190px
  font-size: 12px
  line-height: 1.2
</style>
```

Below is an example configured to also react to `SPACE`, `ENTER` and `h` keys (**focus on it first**), with 0:300:200 (ms) repeat pattern. Hit & hold keys, or click/tap and hold.

Example "Custom keys":

```vue
<template>
  <div class="row justify-center">
    <q-card
      v-touch-repeat:0:300:200.mouse.enter.space.72.104="handleRepeat"
      tabindex="0"
      class="custom-area cursor-pointer bg-primary text-white shadow-2 relative-position row flex-center"
    >
      <div v-if="info" class="custom-info">
        <pre>{{ info }}</pre>
      </div>
      <div v-else class="text-center"> Click/touch and hold. </div>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const info = ref(null)

function handleRepeat({ evt, ...newInfo }) {
  info.value = newInfo

  // native Javascript event
  console.log(evt)
}
</script>

<style lang="sass" scoped>
.custom-area
  width: 96%
  height: 210px
  border-radius: 3px
  padding: 8px

  &:focus
    outline: 1px solid #ccc
    outline-offset: 3px

.custom-info pre
  width: 190px
  font-size: 12px
  line-height: 1.2
</style>
```

Below is an example of applying TouchRepeat to QBtn. Notice how we play with the directive arguments in order to make the blue buttons increment slower than the red ones.

Example "Applied to QBtn":

```vue
<template>
  <div class="row flex-center">
    <q-btn
      v-touch-repeat:0:1000.mouse.enter.space="decrement"
      color="primary"
      push
      round
      class="q-mr-sm"
      icon="remove"
    />

    <q-btn
      v-touch-repeat:0:100.mouse.enter.space="decrement"
      color="red"
      push
      round
      icon="remove"
    />

    <span class="q-mx-md">
      {{ number }}
    </span>

    <q-btn
      v-touch-repeat:0:100.mouse.enter.space="increment"
      color="red"
      push
      round
      class="q-mr-sm"
      icon="add"
    />

    <q-btn
      v-touch-repeat:0:1000.mouse.enter.space="increment"
      color="primary"
      push
      round
      icon="add"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const number = ref(110)

function increment() {
  number.value++
}

function decrement() {
  number.value--
}
</script>
```

### Handling Mouse Events

When you want to handle mouse events too, use the `mouse` modifier:

```html
<div v-touch-repeat.mouse="myHandler">...</div>
```

### Handling Key Events

When you want to handle key events too, use [keycodes](https://keycode.info/) as modifiers:

```html
<div v-touch-repeat.65.70="myHandler">...</div>
```

For common keys, you can use the named modifiers `esc`, `tab`, `enter`, `space`, `up`, `left`, `right`, `down`, and `delete` instead of writing their keycodes. The `delete` modifier handles both Backspace and Delete.

### Inhibiting TouchRepeat

When you want to inhibit TouchRepeat, you can do so by stopping propagation of the `touchstart` / `mousedown` / `keydown` events from the inner content:

```html
<div v-touch-repeat.mouse.enter="userHasHold">
  <!-- ...content -->
  <div @touchstart.stop @mousedown.stop @keydown.stop>
    <!--
      TouchRepeat will not apply here because
      we are calling stopPropagation() on touchstart,
      mousedown and keydown events
    -->
  </div>
  <!-- ...content -->
</div>
```

However, if you are using `capture`, `mouseCapture` or `keyCapture` modifiers then events will first reach the TouchRepeat directive then the inner content, so TouchRepeat will still trigger.

### Events after TouchRepeat triggers

Once at least one repetition has fired, the directive consumes the event that ends the gesture, so that a press and hold does not also count as a tap, as a click or as a key press, be it on the element itself or on any of its parents. For touch events this is `touchend`, for mouse events it is `click` and for key events it is `keyup`.

This means that a `@touchend` listener on the element will not be called after TouchRepeat has started repeating. It is still called when the user lifts the finger before the first repetition. Should you need it in both cases, listen for it in the capture phase:

```html
<div v-touch-repeat="myHandler" @touchend.capture="userHasLifted">
  <!-- ...content -->
</div>
```

The `click` and `keyup` events are stopped at the document level, before they can reach your element, so the capture phase does not help there. Mouse events are otherwise unaffected: `@mousedown` and `@mouseup` are always called, only the subsequent `@click` is suppressed.
