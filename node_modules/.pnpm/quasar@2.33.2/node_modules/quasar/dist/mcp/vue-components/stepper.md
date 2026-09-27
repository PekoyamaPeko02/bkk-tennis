---
title: Stepper
related:
  - title: Quasar Components Transitions
    path: ../options/transitions.md
---
Steppers display progress through a sequence of logical and numbered steps. They may also be used for navigation.
They're usually useful when the user has to follow steps to complete a process, like in a [wizard](https://en.wikipedia.org/wiki/Wizard_(software)).

## QStepper API

Not inlined here: call the `get_api` tool with `name: "QStepper"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## QStep API

Not inlined here: call the `get_api` tool with `name: "QStep"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QStepperNavigation API

Not inlined here: call the `get_api` tool with `name: "QStepperNavigation"` for its `slots` definition.

The `QStepperNavigation` component allows you to place buttons within `QStepper` or `QStep` to
navigate through the steps. It is up to you to add whatever buttons you require.

> [!IMPORTANT]
> To use global navigation, you must add it to the `QStepper` "navigation" slot.

## Usage

> [!TIP]
> If the QStep content also has images and you want to use swipe actions to navigate, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

> [!IMPORTANT]
> **Keep Alive**
>
> - Please take notice of the Boolean `keep-alive` prop for QStepper, if you need this behavior. Do NOT use Vue's native `<keep-alive>` component over QStep.
> - Should you need the `keep-alive-include` or `keep-alive-exclude` props then the QStep `name`s must be valid Vue component names (no spaces allowed, don't start with a number etc).

### Horizontal

```vue
<template>
  <q-stepper v-model="step" ref="stepperRef" color="primary" animated>
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 4 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

### Vertical

```vue
<template>
  <q-stepper v-model="step" vertical color="primary" animated>
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.

      <q-stepper-navigation>
        <q-btn @click="step = 2" color="primary" label="Continue" />
      </q-stepper-navigation>
    </q-step>

    <q-step
      :name="2"
      title="Create an ad group"
      caption="Optional"
      icon="create_new_folder"
      :done="step > 2"
    >
      An ad group contains one or more ads which target a shared set of
      keywords.

      <q-stepper-navigation>
        <q-btn @click="step = 4" color="primary" label="Continue" />
        <q-btn
          flat
          @click="step = 1"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step :name="3" title="Ad template" icon="assignment" disable>
      This step won't show up because it is disabled.
    </q-step>

    <q-step :name="4" title="Create an ad" icon="add_comment">
      Try out different ad text to see what brings in the most customers, and
      learn how to enhance your ads using features like ad extensions. If you
      run into any problems with your ads, find out how to tell if they're
      running and how to resolve approval issues.

      <q-stepper-navigation>
        <q-btn color="primary" label="Finish" />
        <q-btn
          flat
          @click="step = 2"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

### Responsive

The `vertical` prop can change at runtime. The steps and whatever they hold stay mounted across the switch, so a form the user is filling in keeps its state when the stepper adapts to the viewport.

Example "Vertical below the md breakpoint":

```vue
<template>
  <q-stepper
    v-model="step"
    :vertical="$q.screen.lt.md"
    color="primary"
    animated
  >
    <q-step :name="1" title="Pick a plan" icon="settings" :done="step > 1">
      <q-option-group v-model="plan" :options="planOptions" color="primary" />

      <q-stepper-navigation>
        <q-btn @click="step = 2" color="primary" label="Continue" />
      </q-stepper-navigation>
    </q-step>

    <q-step :name="2" title="Your details" icon="person" :done="step > 2">
      <q-input v-model="name" label="Name" filled style="max-width: 300px" />

      <q-stepper-navigation>
        <q-btn @click="step = 3" color="primary" label="Continue" />
        <q-btn
          flat
          @click="step = 1"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step :name="3" title="Review" icon="check">
      Plan: {{ plan }}, name: {{ name || '(none)' }}

      <q-stepper-navigation>
        <q-btn color="primary" label="Finish" />
        <q-btn
          flat
          @click="step = 2"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
const plan = ref('basic')
const name = ref('')

const planOptions = [
  { label: 'Basic', value: 'basic' },
  { label: 'Pro', value: 'pro' },
  { label: 'Enterprise', value: 'enterprise' }
]
</script>
```

### Header navigation

Example "Non-linear header navigation":

```vue
<template>
  <q-btn
    label="Reset"
    push
    color="white"
    text-color="primary"
    @click="reset"
    class="q-mb-md"
  />

  <q-stepper v-model="step" header-nav color="primary" animated>
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="done1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.

      <q-stepper-navigation>
        <q-btn
          @click="
            () => {
              done1 = true
              step = 2
            }
          "
          color="primary"
          label="Continue"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step
      :name="2"
      title="Create an ad group"
      caption="Optional"
      icon="create_new_folder"
      :done="done2"
    >
      An ad group contains one or more ads which target a shared set of
      keywords.

      <q-stepper-navigation>
        <q-btn
          @click="
            () => {
              done2 = true
              step = 3
            }
          "
          color="primary"
          label="Continue"
        />
        <q-btn
          flat
          @click="step = 1"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step :name="3" title="Create an ad" icon="add_comment" :done="done3">
      Try out different ad text to see what brings in the most customers, and
      learn how to enhance your ads using features like ad extensions. If you
      run into any problems with your ads, find out how to tell if they're
      running and how to resolve approval issues.

      <q-stepper-navigation>
        <q-btn color="primary" @click="done3 = true" label="Finish" />
        <q-btn
          flat
          @click="step = 2"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
const done1 = ref(false)
const done2 = ref(false)
const done3 = ref(false)

function reset() {
  done1.value = false
  done2.value = false
  done3.value = false
  step.value = 1
}
</script>
```

Example "Linear header navigation":

```vue
<template>
  <q-btn
    label="Reset"
    push
    color="white"
    text-color="primary"
    @click="step = 1"
    class="q-mb-md"
  />

  <q-stepper v-model="step" header-nav color="primary" animated>
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
      :header-nav="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.

      <q-stepper-navigation>
        <q-btn
          @click="
            () => {
              done1 = true
              step = 2
            }
          "
          color="primary"
          label="Continue"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step
      :name="2"
      title="Create an ad group"
      caption="Optional"
      icon="create_new_folder"
      :done="step > 2"
      :header-nav="step > 2"
    >
      An ad group contains one or more ads which target a shared set of
      keywords.

      <q-stepper-navigation>
        <q-btn
          @click="
            () => {
              done2 = true
              step = 3
            }
          "
          color="primary"
          label="Continue"
        />
        <q-btn
          flat
          @click="step = 1"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>

    <q-step
      :name="3"
      title="Create an ad"
      icon="add_comment"
      :header-nav="step > 3"
    >
      Try out different ad text to see what brings in the most customers, and
      learn how to enhance your ads using features like ad extensions. If you
      run into any problems with your ads, find out how to tell if they're
      running and how to resolve approval issues.

      <q-stepper-navigation>
        <q-btn color="primary" @click="done3 = true" label="Finish" />
        <q-btn
          flat
          @click="step = 2"
          color="primary"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </q-step>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

### Accessibility *(v2.25+)*

The active step's header carries `aria-current="step"` so screen readers announce the user's position in the process, and each step's content renders as a `group` labeled with the step's `title`. Navigable headers (see the `header-nav` prop) are exposed as buttons. A `disable`d step stays part of that set: its header keeps the `button` role and announces itself as unavailable through `aria-disabled="true"`, while `tabindex="-1"` keeps it out of the tab order and neither click nor keyboard can activate it — the same shape [QBtn](button.md#accessibility) and [QChip](chip.md#accessibility) use for their disabled states. A step that opts out of header navigation (or a stepper without it) claims no role at all, since it is not a control in the first place.

### Header options

Example "Signaling step error":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    color="primary"
    header-nav
    animated
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :error="step < 3"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(2)
</script>
```

Example "Alternative labels":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    alternative-labels
    color="primary"
    animated
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

> [!TIP]
> You can also connect `contracted` prop to `$q.screen` to create a responsive behavior, like `:contracted="$q.screen.lt.md"`.
> More info: [Quasar Screen Plugin](../options/screen-plugin.md).

Example "Contracted":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    contracted
    color="primary"
    animated
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

### Style

Play with coloring using the `*-icon` and `*-color` props (on QStepper or override on specific QStep).

Example "Coloring":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    animated
    done-color="deep-orange"
    active-color="purple"
    inactive-color="secondary"
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="deep-orange"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="deep-orange"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

You can also use `prefix` prop (max 2 characters) instead of an icon for each step's header. This will be displayed if the step is not currently being edited and it isn't marked with error or as "done".

Example "Step prefix":

```vue
<template>
  <q-stepper v-model="step" ref="stepperRef" animated active-color="purple">
    <q-step :name="1" prefix="1" title="Select campaign settings">
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="deep-orange"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="deep-orange"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

Example "Force dark mode":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    dark
    class="bg-grey-9"
    active-color="deep-orange"
    done-color="secondary"
    animated
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="deep-orange"
          :label="step === 3 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="deep-orange"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

You can use the `header-class` prop to apply any CSS class(es) to the header. In the example below, we are applying bolded text:

Example "Header Class":

```vue
<template>
  <q-stepper
    v-model="step"
    ref="stepperRef"
    color="primary"
    header-class="text-bold"
    animated
  >
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 4 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```

### Message slot

Example "Message slot with fixed height steps":

```vue
<template>
  <q-stepper v-model="step" ref="stepperRef" color="primary" animated>
    <q-step
      :name="1"
      title="Select campaign settings"
      icon="settings"
      :done="step > 1"
      style="min-height: 200px"
    >
      For each ad campaign that you create, you can control how much you're
      willing to spend on clicks and conversions, which networks and
      geographical locations you want your ads to show on, and more.
    </q-step>

    <!-- ... -->

    <template #navigation>
      <q-stepper-navigation>
        <q-btn
          @click="$refs.stepperRef.next()"
          color="primary"
          :label="step === 4 ? 'Finish' : 'Continue'"
        />
        <q-btn
          v-if="step > 1"
          flat
          color="primary"
          @click="$refs.stepperRef.previous()"
          label="Back"
          class="q-ml-sm"
        />
      </q-stepper-navigation>
    </template>

    <template #message>
      <q-banner v-if="step === 1" class="bg-purple-8 text-white q-px-lg">
        Campaign settings are important...
      </q-banner>
      <q-banner v-else-if="step === 2" class="bg-orange-8 text-white q-px-lg">
        The ad group helps you to...
      </q-banner>
      <q-banner v-else-if="step === 3" class="bg-green-8 text-white q-px-lg">
        The Ad template is disabled - this won't be displayed
      </q-banner>
      <q-banner v-else class="bg-blue-8 text-white q-px-lg">
        The final step is creating the ad...
      </q-banner>
    </template>
  </q-stepper>
</template>

<script setup>
import { ref } from 'vue'

const step = ref(1)
</script>
```
