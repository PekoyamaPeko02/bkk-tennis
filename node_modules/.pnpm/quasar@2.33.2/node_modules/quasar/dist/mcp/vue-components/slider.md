---
title: Slider
related:
  - title: Range
    path: range.md
  - title: Field
    path: field.md
---
The QSlider is a great way for the user to specify a number value between a minimum and maximum value, with optional steps between valid values. The slider also has a focus indicator (highlighted slider button), which allows for keyboard adjustments of the slider.

Also check its “sibling”, the [QRange](range.md) component.

## QSlider API

Not inlined here: call the `get_api` tool with `name: "QSlider"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> You are responsible for accommodating the space around QSlider so that the label and marker labels won't overlap the other content on your page. You can use CSS margin or padding for this purpose.

### Standard

```vue
<template>
  <q-badge color="secondary"> Model: {{ standard }} (0 to 50) </q-badge>

  <q-slider v-model="standard" :min="0" :max="50" />
  <q-slider v-model="standard" :min="0" :max="50" color="green" />
</template>

<script setup>
import { ref } from 'vue'

const standard = ref(2)
</script>
```

### Vertical

Example "Vertical orientation":

```vue
<template>
  <q-badge color="secondary"> Model: {{ standard }} (0 to 50) </q-badge>

  <div class="row justify-around">
    <q-slider
      v-model="standard"
      :min="0"
      :max="50"
      vertical
      label
      switch-label-side
    />

    <q-slider
      v-model="standard"
      :min="0"
      :max="50"
      color="green"
      vertical
      reverse
      label-always
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const standard = ref(10)
</script>
```

### With inner min/max

Sometimes you need to restrict the model value to an interval inside of the track's length. For this purpose, use `inner-min` and `inner-max` props. First prop needs to be higher or equal to `min` prop while the latter needs to be lower or equal to the `max` prop.

Example "Inner min/max":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ value }} (0 to 50 w/ selection 10 to 35 or 15 to 40)
  </q-badge>

  <q-slider
    v-model="value"
    :min="0"
    :max="50"
    :inner-min="10"
    :inner-max="35"
  />
  <q-slider
    v-model="value"
    :min="0"
    :max="50"
    color="green"
    :inner-min="15"
    :inner-max="40"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(25)
</script>
```

### With step

```vue
<template>
  <q-badge color="secondary">
    Model: {{ basicModel }} (0 to 100, step 50)
  </q-badge>
  <q-slider v-model="basicModel" :step="50" />
  <q-badge color="secondary">
    Model: {{ redModel }} (0 to 100, step 25)
  </q-badge>
  <q-slider v-model="redModel" color="red" :step="25" />
  <q-badge color="secondary">
    Model: {{ greenModel }} (0 to 5, step 1)
  </q-badge>
  <q-slider v-model="greenModel" color="green" :min="0" :step="1" :max="5" />
</template>

<script setup>
import { ref } from 'vue'

const basicModel = ref(50)
const redModel = ref(25)
const greenModel = ref(2)
</script>
```

The `step` property can also be floating point number (or numeric `0` if you need infinite precision).

Example "Floating point":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ smallStep }} (0.1 to 1.5, step 0.1)
  </q-badge>

  <q-slider v-model="smallStep" :min="0.1" :max="1.5" :step="0.1" />

  <q-badge color="secondary">
    Model: {{ xsmallStep }} (0.1 to 1, step 0.05)
  </q-badge>

  <q-slider v-model="xsmallStep" :min="0.1" :max="1" :step="0.05" />

  <q-badge color="secondary">
    Model: {{ zeroStep }} (0.0 to 10.5, step 0)
  </q-badge>

  <q-slider v-model="zeroStep" :min="0.0" :max="10.5" :step="0" color="red" />
</template>

<script setup>
import { ref } from 'vue'

const smallStep = ref(0.3)
const xsmallStep = ref(0.53)
const zeroStep = ref(0.5)
</script>
```

Example "Snap to steps":

```vue
<template>
  <q-badge color="secondary"> Model: {{ value }} (-20 to 20) </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    snap
    label
    color="purple"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(0)
</script>
```

### With label

In the example below, move the slider to see the label.

```vue
<template>
  <q-badge color="secondary">
    Model: {{ value }} (-20 to 20, step 4)
  </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    color="light-green"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(0)
</script>
```

Example "Always display label":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ value }} (-20 to 20, step 4)
  </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    label-always
    color="light-green"
  />

  <q-badge color="secondary">
    Model: {{ value }} (-20 to 20, step 4)
  </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    label-always
    switch-label-side
    color="red"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(0)
</script>
```

Example "Custom label value":

```vue
<template>
  <q-badge color="secondary">
    Model: {{ value }} (-20 to 20, step 4)
  </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    :label-value="value + 'px'"
    label-always
    color="purple"
  />

  <q-badge color="secondary">
    Model: {{ value }} (-20 to 20, step 4)
  </q-badge>

  <q-slider
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    :label-value="value + 'px'"
    label-always
    color="red"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(0)
</script>
```

The example below is better highlighting how QSlider handles label positioning so that it always stays inside the QSlider's box horizontally.

Example "Long label":

```vue
<template>
  <q-slider
    class="q-mt-lg"
    v-model="value"
    :min="-20"
    :max="20"
    :step="4"
    label
    :label-value="'Value: ' + value + 'px'"
    label-always
    color="purple"
  />
</template>

<script setup>
import { ref } from 'vue'

const value = ref(16)
</script>
```

### Markers

```vue
<template>
  <q-badge color="secondary"> Model: {{ basicModel }} (0 to 10) </q-badge>
  <q-slider v-model="basicModel" markers :min="0" :max="10" />

  <q-badge color="secondary"> Model: {{ greenModel }} (0 to 10) </q-badge>
  <q-slider
    v-model="greenModel"
    color="green"
    markers
    snap
    :min="0"
    :max="10"
  />

  <q-badge color="secondary">
    Model: {{ orangeModel }} (0 to 16, step 2, marker step 4)
  </q-badge>
  <q-slider
    v-model="orangeModel"
    color="orange"
    snap
    :step="2"
    :min="0"
    :max="16"
    :markers="4"
  />
</template>

<script setup>
import { ref } from 'vue'

const basicModel = ref(2)
const greenModel = ref(0)
const orangeModel = ref(6)
</script>
```

### Marker labels

```vue
<template>
  <q-slider v-model="model" marker-labels :min="0" :max="6" />

  <q-slider
    class="q-mt-xl"
    v-model="model"
    color="deep-orange"
    markers
    :marker-labels="fnMarkerLabel"
    :min="0"
    :max="6"
  />

  <q-slider
    class="q-mt-xl"
    v-model="model"
    color="purple"
    markers
    :marker-labels="objMarkerLabel"
    :min="0"
    :max="6"
  />

  <q-slider
    class="q-mt-xl"
    v-model="priceModel"
    color="green"
    :inner-min="3"
    :inner-max="6"
    markers
    :marker-labels="arrayMarkerLabel"
    label-always
    :label-value="priceLabel"
    switch-label-side
    switch-marker-labels-side
    :min="2"
    :max="7"
  />
</template>

<script setup>
import { computed, ref } from 'vue'

const model = ref(2)
const fnMarkerLabel = val => `${10 * val}%`
const objMarkerLabel = { 0: '0°C', 3: { label: '3°C' }, 5: '5°C', 6: '6°C' }

const priceModel = ref(4)
const priceLabel = computed(() => `$ ${priceModel.value}`)
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
  <q-slider
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
  </q-slider>

  <q-slider
    class="q-mt-xl"
    v-model="secondModel"
    color="orange"
    markers
    :min="0"
    :max="5"
    marker-labels
  >
    <template #marker-label-group="{ markerList }">
      <div
        v-for="val in 4"
        :key="val"
        class="cursor-pointer"
        :class="markerList[val].classes"
        :style="markerList[val].style"
        @click="secondModel = val"
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
        @click="secondModel = val"
      />
    </template>
  </q-slider>

  <q-slider
    class="q-mt-xl"
    v-model="thirdModel"
    color="teal"
    :thumb-color="thirdModel === 0 ? 'grey' : 'teal'"
    snap
    :min="0"
    :max="5"
    :step="0.5"
    marker-labels
    switch-marker-labels-side
  >
    <template #marker-label-group="{ markerMap }">
      <div
        class="row items-center no-wrap"
        :class="markerMap[thirdModel].classes"
        :style="markerMap[thirdModel].style"
      >
        <q-icon
          v-if="thirdModel === 0"
          size="xs"
          color="teal"
          name="star_outline"
        />

        <template v-else>
          <q-icon
            v-for="i in Math.floor(thirdModel)"
            :key="i"
            size="xs"
            color="teal"
            name="star_rate"
          />

          <q-icon
            v-if="thirdModel > Math.floor(thirdModel)"
            size="xs"
            color="teal"
            name="star_half"
          />
        </template>
      </div>
    </template>
  </q-slider>
</template>

<script setup>
import { ref } from 'vue'

const firstModel = ref(2)
const secondModel = ref(3)
const thirdModel = ref(3.5)
</script>
```

### Other customizations

Example "Color customizations":

```vue
<template>
  <q-slider
    v-model="firstModel"
    color="orange"
    thumb-color="purple"
    label-color="black"
    label-text-color="yellow"
    markers
    marker-labels
    marker-labels-class="text-orange"
    switch-marker-labels-side
    label-always
    switch-label-side
    :min="0"
    :max="6"
  />

  <q-slider
    class="q-mt-xl"
    v-model="secondModel"
    color="green"
    track-color="orange"
    inner-track-color="transparent"
    selection-color="red"
    :max="10"
    markers
  />

  <q-slider
    v-model="secondModel"
    color="purple"
    inner-track-color="light-blue-3"
    :max="10"
    :inner-min="2"
    :inner-max="8"
    markers
  />

  <q-slider
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

const firstModel = ref(2)
const secondModel = ref(4)
</script>
```

Example "Hide selection bar":

```vue
<template>
  <q-slider
    v-model="model"
    :min="0"
    :max="10"
    markers
    selection-color="transparent"
  />

  <q-slider
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

const model = ref(2)
</script>
```

Example "Custom track images":

```vue
<template>
  <q-slider
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    track-color="grey-2"
    inner-track-color="transparent"
    selection-color="transparent"
    :track-img="img"
  />

  <q-slider
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    track-color="white"
    inner-track-color="transparent"
    :track-img="img"
  />

  <q-slider
    v-model="model"
    color="deep-orange"
    :max="10"
    track-size="8px"
    inner-track-color="transparent"
    :selection-img="img"
  />

  <q-slider
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

const model = ref(6)
</script>
```

Example "Track & thumb size":

```vue
<template>
  <q-slider
    v-model="model"
    :min="0"
    :max="10"
    color="green"
    track-size="10px"
    thumb-color="black"
    markers
  />

  <q-slider
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

const model = ref(2)
</script>
```

### Lazy input

```vue
<template>
  <q-badge color="secondary"> Model: {{ lazy }} </q-badge>

  <q-slider
    :model-value="lazy"
    @change="
      val => {
        lazy = val
      }
    "
    :min="0"
    :max="45"
    :step="5"
    color="purple"
    label
  />
</template>

<script setup>
import { ref } from 'vue'

const lazy = ref(6)
</script>
```

### Null value

```vue
<template>
  <q-badge color="primary" class="q-mb-lg">
    Model: {{ getNullLabel(basicModel) }} (step 1)
  </q-badge>

  <q-slider v-model="basicModel" />

  <q-badge color="secondary" class="q-mb-lg">
    Model: {{ getNullLabel(limitModel) }} (10 to 70, step 1, inner 20 to 55)
  </q-badge>

  <q-slider
    v-model="limitModel"
    color="secondary"
    :min="10"
    :max="70"
    :inner-min="20"
    :inner-max="55"
  />
</template>

<script setup>
import { ref } from 'vue'

const basicModel = ref(null)
const limitModel = ref(null)

function getNullLabel(val) {
  return val === null ? 'null' : val
}
</script>
```

### Reverse

Example "In reverse":

```vue
<template>
  <q-badge color="secondary"> Model: {{ standard }} (0 to 50) </q-badge>

  <q-slider reverse v-model="standard" :min="0" :max="50" />
  <q-slider reverse v-model="standard" :min="0" :max="50" color="green" />
</template>

<script setup>
import { ref } from 'vue'

const standard = ref(2)
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-badge color="secondary">
      Model: {{ value }} (-20 to 20, step 4)
    </q-badge>

    <q-slider
      v-model="value"
      :min="-20"
      :max="20"
      :step="4"
      dark
      label
      label-always
      color="light-green"
    />

    <q-badge color="secondary">
      Model: {{ value }} (-20 to 20, step 4)
    </q-badge>

    <q-slider
      v-model="value"
      :min="-20"
      :max="20"
      :step="4"
      dark
      label
      label-always
      color="red"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const value = ref(0)
</script>
```

### Readonly and disable

Example "Readonly":

```vue
<template>
  <q-slider v-model="basicModel" readonly />
  <q-slider v-model="greenModel" color="green" readonly :min="0" :max="50" />
</template>

<script setup>
import { ref } from 'vue'

const basicModel = ref(20)
const greenModel = ref(30)
</script>
```

Example "Disable":

```vue
<template>
  <q-slider v-model="basicModel" disable />

  <q-slider v-model="greenModel" color="green" disable :min="0" :max="50" />
</template>

<script setup>
import { ref } from 'vue'

const basicModel = ref(40)
const greenModel = ref(10)
</script>
```

### With QItem

```vue
<template>
  <q-list dense>
    <q-item>
      <q-item-section avatar>
        <q-icon color="teal" name="volume_up" />
      </q-item-section>
      <q-item-section>
        <q-slider v-model="volume" :min="0" :max="10" label color="teal" />
      </q-item-section>
    </q-item>

    <q-item>
      <q-item-section avatar>
        <q-icon color="deep-orange" name="brightness_medium" />
      </q-item-section>
      <q-item-section>
        <q-slider
          v-model="brightness"
          :min="0"
          :max="10"
          label
          color="deep-orange"
        />
      </q-item-section>
    </q-item>

    <q-item>
      <q-item-section avatar>
        <q-icon color="primary" name="mic" />
      </q-item-section>
      <q-item-section>
        <q-slider v-model="mic" :min="0" :max="50" label />
      </q-item-section>
    </q-item>

    <q-separator inset spaced />

    <q-item>
      <q-item-section side>
        <q-icon name="volume_down" />
      </q-item-section>
      <q-item-section>
        <q-slider v-model="volume" :min="0" :max="10" label />
      </q-item-section>
      <q-item-section side>
        <q-icon name="volume_up" />
      </q-item-section>
    </q-item>
  </q-list>
</template>

<script setup>
import { ref } from 'vue'

const volume = ref(6)
const brightness = ref(3)
const mic = ref(8)
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QSlider, otherwise formData will not contain it (if it should):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <div class="q-mt-xl">
      <q-slider
        name="speed"
        v-model="speed"
        label-always
        :min="20"
        :max="140"
        :step="10"
      />
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

const speed = ref(40)
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

QSlider implements the [WAI-ARIA slider pattern](https://www.w3.org/WAI/ARIA/apg/patterns/slider/): the element that receives keyboard focus exposes `role="slider"` with `aria-valuemin`/`aria-valuemax`/`aria-valuenow` tracking the model (a `null` model reports the minimum, where its thumb sits, since the role requires a value — and `aria-valuetext` then says "No value" so it is not announced as a real selection), `aria-orientation` for the vertical mode, and `aria-disabled` / `aria-readonly` when applicable. A `label-value` (e.g. "50%") also becomes `aria-valuetext`, so screen readers announce the same formatted value sighted users see.

The slider is keyboard-operable on every platform (a tablet with a keyboard attached included): the <kbd>Arrow</kbd> keys step the value (the direction follows RTL, `reverse` and `vertical`), <kbd>PageUp</kbd> / <kbd>PageDown</kbd> jump by 10 steps and <kbd>Home</kbd> / <kbd>End</kbd> go straight to the limits.

There is no built-in accessible name, so a QSlider is announced without a label by default. Pass an `aria-label` attribute — attributes fall through onto the focusable `role="slider"` element (only `class`/`style` stay on the wrapping element) — or associate a visible label yourself so screen reader users know what the value controls.
