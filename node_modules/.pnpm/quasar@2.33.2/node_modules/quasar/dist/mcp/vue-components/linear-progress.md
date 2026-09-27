---
title: Linear Progress
related:
  - title: Circular Progress
    path: circular-progress.md
  - title: Inner Loading
    path: inner-loading.md
  - title: Spinners
    path: spinners.md
  - title: Loading Plugin
    path: ../quasar-plugins/loading.md
  - title: LoadingBar
    path: ../quasar-plugins/loading-bar.md
---
The QLinearProgress component displays a colored loading bar. The bar can either have a determinate progress or an indeterminate animation. It should be used to inform the user that an action is occurring in the background.

## QLinearProgress API

Not inlined here: call the `get_api` tool with `name: "QLinearProgress"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

### Determined state

```vue
<template>
  <q-btn size="sm" color="primary" @click="randomize" label="Change Model" />

  <q-linear-progress :value="progress" class="q-mt-md" />

  <q-linear-progress :value="progress" color="warning" class="q-mt-sm" />

  <q-linear-progress :value="progress" color="secondary" class="q-mt-sm" />

  <q-linear-progress
    :value="progress"
    rounded
    color="accent"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    rounded
    color="purple"
    track-color="orange"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    rounded
    color="negative"
    class="q-mt-sm"
  />
</template>

<script setup>
import { ref } from 'vue'

const progress = ref(0.4)

function randomize() {
  progress.value = Math.random()
}
</script>
```

### Indeterminate state

```vue
<template>
  <q-linear-progress indeterminate />

  <q-linear-progress indeterminate color="warning" class="q-mt-sm" />

  <q-linear-progress indeterminate color="secondary" class="q-mt-sm" />

  <q-linear-progress indeterminate rounded color="accent" class="q-mt-sm" />

  <q-linear-progress
    indeterminate
    rounded
    track-color="orange"
    color="purple"
    class="q-mt-sm"
  />

  <q-linear-progress indeterminate rounded color="negative" class="q-mt-sm" />
</template>
```

> [!NOTE]
> For indeterminate state (above) or query state (below) you don't need to specify the `value` property.

Example "Query state":

```vue
<template>
  <q-linear-progress query />

  <q-linear-progress query color="warning" class="q-mt-sm" />

  <q-linear-progress query color="secondary" class="q-mt-sm" />

  <q-linear-progress query color="accent" class="q-mt-sm" />

  <q-linear-progress
    query
    track-color="orange"
    color="purple"
    class="q-mt-sm"
  />

  <q-linear-progress query color="negative" class="q-mt-sm" />
</template>
```

### Reversed

Example "Reverse progress direction":

```vue
<template>
  <q-btn size="sm" color="primary" @click="randomize" label="Change Model" />

  <q-linear-progress reverse :value="progress" class="q-mt-md" />

  <q-linear-progress
    reverse
    :value="progress"
    color="warning"
    class="q-mt-sm"
  />

  <q-linear-progress
    reverse
    :value="progress"
    color="secondary"
    class="q-mt-sm"
  />
</template>

<script setup>
import { ref } from 'vue'

const progress = ref(0.4)

function randomize() {
  progress.value = Math.random()
}
</script>
```

### Style

Example "Custom height":

```vue
<template>
  <q-linear-progress size="10px" :value="progress" />

  <q-linear-progress
    rounded
    size="20px"
    :value="progress"
    color="warning"
    class="q-mt-sm"
  />

  <q-linear-progress
    rounded
    size="15px"
    :value="progress"
    color="secondary"
    class="q-mt-sm"
  />

  <q-linear-progress
    size="25px"
    :value="progress"
    color="accent"
    class="q-mt-sm"
  />
</template>

<script setup>
const progress = 0.4
</script>
```

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-md">
    <q-linear-progress
      v-for="size in ['xs', 'sm', 'md', 'lg', 'xl']"
      :key="size"
      :size="size"
      :value="progress"
      color="primary"
      @click="randomize"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const progress = ref(0.65)

function randomize() {
  progress.value = Math.random()
}
</script>
```

Example "Stripe":

```vue
<template>
  <q-linear-progress stripe size="10px" :value="progress1" />

  <q-linear-progress
    stripe
    rounded
    size="20px"
    :value="progress2"
    color="warning"
    class="q-mt-sm"
  />
</template>

<script setup>
const progress1 = 0.4
const progress2 = 0.725
</script>
```

Example "Force dark mode":

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-linear-progress dark size="10px" :value="progress1" color="warning" />

    <q-linear-progress
      dark
      stripe
      rounded
      size="20px"
      :value="progress2"
      color="red"
      class="q-mt-sm"
    />

    <q-linear-progress
      dark
      rounded
      indeterminate
      color="secondary"
      class="q-mt-sm"
    />

    <q-linear-progress dark query color="cyan" class="q-mt-sm" />
  </div>
</template>

<script setup>
const progress1 = 0.4
const progress2 = 0.62
</script>
```

### Buffer

```vue
<template>
  <q-linear-progress :value="progress" :buffer="buffer" />

  <q-linear-progress
    :value="progress"
    :buffer="buffer"
    color="warning"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    :buffer="buffer"
    color="secondary"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    :buffer="buffer"
    color="accent"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    :buffer="buffer"
    color="purple"
    track-color="orange"
    class="q-mt-sm"
  />

  <q-linear-progress
    :value="progress"
    :buffer="buffer"
    color="negative"
    class="q-mt-sm"
  />
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'

const progress = ref(0.01)
const buffer = ref(0.01)

let interval, bufferInterval

onMounted(() => {
  interval = setInterval(
    () => {
      if (progress.value >= 1) {
        progress.value = 0.01
        buffer.value = 0.01
        return
      }

      progress.value = Math.min(1, buffer.value, progress.value + 0.1)
    },
    700 + Math.random() * 1000
  )

  bufferInterval = setInterval(() => {
    if (buffer.value < 1) {
      buffer.value = Math.min(1, buffer.value + Math.random() * 0.2)
    }
  }, 700)
})

onBeforeUnmount(() => {
  clearInterval(interval)
  clearInterval(bufferInterval)
})
</script>
```

### With a label

To add a label to the progress bar you can use the default slot. Take care to:

- use a `size` big enough to allow showing the label
- set a text color for the label so that it is visible both on the filled and unfilled areas, or use text-shadow CSS, or use a QBadge as in the example below

```vue
<template>
  <q-linear-progress size="25px" :value="progress1" color="accent">
    <div class="absolute-full flex flex-center">
      <q-badge color="white" text-color="accent" :label="progressLabel1" />
    </div>
  </q-linear-progress>

  <q-linear-progress
    size="50px"
    :value="progress2"
    color="accent"
    class="q-mt-sm"
  >
    <div class="absolute-full flex flex-center">
      <q-badge color="white" text-color="accent" :label="progressLabel2" />
    </div>
  </q-linear-progress>
</template>

<script setup>
import { computed, ref } from 'vue'

const progress1 = ref(0.3)
const progressLabel1 = computed(() => (progress1.value * 100).toFixed(2) + '%')
const progress2 = ref(0.9)
const progressLabel2 = computed(() => (progress2.value * 100).toFixed(2) + '%')
</script>
```

## Accessibility *(v2.25+)*

QLinearProgress exposes `role="progressbar"` with `aria-valuemin="0"`, `aria-valuemax="1"` and `aria-valuenow` set to the current value — note that the scale is 0..1, matching the `value` prop. While `indeterminate` (or in `query` mode, which animates the same way), `aria-valuenow` is dropped, as the pattern requires. The bar has no accessible name of its own, so add an `aria-label` describing what is progressing.
