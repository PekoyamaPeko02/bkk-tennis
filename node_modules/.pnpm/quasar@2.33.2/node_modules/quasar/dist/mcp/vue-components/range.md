---
title: Range
related:
  - title: Slider
    path: slider.md
  - title: Field
    path: field.md
---
The QRange component is a great way to offer the user the selection of a sub-range of values between a minimum and maximum value, with optional steps to select those values. An example use case for the Range component would be to offer a price range selection.

Also check out its “sibling”, the [QSlider](slider.md) component.

## QRange API

Not inlined here: call the `get_api` tool with `name: "QRange"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

Notice we are using an object for the selection, which holds values for both the lower value of the selected range - `rangeValues.min` and the higher value - `rangeValues.max`.

### Standard

> [!IMPORTANT]
> You are responsible for accommodating the space around QRange so that the label and marker labels won't overlap the other content on your page. You can use CSS margin or padding for this purpose.

```vue
<template>
  <q-badge color="secondary">
    Model: {{ standard.min }} to {{ standard.max }} (0 to 50)
  </q-badge>

  <q-range v-model="standard" :min="0" :max="50" />
  <q-range v-model="standard" :min="0" :max="50" color="green" />
</template>

<script setup>
import { ref } from 'vue'

const standard = ref({
  min: 10,
  max: 35
})
</script>
```

### Vertical

Example "Vertical orientation":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ standard.min }} to {{ standard.max }} (0 to 50)
  </q-badge>

  <div class="row justify-around">
    <q-range
      v-model="standard"
      :min="0"
      :max="50"
      vertical
      label
      switch-label-side
    />

    <q-range
      v-model="standard"
      :min="0"
      :max="50"
      color="green"
      vertical
      label-always
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const standard = ref({
  min: 10,
  max: 35
})
</script>
```

### With inner min/max

Sometimes you need to restrict the model value to an interval inside of the track's length. For this purpose, use `inner-min` and `inner-max` props. First prop needs to be higher or equal to `min` prop while the latter needs to be lower or equal to the `max` prop.

Example "Inner min/max":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ model.min }} to {{ model.max }} (0 to 50 w/ selection 10 to 35
    or 15 to 40)
  </q-badge>

  <q-range
    v-model="model"
    :min="0"
    :max="50"
    :inner-min="10"
    :inner-max="35"
  />
  <q-range
    v-model="model"
    :min="0"
    :max="50"
    color="green"
    :inner-min="15"
    :inner-max="40"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 20,
  max: 25
})
</script>
```

### Range width limits *(v2.26+)*

Use the `min-range` and `max-range` props to constrain the width of the selection (the difference between the model's `max` and `min`). Think of picking an event's duration: at least 15 minutes long, but placed anywhere within the hour.

External model values that would break these limits get coerced (in the same way `inner-min`/`inner-max` act), with the model's `min` acting as the anchor. Should the two props conflict, `min-range` wins.

Example "Minimum width":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ model.min }} to {{ model.max }} (0 to 10 w/ min-range 2 or 3)
  </q-badge>

  <q-range v-model="model" :min="0" :max="10" :min-range="2" markers />
  <q-range
    v-model="model"
    :min="0"
    :max="10"
    :min-range="3"
    color="green"
    markers
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 4,
  max: 6
})
</script>
```

Example "Maximum width":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ model.min }} to {{ model.max }} (0 to 10 w/ max-range 4 or 5)
  </q-badge>

  <q-range v-model="model" :min="0" :max="10" :max-range="4" markers />
  <q-range
    v-model="model"
    :min="0"
    :max="10"
    :max-range="5"
    color="green"
    markers
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 3,
  max: 8
})
</script>
```

### With step

```vue
<template>
  <q-badge color="secondary">
    Model: {{ step.min }} to {{ step.max }} (0 to 45, step 5)
  </q-badge>

  <q-range
    v-model="step"
    :min="0"
    :max="45"
    :step="5"
    label
    color="deep-orange"
  />
</template>

<script setup>
import { ref } from 'vue'

const step = ref({
  min: 10,
  max: 20
})
</script>
```

The `step` property can also be a floating point number (or numeric `0` if you need infinite precision).

Example "Floating point":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ precision.min }} to {{ precision.max }} (0.1 to 1.5)
  </q-badge>

  <q-range
    v-model="precision"
    :min="0.1"
    :max="1.5"
    :step="0.1"
    color="green"
  />

  <q-badge color="secondary">
    Model: {{ zeroPrecision.min }} to {{ zeroPrecision.max }} (0.1 to 1.5)
  </q-badge>

  <q-range
    v-model="zeroPrecision"
    :min="0.1"
    :max="1.5"
    :step="0"
    color="amber"
  />
</template>

<script setup>
import { ref } from 'vue'

const precision = ref({
  min: 0.2,
  max: 0.7
})

const zeroPrecision = ref({
  min: 0.2,
  max: 0.7
})
</script>
```

Example "Snaps to steps":

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ snap.min }} to {{ snap.max }} (0 to 20, step 2)
  </q-badge>

  <q-range v-model="snap" :min="0" :max="20" :step="2" label snap />
</template>

<script setup>
import { ref } from 'vue'

const snap = ref({
  min: 2,
  max: 12
})
</script>
```

### With label

In the example below, move the slider to see the label.

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ label.min }} to {{ label.max }} (-20 to 20, step 4)
  </q-badge>

  <q-range
    v-model="label"
    :min="-20"
    :max="20"
    :step="4"
    label
    color="purple"
  />
</template>

<script setup>
import { ref } from 'vue'

const label = ref({
  min: -12,
  max: 8
})
</script>
```

Example "Always display label":

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ label.min }} to {{ label.max }} (-20 to 20, step 4)
  </q-badge>

  <q-range
    v-model="label"
    :min="-20"
    :max="20"
    :step="4"
    label-always
    color="brown"
  />
</template>

<script setup>
import { ref } from 'vue'

const label = ref({
  min: -12,
  max: 8
})
</script>
```

Example "Custom label values":

```vue
<template>
  <q-badge color="secondary" class="q-mb-sm">
    Model: {{ model.min }} to {{ model.max }} (-20 to 20, step 4)
  </q-badge>

  <q-range
    v-model="model"
    :min="-20"
    :max="20"
    :step="4"
    :left-label-value="model.min + 'px'"
    :right-label-value="model.max + 'px'"
    label-always
    switch-label-side
    color="purple"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: -12,
  max: 8
})
</script>
```

The example below is better highlighting how QRange handles label positioning so that it always stays inside the QRange's box horizontally.

Example "Long label":

```vue
<template>
  <q-range
    class="q-mt-lg"
    v-model="model"
    :min="-20"
    :max="20"
    :step="4"
    :left-label-value="'Value: ' + model.min + 'px'"
    :right-label-value="'Value: ' + model.max + 'px'"
    label-always
    color="purple"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: -16,
  max: 16
})
</script>
```

### Markers

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ marker.min }} to {{ marker.max }} (-6 to 10, step 2)
  </q-badge>

  <q-range
    v-model="marker"
    :min="-6"
    :max="10"
    :step="2"
    label
    markers
    color="orange"
  />

  <q-range
    v-model="marker"
    :min="-6"
    :max="10"
    :step="2"
    label
    snap
    markers
    color="purple"
  />

  <q-badge color="secondary">
    Model: {{ orangeModel }}<br />(0 to 16, step 2, marker step 4)
  </q-badge>
  <q-range
    v-model="orangeModel"
    :min="-8"
    :max="16"
    :step="2"
    label
    snap
    :markers="4"
    color="orange"
  />
</template>

<script setup>
import { ref } from 'vue'

const marker = ref({
  min: 6,
  max: 8
})
const orangeModel = ref({
  min: 6,
  max: 10
})
</script>
```

### Marker labels

```vue
<template>
  <q-range v-model="model" marker-labels :min="0" :max="6" />

  <q-range
    class="q-mt-xl"
    v-model="model"
    color="deep-orange"
    markers
    :marker-labels="fnMarkerLabel"
    :min="0"
    :max="6"
  />

  <q-range
    class="q-mt-xl"
    v-model="model"
    color="purple"
    markers
    :marker-labels="objMarkerLabel"
    :min="0"
    :max="6"
  />

  <q-range
    class="q-mt-xl"
    v-model="priceModel"
    color="green"
    :inner-min="3"
    :inner-max="6"
    markers
    :marker-labels="arrayMarkerLabel"
    label-always
    :left-label-value="minPriceLabel"
    :right-label-value="maxPriceLabel"
    switch-label-side
    switch-marker-labels-side
    :min="2"
    :max="7"
  />
</template>

<script setup>
import { computed, ref } from 'vue'

const model = ref({
  min: 2,
  max: 4
})
const priceModel = ref({
  min: 4,
  max: 6
})

const fnMarkerLabel = val => `${10 * val}%`
const objMarkerLabel = { 0: '0°C', 3: { label: '3°C' }, 5: '5°C', 6: '6°C' }

const minPriceLabel = computed(() => `$ ${priceModel.value.min}`)
const maxPriceLabel = computed(() => `$ ${priceModel.value.max}`)
const arrayMarkerLabel = [
  { value: 3, label: '$3' },
  { value: 4, label: '$4' },
  { value: 5, label: '$5' },
  { value: 6, label: '$6' }
]
</script>
```

> [!IMPORTANT]
> **TIP on slots**
>
> In order to use the marker label slots (see below), you must enable them by using the `marker-labels` prop.

Example "Marker label slots":

```vue
<template>
  <q-range
    class="q-mt-xl"
    v-model="firstModel"
    color="deep-orange"
    label-always
    markers
    marker-labels
    :min="1"
    :max="10"
    :inner-min="2"
    :inner-max="8"
  >
    <template #marker-label-group="scope">
      <div
        v-for="marker in scope.markerList"
        :key="marker.index"
        :class="[
          `text-deep-orange-${2 + Math.ceil(marker.value / 2)}`,
          marker.classes
        ]"
        :style="marker.style"
        @click="model = marker.value"
        >{{ marker.value }}</div
      >
    </template>
  </q-range>

  <q-range
    class="q-mt-xl"
    v-model="secondModel"
    color="orange"
    markers
    :min="0"
    :max="5"
    marker-labels
    switch-marker-labels-side
  >
    <template #marker-label-group="{ markerList }">
      <div
        v-for="val in 4"
        :key="val"
        :class="markerList[val].classes"
        :style="markerList[val].style"
        >{{ val }}</div
      >

      <q-icon
        v-for="val in [0, 5]"
        :key="val"
        :class="markerList[val].classes"
        :style="markerList[val].style"
        size="sm"
        color="orange"
        :name="val === 0 ? 'volume_off' : 'volume_up'"
      />
    </template>
  </q-range>

  <q-range
    class="q-mt-xl"
    v-model="thirdModel"
    color="teal"
    :left-thumb-color="thirdModel.min === 0 ? 'grey' : 'teal'"
    :right-thumb-color="thirdModel.max === 5 ? 'black' : 'teal'"
    snap
    :min="0"
    :max="5"
    :step="0.5"
    vertical
    marker-labels
  >
    <template #marker-label-group="{ markerMap }">
      <div
        class="row items-center no-wrap"
        :class="markerMap[thirdModel.min].classes"
        :style="markerMap[thirdModel.min].style"
      >
        <q-icon
          v-if="thirdModel.min === 0"
          size="xs"
          color="teal"
          name="star_outline"
        />

        <template v-else>
          <q-icon
            v-for="i in Math.floor(thirdModel.min)"
            :key="i"
            size="xs"
            color="teal"
            name="star_rate"
          />

          <q-icon
            v-if="thirdModel.min > Math.floor(thirdModel.min)"
            size="xs"
            color="teal"
            name="star_half"
          />
        </template>
      </div>

      <div
        class="row items-center no-wrap"
        :class="markerMap[thirdModel.max].classes"
        :style="markerMap[thirdModel.max].style"
      >
        <q-icon
          v-for="i in Math.floor(thirdModel.max)"
          :key="i"
          size="xs"
          color="teal"
          name="star_rate"
        />

        <q-icon
          v-if="thirdModel.max > Math.floor(thirdModel.max)"
          size="xs"
          color="teal"
          name="star_half"
        />
      </div>
    </template>
  </q-range>
</template>

<script setup>
import { ref } from 'vue'

const firstModel = ref({
  min: 2,
  max: 4
})
const secondModel = ref({
  min: 2,
  max: 4
})
const thirdModel = ref({
  min: 2.5,
  max: 4.5
})
</script>
```

### Other customizations

Example "Color customizations":

```vue
<template>
  <q-range
    v-model="model"
    color="orange"
    thumb-color="purple"
    label-color="black"
    label-text-color="yellow"
    markers
    marker-labels
    switch-marker-labels-side
    label-always
    switch-label-side
    :min="0"
    :max="6"
  />

  <q-range
    class="q-mt-xl"
    v-model="model"
    color="orange"
    left-thumb-color="purple-3"
    left-label-color="green"
    right-thumb-color="purple-8"
    right-label-color="black"
    markers
    marker-labels
    switch-marker-labels-side
    label-always
    switch-label-side
    :min="0"
    :max="6"
  />

  <q-range
    class="q-mt-xl"
    v-model="secondModel"
    color="green"
    track-color="orange"
    inner-track-color="transparent"
    selection-color="red"
    :max="10"
    markers
  />

  <q-range
    v-model="secondModel"
    color="purple"
    inner-track-color="light-blue-3"
    :max="10"
    :inner-min="2"
    :inner-max="8"
    markers
  />

  <q-range
    v-model="secondModel"
    color="teal"
    track-color="light-blue-2"
    inner-track-color="light-blue-5"
    :max="10"
    :inner-min="2"
    :inner-max="8"
    markers
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 2,
  max: 4
})

const secondModel = ref({
  min: 3,
  max: 5
})
</script>
```

Example "Hide selection bar":

```vue
<template>
  <q-range
    v-model="model"
    :min="0"
    :max="10"
    markers
    selection-color="transparent"
  />

  <q-range
    v-model="model"
    :min="0"
    :max="10"
    track-color="orange"
    inner-track-color="transparent"
    selection-color="transparent"
    markers
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 2,
  max: 6
})
</script>
```

Example "Custom track images":

```vue
<template>
  <q-range
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    track-color="grey-2"
    inner-track-color="transparent"
    selection-color="transparent"
    :track-img="img"
  />

  <q-range
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    track-color="white"
    inner-track-color="transparent"
    :track-img="img"
  />

  <q-range
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    inner-track-color="transparent"
    :selection-img="img"
  />

  <q-range
    v-model="model"
    color="deep-orange"
    :max="10"
    :inner-min="2"
    :inner-max="8"
    track-size="8px"
    inner-track-color="white"
    :inner-track-img="img"
    selection-color="transparent"
  />
</template>

<script setup>
import { ref } from 'vue'

// supports URL too!
// or relative path (if on Quasar CLI / Vite Plugin)
// or ~@/assets/some-image.png (if on Quasar CLI)
const img =
  'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAH0lEQVQoU2NkYGAwZkAFZ5G5jPRRgOYEVDeB3EBjBQBOZwTVugIGyAAAAABJRU5ErkJggg=='

const model = ref({
  min: 3,
  max: 7
})
</script>
```

Example "Track & thumb size":

```vue
<template>
  <q-range
    v-model="model"
    :min="0"
    :max="10"
    color="green"
    track-size="10px"
    thumb-color="black"
    markers
  />

  <q-range
    v-model="model"
    :min="0"
    :max="10"
    color="green"
    thumb-size="35px"
    markers
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 1,
  max: 6
})
</script>
```

### Dragging range

Use the `drag-range` or `drag-only-range` props to allow the user to move the selected range or only a predetermined range as a whole.

Example "Drag range":

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ range.min }} to {{ range.max }} (0 to 100, step 1)
  </q-badge>

  <q-range v-model="range" :min="0" :max="100" label drag-range />
</template>

<script setup>
import { ref } from 'vue'

const range = ref({
  min: 20,
  max: 65
})
</script>
```

Example "Drag range + snap to step":

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ rangeSnap.min }} to {{ rangeSnap.max }} (0 to 100, step 5)
  </q-badge>

  <q-range
    v-model="rangeSnap"
    :min="0"
    :max="100"
    :step="5"
    drag-range
    label
    markers
    snap
    color="lime"
  />
</template>

<script setup>
import { ref } from 'vue'

const rangeSnap = ref({
  min: 35,
  max: 60
})
</script>
```

Example "Drag only range (fixed interval)":

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ onlyRange.min }} to {{ onlyRange.max }} (0 to 100, step 5)
  </q-badge>

  <q-range
    v-model="onlyRange"
    :min="0"
    :max="100"
    :step="5"
    drag-only-range
    label
    color="info"
  />
</template>

<script setup>
import { ref } from 'vue'

const onlyRange = ref({
  min: 10,
  max: 35
})
</script>
```

### Lazy input

```vue
<template>
  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ lazy.min }} to {{ lazy.max }} (0 to 50, step 1)
  </q-badge>

  <q-range
    :model-value="lazy"
    @change="
      val => {
        lazy = val
      }
    "
    :min="0"
    :max="50"
    label
  />
</template>

<script setup>
import { ref } from 'vue'

const lazy = ref({
  min: 10,
  max: 35
})
</script>
```

### Null values

```vue
<template>
  <q-badge color="primary" class="q-mb-lg">
    Model: {{ getNullLabel(bothNull.min) }} to
    {{ getNullLabel(bothNull.max) }} (0 to 50, step 1)
  </q-badge>

  <q-range v-model="bothNull" color="primary" :min="0" :max="50" />

  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ getNullLabel(minNull.min) }} to
    {{ getNullLabel(minNull.max) }} (0 to 50, step 1)
  </q-badge>

  <q-range v-model="minNull" color="secondary" :min="0" :max="50" />

  <q-badge color="accent" class="q-mb-lg">
    Model: {{ getNullLabel(maxNull.min) }} to
    {{ getNullLabel(maxNull.max) }} (0 to 50, step 1)
  </q-badge>

  <q-range v-model="maxNull" color="accent" :min="0" :max="50" />
</template>

<script setup>
import { ref } from 'vue'

const bothNull = ref({
  min: null,
  max: null
})

const minNull = ref({
  min: null,
  max: 40
})

const maxNull = ref({
  min: 20,
  max: null
})

function getNullLabel(val) {
  return val === null ? 'null' : val
}
</script>
```

### Reverse

Example "In reverse":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ standard.min }} to {{ standard.max }} (0 to 50)
  </q-badge>

  <q-range reverse v-model="standard" :min="0" :max="50" />
  <q-range reverse v-model="standard" :min="0" :max="50" color="green" />
</template>

<script setup>
import { ref } from 'vue'

const standard = ref({
  min: 5,
  max: 15
})
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-badge color="secondary" class="q-mb-lg">
      Model: {{ model.min }} to {{ model.max }} (0 to 50)
    </q-badge>

    <q-range v-model="model" :min="0" :max="50" dark />
    <q-range v-model="model" :min="0" :max="50" color="green" dark />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 10,
  max: 35
})
</script>
```

### Readonly and disable

Example "Readonly":

```vue
<template>
  <q-range v-model="standard" color="secondary" :min="0" :max="50" readonly />
</template>

<script setup>
import { ref } from 'vue'

const standard = ref({
  min: 10,
  max: 35
})
</script>
```

Example "Disable":

```vue
<template>
  <q-range v-model="model" color="secondary" :min="0" :max="50" disable />
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 10,
  max: 35
})
</script>
```

### With QItem

```vue
<template>
  <q-list dense>
    <q-item>
      <q-item-section avatar>
        <q-icon name="local_atm" />
      </q-item-section>
      <q-item-section>
        <q-range v-model="model" :min="0" :max="50" label />
      </q-item-section>
    </q-item>

    <q-item>
      <q-item-section avatar>
        <q-icon name="euro_symbol" />
      </q-item-section>
      <q-item-section>
        <q-range v-model="model" :min="0" :max="50" label />
      </q-item-section>
    </q-item>
  </q-list>
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  min: 10,
  max: 35
})
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QRange, otherwise formData will not contain it (if it should):

The submitted value contains the minimum and maximum values separated by a pipe (`min|max`).

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <div class="q-mt-xl">
      <q-range name="price_range" v-model="range" label-always />
    </div>

    <div>
      <q-btn label="Submit" type="submit" color="primary" />
    </div>
  </q-form>

  <q-card
    v-if="submitResult.length > 0"
    flat
    bordered
    class="q-mt-md"
    :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
  >
    <q-card-section
      >Submitted form contains the following formData (key =
      value):</q-card-section
    >
    <q-separator />
    <q-card-section class="row q-gutter-sm items-center">
      <div
        v-for="(item, index) in submitResult"
        :key="index"
        class="q-px-sm q-py-xs bg-grey-8 text-white rounded-borders text-center text-no-wrap"
        >{{ item.name }} = {{ item.value }}</div
      >
    </q-card-section>
  </q-card>
</template>

<script setup>
import { ref } from 'vue'

const range = ref({
  min: 10,
  max: 50
})
const submitResult = ref([])

function onSubmit(evt) {
  const formData = new FormData(evt.target)
  const data = []

  for (const [name, value] of formData.entries()) {
    data.push({
      name,
      value
    })
  }

  submitResult.value = data
}
</script>
```

## Accessibility *(v2.25+)*

QRange follows the [WAI-ARIA multi-thumb slider pattern](https://www.w3.org/WAI/ARIA/apg/patterns/slider-multithumb/): each thumb is its own `role="slider"` element exposing `aria-valuemin`/`aria-valuemax`/`aria-valuenow` (each thumb's limit follows the other one, mirroring how the values clamp against each other), `aria-orientation` and `aria-disabled`/`aria-readonly`, wrapped in a `role="group"` container. The thumbs are named "Minimum"/"Maximum" through the [Quasar Language Pack](../options/quasar-language-packs.md) (`label.minimum`/`label.maximum`) — override them per instance with the `left-thumb-aria-label`/`right-thumb-aria-label` props. A `left-label-value`/`right-label-value` (e.g. "20%") also becomes its thumb's `aria-valuetext`, so screen readers announce the same formatted value sighted users see. A thumb whose model side is `null` sits on its limit but reports a localized "No value" as `aria-valuetext`, so an untouched range is not announced as a full selection.

The keyboard behavior matches [QSlider](slider.md#accessibility): <kbd>Tab</kbd> reaches each thumb in turn, the <kbd>Arrow</kbd> keys step whichever thumb has focus (RTL/`reverse`/`vertical`-aware), <kbd>PageUp</kbd> / <kbd>PageDown</kbd> jump by 10 steps and <kbd>Home</kbd> / <kbd>End</kbd> go straight to the focused thumb's limits, on every platform.

With `drag-range`, the track container is an additional Tab stop whose keys move the entire selected window, preserving its width; with `drag-only-range` it is the only one, so the slider semantics (localized "Range" name, the minimum as `aria-valuenow`, both formatted values as `aria-valuetext`) move onto it instead of the thumbs.
