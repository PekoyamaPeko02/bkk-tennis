---
title: Select
---
The QSelect component has two types of selection: single or multiple. This component opens up a menu for the selection list and action. A filter can also be used for longer lists.

In case you are looking for a dropdown "button" instead of "input" use [Button Dropdown](button-dropdown.md) instead.

## QSelect API

Not inlined here: call the `get_api` tool with `name: "QSelect"` for its definition, or add `part` (`props`, `computedProps`, `methods`, `events`, `slots`) for one of them.

## Design

### Overview

> [!NOTE]
> For your QSelect you can use only one of the main designs (`filled`, `outlined`, `standout`, `borderless`). You cannot use multiple as they are self-exclusive.

Example "Design Overview":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-select v-model="model" :options="options" label="Standard" />

      <q-select filled v-model="model" :options="options" label="Filled" />

      <q-select outlined v-model="model" :options="options" label="Outlined" />

      <q-select standout v-model="model" :options="options" label="Standout" />

      <q-select
        standout="bg-teal text-white"
        v-model="model"
        :options="options"
        label="Custom standout"
      />

      <q-select
        borderless
        v-model="model"
        :options="options"
        label="Borderless"
      />

      <q-select
        rounded
        filled
        v-model="model"
        :options="options"
        label="Rounded filled"
      />

      <q-select
        rounded
        outlined
        v-model="model"
        :options="options"
        label="Rounded outlined"
      />

      <q-select
        rounded
        standout
        v-model="model"
        :options="options"
        label="Rounded standout"
      />

      <q-select
        square
        filled
        v-model="model"
        :options="options"
        label="Square filled"
      />

      <q-select
        square
        outlined
        v-model="model"
        :options="options"
        label="Square outlined"
      />

      <q-select
        square
        standout
        v-model="model"
        :options="options"
        label="Square standout"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Decorators

```vue
<template>
  <div class="q-pb-lg">
    <q-toggle v-model="dense" label="Dense QSelect" />
    <q-toggle v-model="denseOpts" label="Dense options" />
  </div>

  <div class="q-gutter-md" style="max-width: 300px">
    <q-select
      filled
      v-model="model"
      :options="options"
      label="Label (stacked)"
      stack-label
      :dense="dense"
      :options-dense="denseOpts"
    />

    <q-select
      outlined
      v-model="model"
      :options="options"
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-select>

    <q-select
      standout
      v-model="model"
      :options="options"
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-select>

    <q-select
      filled
      bottom-slots
      v-model="model"
      :options="options"
      label="Label"
      counter
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #prepend>
        <q-icon name="place" @click.stop.prevent />
      </template>
      <template #append>
        <q-icon
          name="close"
          @click.stop.prevent="model = ''"
          class="cursor-pointer"
        />
      </template>

      <template #hint> Field hint </template>
    </q-select>

    <q-select
      rounded
      outlined
      bottom-slots
      v-model="model"
      :options="options"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #before>
        <q-icon name="flight_takeoff" />
      </template>

      <template #append>
        <q-icon
          v-if="model !== ''"
          name="close"
          @click.stop.prevent="model = ''"
          class="cursor-pointer"
        />
        <q-icon name="search" @click.stop.prevent />
      </template>

      <template #hint> Field hint </template>
    </q-select>

    <q-select
      filled
      bottom-slots
      v-model="model"
      :options="options"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #before>
        <q-avatar>
          <img
            alt="User avatar"
            src="https://cdn.quasar.dev/img/avatar5.jpg"
          />
        </q-avatar>
      </template>

      <template #append>
        <q-icon
          v-if="model !== ''"
          name="close"
          @click.stop.prevent="model = ''"
          class="cursor-pointer"
        />
        <q-icon name="schedule" @click.stop.prevent />
      </template>

      <template #hint> Field hint </template>

      <template #after>
        <q-btn round dense flat icon="send" />
      </template>
    </q-select>

    <q-select
      filled
      bottom-slots
      v-model="model"
      :options="options"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
      :options-dense="denseOpts"
    >
      <template #before>
        <q-icon name="event" />
      </template>

      <template #hint> Field hint </template>

      <template #append>
        <q-btn round dense flat icon="add" @click.stop.prevent />
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
const dense = ref(false)
const denseOpts = ref(false)
</script>
```

### Coloring

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-select
      color="purple-12"
      v-model="model"
      :options="options"
      label="Label"
    >
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-select>

    <q-select
      color="teal"
      filled
      v-model="model"
      :options="options"
      label="Label"
    >
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-select>

    <q-select
      color="grey-3"
      outlined
      label-color="orange"
      v-model="model"
      :options="options"
      label="Label"
    >
      <template #append>
        <q-icon name="event" color="orange" />
      </template>
    </q-select>

    <q-select
      color="lime-11"
      bg-color="green"
      filled
      v-model="model"
      :options="options"
      label="Label"
    >
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-select>

    <q-select
      color="teal"
      outlined
      v-model="model"
      :options="options"
      label="Label"
    >
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-select>

    <q-select
      clearable
      color="orange"
      standout
      bottom-slots
      v-model="model"
      :options="options"
      label="Label"
      counter
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="favorite" />
      </template>

      <template #hint> Field hint </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Clearable

As a helper, you can use `clearable` prop so user can reset model to `null` through an appended icon. The second QSelect in the example below is the equivalent of using `clearable`.

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-select
      clearable
      filled
      color="purple-12"
      v-model="model"
      :options="options"
      label="Label"
    />

    <!-- equivalent -->
    <q-select
      color="orange"
      filled
      v-model="model"
      :options="options"
      label="Label"
    >
      <template v-if="model" #append>
        <q-icon
          name="cancel"
          @click.stop.prevent="model = null"
          class="cursor-pointer"
        />
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('Google')
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Disable and readonly

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      disable
      filled
      v-model="model"
      :options="options"
      hint="Disable"
      style="width: 250px"
    />

    <q-select
      readonly
      filled
      v-model="model"
      :options="options"
      hint="Readonly"
      style="width: 250px"
    />

    <q-select
      disable
      readonly
      filled
      v-model="model"
      :options="options"
      hint="Disable and readonly"
      style="width: 250px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('Google')
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Slots with QBtn type "submit"

> [!IMPORTANT]
> When placing a QBtn with type "submit" in one of the "before", "after", "prepend", or "append" slots of a QField, QInput or QSelect, you should also add a `@click` listener on the QBtn in question. This listener should call the method that submits your form. All "click" events in such slots are not propagated to their parent elements.

### Menu transitions

> [!NOTE]
> Please note that transitions do not work when using `options-cover` prop.

In the example below there's a few transitions showcased. For a full list of transitions available, go to [Transitions](../options/transitions.md).

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      label="Flip up/down"
      transition-show="flip-up"
      transition-hide="flip-down"
      filled
      v-model="model"
      :options="options"
      style="width: 250px"
    />

    <q-select
      label="Scale"
      transition-show="scale"
      transition-hide="scale"
      filled
      v-model="model"
      :options="options"
      style="width: 250px"
    />

    <q-select
      label="Jump up"
      transition-show="jump-up"
      transition-hide="jump-up"
      filled
      v-model="model"
      :options="options"
      style="width: 250px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Options list display mode

By default QSelect shows the list of options as a menu on desktop and as a dialog on mobiles. You can force one behavior by using the `behavior` property.

The dialog mode renders a "Close" button (label taken from the [Quasar Language Pack](../options/quasar-language-packs.md)) inside the dialog's control, so users are not forced to tap on the backdrop in order to dismiss it. The button picks up the `color` prop, can be further styled through its `q-select__dialog-close` CSS class, or removed altogether with the `hide-dialog-close` prop.

> [!WARNING]
> Please note that on iOS menu behavior might generate problems, especially when used in combination with `use-input` prop. You can use a conditional `behavior` prop like `:behavior="$q.platform.is.ios ? 'dialog' : 'menu'"` to use dialog mode only on iOS.

Example "Show options in menu":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      label="Simple select"
      :options="stringOptions"
      style="width: 250px"
      behavior="menu"
    />

    <q-select
      filled
      v-model="model"
      use-input
      input-debounce="0"
      label="Simple filter"
      :options="options"
      @filter="filterFn"
      style="width: 250px"
      behavior="menu"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update) {
  if (val === '') {
    update(() => {
      options.value = stringOptions
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

Example "Show options in dialog":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      label="Simple select"
      :options="stringOptions"
      style="width: 250px"
      behavior="dialog"
    />

    <q-select
      filled
      v-model="model"
      use-input
      input-debounce="0"
      label="Simple filter"
      :options="options"
      @filter="filterFn"
      style="width: 250px"
      behavior="dialog"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update) {
  if (val === '') {
    update(() => {
      options.value = stringOptions
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

### Hover to open *(v2.28+)*

With the `hover` prop the options also open when the pointer hovers the select and close once the pointer has left both the select and its options menu. The `hover-hide-delay` prop controls the grace period in which the pointer can travel between the two (or return) before the options close, while `hover-delay` postpones the opening.

Click/tap and keyboard interactions keep toggling the options as usual, so touch devices (which have no hover) simply fall back to them; this also means that clicking a hover-opened select closes its options. The one exception is a click that lands while the options are still animating into view: it focuses the select and keeps them open, so a single move-and-click gesture cannot close what it just opened.

A hover-triggered open does not focus the select, so it emits no `@focus`/`@blur` and does not trigger lazy validation rules on a pointer merely passing over; the select only gets focused (upgrading the open to a regular one, which no longer closes when the pointer leaves) when the user actually clicks or tabs into it.

> [!NOTE]
> The prop only applies while the options show up as a menu; it has no effect with `behavior="dialog"`, nor on mobile platforms unless `behavior="menu"` is used.

Example "Hover to open":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      hover
      label="Hover to open"
      :options="stringOptions"
      style="width: 250px"
    />

    <q-select
      filled
      v-model="model"
      hover
      :hover-delay="300"
      :hover-hide-delay="600"
      label="With delays"
      :options="stringOptions"
      style="width: 250px"
    />

    <q-select
      filled
      v-model="model"
      hover
      use-input
      input-debounce="0"
      label="With filtering"
      :options="options"
      @filter="filterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update) {
  if (val === '') {
    update(() => {
      options.value = stringOptions
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

## The model

> [!IMPORTANT]
> The model for single selection can be anything (String, Object, ...) while the model for multiple selection must be an Array.

Example "Single vs multiple selection":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <q-select
      filled
      v-model="single"
      :options="options"
      label="Single"
      style="width: 250px"
    />

    <q-select
      filled
      v-model="multiple"
      multiple
      :options="options"
      label="Multiple"
      style="width: 250px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const single = ref(null)
const multiple = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

Example "Multiple selection, counter and max-values":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <q-select
      filled
      v-model="model"
      multiple
      :options="options"
      counter
      hint="With counter"
      style="width: 250px"
    />

    <q-select
      filled
      v-model="model2"
      multiple
      :options="options"
      counter
      max-values="2"
      hint="Max 2 selections"
      style="width: 250px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const model2 = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

The model content can be influenced by `emit-value` prop as you'll learn in "The options" section below.

## The options

### Options type

Example "String options":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select filled v-model="model" :options="options" label="Standard" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

Example "Object options":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select filled v-model="model" :options="options" label="Standard" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = [
  {
    label: 'Google',
    value: 'Google',
    description: 'Search engine',
    category: '1'
  },
  // ...
  {
    label: 'Oracle',
    value: 'Oracle',
    disable: true,
    description: 'Databases',
    category: '3'
  }
]
</script>
```

### Affecting model

When `emit-value` is used, the model becomes the determined `value` from the specified selected option. Default is to emit the whole option. It makes sense to use it only when the options are of Object form.

Example "Emit-value":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        label="Standard"
        emit-value
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = [
  {
    label: 'Google',
    value: 'goog',
    description: 'Search engine',
    icon: 'mail'
  },
  // ...
  {
    label: 'Oracle',
    value: 'ora',
    disable: true,
    description: 'Databases',
    icon: 'casino'
  }
]
</script>
```

When `map-options` is used, the model can contain only the `value`, and it will be mapped against the options to determine its label. There is a performance penalty involved, so use it only if absolutely necessary. It's not needed, for example, if the model contains the whole Object (so contains the label prop).

Example "Map options":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        label="Standard"
        emit-value
        map-options
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = [
  {
    label: 'Google',
    value: 'goog'
  },
  // ...
  {
    label: 'Oracle',
    value: 'ora',
    disable: true
  }
]
</script>
```

### Custom prop names

By default, QSelect looks at `label`, `value`, `disable` and `sanitize` props of each option from the options array Objects. But you can override those:

> [!WARNING]
> If you use functions for custom props always check if the option is null. These functions are used both for options in the list and for the selected options.

Example "Custom label, value and disable props":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <div class="col-12">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>
    </div>

    <q-select
      filled
      v-model="model"
      :options="options"
      option-value="id"
      option-label="desc"
      option-disable="inactive"
      emit-value
      map-options
      style="min-width: 250px; max-width: 300px"
    />

    <q-select
      filled
      v-model="model"
      :options="options"
      :option-value="
        opt => (Object(opt) === opt && 'id' in opt ? opt.id : null)
      "
      :option-label="
        opt => (Object(opt) === opt && 'desc' in opt ? opt.desc : '- Null -')
      "
      :option-disable="
        opt => (Object(opt) === opt ? opt.inactive === true : true)
      "
      emit-value
      map-options
      style="min-width: 250px; max-width: 300px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = [
  {
    id: 'goog',
    desc: 'Google'
  },
  // ...
  {
    id: 'ora',
    desc: 'Oracle',
    inactive: true
  }
]
</script>
```

### Customizing menu options

> [!IMPORTANT]
> The list of options is rendered using virtual scroll, so if you render more than one element for an option you must set a `q-virtual-scroll--with-prev` class on all elements except the first one.

Example "Options slot":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        label="Standard"
        color="teal"
        clearable
        options-selected-class="text-deep-orange"
      >
        <template #option="scope">
          <q-item v-bind="scope.itemProps">
            <q-item-section avatar>
              <q-icon :name="scope.opt.icon" />
            </q-item-section>
            <q-item-section>
              <q-item-label>{{ scope.opt.label }}</q-item-label>
              <q-item-label caption>{{ scope.opt.description }}</q-item-label>
            </q-item-section>
          </q-item>
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = [
  {
    label: 'Google',
    value: 'Google',
    description: 'Search engine',
    icon: 'mail'
  },
  // ...
  {
    label: 'Oracle',
    value: 'Oracle',
    disable: true,
    description: 'Databases',
    icon: 'casino'
  }
]
</script>
```

Here is another example where we add a QToggle to each option. The possibilities are endless.

Example "Object options":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        label="Multi with toggle"
        multiple
        emit-value
        map-options
      >
        <template #option="{ itemProps, opt, selected, toggleOption }">
          <q-item v-bind="itemProps">
            <q-item-section>
              <q-item-label v-html="opt.label" />
            </q-item-section>
            <q-item-section side>
              <q-toggle
                :model-value="selected"
                @update:model-value="toggleOption(opt)"
              />
            </q-item-section>
          </q-item>
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref([])
const options = [
  {
    label: 'Google',
    value: 1
  },
  // ...
]
</script>
```

By default, when there are no options, the menu won't appear. But you can customize this scenario and specify what the menu should display. For a plain text message, the `no-option-label` prop (v2.28+) is enough; use the `no-option` slot (it overrides the prop) when you need custom content:

Example "No options slot":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-select filled v-model="model" :options="options" label="No options">
        <template #no-option>
          <q-item>
            <q-item-section class="text-italic text-grey">
              No options slot
            </q-item-section>
          </q-item>
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = []
</script>
```

### Lazy loading

The following example shows a glimpse of how you can play with lazy loading the options. This means, along with many other things, that `options` prop is not required on first render.

Example "Lazy load options":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <q-select
      filled
      v-model="model"
      use-chips
      label="Lazy load opts"
      :options="options"
      @filter="filterFn"
      @filter-abort="abortFilterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>

    <q-btn
      v-if="options"
      label="Reset"
      color="primary"
      @click="options = null"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(null)

function filterFn(val, update, abort) {
  if (options.value !== null) {
    // already loaded
    update()
    return
  }

  setTimeout(() => {
    update(() => {
      options.value = stringOptions
    })
  }, 2000)
}

function abortFilterFn() {
  console.log('delayed filter aborted')
}
</script>
```

> [!NOTE]
> While options are being loaded, the default loading spinner takes the place of the dropdown icon so the field keeps a constant width. For this reason, when `hide-dropdown-icon` is used the default spinner is not displayed at all (it would make the field's width jump); supply a `loading` slot if you still want an inline indicator in that case.

> [!NOTE]
> When the model already holds a value and `map-options` is used, there is nothing to map it against until the options get loaded, so the field would display the raw value. Starting with Quasar v2.28, QSelect asks for the options on its own in this case: it calls your `@filter` handler once with an empty search string and without opening the menu, so the correct label shows up without any user interaction. The same happens when the model value arrives later (a record loaded from the server, for instance). A value that the loaded options do not contain is not requested again. Should you not want this behavior (when the parent component loads the options itself, for example), opt out with the `no-option-prefetch` prop.

You can dynamically load new options when scroll reaches the end:

Example "Dynamic loading options":

```vue
<template>
  <div style="max-width: 300px">
    <q-select
      filled
      v-model="model"
      multiple
      :options="options"
      :loading="loading"
      @virtual-scroll="onScroll"
    />
  </div>
</template>

<script setup>
import { computed, nextTick, ref } from 'vue'

const allOptions = []
for (let i = 0; i <= 100_000; i++) {
  allOptions.push('Opt ' + i)
}

const pageSize = 50
const lastPage = Math.ceil(allOptions.length / pageSize)

const model = ref(null)
const loading = ref(false)

const nextPage = ref(2)
const options = computed(() =>
  allOptions.slice(0, pageSize * (nextPage.value - 1))
)

function onScroll({ to, ref: compRef }) {
  const lastIndex = options.value.length - 1

  if (loading.value !== true && nextPage.value < lastPage && to === lastIndex) {
    loading.value = true

    setTimeout(() => {
      nextPage.value++
      nextTick(() => {
        compRef.refresh()
        loading.value = false
      })
    }, 500)
  }
}
</script>
```

### Cover mode

Example "Menu covering component":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-select
        filled
        v-model="model"
        :options="options"
        options-cover
        stack-label
        label="Standard"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

### Disable TAB selection

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-select
        disable-tab-selection
        filled
        v-model="model"
        :options="options"
        stack-label
        label="Standard"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

## The display value

By default, the selected value is rendered as a single non-wrapping line, truncated with an ellipsis when there is not enough room for it. Should you need to restyle it (allow it to wrap, for example), target its `q-select__selected-value` CSS class (v2.28+).

Example "Custom display value":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        stack-label
        label="Standard"
        :display-value="`Company: ${model ? model : '*none*'}`"
      >
        <template #append>
          <q-icon
            v-if="model !== null"
            class="cursor-pointer"
            name="clear"
            @click.stop.prevent="model = null"
          />
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('Twitter')
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

Example "Chips as display value":

```vue
<template>
  <div class="q-gutter-md row items-start">
    <div style="min-width: 250px; max-width: 300px">
      <q-badge color="secondary" class="q-mb-md">
        Model: {{ modelSingle || '*none*' }}
      </q-badge>

      <q-select
        filled
        v-model="modelSingle"
        :options="options"
        use-chips
        stack-label
        label="Single selection"
      />
    </div>

    <div style="min-width: 250px; max-width: 300px">
      <q-badge color="secondary" class="q-mb-md">
        Model: {{ modelMultiple || '[]' }}
      </q-badge>

      <q-select
        filled
        v-model="modelMultiple"
        multiple
        :options="options"
        use-chips
        stack-label
        label="Multiple selection"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const modelSingle = ref('Apple')
const modelMultiple = ref(['Facebook'])
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

The chips also act as a removal affordance: their remove icon takes an option out of the selection and, when the inner input is empty, so does the <kbd>Backspace</kbd> key. If the selection should only be changed through the list of options, use the `no-chip-remove` prop to disable both.

Example "Chips without removal (v2.28+)":

```vue
<template>
  <div style="max-width: 300px">
    <q-select
      filled
      v-model="model"
      multiple
      :options="options"
      use-chips
      no-chip-remove
      stack-label
      label="Non-removable chips"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(['Google', 'Facebook'])
const options = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']
</script>
```

Example "Selected-item slot":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary" multi-line> Model: "{{ model }}" </q-badge>

      <q-select
        filled
        v-model="model"
        :options="options"
        stack-label
        label="Standard"
      >
        <template #selected>
          Company:
          <q-chip
            v-if="model"
            dense
            square
            color="white"
            text-color="primary"
            class="q-my-none q-ml-xs q-mr-none"
          >
            <q-avatar color="primary" text-color="white" :icon="model.icon" />
            {{ model.label }}
          </q-chip>
          <q-badge v-else>*none*</q-badge>
        </template>
      </q-select>

      <q-select
        filled
        v-model="model"
        :options="options"
        stack-label
        label="Standard"
        color="secondary"
      >
        <template #selected-item="scope">
          <q-chip
            removable
            dense
            @remove="scope.removeAtIndex(scope.index)"
            :tabindex="scope.tabindex"
            color="white"
            text-color="secondary"
            class="q-ma-none"
          >
            <q-avatar
              color="secondary"
              text-color="white"
              :icon="scope.opt.icon"
            />
            {{ scope.opt.label }}
          </q-chip>
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref({
  label: 'Google',
  value: 'goog',
  icon: 'mail'
})

const options = [
  {
    label: 'Google',
    value: 'goog',
    icon: 'mail'
  },
  // ...
  {
    label: 'Oracle',
    value: 'ora',
    disable: true,
    icon: 'casino'
  }
]
</script>
```

## Filtering and autocomplete

### Native attributes with "use-input"

All the attributes set on QSelect that are not in the list of props in the API will be passed to the native input field used (please check `use-input` prop description first to understand what it does) for filtering / autocomplete / adding new value. Some examples: autocomplete, placeholder.

More information: [native input attributes](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input).

> [!TIP]
> **Accessibility**
>
> Attributes are applied to the focusable control even without `use-input`. This is particularly useful for `aria-label` or `aria-labelledby`, which set the accessible name that screen readers announce for the select (taking precedence over the name derived from the `label` prop).

Example "Filtering options":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      use-input
      input-debounce="0"
      label="Simple filter"
      :options="options"
      @filter="filterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>

    <q-select
      filled
      v-model="model"
      use-input
      hide-selected
      input-debounce="0"
      label="Hide selected"
      :options="options"
      @filter="filterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update) {
  if (val === '') {
    update(() => {
      options.value = stringOptions

      // here you have access to "ref" which
      // is the Vue reference of the QSelect
    })
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

Example "Basic filtering":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      :options="options"
      @filter="filterFn"
      hint="Basic filtering"
      style="width: 250px; padding-bottom: 32px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update, abort) {
  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

Example "Filtering on more than 2 chars":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      v-model="model"
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      :options="options"
      @filter="filterFn"
      hint="Minimum 2 characters to trigger filtering"
      style="width: 250px; padding-bottom: 32px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update, abort) {
  if (val.length < 2) {
    abort()
    return
  }

  update(() => {
    const needle = val.toLowerCase()
    options.value = stringOptions.filter(v => v.toLowerCase().includes(needle))
  })
}
</script>
```

Example "Text autocomplete":

```vue
<template>
  <div class="q-gutter-md row">
    <q-select
      filled
      :model-value="model"
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      :options="options"
      @filter="filterFn"
      @input-value="setModel"
      hint="Text autocomplete"
      style="width: 250px; padding-bottom: 32px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = [
  // ...
].reduce((acc, opt) => {
  for (let i = 1; i <= 5; i++) {
    acc.push(opt + ' ' + i)
  }
  return acc
}, [])

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update, abort) {
  update(() => {
    const needle = val.toLocaleLowerCase()
    options.value = stringOptions.filter(v =>
      v.toLocaleLowerCase().includes(needle)
    )
  })
}

function setModel(val) {
  model.value = val
}
</script>
```

Example "Lazy filtering":

```vue
<template>
  <div class="q-gutter-md">
    <q-select
      filled
      v-model="model"
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      label="Lazy filter"
      :options="options"
      @filter="filterFn"
      @filter-abort="abortFilterFn"
      style="width: 250px"
      hint="With hide-selected and fill-input"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>

    <q-select
      filled
      v-model="model"
      use-input
      use-chips
      input-debounce="0"
      label="Lazy filter"
      :options="options"
      @filter="filterFn"
      @filter-abort="abortFilterFn"
      style="width: 250px"
      hint="With use-chips"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update, abort) {
  // call abort() at any time if you can't retrieve data somehow

  setTimeout(() => {
    update(() => {
      if (val === '') {
        options.value = stringOptions
      } else {
        const needle = val.toLowerCase()
        options.value = stringOptions.filter(v =>
          v.toLowerCase().includes(needle)
        )
      }
    })
  }, 1500)
}

function abortFilterFn() {
  console.log('delayed filter aborted')
}
</script>
```

Example "Selecting option after filtering":

```vue
<template>
  <div class="q-gutter-md">
    <q-select
      filled
      v-model="model"
      clearable
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      label="Focus after filtering"
      :options="options"
      @filter="filterFn"
      @filter-abort="abortFilterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>

    <q-select
      filled
      v-model="model"
      clearable
      use-input
      hide-selected
      fill-input
      input-debounce="0"
      label="Autoselect after filtering"
      :options="options"
      @filter="filterFnAutoselect"
      @filter-abort="abortFilterFn"
      style="width: 250px"
    >
      <template #no-option>
        <q-item>
          <q-item-section class="text-grey"> No results </q-item-section>
        </q-item>
      </template>
    </q-select>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = [
  // ...
].reduce((acc, opt) => {
  for (let i = 1; i <= 5; i++) {
    acc.push(opt + ' ' + i)
  }
  return acc
}, [])

const model = ref(null)
const options = ref(stringOptions)

function filterFn(val, update, abort) {
  // call abort() at any time if you can't retrieve data somehow

  setTimeout(() => {
    update(
      () => {
        if (val === '') {
          options.value = stringOptions
        } else {
          const needle = val.toLowerCase()
          options.value = stringOptions.filter(v =>
            v.toLowerCase().includes(needle)
          )
        }
      },

      // "compRef" is the Vue reference to the QSelect
      compRef => {
        if (val !== '' && compRef.options.length !== 0) {
          compRef.setOptionIndex(-1) // reset optionIndex in case there is something selected
          compRef.moveOptionSelection(1, true) // focus the first selectable option and do not update the input-value
        }
      }
    )
  }, 300)
}

function filterFnAutoselect(val, update, abort) {
  // call abort() at any time if you can't retrieve data somehow

  setTimeout(() => {
    update(
      () => {
        if (val === '') {
          options.value = stringOptions
        } else {
          const needle = val.toLowerCase()
          options.value = stringOptions.filter(v =>
            v.toLowerCase().includes(needle)
          )
        }
      },

      // "compRef" is the Vue reference to the QSelect
      compRef => {
        if (
          val !== '' &&
          compRef.options.length !== 0 &&
          compRef.getOptionIndex() === -1
        ) {
          compRef.moveOptionSelection(1, true) // focus the first selectable option and do not update the input-value
          compRef.toggleOption(compRef.options[compRef.getOptionIndex()], true) // toggle the focused option
        }
      }
    )
  }, 300)
}

function abortFilterFn() {
  console.log('delayed filter aborted')
}
</script>
```

## Create new values

> [!TIP]
> The following are just a few examples to get you started into making your own QSelect behavior. This is not exhaustive list of possibilities that QSelect offers.
>
> It makes sense to use this feature along with `use-input` prop.

In order to enable the creation of new values, you need to **either specify** the `new-value-mode` prop **and/or** listen for `@new-value` event. If you use both, then the purpose of listening to `@new-value` would be only to override the `new-value-mode` in your custom scenarios.

### The new-value-mode prop

The `new-value-mode` prop value specifies how the value should be added: `add` (adds a value, even if duplicate), `add-unique` (add only if NOT duplicate) or `toggle` (adds value if it's not already in model, otherwise it removes it).

By using this prop you don't need to also listen for `@new-value` event, unless you have some specific scenarios for which you want to override the behavior.

Example "New value mode":

```vue
<template>
  <div class="q-gutter-y-md">
    <q-select
      label="Mode: 'add'"
      filled
      v-model="modelAdd"
      use-input
      use-chips
      multiple
      hide-dropdown-icon
      input-debounce="0"
      new-value-mode="add"
      style="width: 250px"
    />

    <q-select
      label="Mode: 'add-unique'"
      filled
      v-model="modelAddUnique"
      use-input
      use-chips
      multiple
      hide-dropdown-icon
      input-debounce="0"
      new-value-mode="add-unique"
      style="width: 250px"
    />

    <q-select
      label="Mode: 'toggle'"
      filled
      v-model="modelToggle"
      use-input
      use-chips
      multiple
      hide-dropdown-icon
      input-debounce="0"
      new-value-mode="toggle"
      style="width: 250px"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const modelAdd = ref(null)
const modelAddUnique = ref(null)
const modelToggle = ref(null)
</script>
```

### The @new-value event

The `@new-value` event is emitted with the value to be added and a `done` callback. The `done` callback has two **optional** parameters:

- the value to be added
- the behavior (same values of `new-value-mode` prop, and when it is specified it overrides that prop -- if it is used) -- default behavior (if not using `new-value-mode`) is to add the value even if it would be a duplicate

Calling `done()` with no parameters simply empties the input box value, without tampering with the model in any way.

Example "Listening on @new-value":

```vue
<template>
  <q-select
    label="Allows duplicates"
    filled
    v-model="model"
    use-input
    use-chips
    multiple
    hide-dropdown-icon
    input-debounce="0"
    @new-value="createValue"
    style="width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)

function createValue(val, done) {
  // specific logic to eventually call done(...) -- or not
  done(val)

  // done callback has two optional parameters:
  //  - the value to be added
  //  - the behavior (same values of new-value-mode prop,
  //    and when it is specified it overrides that prop –
  //    if it is used); default behavior (if not using
  //    new-value-mode) is to add the value even if it would
  //    be a duplicate
}
</script>
```

Example "Adding only unique values":

```vue
<template>
  <q-select
    label="Unique values only"
    filled
    v-model="model"
    use-input
    use-chips
    multiple
    hide-dropdown-icon
    input-debounce="0"
    @new-value="createValue"
    style="width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)

function createValue(val, done) {
  // specific logic to eventually call done(...) -- or not
  done(val, 'add-unique')

  // done callback has two optional parameters:
  //  - the value to be added
  //  - the behavior (same values of new-value-mode prop,
  //    and when it is specified it overrides that prop –
  //    if it is used); default behavior (if not using
  //    new-value-mode) is to add the value even if it would
  //    be a duplicate
}
</script>
```

### Using menu and filtering

Filtering and adding the new values to menu:

Example "Filtering and adding to menu":

```vue
<template>
  <q-select
    filled
    v-model="model"
    use-input
    use-chips
    multiple
    input-debounce="0"
    @new-value="createValue"
    :options="filterOptions"
    @filter="filterFn"
    style="width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const filterOptions = ref(stringOptions)

function createValue(val, done) {
  // Calling done(var) when new-value-mode is not set or "add", or done(var, "add") adds "var" content to the model
  // and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "add-unique", or done(var, "add-unique") adds "var" content to the model
  // only if is not already set
  // and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "toggle", or done(var, "toggle") toggles the model with "var" content
  // (adds to model if not already in the model, removes from model if already has it)
  // and it resets the input textbox to empty string
  // ----
  // If "var" content is undefined/null, then it doesn't tampers with the model
  // and only resets the input textbox to empty string

  if (val.length !== 0) {
    if (!stringOptions.includes(val)) {
      stringOptions.push(val)
    }
    done(val, 'toggle')
  }
}

function filterFn(val, update) {
  update(() => {
    if (val === '') {
      filterOptions.value = stringOptions
    } else {
      const needle = val.toLowerCase()
      filterOptions.value = stringOptions.filter(v =>
        v.toLowerCase().includes(needle)
      )
    }
  })
}
</script>
```

Filters new values (in the example below the value to be added requires at least 3 characters to pass), and does not add to menu:

Example "Filtering without adding to menu":

```vue
<template>
  <q-select
    filled
    v-model="model"
    use-input
    use-chips
    multiple
    input-debounce="0"
    @new-value="createValue"
    :options="filterOptions"
    @filter="filterFn"
    style="width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const filterOptions = ref(stringOptions)

function createValue(val, done) {
  // Calling done(var) when new-value-mode is not set or "add", or done(var, "add") adds "var" content to the model
  // and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "add-unique", or done(var, "add-unique") adds "var" content to the model
  // only if is not already set
  // and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "toggle", or done(var, "toggle") toggles the model with "var" content
  // (adds to model if not already in the model, removes from model if already has it)
  // and it resets the input textbox to empty string
  // ----
  // If "var" content is undefined/null, then it doesn't tampers with the model
  // and only resets the input textbox to empty string

  if (val.length > 2 && !stringOptions.includes(val)) {
    done(val, 'add-unique')
  }
}

function filterFn(val, update) {
  update(() => {
    if (val === '') {
      filterOptions.value = stringOptions
    } else {
      const needle = val.toLowerCase()
      filterOptions.value = stringOptions.filter(v =>
        v.toLowerCase().includes(needle)
      )
    }
  })
}
</script>
```

Generating multiple values from input:

Example "Generating multiple values":

```vue
<template>
  <q-select
    filled
    label="Select multiple values"
    hint="Separate multiple values by [,;|]"
    v-model="model"
    use-input
    use-chips
    multiple
    input-debounce="0"
    @new-value="createValue"
    :options="filterOptions"
    @filter="filterFn"
    style="width: 250px"
  />
</template>

<script setup>
import { ref } from 'vue'

const stringOptions = ['Google', 'Facebook', 'Twitter', 'Apple', 'Oracle']

const model = ref(null)
const filterOptions = ref(stringOptions)

function createValue(val, done) {
  // Calling done(var) when new-value-mode is not set or is "add", or done(var, "add") adds "var" content to the model
  // and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "add-unique", or done(var, "add-unique") adds "var" content to the model
  // only if is not already set and it resets the input textbox to empty string
  // ----
  // Calling done(var) when new-value-mode is "toggle", or done(var, "toggle") toggles the model with "var" content
  // (adds to model if not already in the model, removes from model if already has it)
  // and it resets the input textbox to empty string
  // ----
  // If "var" content is undefined/null, then it doesn't tampers with the model
  // and only resets the input textbox to empty string

  if (val.length !== 0) {
    const modelValue = [...(model.value || [])]

    val
      .split(/[,;|]+/)
      .map(v => v.trim())
      .filter(v => v.length !== 0)
      .forEach(v => {
        if (!stringOptions.includes(v)) {
          stringOptions.push(v)
        }
        if (!modelValue.includes(v)) {
          modelValue.push(v)
        }
      })

    done(null)
    model.value = modelValue
  }
}

function filterFn(val, update) {
  update(() => {
    if (val === '') {
      filterOptions.value = stringOptions
    } else {
      const needle = val.toLowerCase()
      filterOptions.value = stringOptions.filter(v =>
        v.toLowerCase().includes(needle)
      )
    }
  })
}
</script>
```

## Sanitization

**By default, all options (included selected ones) are sanitized**. This means that displaying them in HTML format is disabled. However, if you require HTML on your options and you trust their content, then there are a few ways to do this.

You can force the HTML form of the menu options by:

- setting `html` key of the trusted option to `true` (for specific trusted options)
- or by setting `options-html` prop of QSelect (for all options)

The displayed value of QSelect is displayed as HTML if:

- the `display-value-html` prop of QSelect is set
- or you are not using `display-value` and
  - the `options-html` prop of QSelect is set
  - any selected option has `html` key set to `true`

> [!CAUTION]
> If you use `selected` or `selected-item` slots, then you are responsible for sanitization of the display value. The `display-value-html` prop will not apply.

Example "Options in HTML form":

```vue
<template>
  <div style="max-width: 300px">
    <q-badge color="secondary" multi-line class="q-mb-md">
      Model: {{ model || 'empty' }}
    </q-badge>

    <div class="q-gutter-md">
      <q-toggle v-model="optionsHtml" label="Options in HTML form" />

      <q-select
        filled
        v-model="model"
        :options="options"
        label="Standard"
        :options-html="optionsHtml"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model = ref(null)
const optionsHtml = ref(false)
const options = [
  {
    label: '<span class="text-primary text-bold text-underline">Goo</span>gle',
    value: 'Google'
  },
  {
    label:
      '<span class="text-primary">This is</span> in <span class="text-negative text-bold">HTML form</span> through an option prop',
    value: 'Facebook',
    html: true
  }
]
</script>
```

Example "Display value in HTML form":

```vue
<template>
  <div style="max-width: 300px">
    <q-badge color="secondary" multi-line class="q-mb-md">
      Model: {{ model || 'empty' }}
    </q-badge>

    <div class="q-gutter-md">
      <q-toggle v-model="displayHtml" label="Display value in HTML form" />

      <q-select
        filled
        v-model="model"
        :options="options"
        stack-label
        label="Standard"
        :display-value="`Company: ${model ? model.label : '*none*'}`"
        :display-value-html="displayHtml"
      >
        <template #append>
          <q-icon
            v-if="model !== null"
            class="cursor-pointer"
            name="clear"
            @click.stop.prevent="model = null"
          />
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const options = [
  {
    label: '<span class="text-primary">G</span>oogle',
    value: 'Google'
  },
  {
    label: '<span class="text-red">Face</span>book',
    value: 'Facebook'
  }
]

const model = ref(options[0])
const displayHtml = ref(false)
</script>
```

## Render performance

The render performance is NOT affected much by the number of options, unless `map-options` is used on a large set.
Notice the infinite scroll in place which renders additional options as the user scrolls through the list.

> [!TIP]
>
> - (Composition API) To get the best performance while using lots of options, do not wrap the array that you are passing in the `options` prop with ref()/computed()/reactive()/etc. This allows Vue to skip making the list "responsive" to changes.
> - (Options API) To get the best performance while using lots of options, freeze the array that you are passing in the `options` prop using `Object.freeze(items)`. This allows Vue to skip making the list "responsive" to changes.

Example "100k options":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-select filled v-model="model" multiple :options="options" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const options = []
for (let i = 0; i <= 100_000; i++) {
  options.push('Opt ' + i)
}

const model = ref(null)
</script>
```

## Keyboard navigation

When QSelect is focused:

- pressing <kbd>Enter</kbd>, <kbd>Arrow Down</kbd> (or <kbd>Space</kbd> if `use-input` is not set) will open the list of options
- if `use-chips` is set (and `no-chip-remove` is not):
  - pressing <kbd>Shift</kbd> + <kbd>Tab</kbd> will navigate backwards through the QChips (if a QChip is selected <kbd>Tab</kbd> will navigate forward through the QChips)
  - pressing <kbd>Enter</kbd> when a QChip is selected will remove that option from the selection
  - pressing <kbd>Backspace</kbd> will remove the last option from the selection, unless that option is disabled (when `use-input` is set the input should be empty)
- pressing <kbd>Backspace</kbd> when `clearable` is set then:
  - it clears the model (with `null` value) for single selection
  - it removes the last added value for multiple selection
- pressing <kbd>Tab</kbd> (or <kbd>Shift</kbd> + <kbd>Tab</kbd> if `use-chips` is not set or the first QChip is selected) will navigate to the next or previous focusable element on page
- typing text (<kbd>0</kbd> - <kbd>9</kbd> or <kbd>A</kbd> - <kbd>Z</kbd>) if `use-input` is not set will:
  - create a search buffer (will be reset when a new key is not typed for 1.5 seconds) that will be used to search in the options labels
  - select the next option starting with that letter (after the current focused one) if the first key in buffer is typed multiple times
  - select the next option (starting with the current focused one) that matches the typed text (the match is fuzzy - the option label should start with the first letter and contain all the letters)

You can prevent QSelect's action for most keys by preventing its `keydown` event; for example, `@keydown.enter.prevent` keeps <kbd>Enter</kbd> from opening the list of options. The exception is <kbd>Esc</kbd>, whose handling (closing the list of options) is tied to the `keyup` event and cannot be cancelled this way.

When the list of options is opened:

- pressing <kbd>Arrow Up</kbd> or <kbd>Arrow Down</kbd> will navigate up or down in the list of options
- pressing <kbd>Page Up</kbd> or <kbd>Page Down</kbd> will navigate one page up or down in the list of options
- pressing <kbd>Home</kbd> or <kbd>End</kbd> will navigate to the start or end of the list of options (only if you are not using `use-input`, or the input is empty)
- when navigating using arrow keys, navigation will wrap when reaching the start or end of the list
- pressing <kbd>Enter</kbd> (or <kbd>Space</kbd> when `use-input` is not set, or <kbd>Tab</kbd> when `multiple` and `disable-tab-selection` are not set) when an option is selected in the list will:
  - select the option and close the list of options if `multiple` and `disable-tab-selection` are not set
  - toggle the option if `multiple` is set
  - exception: when creating new values (`new-value-mode` prop or `@new-value` event), text you have typed takes precedence over the option that got highlighted automatically for mirroring the current value (upon opening the list or after filtering it); the typed text is then submitted as a new value; an option that you navigated or hovered to still gets selected instead

## Accessibility *(v2.25+)*

QSelect follows the [WAI-ARIA combobox pattern](https://www.w3.org/WAI/ARIA/apg/patterns/combobox/). The focus target is always an `<input>` with `role="combobox"` — the real filter input when `use-input` is set, otherwise a readonly input holding the displayed selection, so screen readers read the current value directly off it. It is rendered whatever the field's state, so it always carries the accessible name, the current value and the `id` that the label's `for` points at: a `readonly` QSelect keeps it focusable (showing the field's focused state and emitting `@focus`/`@blur` when reached) and marked `aria-readonly="true"`, while a `disable`d one stays in the accessibility tree — announced as unavailable — but leaves the tab order through the native `disabled` attribute. Neither can open the popup. It carries `aria-expanded` reflecting the popup state, `aria-controls` referencing the list of options (only while the popup with options actually exists, so the reference never points at a missing element), `aria-activedescendant` tracking the highlighted option and `aria-autocomplete` — `list` when `use-input` lets the typed text filter the options, `none` otherwise. Per the pattern, focus never leaves this input while the popup is open — the list is operated from it, with the keys detailed in the [Keyboard navigation](#keyboard-navigation) section above.

The popup content is a `listbox` (always carrying `aria-multiselectable`, `true` or `false` according to the `multiple` prop) whose options carry `aria-selected` plus `aria-setsize` and `aria-posinset`: since the list is virtually scrolled, only a slice of the options exists in the DOM at any time, and these attributes let screen readers still announce each option's true position within the full set.

When the options render in a dialog (see [Options list display mode](#options-list-display-mode)), the control inside the dialog carries this same combobox contract, and the dialog's "Close" button sits in the Tab order right after it, activating with <kbd>Enter</kbd> or <kbd>Space</kbd> and showing the browser's native focus indicator. However the dialog gets dismissed (the Close button, <kbd>Esc</kbd>, tapping the backdrop or selecting an option outside of `multiple` mode), focus returns to the QSelect control, with one deliberate exception: on mobile platforms a `use-input` QSelect restores it only for keyboard-initiated dismissals, so the virtual keyboard that was just put away is not summoned back. Note that iOS lets <kbd>Tab</kbd> reach buttons (this one, or any other on a page) only with the system's Full Keyboard Access setting enabled; <kbd>Esc</kbd> works regardless.

The `label` prop doubles as the combobox's `aria-label`; an `aria-label` or `aria-labelledby` attribute set on QSelect takes precedence, as it is applied to the focusable control (see the Accessibility tip under [Native attributes with "use-input"](#native-attributes-with-use-input)). Two slot-related responsibilities are yours: when using the `option` slot, `v-bind="scope.itemProps"` onto your item, otherwise the option loses its `role="option"`, its id (the `aria-activedescendant` target), `aria-selected` and position attributes; and content placed in the `before-options`/`after-options` slots sits outside the listbox and out of keyboard reach while the popup is open, so avoid interactive elements there. Label association and error announcements are inherited from the field frame — see [QField's Accessibility section](field.md#accessibility).

## Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QSelect, otherwise formData will not contain it (if it should) - all value are converted to string (native behaviour, so do not use Object values):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-select
      name="preferred_genre"
      v-model="preferred"
      :options="options"
      color="primary"
      filled
      clearable
      label="Preferred genre"
    />

    <q-select
      name="accepted_genres"
      v-model="accepted"
      multiple
      :options="options"
      color="primary"
      filled
      clearable
      label="Accepted genres"
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

const preferred = ref('rock')
const accepted = ref([])
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

  submitted.value = true
  submitResult.value = data
  submitEmpty.value = data.length === 0
}
</script>
```
