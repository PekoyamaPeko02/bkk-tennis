---
title: Color Picker
related:
  - title: Color Utils
    path: ../quasar-utils/color-utils.md
---
The QColor component provides a method to input colors.

> [!NOTE]
> For handling colors, also check out [Quasar Color Utils](../quasar-utils/color-utils.md).

## QColor API

Not inlined here: call the `get_api` tool with `name: "QColor"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-color v-model="hex" class="my-picker" />
    <q-color v-model="hexa" class="my-picker" />
    <q-color v-model="rgb" class="my-picker" />
    <q-color v-model="rgba" class="my-picker" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#FF00FF')
const hexa = ref('#FF00FFCC')
const rgb = ref('rgb(0,0,0)')
const rgba = ref('rgba(255,0,255,0.8)')
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 250px
</style>
```

### With QInput

Example "Input":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <q-input filled v-model="color" class="my-input">
      <template #append>
        <q-icon name="colorize" class="cursor-pointer">
          <q-popup-proxy
            cover
            transition-show="scale"
            transition-hide="scale"
          >
            <q-color v-model="color" />
          </q-popup-proxy>
        </q-icon>
      </template>
    </q-input>

    <q-input
      filled
      v-model="secondColor"
      :rules="['anyColor']"
      hint="With validation"
      class="my-input"
    >
      <template #append>
        <q-icon name="colorize" class="cursor-pointer">
          <q-popup-proxy
            cover
            transition-show="scale"
            transition-hide="scale"
          >
            <q-color v-model="secondColor" />
          </q-popup-proxy>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const color = ref('#FF00FF')
const secondColor = ref('#027be3')
</script>

<style lang="sass" scoped>
.my-input
  max-width: 250px
</style>
```

There are **helpers** for QInput `rules` prop: [full list](https://github.com/quasarframework/quasar/blob/dev/ui/src/utils/patterns/patterns.js). You can use these for convenience or write the string specifying your [custom needs](input.md#internal-validation).

Examples: "hexColor", "rgbOrRgbaColor", "anyColor".

More info: [QInput](input.md).

### No header or footer

You can choose if you don't want to render the header and/or footer, like in example below:

Example "No header/footer":

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-color v-model="hex" no-header class="my-picker" />
    <q-color v-model="hex" no-header-tabs class="my-picker" />
    <q-color v-model="hex" no-footer class="my-picker" />
    <q-color v-model="hex" no-header no-footer class="my-picker" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#FF00FF')
</script>

<style lang="sass" scoped>
.my-picker
  width: 250px
</style>
```

### Custom default view

You can also pick the default view, like in example below, where we also specify we don't want to render the header and footer. The end result generates a nice color palette that the user can pick from:

```vue
<template>
  <q-badge color="grey-3" text-color="black" class="q-mb-sm">
    {{ hex }}
  </q-badge>

  <q-color
    v-model="hex"
    no-header
    no-footer
    default-view="palette"
    class="my-picker"
  />
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#FF00FF')
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 250px
</style>
```

### Custom palette

```vue
<template>
  <q-badge color="grey-3" text-color="black" class="q-mb-sm">
    {{ hex }}
  </q-badge>

  <q-color
    v-model="hex"
    default-view="palette"
    :palette="[
      '#019A9D',
      '#D9B801',
      '#E8045A',
      '#B2028A',
      '#2A0449',
      '#019A9D'
    ]"
    class="my-picker"
  />
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#FF00FF')
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 250px
</style>
```

### Palette slot *(v2.31+)*

The `palette` slot replaces the default swatches of the palette view while keeping the Spectrum and Tune views and the view switcher. Its scope carries the colors (`palette`), a `select(color)` function and an `editable` flag, so you decide the layout and the labels. The default swatches are keyboard and screen reader accessible (see the Accessibility section below); keep yours that way too.

Example "Palette slot":

```vue
<template>
  <q-badge color="grey-3" text-color="black" class="q-mb-sm">
    {{ hex }}
  </q-badge>

  <q-color
    v-model="hex"
    default-view="palette"
    :palette="palleteOptions"
    class="my-picker"
  >
    <template #palette="{ palette, select, editable }">
      <div
        class="row q-gutter-sm q-pa-md"
        role="group"
        aria-label="Brand colors"
      >
        <q-btn
          v-for="color in palette"
          :key="color"
          no-caps
          :outline="hex !== color"
          :unelevated="hex === color"
          :disable="!editable"
          :aria-pressed="hex === color"
          @click="select(color)"
        >
          <span class="swatch q-mr-sm" :style="{ backgroundColor: color }" />
          {{ swatches[color] }}
        </q-btn>
      </div>
    </template>
  </q-color>
</template>

<script setup>
import { ref } from 'vue'

const swatches = {
  '#019a9d': 'Teal',
  '#d9b801': 'Mustard',
  '#e8045a': 'Raspberry',
  '#b2028a': 'Magenta',
  '#2a0449': 'Indigo',
  '#1f3a93': 'Navy',
  '#2e7d32': 'Forest'
}

const hex = ref('#019a9d')
const palleteOptions = Object.keys(swatches)
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 350px

.swatch
  display: inline-block
  width: 1em
  height: 1em
  border-radius: 2px
  border: 1px solid rgba(0, 0, 0, .2)
</style>
```

### Force dark mode

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-color v-model="hex" dark class="my-picker" />
    <q-color v-model="hexa" dark class="my-picker" />
    <q-color v-model="rgb" dark class="my-picker" />
    <q-color v-model="rgba" dark class="my-picker" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#FF00FF')
const hexa = ref('#FF00FFCC')
const rgb = ref('rgb(0,0,0)')
const rgba = ref('rgba(255,0,255,0.8)')
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 250px
</style>
```

### Default value

```vue
<template>
  <q-color
    v-model="nullModel"
    default-value="#285de0"
    style="max-width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const nullModel = ref(null)
</script>
```

### Lazy update

Example "Lazy model":

```vue
<template>
  <q-badge color="grey-3" text-color="black" class="q-mb-sm">
    {{ hex }}
  </q-badge>

  <q-color
    :model-value="hex"
    @change="
      val => {
        hex = val
      }
    "
    style="max-width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const hex = ref('#112e1b')
</script>
```

### Disable and readonly

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-color v-model="color" disable class="my-picker" />

    <q-color v-model="color" readonly class="my-picker" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const color = ref('#ff00ff')
</script>

<style lang="sass" scoped>
.my-picker
  max-width: 250px
</style>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QColor, otherwise formData will not contain it (if it should):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-color
      name="accent_color"
      v-model="color"
      style="width: 200px; max-width: 100%"
    />

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

const color = ref('#f66363')
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

All three views are keyboard and screen reader accessible: the Tune view through its native text/number inputs plus sliders, and, since v2.31, the Spectrum view through its spectrum panel and the Palette view through its swatches.

The spectrum panel is a slider (`role="slider"`) named by the localized `colorPicker.spectrum` label; since it drives two axes at once, its accessible value spells both out (`colorPicker.saturation` and `colorPicker.brightness`, e.g. "Saturation 40%, Brightness 70%"). It is a <kbd>Tab</kbd> stop: <kbd>Left</kbd>/<kbd>Right</kbd> change the saturation and <kbd>Up</kbd>/<kbd>Down</kbd> the brightness by one percent (ten with <kbd>Shift</kbd>), <kbd>Home</kbd>/<kbd>End</kbd> jump to zero/full saturation and <kbd>PageUp</kbd>/<kbd>PageDown</kbd> change the brightness by ten percent. Like the sliders next to it, the panel emits `change` once the key is released.

The palette swatches are buttons named by their color value, wrapped in a group carrying the localized `colorPicker.palette` label, and the swatch matching the current model is exposed as pressed. The palette holds a single <kbd>Tab</kbd> stop (the selected swatch, else the first one); the arrow keys move between swatches (<kbd>Up</kbd>/<kbd>Down</kbd> by one visual row), <kbd>Home</kbd>/<kbd>End</kbd> jump to the first/last swatch and <kbd>Enter</kbd>/<kbd>Space</kbd> pick the focused one. Swatches rendered through the `palette` slot are yours to make accessible.

The parts that are exposed carry localized accessible names from the [Quasar Language Pack](../options/quasar-language-packs.md) (`colorPicker.*`): the view tabs, the header's color value field and the hue/opacity sliders, none of which the consumer can name from the outside.

A `disable`d QColor exposes `aria-disabled="true"` on its root element. A `readonly` or `disable`d picker also drops the spectrum panel and the swatches out of the Tab order, the way its sliders do, while the spectrum panel keeps reporting the current color with `aria-readonly`/`aria-disabled`.
