---
title: Option Group
related:
  - title: Radio
    path: radio.md
  - title: Checkbox
    path: checkbox.md
  - title: Toggle
    path: toggle.md
  - title: Button Toggle
    path: button-toggle.md
---
The QOptionGroup component is a helper component that allows you better control for grouping binary (on or off, true or false, 1 or 0) form input components like checkboxes, radios or toggles. A good use for this component is for offering a set of options or settings to turn on and off.

## QOptionGroup API

Not inlined here: call the `get_api` tool with `name: "QOptionGroup"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

### Standard

```vue
<template>
  <q-option-group v-model="group" :options="options" color="primary" />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### With QCheckbox or QToggle

Example "With checkboxes":

```vue
<template>
  <q-option-group
    v-model="group"
    :options="options"
    color="green"
    type="checkbox"
  />
</template>

<script setup>
import { ref } from 'vue'

const group = ref(['op1'])
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

> [!IMPORTANT]
> The model for checkboxes/toggles must be an array.

Example "With toggles":

```vue
<template>
  <q-option-group
    v-model="group"
    :options="options"
    color="yellow"
    type="toggle"
  />
</template>

<script setup>
import { ref } from 'vue'

const group = ref(['op1'])
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### Using label slots

There are two types of slots. A generic one (`label`) which applies to all options, unless a more specific index-based one is used (`label-N` where N is the 0-based index of the option). Both types of slots receive the respective option as parameter.

Notice how we use the specific label slot for first option (option at index 0) and we also add a QTooltip.

Example "Label slots":

```vue
<template>
  <q-option-group v-model="group" type="checkbox" :options="options">
    <template #label="opt">
      <div class="row items-center">
        <span class="text-teal">{{ opt.label }}</span>
        <q-icon :name="opt.icon" color="teal" size="1.5em" class="q-ml-sm" />
      </div>
    </template>

    <template #label-0="opt">
      <!-- custom label for option at index 0 -->
      <span class="text-weight-bold">{{ opt.label }}</span>
      <span> (has QTooltip)</span>
      <q-tooltip class="bg-primary" :offset="[0, 0]">Tooltip</q-tooltip>
    </template>
  </q-option-group>
</template>

<script setup>
import { ref } from 'vue'

const group = ref(['op1'])
const options = [
  {
    value: 'op1',
    label: 'Good food',
    icon: 'restaurant_menu'
  },
  {
    value: 'op2',
    label: 'Good service',
    icon: 'room_service',
    color: 'teal'
  },
  {
    value: 'op3',
    label: 'Pleasant surroundings',
    icon: 'photo',
    color: 'teal'
  }
]
</script>
```

### With labels on left side

Example "With option labels on the left side":

```vue
<template>
  <q-option-group v-model="group" :options="options" color="red" left-label />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### Inline

```vue
<template>
  <q-option-group v-model="group" :options="options" color="primary" inline />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### Dense

Example "Dense and inline":

```vue
<template>
  <q-option-group
    v-model="group"
    :options="options"
    color="primary"
    inline
    dense
  />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### Disable

Example "Disabled":

```vue
<template>
  <q-option-group v-model="group" :options="options" color="green" disable />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

> [!NOTE]
> The objects within the `options` array can hold any of the props found in QToggle, QCheckbox or QRadio for instance `disable` or `leftLabel`. See below for an example.

### Disable Certain Options

```vue
<template>
  <q-option-group v-model="group" :options="options" color="green" />
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2',
    disable: false
  },
  {
    label: 'Option 3',
    value: 'op3',
    disable: true
  },
  {
    label: 'Option 4',
    value: 'op4',
    disable: true
  }
]
</script>
```

### Custom Label, Value and Disable props *(v2.17+)*

By default, QOptionGroup looks at `label`, `value`, `disable` props of each option from the options array Objects. But you can override those:

Example "Custom Label, Value and Disable props":

```vue
<template>
  <!-- as string -->
  <q-option-group
    v-model="modelOne"
    :options="options"
    color="primary"
    option-value="key"
    option-label="title"
    option-disable="inactive"
  />

  <!-- or as function -->
  <q-option-group
    class="q-mt-lg"
    v-model="modelTwo"
    :options="options"
    color="primary"
    :option-value="optValueFn"
    :option-label="optLabelFn"
    :option-disable="optDisableFn"
  />
</template>

<script setup>
import { ref } from 'vue'

const modelOne = ref('op1')
const modelTwo = ref('op1')

const options = [
  {
    title: 'Option 1',
    key: 'op1',
    inactive: false
  },
  {
    title: 'Option 2 (disabled)',
    key: 'op2',
    inactive: true
  },
  {
    title: 'Option 3',
    key: 'op3',
    inactive: false
  }
]

const optValueFn = item => item.key
const optLabelFn = item => item.title
const optDisableFn = item => item.inactive
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-option-group v-model="group" :options="options" dark color="yellow" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const group = ref('op1')
const options = [
  {
    label: 'Option 1',
    value: 'op1'
  },
  {
    label: 'Option 2',
    value: 'op2'
  },
  {
    label: 'Option 3',
    value: 'op3'
  }
]
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QOptionGroup, otherwise formData will not contain it (if it should) - all value are converted to string (native behaviour, so do not use Object values):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <div
      class="q-pa-sm rounded-borders"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
    >
      Preferred genre:
      <q-option-group
        name="preferred_genre"
        v-model="preferred"
        :options="options"
        color="primary"
        inline
      />
    </div>

    <div
      class="q-pa-sm rounded-borders"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'"
    >
      Accepted genres:
      <q-option-group
        name="accepted_genres"
        v-model="accepted"
        :options="options"
        type="checkbox"
        color="primary"
        inline
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

const preferred = ref('rock')
const accepted = ref([])
const submitResult = ref([])

const options = [
  {
    label: 'Rock',
    value: 'rock'
  },
  {
    label: 'Funk',
    value: 'funk'
  },
  {
    label: 'Pop',
    value: 'pop'
  }
]

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

QOptionGroup wraps its options in a container with `role="radiogroup"` when `type` is `radio`, or a generic `role="group"` for checkboxes and toggles, and sets `aria-disabled="true"` on it when the whole group is disabled. The per-option semantics (`radio`/`checkbox`/`switch` roles, `aria-checked`, labels) come from the underlying QRadio/QCheckbox/QToggle components. The group itself has no accessible name by default — pass an `aria-label` (or `aria-labelledby`) attribute to QOptionGroup so screen readers can announce what the group is about.

For radio groups, the keyboard interaction below implements a roving tabindex; the horizontal arrow keys are RTL-aware.

### Keyboard navigation

When `type` is `radio` (the default), QOptionGroup follows the [WAI-ARIA radio group pattern](https://www.w3.org/WAI/ARIA/apg/patterns/radio/): the whole group is a single Tab stop (pressing <kbd>Tab</kbd> moves focus into the group — onto the selected radio — and out of it), while the arrow keys move focus within the group, selecting the newly focused radio, wrapping around and skipping disabled options.

Standalone QRadio components (used without QOptionGroup) have no knowledge of their siblings, so they cannot offer arrow key navigation — each is its own Tab stop.

For checkboxes and toggles, each control remains an individual Tab stop, as each one is toggled independently.
