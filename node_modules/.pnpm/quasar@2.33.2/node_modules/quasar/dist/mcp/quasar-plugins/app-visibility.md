---
title: App Visibility
---
Quasar makes use of the Web [Page Visibility API](https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API) which lets you know when a website/app is visible or in focus.

## AppVisibility API

Not inlined here: call the `get_api` tool with `name: "AppVisibility"` for its definition, or add `part` (`props`, `injection`) for one of them.

## Installation

Add to `quasar.config.js`:

```js
framework: {
    plugins: [
      'AppVisibility'
    ]
}
```

## Usage

Example "Outside of a Vue file":

```js
import { AppVisibility } from 'quasar'
AppVisibility.appVisible // Boolean

// inside of a Vue file
import { useQuasar } from 'quasar'
setup () {
  const $q = useQuasar()
  // now use $q.appVisible (Boolean)
}
```

Example "AppVisibility":

```vue
<template>
  <div>
    Switch to another browser tab or app then come back here to see some
    changes.
  </div>

  <q-markup-table v-if="eventList.length > 0" class="q-mt-md">
    <tbody>
      <tr v-for="evt in eventList" :key="evt.timestamp">
        <td>{{ evt.timestamp }}</td>
        <td>{{ evt.label }}</td>
      </tr>
    </tbody>
  </q-markup-table>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { ref, watch } from 'vue'

function pad(number) {
  return (number < 10 ? '0' : '') + number
}

const $q = useQuasar()
const eventList = ref([])

watch(
  () => $q.appVisible,
  state => {
    const date = new Date()
    eventList.value.unshift({
      timestamp:
        pad(date.getHours()) +
        ':' +
        pad(date.getMinutes()) +
        ':' +
        pad(date.getSeconds()) +
        '.' +
        date.getMilliseconds(),
      label: `App became ${state ? 'visible' : 'hidden'}`
    })
  }
)
</script>
```

## Watching for status change

```html
<template>...</template>

<script setup>
  import { useQuasar } from 'quasar'
  import { watch } from 'vue'

  const $q = useQuasar()

  watch(
    () => $q.appVisible,
    val => {
      console.log(val ? 'App became visible' : 'App went in the background')
    }
  )
</script>
```
