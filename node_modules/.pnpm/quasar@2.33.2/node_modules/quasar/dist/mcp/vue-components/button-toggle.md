---
title: Button Toggle
related:
  - title: Button
    path: button.md
  - title: Tabs
    path: tabs.md
  - title: Option Group
    path: option-group.md
  - title: Radio
    path: radio.md
  - title: Checkbox
    path: checkbox.md
  - title: Toggle
    path: toggle.md
---
The QBtnToggle component is another basic element for user input, similar to QRadio but with buttons. You can use this to supply a way for the user to pick an option from multiple choices.

## QBtnToggle API

Not inlined here: call the `get_api` tool with `name: "QBtnToggle"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <q-btn-toggle
    v-model="model"
    toggle-color="primary"
    :options="[
      { label: 'One', value: 'one' },
      { label: 'Two', value: 'two' },
      { label: 'Three', value: 'three' }
    ]"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
</script>
```

### Design

> [!TIP]
> Since QBtnToggle uses QBtn, you can use design related props of QBtn to style this component.

Example "Some design examples":

```vue
<template>
  <div class="q-gutter-md">
    <div>
      <q-btn-toggle
        v-model="model"
        push
        glossy
        toggle-color="primary"
        :options="[
          { label: 'One', value: 'one' },
          { label: 'Two', value: 'two' },
          { label: 'Three', value: 'three' }
        ]"
      />
    </div>

    <div>
      <q-btn-toggle
        v-model="model"
        toggle-color="primary"
        flat
        :options="[
          { label: 'One', value: 'one' },
          { label: 'Two', value: 'two' },
          { label: 'Three', value: 'three' }
        ]"
      />
    </div>

    <div>
      <q-btn-toggle
        v-model="model"
        color="brown"
        text-color="white"
        toggle-color="orange"
        toggle-text-color="black"
        rounded
        unelevated
        glossy
        :options="[
          { label: 'One', value: 'one' },
          { label: 'Two', value: 'two' },
          { label: 'Three', value: 'three' },
          { label: 'Four', value: 'four' }
        ]"
      />
    </div>

    <div>
      <q-btn-toggle
        v-model="model"
        class="my-custom-toggle"
        no-caps
        rounded
        unelevated
        toggle-color="primary"
        color="white"
        text-color="primary"
        :options="[
          { label: 'Option 1', value: 'one' },
          { label: 'Option 2', value: 'two' }
        ]"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('one')
</script>

<style lang="sass" scoped>
.my-custom-toggle
  border: 1px solid #027be3
</style>
```

Example "Spread horizontally":

```vue
<template>
  <div class="q-gutter-y-md">
    <q-btn-toggle
      v-model="model"
      spread
      no-caps
      toggle-color="purple"
      color="white"
      text-color="black"
      :options="[
        { label: 'Option 1', value: 'one' },
        { label: 'Option 2', value: 'two' }
      ]"
    />

    <q-btn-toggle
      v-model="secondModel"
      spread
      class="my-custom-toggle"
      no-caps
      rounded
      unelevated
      toggle-color="primary"
      color="white"
      text-color="primary"
      :options="[
        { label: 'Option 1', value: 'one' },
        { label: 'Option 2', value: 'two' }
      ]"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('one')
const secondModel = ref('one')
</script>

<style lang="sass" scoped>
.my-custom-toggle
  border: 1px solid #027be3
</style>
```

### Custom content

First QBtnToggle below has tooltips on each button. Second QBtnToggle has customized the content. Notice the `slot` prop in the `options` Object definition. When you use this `slot` prop, you don't necessary need the `label` / `icon` props in `options`.

Example "Custom buttons content":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn-toggle
      v-model="model"
      push
      glossy
      toggle-color="teal"
      :options="[
        { label: 'One', value: 'one', slot: 'one' },
        { label: 'Two', value: 'two', slot: 'two' },
        { label: 'Three', value: 'three', slot: 'three' }
      ]"
    >
      <template #one>
        <q-tooltip>One!</q-tooltip>
      </template>

      <template #two>
        <q-tooltip>Two!</q-tooltip>
      </template>

      <template #three>
        <q-tooltip>Three!</q-tooltip>
      </template>
    </q-btn-toggle>

    <q-btn-toggle
      v-model="model"
      push
      rounded
      glossy
      toggle-color="purple"
      :options="[
        { value: 'one', slot: 'one' },
        { value: 'two', slot: 'two' },
        { value: 'three', slot: 'three' }
      ]"
    >
      <template #one>
        <div class="row items-center no-wrap">
          <div class="text-center"> Pick<br />boat </div>
          <q-icon right name="directions_boat" />
        </div>
      </template>

      <template #two>
        <div class="row items-center no-wrap">
          <div class="text-center"> Pick<br />car </div>
          <q-icon right name="directions_car" />
        </div>
      </template>

      <template #three>
        <div class="row items-center no-wrap">
          <div class="text-center"> Pick<br />railway </div>
          <q-icon right name="directions_railway" />
        </div>
      </template>
    </q-btn-toggle>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('three')
</script>
```

### Disable and readonly

You can either disable a QBtnToggle by providing a `disable` attribute, or disable an individual button by providing the property `disable: true` to its entry, in the options.

```vue
<template>
  <q-btn-toggle
    disable
    v-model="model"
    toggle-color="primary"
    push
    glossy
    :options="[
      { label: 'One', value: 'one' },
      { label: 'Two', value: 'two' },
      { label: 'Three', value: 'three' }
    ]"
  />

  <q-btn-toggle
    readonly
    v-model="model"
    toggle-color="primary"
    push
    glossy
    class="q-ml-md"
    :options="[
      { label: 'One', value: 'one' },
      { label: 'Two', value: 'two' },
      { label: 'Three', value: 'three' }
    ]"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref('two')
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QBtnToggle, otherwise formData will not contain it (if it should) - all value are converted to string (native behaviour, so do not use Object values):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-btn-toggle
      name="genre"
      v-model="genre"
      push
      glossy
      toggle-color="teal"
      :options="[
        { label: 'Rock', value: 'rock' },
        { label: 'Funk', value: 'funk' },
        { label: 'Pop', value: 'pop' }
      ]"
    />

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

const genre = ref(null)
const submitted = ref(false)
const submitEmpty = ref(false)
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
  submitEmpty.value = data.length === 0
  submitted.value = true
}
</script>
```

## Accessibility *(v2.25+)*

Each option renders as a button with `aria-pressed` reflecting whether it is the currently selected value, so screen readers announce the selection state. Give icon-only options an accessible name through the option's `attrs` property (e.g. `attrs: { 'aria-label': 'Justify left' }`).

Note that QBtnToggle exposes no group semantics: the wrapping element carries no role or accessible name, and there is no arrow-key navigation between options — each enabled option is its own Tab stop.
