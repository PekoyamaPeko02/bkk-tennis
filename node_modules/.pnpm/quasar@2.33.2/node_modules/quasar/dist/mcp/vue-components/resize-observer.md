---
title: Resize Observer
related:
  - title: Scroll Observer
    path: scroll-observer.md
---
QResizeObserver is a Quasar component that emits a `resize` event whenever the wrapping DOM element / component (defined as direct parent of QResizeObserver) changes its size (width and/or height). The reported size is the element's outer size (padding and border included), so a padding change on the wrapping element itself is reported too. Note that no polling is involved, but overusing it is costly too.

## QResizeObserver API

Not inlined here: call the `get_api` tool with `name: "QResizeObserver"` for its definition, or add `part` (`props`, `methods`, `events`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn
      color="primary"
      push
      @click="setRandomSize"
      label="Set Random Size"
    />

    <div :style="style" class="container bg-amber rounded-borders glossy">
      <!--
        we listen for size changes on this next
        <div>, so we place the observer as direct child:
      -->
      <q-resize-observer @resize="onResize" />
    </div>

    <div v-if="report" class="q-gutter-sm">
      Reported:
      <q-badge>width: {{ report.width }}</q-badge>
      <q-badge>height: {{ report.height }}</q-badge>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const style = ref({ width: '200px', height: '200px' })
const report = ref(null)

function onResize(size) {
  report.value = size
  // {
  //   width: 20 // width of container (in px)
  //   height: 50 // height of container (in px)
  // }
}

function setRandomSize() {
  style.value = {
    width: Math.floor(100 + Math.random() * 200) + 'px',
    height: Math.floor(100 + Math.random() * 200) + 'px'
  }
}
</script>

<style lang="sass" scoped>
.container
  transition: width .3s, height .3s
</style>
```

Please note that QResizeObserver will issue an event as soon as it gets rendered and attached to DOM, so you can have the initial size of the container.

## Accessibility *(v2.25+)*

QResizeObserver renders nothing, so it has no accessibility surface.
