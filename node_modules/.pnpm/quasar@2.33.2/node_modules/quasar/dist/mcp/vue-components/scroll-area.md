---
title: Scroll Area
related:
  - title: Layout Drawer
    path: ../layout/drawer.md
---
The QScrollArea component offers a neat way of customizing the scrollbars by encapsulating your content. Think of it as a DOM element which has `overflow: auto`, but with your own custom styled scrollbar instead of browser's default one and a few nice features on top.

## QScrollArea API

Not inlined here: call the `get_api` tool with `name: "QScrollArea"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

The following examples are best seen on desktop as they make too little sense on a mobile device.

> [!NOTE]
> You can also take a look at [Layout Drawer](../layout/drawer.md) to see some more examples of it in action.

### Basic

Example "Vertical content":

```vue
<template>
  <div class="q-ma-md">
    <q-scroll-area style="height: 200px; max-width: 300px">
      <div v-for="n in 100" :key="n" class="q-py-xs">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>
```

Example "Horizontal content":

```vue
<template>
  <q-scroll-area style="height: 230px; max-width: 300px">
    <div class="row no-wrap">
      <div v-for="n in 10" :key="n" style="width: 150px" class="q-pa-sm">
        Lorem ipsum dolor sit amet consectetur adipisicing elit. Architecto
        fuga quae veritatis blanditiis sequi id expedita amet esse aspernatur!
        Iure, doloribus!
      </div>
    </div>
  </q-scroll-area>
</template>
```

Example "Vertical and horizontal content":

```vue
<template>
  <q-scroll-area style="height: 230px; max-width: 300px">
    <div class="row no-wrap" v-for="r in 4" :key="'r' + r">
      <div v-for="n in 10" :key="n" style="width: 150px" class="q-pa-sm">
        Lorem ipsum dolor sit amet consectetur adipisicing elit. Architecto
        fuga quae veritatis blanditiis sequi id expedita amet esse aspernatur!
        Iure, doloribus!
      </div>
    </div>
  </q-scroll-area>
</template>
```

### Styled

Example "Styled thumb and bar":

```vue
<template>
  <div class="q-ma-md">
    <q-scroll-area
      :horizontal-offset="[0, 2]"
      :thumb-style="thumbStyle"
      :bar-style="barStyle"
      style="height: 200px; max-width: 300px"
    >
      <div v-for="n in 100" :key="n" class="q-pa-xs">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>

<script setup>
const thumbStyle = {
  borderRadius: '5px',
  backgroundColor: '#027be3',
  width: '5px',
  opacity: 0.75
}

const barStyle = {
  borderRadius: '9px',
  backgroundColor: '#027be3',
  width: '9px',
  opacity: 0.2
}
</script>
```

```vue
<template>
  <div class="q-ma-md">
    <q-scroll-area
      :horizontal-offset="[0, 2]"
      :thumb-style="thumbStyle"
      :content-style="contentStyle"
      :content-active-style="contentActiveStyle"
      style="height: 200px; max-width: 300px"
    >
      <div v-for="n in 100" :key="n" class="q-pa-xs">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>

<script setup>
const contentStyle = {
  backgroundColor: 'rgba(0,0,0,0.02)',
  color: '#555'
}

const contentActiveStyle = {
  backgroundColor: '#eee',
  color: 'black'
}

const thumbStyle = {
  borderRadius: '5px',
  backgroundColor: '#027be3',
  width: '5px',
  opacity: '0.75'
}
</script>
```

### Dark design

Example "Force dark mode":

```vue
<template>
  <div class="q-ma-md">
    <q-scroll-area
      dark
      class="bg-grey-9 text-white rounded-borders"
      style="height: 200px; max-width: 300px"
    >
      <div v-for="n in 100" :key="n" class="q-py-sm q-px-md">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>
```

### Controlling scrollbar visibility

When using the `visible` Boolean prop, the default mouse over/leave behavior is disabled, leaving you in full control of the scrollbar visibility.

```vue
<template>
  <div class="q-ma-md">
    <div>
      <q-toggle v-model="visible" label="Show scrollbar" />
    </div>

    <q-scroll-area :visible="visible" style="height: 200px; max-width: 300px">
      <div v-for="n in 100" :key="n" class="q-py-xs">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const visible = ref(true)
</script>
```

### Delay

When content changes, the scrollbar appears then disappears again. You can set a certain delay (amount of time in milliseconds) before scrollbar disappears again (if component is not hovered):

```vue
<template>
  <q-btn-group class="q-mb-md">
    <q-btn color="primary" @click="less">Less</q-btn>
    <q-btn color="secondary" @click="more">More</q-btn>
  </q-btn-group>

  <q-scroll-area :delay="1200" style="height: 200px; max-width: 300px">
    <div v-for="n in number" :key="n">
      Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
      tempor incididunt ut labore et dolore magna aliqua.
    </div>
  </q-scroll-area>
</template>

<script setup>
import { ref } from 'vue'

const number = ref(4)

function less() {
  if (number.value > 1) {
    number.value--
  }
}

function more() {
  number.value++
}
</script>
```

### Scroll position

```vue
<template>
  <div class="row q-gutter-md q-mb-md">
    <q-btn
      :label="`Scroll to ${position}px`"
      color="primary"
      @click="scroll"
    />
    <q-btn
      :label="`Animate to ${position}px`"
      color="primary"
      @click="animateScroll"
    />
  </div>

  <q-scroll-area ref="scrollAreaRef" style="height: 150px; max-width: 300px">
    <ol>
      <li v-for="n in 1000" :key="n">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit.
      </li>
    </ol>
  </q-scroll-area>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const position = ref(300)
const scrollAreaRef = useTemplateRef('scrollAreaRef')

function scroll() {
  scrollAreaRef.value.setScrollPosition('vertical', position.value)
  position.value = Math.floor(Math.random() * 1001) * 20
}

function animateScroll() {
  scrollAreaRef.value.setScrollPosition('vertical', position.value, 300)
  position.value = Math.floor(Math.random() * 1001) * 20
}
</script>
```

### Scroll event

Below is an example of using the `@scroll` event to synchronize the scrolling between two containers.

Example "Synchronized":

```vue
<template>
  <div class="q-ma-md row no-wrap">
    <q-scroll-area
      visible
      :horizontal-offset="[0, 2]"
      :thumb-style="thumbStyle"
      :bar-style="barStyle"
      style="height: 200px"
      class="col"
      ref="firstRef"
      @scroll="onScrollFirst"
    >
      <div v-for="n in 100" :key="n" class="q-pa-sm">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>

    <q-scroll-area
      visible
      :horizontal-offset="[0, 2]"
      :thumb-style="thumbStyle"
      :bar-style="barStyle"
      style="height: 200px"
      class="col"
      ref="secondRef"
      @scroll="onScrollSecond"
    >
      <div v-for="n in 100" :key="n" class="q-pa-sm">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
        tempor incididunt ut labore et dolore magna aliqua.
      </div>
    </q-scroll-area>
  </div>
</template>

<script setup>
import { useTemplateRef } from 'vue'

const firstRef = useTemplateRef('firstRef')
const secondRef = useTemplateRef('secondRef')

let ignoreSource

function scroll(source, position) {
  // if we previously just updated
  // the scroll position, then ignore
  // this update as otherwise we'll flicker
  // the position from one scroll area to
  // the other in an infinite loop
  if (ignoreSource === source) {
    ignoreSource = null
    return
  }

  // we'll now update the other scroll area,
  // which will also trigger a @scroll event...
  // and we need to ignore that one
  ignoreSource = source === 'first' ? 'second' : 'first'

  const areaRef = source === 'first' ? secondRef : firstRef

  areaRef.value.setScrollPosition('vertical', position)
}

const thumbStyle = {
  borderRadius: '7px',
  backgroundColor: '#027be3',
  width: '4px',
  opacity: 0.75
}

const barStyle = {
  borderRadius: '9px',
  backgroundColor: '#027be3',
  width: '8px',
  opacity: 0.2
}

function onScrollFirst({ verticalPosition }) {
  scroll('first', verticalPosition)
}

function onScrollSecond({ verticalPosition }) {
  scroll('second', verticalPosition)
}
</script>
```

## Accessibility *(v2.25+)*

The custom scrollbars and their thumbs are hidden from assistive technology — they are redundant, pointer-only controls over what remains a natively scrollable container.

Whenever the content actually overflows, the scroll container becomes a Tab stop on its own, so the browser's native keyboard scrolling — arrow keys, <kbd>PageUp</kbd>/<kbd>PageDown</kbd>, <kbd>Home</kbd>/<kbd>End</kbd> — works without any setup (WCAG 2.1.1). A QScrollArea whose content fits stays out of the tab order, since there would be nothing to scroll. The `tabindex` prop still overrides both cases — pass `-1` to opt out entirely.
