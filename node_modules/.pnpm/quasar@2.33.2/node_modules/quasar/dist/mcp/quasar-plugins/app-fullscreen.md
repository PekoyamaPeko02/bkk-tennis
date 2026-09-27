---
title: Fullscreen Plugin
---
There are times when you want your website or App to run in fullscreen.
Quasar makes it easy by wrapping the [Web Fullscreen API](https://developer.mozilla.org/en-US/docs/Web/API/Fullscreen_API).

> [!WARNING]
> Please note that the behavior is different depending on the platform the code is running on, due to the fact that there isn't a fixed Web standard for Web Fullscreen API yet.

## AppFullscreen API

Not inlined here: call the `get_api` tool with `name: "AppFullscreen"` for its definition, or add `part` (`props`, `methods`, `injection`) for one of them.

## Installation

Add to `quasar.config.js`:

```js
framework: {
    plugins: [
      'AppFullscreen'
    ]
}
```

## Usage

> [!NOTE]
> For an exhaustive list of properties and methods, please check out the API section.

Example "Outside of a Vue file":

```js
import { AppFullscreen } from 'quasar'

// Requesting fullscreen mode:
AppFullscreen.request()
  .then(() => {
    // success!
  })
  .catch(err => {
    // oh, no!!!
  })

// Exiting fullscreen mode:
AppFullscreen.exit()
  .then(() => {
    // success!
  })
  .catch(err => {
    // oh, no!!!
  })
```

Example "Inside of a Vue file":

```js
import { useQuasar } from 'quasar'

setup () {
  const $q = useQuasar()

  // Requesting fullscreen mode:
  $q.fullscreen.request()
    .then(() => {
      // success!
    })
    .catch(err => {
      // oh, no!!!
    })

  // Exiting fullscreen mode:
  $q.fullscreen.exit()
    .then(() => {
      // success!
    })
    .catch(err => {
      // oh, no!!!
    })
}
```

Example "Basic":

```vue
<template>
  <q-btn
    color="secondary"
    @click="$q.fullscreen.toggle()"
    :icon="$q.fullscreen.isActive ? 'fullscreen_exit' : 'fullscreen'"
    :label="$q.fullscreen.isActive ? 'Exit Fullscreen' : 'Go Fullscreen'"
  />
</template>
```

Example "On custom element":

```vue
<template>
  <q-btn
    color="secondary"
    @click="toggle"
    :icon="$q.fullscreen.isActive ? 'fullscreen_exit' : 'fullscreen'"
    :label="$q.fullscreen.isActive ? 'Exit Fullscreen' : 'Go Fullscreen'"
  />
</template>

<script setup>
import { useQuasar } from 'quasar'

const $q = useQuasar()

function toggle(e) {
  const target = e.target.parentNode.parentNode.parentNode

  $q.fullscreen
    .toggle(target)
    .then(() => {
      // success!
    })
    .catch(err => {
      // uh, oh, error!!
      alert(err)
      console.error(err)
    })
}
</script>
```

> [!WARNING]
> On some phones this will have little effect:
>
> - For example, on Samsung S4, when App goes into fullscreen, the top bar will slide up but still remain on screen.
> - On Nexus phones, on the other hand, like Nexus 5, Android navigation buttons and top bar disappear completely.
>
> It all depends on the Web Fullscreen API support of the platform the code is running on.

## Watching for fullscreen changes

```html
<template>...</template>

<script setup>
  import { useQuasar } from 'quasar'
  import { watch } from 'vue'

  const $q = useQuasar()

  watch(
    () => $q.fullscreen.isActive,
    val => {
      console.log(val ? 'In fullscreen now' : 'Exited fullscreen')
    }
  )
</script>
```
