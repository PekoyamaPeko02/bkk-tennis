---
title: Slide Transition
related:
  - title: Expansion Item
    path: expansion-item.md
  - title: Quasar Components Transitions
    path: ../options/transitions.md
---
QSlideTransition slides the DOM element (or component) up or down, based on its visibility: works alongside `v-show` and `v-if` on a single element, similar to Vue's Transition component with the only difference being that it's not a group transition too (it only applies to one DOM element or component).

## QSlideTransition API

Not inlined here: call the `get_api` tool with `name: "QSlideTransition"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div style="max-width: 500px">
    <q-toggle v-model="visible" label="Visible image" class="q-mb-md" />

    <q-slide-transition>
      <div v-show="visible">
        <img
          alt="Quasar"
          class="responsive"
          src="https://cdn.quasar.dev/img/quasar.jpg"
        />
      </div>
    </q-slide-transition>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const visible = ref(true)
</script>
```

## Accessibility *(v2.25+)*

QSlideTransition is renderless and only animates the height of its child, so it has no ARIA surface of its own. Note that the animation is driven from JavaScript (a Web Animation where the browser supports `calc-size()`, an inline CSS transition elsewhere) and therefore plays regardless of the user's `prefers-reduced-motion` setting — motion-sensitive apps may want to skip the animated collapse for such users.
