---
title: Radio
related:
  - title: Option Group
    path: option-group.md
  - title: Button Toggle
    path: button-toggle.md
  - title: Checkbox
    path: checkbox.md
  - title: Toggle
    path: toggle.md
---
The QRadio component is another basic element for user input. You can use this to supply a way for the user to pick an option from multiple choices.

> [!NOTE]
> Please also refer to the [QOptionGroup](option-group.md) on other possibilities for creating groups of Radios.

## QRadio API

Not inlined here: call the `get_api` tool with `name: "QRadio"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Standard

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio v-model="shape" val="line" label="Line" />
    <q-radio v-model="shape" val="rectangle" label="Rectangle" />
    <q-radio v-model="shape" val="ellipse" label="Ellipse" />
    <q-radio v-model="shape" val="polygon" label="Polygon" />
  </div>

  <div class="q-px-sm">
    Your selection is: <strong>{{ shape }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### With custom icons

Example "With icons":

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio
      v-model="shape"
      checked-icon="task_alt"
      unchecked-icon="panorama_fish_eye"
      val="line"
      label="Line"
    />
    <q-radio
      v-model="shape"
      checked-icon="task_alt"
      unchecked-icon="panorama_fish_eye"
      val="rectangle"
      label="Rectangle"
    />
    <q-radio
      v-model="shape"
      checked-icon="task_alt"
      unchecked-icon="panorama_fish_eye"
      val="ellipse"
      label="Ellipse"
    />
    <q-radio
      v-model="shape"
      checked-icon="task_alt"
      unchecked-icon="panorama_fish_eye"
      val="polygon"
      label="Polygon"
    />
  </div>

  <div class="q-px-sm">
    Your selection is: <strong>{{ shape }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### Dense

```vue
<template>
  <div class="q-gutter-sm">
    <div class="q-gutter-sm">
      <q-radio dense v-model="shape" val="line" label="Line" />
      <q-radio dense v-model="shape" val="rectangle" label="Rectangle" />
      <q-radio dense v-model="shape" val="ellipse" label="Ellipse" />
      <q-radio dense v-model="shape" val="polygon" label="Polygon" />
    </div>

    <div class="q-px-sm q-pt-sm">
      Your selection is: <strong>{{ shape }}</strong>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### Coloring

In the second row in the example below, the property `keep-color` is being used to retain the passed in color when the radio button is not in a toggled state.

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio v-model="color" val="teal" label="Teal" color="teal" />
    <q-radio v-model="color" val="orange" label="Orange" color="orange" />
    <q-radio v-model="color" val="red" label="Red" color="red" />
    <q-radio v-model="color" val="cyan" label="Cyan" color="cyan" />
  </div>
  <div class="q-gutter-sm">
    <q-radio
      keep-color
      v-model="color"
      val="teal"
      label="Teal"
      color="teal"
    />
    <q-radio
      keep-color
      v-model="color"
      val="orange"
      label="Orange"
      color="orange"
    />
    <q-radio keep-color v-model="color" val="red" label="Red" color="red" />
    <q-radio
      keep-color
      v-model="color"
      val="cyan"
      label="Cyan"
      color="cyan"
    />
  </div>
  <div class="q-px-sm q-mt-sm">
    Your selection is: <strong>{{ color }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const color = ref('cyan')
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <div class="q-gutter-sm">
      <q-radio dark v-model="shape" val="line" label="Line" />
      <q-radio dark v-model="shape" val="rectangle" label="Rectangle" />
      <q-radio dark v-model="shape" val="ellipse" label="Ellipse" />
      <q-radio dark v-model="shape" val="polygon" label="Polygon" />
    </div>
    <div class="q-px-sm">
      Your selection is: <strong>{{ shape }}</strong>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### Disable

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio disable v-model="shape" val="line" label="Line" />
    <q-radio disable v-model="shape" val="rectangle" label="Rectangle" />
    <q-radio disable v-model="shape" val="ellipse" label="Ellipse" />
    <q-radio disable v-model="shape" val="polygon" label="Polygon" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### Label on left-side

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio left-label v-model="shape" val="line" label="Line" />
    <q-radio left-label v-model="shape" val="rectangle" label="Rectangle" />
    <q-radio left-label v-model="shape" val="ellipse" label="Ellipse" />
    <q-radio left-label v-model="shape" val="polygon" label="Polygon" />
  </div>
  <div class="q-gutter-sm">
    <q-radio left-label v-model="shape" dense val="line" label="Line" />
    <q-radio
      left-label
      v-model="shape"
      dense
      val="rectangle"
      label="Rectangle"
    />
    <q-radio left-label v-model="shape" dense val="ellipse" label="Ellipse" />
    <q-radio left-label v-model="shape" dense val="polygon" label="Polygon" />
  </div>
  <div class="q-mt-md">
    Your selection is: <strong>{{ shape }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### Sizes

Apart from the standard sizes below, you can define your own through the `size` property (last one is a custom size).

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-radio size="xs" v-model="shape" val="xs" label="Size 'xs'" />
    <q-radio size="sm" v-model="shape" val="sm" label="Size 'sm'" />
    <q-radio size="md" v-model="shape" val="md" label="Size 'md'" />
    <q-radio size="lg" v-model="shape" val="lg" label="Size 'lg'" />
    <q-radio size="xl" v-model="shape" val="xl" label="Size 'xl'" />

    <!-- custom size -->
    <q-radio size="150px" v-model="shape" val="150px" label="Size '150px'" />
  </div>

  <div class="q-px-sm">
    Your selection is: <strong>{{ shape }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref('line')
</script>
```

### With QOptionGroup

> [!TIP]
> You can also use [QOptionGroup](option-group.md), which simplifies the usage when you have groups of radios, like in example below.

Example "Usage with QOptionGroup":

```vue
<template>
  <q-option-group :options="options" type="radio" v-model="group" />
</template>

<script setup>
import { ref } from 'vue'

const group = ref(null)
const options = [
  { label: 'Battery too low', value: 'bat' },
  { label: 'Friend request', value: 'friend', color: 'green' },
  { label: 'Picture uploaded', value: 'upload', color: 'red' }
]
</script>
```

### With QItem

In the example below, we are rendering a `<label>` tag (notice `tag="label"`) so the QRadio will respond to clicks on QItems to change toggle state.

```vue
<template>
  <q-list>
    <!--
      Rendering a <label> tag (notice tag="label")
      so QRadios will respond to clicks on QItems to
      change Toggle state.
    -->

    <q-item tag="label" v-ripple>
      <q-item-section avatar>
        <q-radio v-model="color" val="teal" color="teal" />
      </q-item-section>
      <q-item-section>
        <q-item-label>Teal</q-item-label>
      </q-item-section>
    </q-item>

    <q-item tag="label" v-ripple>
      <q-item-section avatar>
        <q-radio v-model="color" val="orange" color="orange" />
      </q-item-section>
      <q-item-section>
        <q-item-label>Orange</q-item-label>
        <q-item-label caption>With description </q-item-label>
      </q-item-section>
    </q-item>

    <q-item tag="label" v-ripple>
      <q-item-section avatar top>
        <q-radio v-model="color" val="cyan" color="cyan" />
      </q-item-section>
      <q-item-section>
        <q-item-label>Cyan</q-item-label>
        <q-item-label caption
          >Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
          eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim
          ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut
          aliquip ex ea commodo consequat. Duis aute irure dolor in
          reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
          pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
          culpa qui officia deserunt mollit anim id est laborum.</q-item-label
        >
      </q-item-section>
    </q-item>
  </q-list>

  <div class="q-px-sm q-mt-sm">
    Your selection is: <strong>{{ color }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const color = ref('cyan')
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QRadio, otherwise formData will not contain it (if it should) - all value are converted to string (native behaviour, so do not use Object values):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-radio name="shape" v-model="shape" val="line" label="Line" />
    <q-radio name="shape" v-model="shape" val="rectangle" label="Rectangle" />
    <q-radio name="shape" v-model="shape" val="ellipse" label="Ellipse" />
    <q-radio name="shape" v-model="shape" val="polygon" label="Polygon" />

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

const shape = ref('line')
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

QRadio exposes `role="radio"` and reflects its state through `aria-checked`. The `label` prop provides the accessible name (`aria-label`) and a disabled radio exposes `aria-disabled="true"`. It is reachable with <kbd>Tab</kbd> (see the `tabindex` prop) and selects on <kbd>Enter</kbd> or <kbd>Space</kbd>.

A standalone QRadio has no knowledge of its siblings: each one is its own Tab stop and there is no enclosing `radiogroup`. For the full [WAI-ARIA radio group pattern](https://www.w3.org/WAI/ARIA/apg/patterns/radio/) — a single Tab stop for the whole group, with the arrow keys moving focus and selection (roving tabindex) — wrap your radios in a [QOptionGroup](option-group.md#accessibility).
