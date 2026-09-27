---
title: Infinite Scroll
related:
  - title: Spinners
    path: spinners.md
  - title: Pull to Refresh
    path: pull-to-refresh.md
  - title: Intersection
    path: intersection.md
  - title: Virtual Scroll
    path: virtual-scroll.md
---
The QInfiniteScroll component allows you to load new content as the user scrolls the page.

## QInfiniteScroll API

Not inlined here: call the `get_api` tool with `name: "QInfiniteScroll"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!NOTE]
> Infinite Scroll loads items in advance when the end of its content comes within `offset` (default = 500) pixels of the scroll target's visible area. If the content you fetch has height less than the scroll target container's height on screen then Infinite Scroll will continue loading more content. So make sure you load enough content.

> [!IMPORTANT]
> In your `@load` function, don't forget to call the passed in `done()` function when you have finished loading more data.

Scroll to the bottom to see QInfiniteScroll in action.

Example "Basic":

```vue
<template>
  <q-infinite-scroll @load="onLoad" :offset="250">
    <div v-for="(item, index) in items" :key="index" class="caption">
      <p
        >Lorem ipsum dolor sit amet consectetur adipisicing elit. Rerum
        repellendus sit voluptate voluptas eveniet porro. Rerum blanditiis
        perferendis totam, ea at omnis vel numquam exercitationem aut, natus
        minima, porro labore.</p
      >
    </div>
    <template #loading>
      <div class="row justify-center q-my-md">
        <q-spinner-dots color="primary" size="40px" />
      </div>
    </template>
  </q-infinite-scroll>
</template>

<script setup>
import { ref } from 'vue'

const items = ref([{}, {}, {}, {}, {}, {}, {}])

function onLoad(index, done) {
  setTimeout(() => {
    items.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 2000)
}
</script>
```

Example "Custom Scroll Target Container":

```vue
<template>
  <div
    ref="scrollTargetRef"
    class="q-pa-md"
    style="max-height: 250px; overflow: auto"
  >
    <q-infinite-scroll
      @load="onLoadRef"
      :offset="250"
      :scroll-target="scrollTargetRef"
    >
      <div v-for="(item, index) in itemsRef" :key="index" class="caption">
        <p
          >Lorem ipsum dolor sit amet consectetur adipisicing elit. Rerum
          repellendus sit voluptate voluptas eveniet porro. Rerum blanditiis
          perferendis totam, ea at omnis vel numquam exercitationem aut, natus
          minima, porro labore.</p
        >
      </div>
      <template #loading>
        <div class="row justify-center q-my-md">
          <q-spinner-dots color="primary" size="40px" />
        </div>
      </template>
    </q-infinite-scroll>
  </div>

  <q-separator style="height: 2px" />

  <div
    id="scroll-target-id"
    class="q-pa-md"
    style="max-height: 248px; overflow: auto"
  >
    <q-infinite-scroll
      @load="onLoadId"
      :offset="250"
      scroll-target="#scroll-target-id"
    >
      <div v-for="(item, index) in itemsId" :key="index" class="caption">
        <p
          >Lorem ipsum dolor sit amet consectetur adipisicing elit. Rerum
          repellendus sit voluptate voluptas eveniet porro. Rerum blanditiis
          perferendis totam, ea at omnis vel numquam exercitationem aut, natus
          minima, porro labore.</p
        >
      </div>
      <template #loading>
        <div class="row justify-center q-my-md">
          <q-spinner-dots color="primary" size="40px" />
        </div>
      </template>
    </q-infinite-scroll>
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const itemsRef = ref([{}, {}, {}, {}, {}, {}, {}])
const itemsId = ref([{}, {}, {}, {}, {}, {}, {}])
const scrollTargetRef = useTemplateRef('scrollTargetRef')

function onLoadRef(index, done) {
  setTimeout(() => {
    itemsRef.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 2000)
}

function onLoadId(index, done) {
  setTimeout(() => {
    itemsId.value.push({}, {}, {}, {}, {}, {}, {})
    done()
  }, 2000)
}
</script>
```

Example "Reverse (Messenger style)":

```vue
<template>
  <q-infinite-scroll @load="onLoad" reverse>
    <template #loading>
      <div class="row justify-center q-my-md">
        <q-spinner color="primary" name="dots" size="40px" />
      </div>
    </template>

    <div v-for="(item, index) in items" :key="index" class="caption q-py-sm">
      <q-badge class="shadow-1">
        {{ items.length - index }}
      </q-badge>
      Lorem ipsum dolor sit amet consectetur adipisicing elit. Rerum
      repellendus sit voluptate voluptas eveniet porro. Rerum blanditiis
      perferendis totam, ea at omnis vel numquam exercitationem aut, natus
      minima, porro labore.
    </div>
  </q-infinite-scroll>
</template>

<script setup>
import { ref } from 'vue'

const items = ref([{}, {}, {}, {}, {}, {}, {}])

function onLoad(index, done) {
  setTimeout(() => {
    items.value.splice(0, 0, {}, {}, {}, {}, {}, {}, {})
    done()
  }, 2000)
}
</script>
```

## Tips

> [!NOTE]
> **Scrolling container**
>
> Please read [here](scroll-observer.md#determining-scrolling-container) about how Quasar determines the container to attach scrolling events to.

- Works best when placed as direct child of the Vue component rendering your Page
- If you change the parent of this component, don't forget to call `updateScrollTarget()` on the QInfiniteScroll Vue reference.
- If you need to specify the scroll target inner element (because the auto detected one is not the desired one) pass a CSS selector (as string), the DOM element or a Vue component reference (which stands for its root element) in the `scroll-target` prop
- The `offset` is measured against the scroll target's visible area, so a scrolling container that is not detected (nor specified) as the scroll target only reveals the end of the content as it actually scrolls into view; there, loading starts as if `offset` were 0

> [!IMPORTANT]
> If you pass a custom scroll target container with `scroll-target` prop you must make sure that the element exists and that it can be overflowed (it must have a maximum height and an overflow that allows scrolling).
>
> If the scroll target container cannot be overflowed you'll get a forever loading situation.

> [!WARNING]
> Inside a QDialog or any other `position: fixed` container the page itself cannot act as the scroll target: content rendered there never changes the page's scroll size, so the component would have no way to decide when to load. Point the `scroll-target` prop to a scrollable element of the overlay (or wrap the content in one, e.g. with the `scroll` CSS class and a maximum height); without one, automatic loading stays off in such a placement (the `trigger()` method still works).

Example "Usage in QMenu":

```vue
<template>
  <div class="flex flex-center" style="height: 100px">
    <q-btn color="brown" label="Menu with QInfiniteScroll" no-caps>
      <q-menu anchor="bottom middle" self="top middle" :offset="[0, 8]">
        <q-item-label header> Notifications </q-item-label>

        <q-separator />

        <q-list ref="scrollTargetRef" class="scroll" style="max-height: 250px">
          <q-infinite-scroll
            @load="onLoadMenu"
            :offset="250"
            :scroll-target="scrollTargetRef"
          >
            <q-item v-for="(item, index) in itemsMenu" :key="index">
              <q-item-section>
                {{ index + 1 }}. Lorem ipsum dolor sit amet consectetur
                adipisicing elit. Rerum repellendus sit voluptate voluptas
                eveniet porro. Rerum blanditiis perferendis totam, ea at omnis
                vel numquam exercitationem aut, natus minima, porro labore.
              </q-item-section>
            </q-item>

            <template #loading>
              <div class="text-center q-my-md">
                <q-spinner-dots color="primary" size="40px" />
              </div>
            </template>
          </q-infinite-scroll>
        </q-list>
      </q-menu>
    </q-btn>
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const itemsMenu = ref([{}, {}, {}, {}, {}, {}, {}])
const scrollTargetRef = useTemplateRef('scrollTargetRef')

function onLoadMenu(index, done) {
  if (index > 1) {
    setTimeout(() => {
      itemsMenu.value.push({}, {}, {}, {}, {}, {}, {})
      done()
    }, 2000)
  } else {
    setTimeout(() => {
      done()
    }, 200)
  }
}
</script>
```

## Accessibility *(v2.25+)*

Loading is triggered by native scrolling, so keyboard users trigger it too — as long as the scroll target itself can be scrolled with the keyboard (when using a [QScrollArea](scroll-area.md#accessibility) as target, see its Accessibility section).

The loading indicator is not announced to screen readers. If the arrival of new content matters to your users, add text with `role="status"` inside the `loading` slot so assistive technology reports that more content is loading.
