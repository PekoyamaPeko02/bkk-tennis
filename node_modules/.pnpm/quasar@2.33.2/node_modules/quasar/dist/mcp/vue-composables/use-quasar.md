---
title: useQuasar composable
related:
  - title: The $q object
    path: ../options/the-q-object.md
---
The useQuasar composable is used in order to get access to the [$q Object](../options/the-q-object.md).

## Syntax

```js
import { useQuasar } from 'quasar'

setup () {
  const $q = useQuasar()
}
```

## Example

```html
<template>
  <div>
    <div v-if="$q.platform.is.ios"> Gets rendered only on iOS platform. </div>
  </div>
</template>

<script setup>
  import { useQuasar } from 'quasar'

  const $q = useQuasar()

  console.log($q.platform.is.ios)

  // showing an example on a method, but
  // can be any part of Vue script
  function show() {
    // prints out Quasar version
    console.log($q.version)
  }
</script>
```
