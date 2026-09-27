---
title: v-scroll-fire directive
related:
  - title: v-scroll directive
    path: scroll.md
  - title: Intersection
    path: ../vue-components/intersection.md
  - title: useIntersection composable
    path: ../vue-composables/use-intersection.md
---
"Scroll Fire" is a directive that enables a method to be called (once and only once) when the DOM element (or component) that it is applied to comes into view.

The element is watched through the same shared [IntersectionObserver](https://developer.mozilla.org/en-US/docs/Web/API/Intersection_Observer_API) that powers the [Intersection](intersection.md) directive, so there is no scroll listener and no cost while nothing changes. "Into view" means the user can actually see it: scroll containers and the page are both accounted for, and the method also fires when the element becomes visible without a scroll (a layout change, a resize, content above it collapsing).

## ScrollFire API

Not inlined here: call the `get_api` tool with `name: "ScrollFire"` for its definition, or add `part` (`value`, `arg`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <p class="text-weight-bold">
    Please scroll down to see the image have a short bounce effect when being
    visible for first time.
  </p>
  <p v-for="n in 10" :key="n">{{ lorem }}</p>

  <p>
    Scroll Fire below: the first logo bounces as soon as any part of it shows
    up, the second one only once it is fully visible. Reload the page to see
    the effect again.
  </p>
  <p class="text-center">
    <img
      alt="Quasar logo"
      v-scroll-fire="bounceImage"
      src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
      style="height: 100px; width: 100px"
    />
  </p>

  <p>{{ lorem }}</p>

  <p class="text-center">
    <img
      alt="Quasar logo"
      v-scroll-fire:1="bounceImage"
      src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
      style="height: 100px; width: 100px"
    />
  </p>

  <p>{{ lorem }}</p>
</template>

<script setup>
const lorem =
  'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.'

function bounceImage(el) {
  // in this example, when the image comes into view,
  // we bounce it for 2 seconds

  el.classList.add('animate-bounce')

  setTimeout(() => {
    // we make sure the node is still in DOM
    // (user hasn't navigated away from the Vue component
    // rendering our `<div>`)
    // so we don't generate an error
    if (document.body.contains(el)) {
      // then remove the helper class to
      // stop bouncing
      el.classList.remove('animate-bounce')
    }
  }, 2000)
}
</script>

<style lang="sass" scoped>
.animate-bounce
  animation: q-bounce 2s infinite

@keyframes q-bounce
  0%, 20%, 50%, 80%, 100%
    transform: translateY(0)
  40%
    transform: translateY(-30px)
  60%
    transform: translateY(-15px)
</style>
```

### Threshold *(v2.30+)*

By default the method fires as soon as any part of the element becomes visible. Pass the fraction of the element (between 0 and 1) that must be visible as the directive's argument:

```html
<!-- fires once half of the element is visible -->
<div v-scroll-fire:0.5="handler" />

<!-- fires only once the element is fully visible -->
<div v-scroll-fire:1="handler" />
```

> [!WARNING]
> An element taller than what can be displayed at once never becomes fully visible, so a threshold of 1 would never fire for it. Use a lower threshold for such elements.

### Disabling and re-arming

Assign `undefined` to disable the directive. Since the method fires only once, assigning a new function after it fired does not fire again; disable it first (`undefined`), then assign the function to arm it again.
