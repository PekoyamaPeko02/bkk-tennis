---
title: Checkbox
related:
  - title: Toggle
    path: toggle.md
  - title: Option Group
    path: option-group.md
  - title: Radio
    path: radio.md
  - title: Button Toggle
    path: button-toggle.md
---
The QCheckbox component is another basic element for user input. You can use this to supply a way for the user to toggle an option.

> [!NOTE]
> Please also refer to the [QOptionGroup](option-group.md) on other possibilities for creating groups of Checkboxes.

## QCheckbox API

Not inlined here: call the `get_api` tool with `name: "QCheckbox"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Standard

```vue
<template>
  <q-checkbox v-model="val" />
</template>

<script setup>
import { ref } from 'vue'

const val = ref(true)
</script>
```

### With custom icons

Example "With icons":

```vue
<template>
  <q-checkbox
    v-model="val"
    checked-icon="star"
    unchecked-icon="star_border"
    indeterminate-icon="help"
  />
</template>

<script setup>
import { ref } from 'vue'

const val = ref(true)
</script>
```

### Label

```vue
<template>
  <div class="q-gutter-sm">
    <div>
      <q-checkbox v-model="right" label="Label on Right" />
    </div>

    <div>
      <q-checkbox left-label v-model="left" label="Label on Left" />
    </div>

    <div>
      <q-checkbox
        v-model="right2"
        label="Swipe"
        checked-icon="swipe_left"
        unchecked-icon="swipe_right"
        color="green"
        keep-color
      />
    </div>

    <div>
      <q-checkbox
        left-label
        v-model="left2"
        label="I agree"
        checked-icon="task_alt"
        unchecked-icon="highlight_off"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const left = ref(true)
const right = ref(false)
const left2 = ref(true)
const right2 = ref(false)
</script>
```

### Coloring

In the second row in the example below, the property `keep-color` is being used to retain the passed in color when the checkbox is not in a toggled state.

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox v-model="teal" label="Teal" color="teal" />
    <q-checkbox v-model="orange" label="Orange" color="orange" />
    <q-checkbox v-model="red" label="Red" color="red" />
    <q-checkbox v-model="cyan" label="Cyan" color="cyan" />
  </div>
  <div class="q-gutter-sm">
    <q-checkbox keep-color v-model="teal" label="Teal" color="teal" />
    <q-checkbox keep-color v-model="orange" label="Orange" color="orange" />
    <q-checkbox keep-color v-model="red" label="Red" color="red" />
    <q-checkbox keep-color v-model="cyan" label="Cyan" color="cyan" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const teal = ref(true)
const orange = ref(false)
const red = ref(false)
const cyan = ref(true)
</script>
```

### Dense

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox dense v-model="teal" label="Teal" color="teal" />
    <q-checkbox dense v-model="orange" label="Orange" color="orange" />
    <q-checkbox dense v-model="red" label="Red" color="red" />
    <q-checkbox dense v-model="cyan" label="Cyan" color="cyan" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const teal = ref(true)
const orange = ref(false)
const red = ref(true)
const cyan = ref(false)
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <div class="q-gutter-sm">
      <q-checkbox dark v-model="teal" label="Teal" color="teal" />
      <q-checkbox dark v-model="orange" label="Orange" color="orange" />
      <q-checkbox dark v-model="red" label="Red" color="red" />
      <q-checkbox dark v-model="cyan" label="Cyan" color="cyan" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const teal = ref(true)
const orange = ref(false)
const red = ref(true)
const cyan = ref(false)
</script>
```

### Sizes

Apart from the standard sizes below, you can define your own through the `size` property (last one is a custom size).

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox size="xs" v-model="shape" val="xs" label="Size 'xs'" />
    <q-checkbox size="sm" v-model="shape" val="sm" label="Size 'sm'" />
    <q-checkbox size="md" v-model="shape" val="md" label="Size 'md'" />
    <q-checkbox size="lg" v-model="shape" val="lg" label="Size 'lg'" />
    <q-checkbox size="xl" v-model="shape" val="xl" label="Size 'xl'" />

    <!-- custom size -->
    <q-checkbox
      size="150px"
      v-model="shape"
      val="150px"
      label="Size '150px'"
    />
  </div>

  <div class="q-px-sm">
    Your selection is: <strong>{{ shape }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const shape = ref(['line'])
</script>
```

### Indeterminate state

In the example below, as soon as you click on the first checkbox it starts toggling between true/false. The second checkbox, on the other hand toggles between the three states (indeterminate/true/false) with help from `toggle-indeterminate`. You can optionally set the property `indeterminate-value`, otherwise the indeterminate value will be considered `null`.

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox
      indeterminate-value="maybe"
      v-model="theModel2"
      label="Did you eat lunch today?"
    />
  </div>

  <div class="q-px-sm">
    The model data: <strong>{{ JSON.stringify(theModel2) }}</strong>
  </div>

  <div class="q-gutter-sm">
    <q-checkbox
      toggle-indeterminate
      v-model="theModel"
      label="Did you eat lunch today?"
    />
  </div>

  <div class="q-px-sm row no-wrap items-center">
    <div class="col">
      The model data: <strong>{{ JSON.stringify(theModel) }}</strong>
    </div>
    <q-btn
      color="primary"
      label="Reset"
      @click="onResetClick"
      class="q-ml-md"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const theModel = ref(null)
const theModel2 = ref('maybe')

function onResetClick() {
  theModel.value = null
  theModel2.value = 'maybe'
}
</script>
```

### Toggle order

By default, QCheckbox follows this chain when toggling: indeterminate -> checked -> unchecked. However, you can change this behavior through the `toggle-order` prop. This property determines the order of the states and can be `tf` (default) or `ft` (`t` stands for state of true/checked while `f` for state of false/unchecked).

Toggling order is:

- if `toggle-indeterminate` is true, then: indet -> first state -> second state -> indet (and repeat)
- otherwise (no toggle-indeterminate): indet -> first state -> second state -> first state -> second state -> ...

```vue
<template>
  <q-btn
    class="q-mb-md"
    color="primary"
    label="Reset models"
    @click="resetModels"
  />

  <div class="q-gutter-sm">
    <q-checkbox v-model="teal" label="'tf' order" color="teal" />
    <q-checkbox
      toggle-order="ft"
      v-model="orange"
      label="'ft' order"
      color="orange"
    />
    <q-checkbox
      toggle-indeterminate
      v-model="red"
      label="'tf' order + toggle-indeterminate"
      color="red"
    />
    <q-checkbox
      toggle-indeterminate
      toggle-order="ft"
      v-model="cyan"
      label="'ft' order + toggle-indeterminate"
      color="cyan"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const teal = ref(null)
const orange = ref(null)
const red = ref(true)
const cyan = ref(false)

function resetModels() {
  teal.value = null
  orange.value = null
  red.value = null
  cyan.value = null
}
</script>
```

### Array model

Example "Array as model":

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox v-model="selection" val="teal" label="Teal" color="teal" />
    <q-checkbox
      v-model="selection"
      val="orange"
      label="Orange"
      color="orange"
    />
    <q-checkbox v-model="selection" val="red" label="Red" color="red" />
    <q-checkbox v-model="selection" val="cyan" label="Cyan" color="cyan" />
  </div>

  <div class="q-px-sm">
    The model data: <strong>{{ selection }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const selection = ref(['teal', 'red'])
</script>
```

### Custom model values

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox
      v-model="customModel"
      color="secondary"
      label="Do you agree with the terms & conditions?"
      true-value="yes"
      false-value="no"
    />
  </div>

  <div class="q-px-sm">
    The model data: <strong>'{{ customModel }}'</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const customModel = ref('no')
</script>
```

### With QOptionGroup

> [!TIP]
> You can also use [QOptionGroup](option-group.md), which simplifies the usage when you have groups of checkboxes, like in example below.

Example "Usage with QOptionGroup":

```vue
<template>
  <q-option-group :options="options" type="checkbox" v-model="group" />
</template>

<script setup>
import { ref } from 'vue'

const group = ref([])
const options = [
  { label: 'Battery too low', value: 'bat' },
  { label: 'Friend request', value: 'friend', color: 'green' },
  { label: 'Picture uploaded', value: 'upload', color: 'red' }
]
</script>
```

### With QItem

In the example below, we are rendering a `<label>` tag (notice `tag="label"`) so the QCheckbox will respond to clicks on QItems to change toggle state.

```vue
<template>
  <div class="q-gutter-sm">
    <q-list>
      <!--
      Rendering a <label> tag (notice tag="label")
      so QCheckboxes will respond to clicks on QItems to
      change Toggle state.
    -->

      <q-item tag="label" v-ripple>
        <q-item-section avatar>
          <q-checkbox v-model="color" val="teal" color="teal" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Teal</q-item-label>
        </q-item-section>
      </q-item>

      <q-item tag="label" v-ripple>
        <q-item-section avatar>
          <q-checkbox v-model="color" val="orange" color="orange" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Orange</q-item-label>
          <q-item-label caption>With description</q-item-label>
        </q-item-section>
      </q-item>

      <q-item tag="label" v-ripple>
        <q-item-section avatar top>
          <q-checkbox v-model="color" val="cyan" color="cyan" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Cyan</q-item-label>
          <q-item-label caption>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
            eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut
            enim ad minim veniam, quis nostrud exercitation ullamco laboris
            nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in
            reprehenderit in voluptate velit esse cillum dolore eu fugiat
            nulla pariatur. Excepteur sint occaecat cupidatat non proident,
            sunt in culpa qui officia deserunt mollit anim id est laborum.
          </q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </div>

  <div class="q-px-sm q-mt-sm">
    Your selection is: <strong>{{ color }}</strong>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const color = ref(['cyan'])
</script>
```

### Disable

```vue
<template>
  <div class="q-gutter-sm">
    <q-checkbox disable v-model="teal" label="One" />
    <q-checkbox disable v-model="orange" label="Two" />
    <q-checkbox disable v-model="red" label="Three" />
    <q-checkbox disable v-model="cyan" label="Four" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const teal = ref(true)
const orange = ref(false)
const red = ref(false)
const cyan = ref(true)
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QCheckbox, otherwise formData will not contain it (if it should) - all value are converted to string (native behaviour, so do not use Object values):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <div
      class="q-pa-sm rounded-borders"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
    >
      <q-checkbox
        name="accept_agreement"
        v-model="acceptAgreement"
        label="Accept agreement"
      />

      <q-checkbox
        name="subscribe_newsletter"
        v-model="subscribeNewsletter"
        label="Subscribe to newsletter"
        true-value="YES"
      />
    </div>

    <div
      class="q-pa-sm rounded-borders"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
    >
      <q-checkbox
        name="music_genre"
        v-model="genreRock"
        true-value="rock"
        label="Rock"
      />

      <q-checkbox
        name="music_genre"
        v-model="genreFunk"
        true-value="funk"
        label="Funk"
      />

      <q-checkbox
        name="music_genre"
        v-model="genrePop"
        true-value="pop"
        label="Pop"
      />
    </div>

    <div>
      <q-btn label="Submit" type="submit" color="primary" />
    </div>
  </q-form>

  <q-card
    v-if="submitted"
    flat
    bordered
    class="q-mt-md"
    :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
  >
    <template v-if="submitEmpty">
      <q-card-section>
        Submitted form contains empty formData.
      </q-card-section>
    </template>
    <template v-else>
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
    </template>
  </q-card>
</template>

<script setup>
import { ref } from 'vue'

const submitted = ref(false)
const submitEmpty = ref(false)
const submitResult = ref([])

const acceptAgreement = ref(false)
const subscribeNewsletter = ref(null)

const genreRock = ref('rock')
const genreFunk = ref(false)
const genrePop = ref('pop')

function onSubmit(evt) {
  const formData = new FormData(evt.target)
  const data = []

  for (const [name, value] of formData.entries()) {
    data.push({
      name,
      value
    })
  }

  submitted.value = true
  submitResult.value = data
  submitEmpty.value = data.length === 0
}
</script>
```

## Accessibility *(v2.25+)*

QCheckbox implements the [WAI-ARIA checkbox pattern](https://www.w3.org/WAI/ARIA/apg/patterns/checkbox/): it exposes `role="checkbox"` with a three-state `aria-checked` (`true`, `false`, or `mixed` for the indeterminate state). The `label` prop doubles as the accessible name (`aria-label`) — with a slot-only label, the name is derived from the rendered content — and a disabled checkbox exposes `aria-disabled="true"`. The native `<input type="checkbox">` you may notice in the DOM exists only for native form submission and `<label>` click forwarding; it is hidden from assistive technology.

The checkbox is a Tab stop (see the `tabindex` prop) and toggles on <kbd>Enter</kbd> or <kbd>Space</kbd>. Keyboard focus draws a visible focus ring, while mouse clicks deliberately do not, so pointer users are not distracted by it.
