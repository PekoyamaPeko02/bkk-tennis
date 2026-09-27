---
title: Intersection
related:
  - title: v-intersection directive
    path: ../vue-directives/intersection.md
  - title: useIntersection composable
    path: ../vue-composables/use-intersection.md
  - title: Quasar Components Transitions
    path: ../options/transitions.md
---
The QIntersection component handles the visibility state by itself (does not require you to add it and handle it manually) and can optionally have a show/hide transition as well. It is built on the same engine as the [Intersection directive](../vue-directives/intersection.md) and the [useIntersection composable](../vue-composables/use-intersection.md): all three share one Intersection Observer per configuration, so long lists of QIntersection stay cheap to scroll.

The main benefit of using QIntersection is, however, that the DOM tree is freed up of hidden nodes thus using the minimum possible RAM memory and making the page feel very snappy. As well, you can specify the `tag` property for the wrapper element to match your own needs, thus eliminating yet another DOM node.

Under the hood, it uses the [Intersection Observer API](https://developer.mozilla.org/en-US/docs/Web/API/Intersection_Observer_API).

## QIntersection API

Not inlined here: call the `get_api` tool with `name: "QIntersection"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> In most cases, it is required that you apply CSS to the QIntersection element so that it acts as a necessary filler when the inner content is not rendered. This will allow for a smooth scrolling experience, because otherwise the scroll will jump erratically.
>
> An example of such needed CSS would be, for example, a fixed height or at least a min-height (and possibly even a fixed width, as in the examples below, where multiple QIntersections can be displayed on same row).

> [!IMPORTANT]
> If using the `transition` prop, it is required that the content be wrapped in one and only one element.

### Basic

```vue
<template>
  <div class="row justify-center q-gutter-sm">
    <q-intersection v-for="index in 60" :key="index" class="example-item">
      <q-card flat bordered class="q-ma-sm">
        <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />

        <q-card-section>
          <div class="text-h6">Card #{{ index }}</div>
          <div class="text-subtitle2">by John Doe</div>
        </q-card-section>
      </q-card>
    </q-intersection>
  </div>
</template>

<style lang="sass" scoped>
.example-item
  height: 290px
  width: 290px
</style>
```

### With transition

In the example below we used a Quasar transition. For a full list, please head to [Transitions](../options/transitions.md) page.

```vue
<template>
  <div class="row justify-center q-gutter-sm">
    <q-intersection
      v-for="index in 60"
      :key="index"
      transition="scale"
      class="example-item"
    >
      <q-card flat bordered class="q-ma-sm">
        <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />

        <q-card-section>
          <div class="text-h6">Card #{{ index }}</div>
          <div class="text-subtitle2">by John Doe</div>
        </q-card-section>
      </q-card>
    </q-intersection>
  </div>
</template>

<style lang="sass" scoped>
.example-item
  height: 290px
  width: 290px
</style>
```

Example "A list with transition":

```vue
<template>
  <div class="flex justify-center">
    <div style="max-width: 90%; width: 300px">
      <q-intersection
        v-for="index in 60"
        :key="index"
        transition="flip-right"
        class="example-item"
      >
        <q-item clickable v-ripple>
          <q-item-section avatar>
            <q-avatar color="primary" text-color="white"> Q </q-avatar>
          </q-item-section>

          <q-item-section>
            <q-item-label>Contact #{{ index }}</q-item-label>
            <q-item-label caption lines="1">some@email.com</q-item-label>
          </q-item-section>

          <q-item-section side>
            <q-icon name="chat_bubble" color="green" />
          </q-item-section>
        </q-item>
      </q-intersection>
    </div>
  </div>
</template>

<style lang="sass" scoped>
.example-item
  height: 56px
</style>
```

### Only once

Triggering only once means, however, that you lose the benefit of freeing up the DOM tree. The content will remain in DOM regardless of visibility.

Example "Triggering only once":

```vue
<template>
  <div class="row justify-center q-gutter-sm">
    <q-intersection
      v-for="index in 60"
      :key="index"
      once
      transition="scale"
      class="example-item"
    >
      <q-card flat bordered class="q-ma-sm">
        <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />

        <q-card-section>
          <div class="text-h6">Card #{{ index }}</div>
          <div class="text-subtitle2">by John Doe</div>
        </q-card-section>
      </q-card>
    </q-intersection>
  </div>
</template>

<style lang="sass" scoped>
.example-item
  height: 290px
  width: 290px
</style>
```

### Custom root

By default, visibility is judged against the browser viewport. The `root` property makes an ancestor element the viewport instead, which is what you want when `margin` should be measured from that element's edges, or when the element is expected to be off-screen itself (like a scrolling panel that gets revealed later).

Example "Root viewport":

```vue
<template>
  <div ref="myListRef" class="scroll root-container">
    <div class="row justify-center q-gutter-sm">
      <q-intersection
        v-for="index in 60"
        :key="index"
        :root="myListRef"
        transition="scale"
        class="example-item"
      >
        <q-card flat bordered class="q-ma-sm">
          <img
            alt="Mountains"
            src="https://cdn.quasar.dev/img/mountains.jpg"
          />

          <q-card-section>
            <div class="text-h6">Card #{{ index }}</div>
            <div class="text-subtitle2">by John Doe</div>
          </q-card-section>
        </q-card>
      </q-intersection>
    </div>
  </div>
</template>

<script setup>
import { useTemplateRef } from 'vue'

const myListRef = useTemplateRef('myListRef')
</script>

<style lang="sass" scoped>
.root-container
  height: 250px
  border: 1px solid #fff
  outline: 1px solid #000
  border-radius: 4px

.example-item
  height: 290px
  width: 290px
</style>
```

## Accessibility *(v2.25+)*

Off-screen content is genuinely unmounted (not merely hidden), so it is correctly absent from the accessibility tree as well. Keep in mind that the wrapper element itself always renders, so if you give the `tag` prop a landmark or otherwise semantic element, those semantics apply even while the content inside is unmounted.
