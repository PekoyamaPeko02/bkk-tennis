---
title: Inner Loading
related:
  - title: Linear Progress
    path: linear-progress.md
  - title: Circular Progress
    path: circular-progress.md
  - title: Spinners
    path: spinners.md
  - title: Skeleton
    path: skeleton.md
  - title: Loading Plugin
    path: ../quasar-plugins/loading.md
  - title: LoadingBar
    path: ../quasar-plugins/loading-bar.md
---
The QInnerLoading component allows you to add a progress animation within a component. Much like the [Loading Plugin](../quasar-plugins/loading.md), its purpose is to offer visual confirmation to the user that some process is happening in the background, which takes an excessive amount of time. QInnerLoading will add an opaque overlay over the delayed element along with a [Spinner](spinners.md).

## QInnerLoading API

Not inlined here: call the `get_api` tool with `name: "QInnerLoading"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> In order for the spinner to be properly placed in the center of the element you want the loading display to show over, that element must have CSS position set to `relative` (or the `relative-position` CSS class declared).

> [!IMPORTANT]
> QInnerLoading must be the last element inside its parent so it can appear on top of the other content.

### Basic

```vue
<template>
  <div class="q-gutter-md">
    <q-btn color="primary" @click="showTextLoading"> Show it </q-btn>

    <q-card class="relative-position card-example" flat bordered>
      <q-card-section class="q-pb-none">
        <div class="text-h6">Lorem Ipsum</div>
      </q-card-section>

      <q-card-section>
        <transition
          appear
          enter-active-class="animated fadeIn"
          leave-active-class="animated fadeOut"
        >
          <div v-show="showSimulatedReturnData">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent
            vel magna eu risus laoreet tristique. Nulla ut fermentum elit, nec
            consequat augue. Morbi et dolor nec metus tincidunt pellentesque.
            Nullam non semper ante. Fusce pellentesque sagittis felis quis
            porta. Aenean condimentum neque sed erat suscipit malesuada. Nulla
            eget rhoncus enim. Duis dictum interdum eros.
          </div>
        </transition>
      </q-card-section>

      <q-inner-loading :showing="visible">
        <q-spinner-gears size="50px" color="primary" />
      </q-inner-loading>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

// Don't forget to specify which animations
// you are using in quasar.config file > animations.
// Alternatively, if using UMD, load animate.css from CDN.

const visible = ref(false)
const showSimulatedReturnData = ref(false)

function showTextLoading() {
  visible.value = true
  showSimulatedReturnData.value = false

  setTimeout(() => {
    visible.value = false
    showSimulatedReturnData.value = true
  }, 3000)
}
</script>

<style lang="sass" scoped>
.card-example
  width: 288px
  height: 315px
</style>
```

### Label

You can add a label when using the default slot, but you can also use the "label" props instead:

Example "Label props":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn color="primary" @click="showTextLoading"> Show it </q-btn>

    <q-card class="relative-position card-example" flat bordered>
      <q-card-section class="q-pb-none">
        <div class="text-h6">Lorem Ipsum</div>
      </q-card-section>

      <q-card-section>
        <transition
          appear
          enter-active-class="animated fadeIn"
          leave-active-class="animated fadeOut"
        >
          <div v-show="showSimulatedReturnData">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent
            vel magna eu risus laoreet tristique. Nulla ut fermentum elit, nec
            consequat augue. Morbi et dolor nec metus tincidunt pellentesque.
            Nullam non semper ante. Fusce pellentesque sagittis felis quis
            porta. Aenean condimentum neque sed erat suscipit malesuada. Nulla
            eget rhoncus enim. Duis dictum interdum eros.
          </div>
        </transition>
      </q-card-section>

      <q-inner-loading
        :showing="visible"
        label="Please wait..."
        label-class="text-teal"
        label-style="font-size: 1.1em"
      />
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

// Don't forget to specify which animations
// you are using in quasar.config file > animations.
// Alternatively, if using UMD, load animate.css from CDN.

const visible = ref(false)
const showSimulatedReturnData = ref(false)

function showTextLoading() {
  visible.value = true
  showSimulatedReturnData.value = false

  setTimeout(() => {
    visible.value = false
    showSimulatedReturnData.value = true
  }, 3000)
}
</script>

<style lang="sass" scoped>
.card-example
  width: 288px
  height: 315px
</style>
```

## Accessibility *(v2.25+)*

QInnerLoading is a visual overlay only: the content it covers stays in the accessibility tree and remains focusable, and the loading state itself is never announced. Pass `role="status"` as an attribute and use the `label` prop so screen readers hear the state change, and manage the covered region yourself — set `aria-busy="true"` on it, or make it `inert` — when it shouldn't be reachable while loading.
