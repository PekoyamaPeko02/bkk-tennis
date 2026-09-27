---
title: v-touch-hold directive
related:
  - title: v-touch-swipe directive
    path: touch-swipe.md
  - title: v-touch-repeat directive
    path: touch-repeat.md
  - title: v-touch-pan directive
    path: touch-pan.md
---
Quasar offers full-featured Vue directives that can totally replace libraries like Hammerjs: `v-touch-pan`, `v-touch-swipe`, `v-touch-hold` and even `v-touch-repeat`.

> **These directives also work with mouse events, not only touch events**, so you are able to build cool functionality for your App on desktops too.

We will be describing `v-touch-hold` directive on the lines below.

## TouchHold API

Not inlined here: call the `get_api` tool with `name: "TouchHold"` for its definition, or add `part` (`value`, `arg`, `modifiers`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div class="row justify-center">
    <q-card
      v-touch-hold.mouse="handleHold"
      class="custom-area cursor-pointer bg-primary text-white shadow-2 relative-position row flex-center"
    >
      <div v-if="info" class="custom-info">
        <pre>{{ info }}</pre>
      </div>
      <div v-else class="text-center">
        Click/touch and hold for at least 600ms.
      </div>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const info = ref(null)

function handleHold({ evt, ...newInfo }) {
  info.value = newInfo

  // native Javascript event
  console.log(evt)
}
</script>

<style lang="sass" scoped>
.custom-area
  width: 90%
  height: 170px
  border-radius: 3px
  padding: 8px

.custom-info pre
  width: 190px
  font-size: 12px
  line-height: 1.2
</style>
```

The default wait time is 600ms, but you can change it:

Example "Custom wait time":

```vue
<template>
  <div class="row justify-center">
    <q-card
      v-touch-hold:2000.mouse="handleHold"
      class="custom-area cursor-pointer bg-purple text-white shadow-2 relative-position row flex-center"
    >
      <div v-if="info" class="custom-info">
        <pre>{{ info }}</pre>
      </div>
      <div v-else class="text-center">
        Click/touch and hold for 2 seconds.
      </div>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const info = ref(null)

function handleHold({ evt, ...newInfo }) {
  info.value = newInfo

  // native Javascript event
  console.log(evt)
}
</script>

<style lang="sass" scoped>
.custom-area
  width: 90%
  height: 170px
  border-radius: 3px
  padding: 8px

.custom-info pre
  width: 190px
  font-size: 12px
  line-height: 1.2
</style>
```

> [!NOTE]
> TouchHold also has a default sensitivity of 5px for touch events and 7px for mouse events, which means that it allows a slight movement of the finger or mouse without aborting, improving the user experience.

However, you can change this sensitivity too (notice the directive argument below - `600:12:15` - 600ms wait time, 12px sensitivity for touch events, 15px sensitivity for mouse events):

Example "Custom sensitivity":

```vue
<template>
  <div class="row justify-center">
    <q-card
      v-touch-hold:600:12:15.mouse="handleHold"
      class="custom-area cursor-pointer bg-purple text-white shadow-2 relative-position row flex-center"
    >
      <div v-if="info" class="custom-info">
        <pre>{{ info }}</pre>
      </div>
      <div v-else class="text-center">
        Sensitivity: 12px for touch events and 15px for mouse events.
      </div>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const info = ref(null)

function handleHold({ evt, ...newInfo }) {
  info.value = newInfo

  // native Javascript event
  console.log(evt)
}
</script>

<style lang="sass" scoped>
.custom-area
  width: 90%
  height: 170px
  border-radius: 3px
  padding: 8px

.custom-info pre
  width: 190px
  font-size: 12px
  line-height: 1.2
</style>
```

### Handling Mouse Events

When you want to also handle mouse events too, use the `mouse` modifier:

```html
<div v-touch-hold.mouse="userHasHold">...</div>
```

### Inhibiting TouchHold

When you want to inhibit TouchHold, you can do so by stopping propagation of the `touchstart` / `mousedown` events from the inner content:

```html
<div v-touch-hold.mouse="userHasHold">
  <!-- ...content -->
  <div @touchstart.stop @mousedown.stop>
    <!--
      TouchHold will not apply here because
      we are calling stopPropagation() on touchstart
      and mousedown events
    -->
  </div>
  <!-- ...content -->
</div>
```

However, if you are using `capture` or `mouseCapture` modifiers then events will first reach the TouchHold directive then the inner content, so TouchHold will still trigger.

### Events after TouchHold triggers

Once the wait time is up and TouchHold triggers, the directive consumes the event that ends the gesture, so that a long press does not also count as a tap or as a click, be it on the element itself or on any of its parents. For touch events this is `touchend` (whose default action, the emulated `click`, gets cancelled as well) and for mouse events this is `click`.

This means that a `@touchend` listener on the element will not be called after TouchHold has triggered. It is still called when the user lifts the finger before the wait time is up. Should you need it in both cases, listen for it in the capture phase:

```html
<div v-touch-hold="userHasHold" @touchend.capture="userHasLifted">
  <!-- ...content -->
</div>
```

Mouse events are not affected the same way: `@mousedown` and `@mouseup` are always called, only the subsequent `@click` is suppressed.
