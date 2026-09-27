---
title: v-ripple directive
---
Material Ripple effect can easily be added to any DOM element (or component) through the `v-ripple` Quasar directive.

> [!WARNING]
> Do not use this directive on components that already have material ripples baked in: QBtn, QBtnDropdown, QBtnToggle, QChip, QPagination, QTab and QRouteTab. Rather configure the internal ripples through those components' `ripple` property.

## Ripple API

Not inlined here: call the `get_api` tool with `name: "Ripple"` for its definition, or add `part` (`quasarConfOptions`, `value`, `arg`, `modifiers`) for one of them.

## Configuration

Add to `quasar.config.js`:

```js
framework: {
    config: {
      ripple: { /* look at QuasarConfOptions from the API card */ }
    }
}
```

## Usage

> [!IMPORTANT]
> Make sure that your DOM element or component has CSS `position: relative` or Quasar CSS helper class `relative-position` attached to it.

### Basic

```vue
<template>
  <div class="row justify-center">
    <div
      v-ripple
      class="relative-position container flex flex-center text-white"
      :class="classes"
    >
      Click/tap me
    </div>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

const colors = [
  // ...
]

const color = ref(colors[0])
const classes = computed(() => `bg-${color.value}`)
const index = ref(0)

let timer

onMounted(() => {
  timer = setInterval(() => {
    index.value = (index.value + 1) % colors.length
    color.value = colors[index.value]
  }, 3000)
})

onBeforeUnmount(() => {
  clearTimeout(timer)
})
</script>

<style lang="sass" scoped>
.container
  border-radius: 3px
  cursor: pointer
  transition: background 1.5s
  height: 150px
  width: 80%
  max-width: 500px
</style>
```

### Coloring

The Material Ripple takes the CSS color of text by default, but you can configure it:

Example "Colored":

```vue
<template>
  <div class="column items-center">
    <div
      v-ripple:purple
      class="relative-position container bg-grey-3 text-black flex flex-center"
    >
      Purple colored ripple
    </div>

    <div
      v-ripple="{ color: 'yellow' }"
      class="relative-position container bg-cyan text-white flex flex-center q-mt-sm"
      style="height: 50px"
    >
      Yellow colored ripple
    </div>

    <div
      v-ripple
      class="relative-position container bg-purple text-yellow flex flex-center q-mt-sm"
      style="height: 50px"
    >
      Inheriting text color
    </div>
  </div>
</template>

<style lang="sass" scoped>
.container
  border-radius: 3px
  cursor: pointer
  height: 50px
  width: 80%
  max-width: 500px
</style>
```

### Positioning

You can also configure if the ripple should always start from center or not, regardless of the touch point:

```vue
<template>
  <div class="q-gutter-md row justify-center">
    <div
      v-ripple.center
      class="relative-position container bg-grey-3 text-black inline flex flex-center"
    >
      Center
    </div>

    <div
      v-ripple
      class="relative-position container bg-grey-3 text-black inline flex flex-center text-center"
    >
      Touch point<br />(default)
    </div>
  </div>
</template>

<style lang="sass" scoped>
.container
  border-radius: 50%
  cursor: pointer
  width: 150px
  height: 150px
</style>
```

### Triggering early

By default, the Ripple directive is triggered on click or keyup. However, you can change that and make it trigger earlier, on the first user interaction (pointerdown, keydown). In most situations there is no difference in user perception (the delay between the first and last event of the interaction is small), but on press-and-hold or slow taps the early feedback feels closer to native Material behavior.

Starting with v2.26, an early ripple knows when the interaction can no longer become a click and cancels itself: if the browser claims the gesture (the touchpoint moves and it becomes a scroll/pan) or the pressed pointer is dragged off the element, the ripple gracefully fades out. On touchscreens it also waits out the browser's tap-vs-scroll disambiguation for a few milliseconds before painting, so flick-scrolling across ripple-enabled elements does not flash ripples.

Example "Triggering immediately":

```vue
<template>
  <div class="column items-center">
    <div
      v-ripple.early
      class="relative-position container bg-grey-3 text-black flex flex-center"
    >
      I have ripple triggering early
    </div>

    <div
      v-ripple="{ early: true }"
      class="relative-position container bg-cyan text-white flex flex-center q-mt-sm"
      style="height: 50px"
    >
      I too have ripple triggering early
    </div>
  </div>
</template>

<style lang="sass" scoped>
.container
  border-radius: 3px
  cursor: pointer
  height: 50px
  width: 80%
  max-width: 500px
</style>
```

### Disable

If for some reason you have a scenario where the ripples need to be disabled, then you can assign a Boolean as value for the directive:

```vue
<template>
  <div class="column items-center">
    <div
      v-ripple="state"
      class="relative-position container bg-cyan text-black flex flex-center"
    />

    <q-toggle
      v-model="state"
      label="Use ripple for container above"
      class="q-mt-md"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const state = ref(true)
</script>

<style lang="sass" scoped>
.container
  border-radius: 3px
  cursor: pointer
  height: 50px
  width: 80%
  max-width: 500px
</style>
```
