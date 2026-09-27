---
title: v-scroll directive
related:
  - title: v-scroll-fire directive
    path: scroll-fire.md
  - title: Scroll Observer
    path: ../vue-components/scroll-observer.md
---
This is a Vue directive which takes one parameter (a Function) and fires when user scrolls the page containing that DOM node.

> [!NOTE]
>
> - One alternative to using this directive is to place a [QScrollObserver](../vue-components/scroll-observer.md) component on your page.
> - There is one more scrolling-related directive available called [Scroll Fire](scroll-fire.md).

## Scroll API

Not inlined here: call the `get_api` tool with `name: "Scroll"` for its `value` definition.

## Usage

```html
<template>
  ...
  <div v-scroll="onScroll">...</div>
  ...
</template>

<script setup>
  function onScroll(position) {
    // when this method is invoked then it means user
    // has scrolled the page to `position`
    //
    // `position` is an Integer designating the current
    // scroll position in pixels.
  }
</script>
```

```html
<template>
  ...
  <div v-scroll="onScroll">...</div>
  ...
</template>

<script setup>
  import { debounce } from 'quasar'

  const onScroll = debounce(position => {
    // when this method is invoked then it means user
    // has scrolled the page to `position`
    //
    // `position` is an Integer designating the current
    // scroll position in pixels.
  }, 200) // debounce for 200ms
</script>
```

### Determining Scrolling Container

Please read [here](../vue-components/scroll-observer.md#determining-scrolling-container) about how Quasar determines the container to attach scrolling events to.
