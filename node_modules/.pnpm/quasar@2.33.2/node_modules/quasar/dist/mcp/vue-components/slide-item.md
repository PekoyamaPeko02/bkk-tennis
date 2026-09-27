---
title: Slide Item
related:
  - title: List and List Items
    path: list-and-list-items.md
  - title: Expansion Item
    path: expansion-item.md
---
The QSlideItem component is essentially a [QItem](list-and-list-items.md) with two additional slots (`left` and `right`) which allows user to drag the item (through mouse or with the finger on a touch device) to one of the sides in order to apply a specific action.

## QSlideItem API

Not inlined here: call the `get_api` tool with `name: "QSlideItem"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

Drag with the mouse or use your finger to pan to left or right side to see QSlideItem in action.

> [!TIP]
> If your content also has images, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

Example "Basic":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered separator>
      <q-slide-item @left="onLeft" @right="onRight">
        <template #left>
          <q-icon name="done" />
        </template>
        <template #right>
          <q-icon name="alarm" />
        </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar color="primary" text-color="white" icon="bluetooth" />
          </q-item-section>
          <q-item-section>Icons only</q-item-section>
        </q-item>
      </q-slide-item>

      <q-slide-item @left="onLeft" @right="onRight">
        <template #left> Left </template>
        <template #right> Right content.. long </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar6.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>Text only</q-item-section>
        </q-item>
      </q-slide-item>

      <q-slide-item @left="onLeft" @right="onRight">
        <template #left>
          <div class="row items-center"> <q-icon left name="done" /> Left </div>
        </template>
        <template #right>
          <div class="row items-center">
            Right content.. long <q-icon right name="alarm" />
          </div>
        </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar4.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>Text and icons</q-item-section>
        </q-item>
      </q-slide-item>
    </q-list>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { onBeforeUnmount } from 'vue'

const $q = useQuasar()
let timer

function finalize(reset) {
  timer = setTimeout(() => {
    reset()
  }, 1000)
}

onBeforeUnmount(() => {
  clearTimeout(timer)
})

function onLeft({ reset }) {
  $q.notify('Left action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onRight({ reset }) {
  $q.notify('Right action triggered. Resetting in 1 second.')
  finalize(reset)
}
</script>
```

Example "Vertical":

```vue
<template>
  <div style="max-width: 220px">
    <q-list bordered separator>
      <q-slide-item @top="onTop" @bottom="onBottom">
        <template #top>
          <q-icon name="link" />
        </template>
        <template #bottom>
          <q-icon name="link_off" />
        </template>

        <q-item style="height: 150px">
          <q-item-section avatar>
            <q-avatar color="primary" text-color="white" icon="fingerprint" />
          </q-item-section>
          <q-item-section>Slide vertically</q-item-section>
        </q-item>
      </q-slide-item>
    </q-list>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { onBeforeUnmount } from 'vue'

const $q = useQuasar()
let timer

function finalize(reset) {
  timer = setTimeout(() => {
    reset()
  }, 1000)
}

onBeforeUnmount(() => {
  clearTimeout(timer)
})

function onTop({ reset }) {
  $q.notify('Top action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onBottom({ reset }) {
  $q.notify('Bottom action triggered. Resetting in 1 second.')
  finalize(reset)
}
</script>
```

Example "Custom colors":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered separator>
      <q-slide-item
        @left="onLeft"
        @right="onRight"
        left-color="red"
        right-color="purple"
      >
        <template #left>
          <div class="row items-center"> <q-icon left name="done" /> Left </div>
        </template>
        <template #right>
          <div class="row items-center">
            Right content.. long <q-icon right name="alarm" />
          </div>
        </template>

        <q-item>
          <q-item-section avatar>
            <q-icon color="primary" name="cell_wifi" />
          </q-item-section>
          <q-item-section>Custom colors (red, purple)</q-item-section>
        </q-item>
      </q-slide-item>

      <q-slide-item
        @left="onLeft"
        @right="onRight"
        left-color="amber"
        right-color="primary"
      >
        <template #left>
          <div class="row items-center text-black">
            <q-icon left name="done" /> Left
          </div>
        </template>
        <template #right>
          <div class="row items-center">
            Right content.. long <q-icon right name="alarm" />
          </div>
        </template>

        <q-item class="bg-grey-9 text-white">
          <q-item-section avatar>
            <q-icon color="amber" name="event" />
          </q-item-section>
          <q-item-section>Custom colors 2</q-item-section>
        </q-item>
      </q-slide-item>
    </q-list>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { onBeforeUnmount } from 'vue'

const $q = useQuasar()
let timer

function finalize(reset) {
  timer = setTimeout(() => {
    reset()
  }, 1000)
}

onBeforeUnmount(() => {
  clearTimeout(timer)
})

function onLeft({ reset }) {
  $q.notify('Left action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onRight({ reset }) {
  $q.notify('Right action triggered. Resetting in 1 second.')
  finalize(reset)
}
</script>
```

Example "Customize while sliding":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered separator>
      <q-slide-item
        :left-color="leftColor"
        :right-color="rightColor"
        @left="onLeft"
        @right="onRight"
        @slide="onSlide"
      >
        <template #left> Left </template>
        <template #right> Right content.. long </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar6.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>Text only</q-item-section>
        </q-item>
      </q-slide-item>
    </q-list>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { computed, onBeforeUnmount, ref } from 'vue'

const $q = useQuasar()
let timer

const slideRatio = ref({
  left: 0,
  right: 0
})

const leftColor = computed(() =>
  slideRatio.value.left >= 1
    ? 'red-10'
    : 'red-' + (3 + Math.round(Math.min(3, slideRatio.value.left * 3)))
)

const rightColor = computed(() =>
  slideRatio.value.right >= 1
    ? 'green-10'
    : 'green-' + (3 + Math.round(Math.min(3, slideRatio.value.right * 3)))
)

function finalize(reset) {
  clearTimeout(timer)
  timer = setTimeout(() => {
    reset()
  }, 1000)
}

onBeforeUnmount(() => {
  clearTimeout(timer)
})

function onLeft({ reset }) {
  $q.notify('Left action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onRight({ reset }) {
  $q.notify('Right action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onSlide({ side, ratio, isReset }) {
  clearTimeout(timer)
  timer = setTimeout(
    () => {
      slideRatio.value[side] = ratio
    },
    isReset ? 200 : void 0
  )
}
</script>
```

Example "One sided or no sides":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered separator>
      <q-slide-item @left="onLeft" @right="onRight">
        <template #left>
          <q-icon name="done" />
        </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar2.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>Only left action</q-item-section>
        </q-item>
      </q-slide-item>

      <q-slide-item @left="onLeft" @right="onRight">
        <template #right>
          <q-icon name="alarm" />
        </template>

        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar3.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>Only right action</q-item-section>
        </q-item>
      </q-slide-item>

      <q-slide-item @left="onLeft" @right="onRight">
        <q-item>
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/avatar5.jpg"
                draggable="false"
              />
            </q-avatar>
          </q-item-section>
          <q-item-section>No actions</q-item-section>
        </q-item>
      </q-slide-item>
    </q-list>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { onBeforeUnmount } from 'vue'

const $q = useQuasar()
let timer

function finalize(reset) {
  timer = setTimeout(() => {
    reset()
  }, 1000)
}

onBeforeUnmount(() => {
  clearTimeout(timer)
})

function onLeft({ reset }) {
  $q.notify('Left action triggered. Resetting in 1 second.')
  finalize(reset)
}

function onRight({ reset }) {
  $q.notify('Right action triggered. Resetting in 1 second.')
  finalize(reset)
}
</script>
```

## Accessibility *(v2.25+)*

> [!WARNING]
> The slide actions are pointer gestures only — there is no keyboard interaction and no assistive technology path to trigger them. Any behavior you bind to the slide events is unreachable for keyboard and screen reader users.

Always provide an equivalent way to perform the same actions — visible buttons, or a [QMenu](menu.md) with the same commands — so the gestures remain a convenience rather than the only route.
