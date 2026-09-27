---
title: Pull to Refresh
related:
  - title: Infinite Scroll
    path: infinite-scroll.md
  - title: Intersection
    path: intersection.md
  - title: Icon
    path: icon.md
---
The QPullToRefresh is a component that allows the user to pull down in order to refresh page content (or retrieve the newest content).

## QPullToRefresh API

Not inlined here: call the `get_api` tool with `name: "QPullToRefresh"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Basic

> [!IMPORTANT]
> In your `@refresh` function, don't forget to call the passed in `done()` function when you have finished loading more data.

To refresh, pull down (with mouse or through finger touch) on the content below when the inner scroll position is the top.

```vue
<template>
  <div class="scroll" style="height: 300px">
    <q-pull-to-refresh @refresh="refresh">
      <div v-for="(item, index) in items" :key="index" class="q-mb-sm">
        <q-badge color="secondary">
          {{ items.length - index }}
        </q-badge>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim
        veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea
        commodo consequat. Duis aute irure dolor in reprehenderit in voluptate
        velit esse cillum dolore eu fugiat nulla pariatur.
      </div>
    </q-pull-to-refresh>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const items = ref([{}, {}, {}, {}, {}, {}, {}, {}, {}])

function refresh(done) {
  setTimeout(() => {
    items.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 1000)
}
</script>
```

### Custom icon

```vue
<template>
  <div class="scroll" style="height: 300px">
    <q-pull-to-refresh @refresh="refresh" color="yellow-9" icon="lightbulb">
      <div v-for="(item, index) in items" :key="index" class="q-mb-sm">
        <q-badge color="secondary">
          {{ items.length - index }}
        </q-badge>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim
        veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea
        commodo consequat. Duis aute irure dolor in reprehenderit in voluptate
        velit esse cillum dolore eu fugiat nulla pariatur.
      </div>
    </q-pull-to-refresh>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const items = ref([{}, {}, {}, {}, {}, {}, {}, {}, {}])

function refresh(done) {
  setTimeout(() => {
    items.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 1000)
}
</script>
```

### Custom coloring

```vue
<template>
  <div class="scroll" style="height: 300px">
    <q-pull-to-refresh
      @refresh="refresh"
      color="orange-2"
      bg-color="black"
      icon="autorenew"
    >
      <div v-for="(item, index) in items" :key="index" class="q-mb-sm">
        <q-badge color="accent">
          {{ items.length - index }}
        </q-badge>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim
        veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea
        commodo consequat. Duis aute irure dolor in reprehenderit in voluptate
        velit esse cillum dolore eu fugiat nulla pariatur.
      </div>
    </q-pull-to-refresh>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const items = ref([{}, {}, {}, {}, {}, {}, {}, {}, {}])

function refresh(done) {
  setTimeout(() => {
    items.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 1000)
}
</script>
```

### Side *(v2.30+)*

The `side` prop picks the edge of the content the pull starts from (`top` by default): the refresh is triggered by pulling from that edge towards the inside of the content, while the inner scroll position sits at that edge, and the puller comes in from it. Use `bottom` for messenger-styled content, where the newest entries sit at the bottom, and `left` or `right` for horizontally scrolling content.

Example "Side":

```vue
<template>
  <q-btn-toggle
    v-model="side"
    class="q-ma-md"
    no-caps
    push
    glossy
    toggle-color="primary"
    :options="sideOptions"
  />

  <q-separator />

  <div ref="scrollTargetRef" class="scroll" style="height: 220px">
    <q-pull-to-refresh :side="side" @refresh="refresh">
      <div class="no-wrap" :class="orientationClass">
        <div
          v-for="(item, index) in items"
          :key="index"
          class="col-auto q-pa-md"
          style="width: 250px"
        >
          <q-badge color="secondary">
            {{ index + 1 }}
          </q-badge>
          Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do
          eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim
          ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut
          aliquip ex ea commodo consequat.
        </div>
      </div>
    </q-pull-to-refresh>
  </div>
</template>

<script setup>
import { computed, nextTick, ref, useTemplateRef, watch } from 'vue'

const side = ref('top')
const sideOptions = ['top', 'bottom', 'left', 'right'].map(value => ({
  label: value,
  value
}))

const scrollTargetRef = useTemplateRef('scrollTargetRef')
const items = ref([{}, {}, {}, {}, {}, {}])
const orientationClass = computed(() =>
  side.value === 'top' || side.value === 'bottom' ? 'column' : 'row'
)

// the pull can only start while the content is scrolled to the chosen side
function scrollToSide() {
  const el = scrollTargetRef.value
  el.scrollTop = side.value === 'bottom' ? el.scrollHeight : 0
  el.scrollLeft = side.value === 'right' ? el.scrollWidth : 0
}

watch(side, scrollToSide)

function refresh(done) {
  setTimeout(() => {
    items.value.push({}, {})
    done()
    nextTick(scrollToSide)
  }, 1000)
}
</script>
```

## Tips

> [!NOTE]
> **Scrolling container**
>
> Please read [here](scroll-observer.md#determining-scrolling-container) about how Quasar determines the container to attach scrolling events to.

- If using a QLayout, then it's recommended that you put QPullToRefresh as direct child of QPage and wrap your page content with it.
- Quasar detects the scrolling container by its `scroll`, `scroll-y` or `overflow-auto` class; for a `left` or `right` side inside a container that only has the `scroll-x` class, point the `scroll-target` prop at it.
- If you change the parent of this component, don't forget to call `updateScrollTarget()` on the QPullToRefresh Vue reference.
- QPullToRefresh also allows text selection, so if your content also has images, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

## Accessibility *(v2.25+)*

The pull gesture is pointer-only — keyboard and assistive technology users cannot perform it. The component exposes a `trigger()` method on its ref for exactly this reason: wire it to a visible refresh button so everyone has a way to refresh. The refreshing spinner is not announced to screen readers either, so if completion matters to your users, announce it yourself (e.g. through a live region or a notification) when your `@refresh` handler finishes.
