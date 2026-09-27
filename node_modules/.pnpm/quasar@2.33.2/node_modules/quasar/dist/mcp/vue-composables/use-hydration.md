---
title: useHydration composable
related:
  - title: No SSR
    path: ../vue-components/no-ssr.md
---
The useHydration composable is useful when you build for SSR or SSG (but can be used for non SSR/SSG builds as well). It is a lower level util of the [QNoSsr](../vue-components/no-ssr.md) component.

## Syntax

```js
import { useHydration } from 'quasar'

setup () {
  const { isHydrated } = useHydration()
}
```

```js
function useHydration(): {
  isHydrated: Ref<boolean>;
};
```

## Example

```html
<template>
  <div>
    <div v-if="isHydrated"> Gets rendered only after hydration. </div>
  </div>
</template>

<script setup>
  import { useHydration } from 'quasar'
  const { isHydrated } = useHydration()
</script>
```
