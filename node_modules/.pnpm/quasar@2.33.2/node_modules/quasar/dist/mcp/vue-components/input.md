---
title: Input
---
The QInput component is used to capture text input from the user. It uses `v-model`, similar to a regular input. It has support for errors and validation, and comes in a variety of styles, colors, and types.

## QInput API

Not inlined here: call the `get_api` tool with `name: "QInput"` for its definition, or add `part` (`props`, `computedProps`, `methods`, `events`, `slots`) for one of them.

## Design

> [!NOTE]
> For your QInput you can use only one of the main designs (`filled`, `outlined`, `standout`, `borderless`). You cannot use multiple as they are self-exclusive.

Example "Design Overview":

```vue
<template>
  <div class="q-gutter-md" style="max-width: 300px">
    <q-input v-model="text" label="Standard" />

    <q-input filled v-model="text" label="Filled" />

    <q-input outlined v-model="text" label="Outlined" />

    <q-input standout v-model="text" label="Standout" />

    <q-input
      standout="bg-teal text-white"
      v-model="text"
      label="Custom standout"
    />

    <q-input borderless v-model="text" label="Borderless" />

    <q-input rounded filled v-model="text" label="Rounded filled" />

    <q-input rounded outlined v-model="text" label="Rounded outlined" />

    <q-input rounded standout v-model="text" label="Rounded standout" />

    <q-input square filled v-model="text" label="Square filled" />

    <q-input square outlined v-model="text" label="Square outlined" />

    <q-input square standout v-model="text" label="Square standout" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Coloring

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input color="purple-12" v-model="text" label="Label">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input color="teal" filled v-model="text" label="Label">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input
      color="grey-3"
      label-color="orange"
      outlined
      v-model="text"
      label="Label"
    >
      <template #append>
        <q-icon name="event" color="orange" />
      </template>
    </q-input>

    <q-input
      color="lime-11"
      bg-color="green"
      filled
      v-model="text"
      label="Label"
    >
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input color="teal" outlined v-model="text" label="Label">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      color="orange"
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
      clearable
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="favorite" />
      </template>

      <template #hint> Field hint </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Standard

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-toggle v-model="dense" label="Dense QInput" />

    <q-input v-model="text" :dense="dense" />

    <q-input
      v-model="text"
      label="Label (stacked)"
      stack-label
      :dense="dense"
    />

    <q-input v-model="text" label="Label" :dense="dense" />

    <q-input
      v-model="ph"
      label="Label"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      v-model="ph"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input v-model="text" :dense="dense">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input v-model="text" :dense="dense">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input bottom-slots v-model="text" label="Label" counter :dense="dense">
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="flight_takeoff" />
      </template>

      <template #append>
        <q-icon
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="search" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
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
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="schedule" />
      </template>

      <template #hint> Field hint </template>

      <template #after>
        <q-btn round dense flat icon="send" />
      </template>
    </q-input>

    <q-input
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="event" />
      </template>

      <template #hint> Field hint </template>

      <template #append>
        <q-btn round dense flat icon="add" />
      </template>
    </q-input>

    <q-input v-model="text" hint="Disable" :dense="dense" disable />

    <q-input v-model="text" hint="Readonly" :dense="dense" readonly />

    <q-input
      v-model="text"
      hint="Disable and readonly"
      :dense="dense"
      disable
      readonly
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const ph = ref('')
const dense = ref(false)
</script>
```

### Filled

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-toggle v-model="dense" label="Dense QInput" />

    <q-input filled v-model="text" :dense="dense" />

    <q-input
      filled
      v-model="text"
      label="Label (stacked)"
      stack-label
      :dense="dense"
    />

    <q-input filled v-model="text" label="Label" :dense="dense" />

    <q-input
      filled
      v-model="ph"
      label="Label"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      filled
      v-model="ph"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      filled
      square
      v-model="text"
      hint="With perfect square borders"
      :dense="dense"
    />

    <q-input filled v-model="text" :dense="dense">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input filled v-model="text" :dense="dense">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      filled
      bottom-slots
      v-model="text"
      label="Label"
      counter
      :dense="dense"
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      filled
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="flight_takeoff" />
      </template>

      <template #append>
        <q-icon
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="search" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      filled
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
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
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="schedule" />
      </template>

      <template #hint> Field hint </template>

      <template #after>
        <q-btn round dense flat icon="send" />
      </template>
    </q-input>

    <q-input
      filled
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="event" />
      </template>

      <template #hint> Field hint </template>

      <template #append>
        <q-btn round dense flat icon="add" />
      </template>
    </q-input>

    <q-input filled v-model="text" hint="Disable" :dense="dense" disable />

    <q-input filled v-model="text" hint="Readonly" :dense="dense" readonly />

    <q-input
      filled
      v-model="text"
      hint="Disable and readonly"
      :dense="dense"
      disable
      readonly
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const ph = ref('')
const dense = ref(false)
</script>
```

### Outlined

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-toggle v-model="dense" label="Dense QInput" />

    <q-input outlined v-model="text" :dense="dense" />

    <q-input
      outlined
      v-model="text"
      label="Label (stacked)"
      stack-label
      :dense="dense"
    />

    <q-input outlined v-model="text" label="Label" :dense="dense" />

    <q-input
      outlined
      v-model="ph"
      label="Label"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      outlined
      v-model="ph"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      outlined
      square
      v-model="text"
      hint="With perfect square borders"
      :dense="dense"
    />

    <q-input outlined v-model="text" :dense="dense">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input outlined v-model="text" :dense="dense">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      outlined
      bottom-slots
      v-model="text"
      label="Label"
      counter
      :dense="dense"
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      outlined
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="flight_takeoff" />
      </template>

      <template #append>
        <q-icon
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="search" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      outlined
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
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
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="schedule" />
      </template>

      <template #hint> Field hint </template>

      <template #after>
        <q-btn round dense flat icon="send" />
      </template>
    </q-input>

    <q-input
      outlined
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="event" />
      </template>

      <template #hint> Field hint </template>

      <template #append>
        <q-btn round dense flat icon="add" />
      </template>
    </q-input>

    <q-input outlined v-model="text" hint="Disable" :dense="dense" disable />

    <q-input
      outlined
      v-model="text"
      hint="Readonly"
      :dense="dense"
      readonly
    />

    <q-input
      outlined
      v-model="text"
      hint="Disable and readonly"
      :dense="dense"
      disable
      readonly
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const ph = ref('')
const dense = ref(false)
</script>
```

### Standout

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-toggle v-model="dense" label="Dense QInput" />

    <q-input standout v-model="text" :dense="dense" />

    <q-input
      standout="bg-teal text-white"
      v-model="text"
      label="Custom standout"
      :dense="dense"
    />

    <q-input
      standout
      v-model="text"
      label="Label (stacked)"
      stack-label
      :dense="dense"
    />

    <q-input standout v-model="text" label="Label" :dense="dense" />

    <q-input
      standout
      v-model="ph"
      label="Label"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      standout
      v-model="ph"
      placeholder="Placeholder"
      hint="With placeholder"
      :dense="dense"
    />

    <q-input
      standout
      square
      v-model="text"
      hint="With perfect square borders"
      :dense="dense"
    />

    <q-input standout v-model="text" :dense="dense">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input standout v-model="text" :dense="dense">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
      :dense="dense"
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="flight_takeoff" />
      </template>

      <template #append>
        <q-icon
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="search" />
      </template>

      <template #hint> Field hint </template>
    </q-input>

    <q-input
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
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
          v-if="text !== ''"
          name="close"
          @click="text = ''"
          class="cursor-pointer"
        />
        <q-icon name="schedule" />
      </template>

      <template #hint> Field hint </template>

      <template #after>
        <q-btn round dense flat icon="send" />
      </template>
    </q-input>

    <q-input
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
      maxlength="12"
      :dense="dense"
    >
      <template #before>
        <q-icon name="event" />
      </template>

      <template #hint> Field hint </template>

      <template #append>
        <q-btn round dense flat icon="add" />
      </template>
    </q-input>

    <q-input standout v-model="text" hint="Disable" :dense="dense" disable />

    <q-input
      standout
      v-model="text"
      hint="Readonly"
      :dense="dense"
      readonly
    />

    <q-input
      standout
      v-model="text"
      hint="Disable and readonly"
      :dense="dense"
      disable
      readonly
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const ph = ref('')
const dense = ref(false)
</script>
```

One of the most appropriate use cases for Standout design is in a QToolbar:

Example "Standout in QToolbar":

```vue
<template>
  <div class="q-gutter-y-md column" style="width: 300px; max-width: 100%">
    <q-toolbar class="bg-primary text-white rounded-borders">
      <q-btn round dense flat icon="menu" class="q-mr-xs" />
      <q-avatar class="gt-xs">
        <img
          alt="Quasar logo"
          src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
        />
      </q-avatar>

      <q-space />

      <q-input
        dark
        dense
        standout
        v-model="text"
        input-class="text-right"
        class="q-ml-md"
      >
        <template #append>
          <q-icon v-if="text === ''" name="search" />
          <q-icon
            v-else
            name="clear"
            class="cursor-pointer"
            @click="text = ''"
          />
        </template>
      </q-input>
    </q-toolbar>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Borderless

The `borderless` design allows you to seamlessly integrate your QInput into other components without QInput drawing a border around itself or changing its background color:

```vue
<template>
  <div class="q-gutter-y-md column" style="width: 300px; max-width: 100%">
    <q-toolbar class="bg-primary text-white rounded-borders">
      <q-btn round dense flat icon="menu" class="q-mr-xs" />
      <q-avatar class="gt-xs">
        <img
          alt="Quasar logo"
          src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
        />
      </q-avatar>

      <q-space />

      <q-input
        dark
        borderless
        v-model="text"
        input-class="text-right"
        class="q-ml-md"
      >
        <template #append>
          <q-icon v-if="text === ''" name="search" />
          <q-icon
            v-else
            name="clear"
            class="cursor-pointer"
            @click="text = ''"
          />
        </template>
      </q-input>
    </q-toolbar>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Rounded design

The `rounded` prop only works along with Filled, Outlined and Standout designs, as showcased in the example below:

Example "Rounded":

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input rounded filled v-model="text">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input rounded outlined v-model="text">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      rounded
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Square borders

The `square` prop only makes sense along with Filled, Outlined and Standout designs, as showcased in the example below:

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input square filled v-model="text">
      <template #prepend>
        <q-icon name="event" />
      </template>
    </q-input>

    <q-input square outlined v-model="text">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      square
      standout
      bottom-slots
      v-model="text"
      label="Label"
      counter
    >
      <template #prepend>
        <q-icon name="place" />
      </template>
      <template #append>
        <q-icon name="close" @click="text = ''" class="cursor-pointer" />
      </template>

      <template #hint> Field hint </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <div class="q-gutter-y-md column" style="max-width: 300px">
      <div>
        <q-toggle v-model="readonly" label="Readonly" dark />
        <q-toggle v-model="disable" label="Disable" dark />
      </div>

      <q-input dark v-model="text" :readonly="readonly" :disable="disable">
        <template #prepend>
          <q-icon name="event" />
        </template>
      </q-input>

      <q-input
        dark
        filled
        v-model="text"
        :readonly="readonly"
        :disable="disable"
      >
        <template #prepend>
          <q-icon name="event" />
        </template>
      </q-input>

      <q-input
        dark
        outlined
        v-model="text"
        :readonly="readonly"
        :disable="disable"
      >
        <template #append>
          <q-avatar>
            <img
              alt="Quasar logo"
              src="https://cdn.quasar.dev/logo-v2/svg/logo-dark.svg"
            />
          </q-avatar>
        </template>
      </q-input>

      <q-input
        dark
        standout
        bottom-slots
        v-model="text"
        label="Label"
        counter
        :readonly="readonly"
        :disable="disable"
      >
        <template #prepend>
          <q-icon name="place" />
        </template>
        <template #append>
          <q-icon name="close" @click="text = ''" class="cursor-pointer" />
        </template>

        <template #hint> Field hint </template>
      </q-input>

      <q-input
        dark
        borderless
        v-model="text"
        :readonly="readonly"
        :disable="disable"
      >
        <template #append>
          <q-icon name="search" />
        </template>
      </q-input>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const readonly = ref(false)
const disable = ref(false)
</script>
```

## Basic features

### Native attributes

All the attributes set on `QInput` that are not in the list of `props` in the **API** will be passed to the native field (`input` or `textarea`). Some examples: autocomplete, placeholder.

Please check these resources for more information about native attributes (for input check also the specific attributes for each type):

- [input](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input)
- [textarea](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/textarea)

### Clearable

As a helper, you can use `clearable` prop so user can reset model to `null` through an appended icon. The second QInput in the example below is the equivalent of using `clearable`.

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input
      clearable
      filled
      color="purple-12"
      v-model="text"
      label="Label"
    />

    <!-- equivalent -->
    <q-input color="orange" filled v-model="text" label="Label">
      <template v-if="text" #append>
        <q-icon
          name="cancel"
          @click.stop.prevent="text = null"
          class="cursor-pointer"
        />
      </template>
    </q-input>

    <!-- clear-icon -->
    <q-input
      clearable
      clear-icon="close"
      filled
      color="purple-12"
      v-model="text"
      label="Label"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('Some text')
</script>
```

### Input types

The following QInputs make use of the `type` prop in order to render native equivalent `<input type="...">` inside of them.

> [!NOTE]
> Support and behavior is the subject entirely of the browser rendering the page and not Quasar's core code.

```vue
<template>
  <div class="q-gutter-md row items-start">
    <q-input v-model="password" filled type="password" hint="Password" />

    <q-input
      v-model="password"
      filled
      :type="isPwd ? 'password' : 'text'"
      hint="Password with toggle"
    >
      <template #append>
        <q-icon
          :name="isPwd ? 'visibility_off' : 'visibility'"
          class="cursor-pointer"
          @click="isPwd = !isPwd"
        />
      </template>
    </q-input>

    <q-input v-model="email" filled type="email" hint="Email" />

    <q-input v-model="search" filled type="search" hint="Search">
      <template #append>
        <q-icon name="search" />
      </template>
    </q-input>

    <q-input v-model="tel" filled type="tel" hint="Telephone number" />

    <q-input v-model="url" filled type="url" hint="URL" />

    <q-input v-model="time" filled type="time" hint="Native time" />

    <q-input v-model="date" filled type="date" hint="Native date" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const password = ref('')
const isPwd = ref(true)
const email = ref('')
const search = ref('')
const tel = ref('')
const url = ref('')
const time = ref('')
const date = ref('')
</script>
```

> [!TIP]
> Some input types (like `date` or `time`) always render some controls, so you if you're using a `label` then you might want to set it along with `stack-label`, otherwise the label will overlap native browser controls.

#### Input of number type

You'll be using `v-model.number` (notice the `number` modifier) along with `type="number"` prop:

```vue
<template>
  <q-input
    v-model.number="model"
    type="number"
    filled
    style="max-width: 200px"
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref(10)
</script>
```

#### Input of file type

> [!TIP]
> **ALTERNATIVES**
>
> **Instead of using a QInput with `type="file"`, you might want to use [QFile](file.md) picker instead or even [QUploader](uploader.md)**. However, should you wish to use QInput, please read the warning below.

> [!WARNING]
> Do NOT use a `v-model` when QInput is of `type="file"`. Browser security policy does not allow a value to be set to such an input. As a result, you can only read it (attach an `@update:model-value` event), but not write it.

```vue
<template>
  <div class="q-gutter-md row items-start">
    <!--
      Due to browser security policy,
      we can only read the value, but not
      write to it, so we only have an @update:model-value listener
    -->

    <q-input
      @update:model-value="
        val => {
          file = val[0]
        }
      "
      filled
      type="file"
      hint="Native file"
    />

    <q-input
      @update:model-value="
        val => {
          files = val
        }
      "
      multiple
      filled
      type="file"
      hint="Native file (multiple)"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const file = ref(null)
const files = ref(null)
</script>
```

### Textarea

```vue
<template>
  <div style="max-width: 300px">
    <q-input v-model="text" filled type="textarea" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

The line spacing of a textarea follows its font-size, so giving the native field a bigger or smaller font (through `input-class` or `input-style`) keeps the lines evenly spaced.

When you need QInput to grow along with its content, then use the `autogrow` prop like in the example below:

Example "Autogrow":

```vue
<template>
  <div style="max-width: 300px">
    <q-input v-model="text" filled autogrow />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
</script>
```

### Prefix and suffix

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input filled v-model="email" type="email" suffix="@gmail.com">
      <template #before>
        <q-icon name="mail" />
      </template>
    </q-input>

    <q-input outlined v-model.number="number" type="number" prefix="$">
      <template #append>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>
    </q-input>

    <q-input
      standout
      v-model="email"
      type="email"
      prefix="Email:"
      suffix="@gmail.com"
    >
      <template #prepend>
        <q-icon name="mail" />
      </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const email = ref('')
const number = ref(null)
const text = ref('')
</script>
```

### Custom Label

Using the `label` slot you can customize the aspect of the label or add special features as `QTooltip`.

> [!IMPORTANT]
> Do not forget to set the `label-slot` property.
>
> If you want to interact with the content of the label (QTooltip) add the `all-pointer-events` class on the element in the slot.

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input
      filled
      v-model="email"
      suffix="@gmail.com"
      input-class="text-right"
      label-slot
      clearable
    >
      <template #label>
        <div class="row items-center all-pointer-events">
          <q-icon
            class="q-mr-xs"
            color="deep-orange"
            size="24px"
            name="mail"
          />
          Email (hover for more info)

          <q-tooltip
            class="bg-grey-8"
            anchor="top left"
            self="bottom left"
            :offset="[0, 8]"
            >Email address</q-tooltip
          >
        </div>
      </template>
    </q-input>

    <q-input outlined v-model="number" prefix="$" label-slot clearable>
      <template #prepend>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
          />
        </q-avatar>
      </template>

      <template #label>
        <span class="text-weight-bold text-deep-orange">You</span>
        can customize the
        <span
          class="q-px-sm bg-deep-orange text-white text-italic rounded-borders"
          >label</span
        >
      </template>
    </q-input>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const email = ref('john.doe')
const number = ref(123)
</script>
```

### Shadow text

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input
      v-model="inputModel"
      filled
      clearable
      color="purple-12"
      label="Input with shadow text"
      hint="Press TAB to autocomplete suggested value or ESC to cancel suggestion"
      :shadow-text="inputShadowText"
      @keydown="processInputFill"
      @focus="processInputFill"
    />

    <q-input
      v-model="textareaModel"
      filled
      clearable
      autogrow
      color="green-8"
      label="Autogrow textarea with shadow text"
      hint="Press TAB to autocomplete suggested value or ESC to cancel suggestion"
      :shadow-text="textareaShadowText"
      @keydown="processTextareaFill"
      @focus="processTextareaFill"
    />

    <q-input
      v-model="textareaModel"
      filled
      clearable
      type="textarea"
      color="red-12"
      label="Textarea with shadow text"
      hint="Press TAB to autocomplete suggested value or ESC to cancel suggestion"
      :shadow-text="textareaShadowText"
      @keydown="processTextareaFill"
      @focus="processTextareaFill"
    />
  </div>
</template>

<script setup>
import { event } from 'quasar'
import { computed, ref } from 'vue'

const { stopAndPrevent } = event

const inputModel = ref('')
const inputFillCancelled = ref(false)
const inputShadowText = computed(() => {
  if (inputFillCancelled.value) return ''

  const t = 'Text filled when you press TAB'
  const empty =
    typeof inputModel.value !== 'string' || inputModel.value.length === 0

  if (empty) {
    return t
  } else if (t.indexOf(inputModel.value) !== 0) {
    return ''
  }

  return t.split(inputModel.value).slice(1).join(inputModel.value)
})

const textareaModel = ref('')
const textareaFillCancelled = ref(false)
const textareaShadowText = computed(() => {
  if (textareaFillCancelled.value) {
    return ''
  }

  const t = 'This text\nwill be filled\non multiple lines\nwhen you press TAB',
    empty =
      typeof textareaModel.value !== 'string' ||
      textareaModel.value.length === 0

  if (empty) {
    return t.split('\n')[0]
  } else if (t.indexOf(textareaModel.value) !== 0) {
    return ''
  }

  return t
    .split(textareaModel.value)
    .slice(1)
    .join(textareaModel.value)
    .split('\n')[0]
})

function processInputFill(e) {
  if (e === void 0) return

  if (e.keyCode === 27) {
    if (inputFillCancelled.value !== true) {
      inputFillCancelled.value = true
    }
  } else if (e.keyCode === 9) {
    if (
      inputFillCancelled.value !== true &&
      inputShadowText.value.length !== 0
    ) {
      stopAndPrevent(e)
      inputModel.value =
        (typeof inputModel.value === 'string' ? inputModel.value : '') +
        inputShadowText.value
    }
  } else if (inputFillCancelled.value) {
    inputFillCancelled.value = false
  }
}

function processTextareaFill(e) {
  if (e === void 0) return

  if (e.keyCode === 27) {
    if (!textareaFillCancelled.value) {
      textareaFillCancelled.value = true
    }
  } else if (e.keyCode === 9) {
    if (!textareaFillCancelled.value && textareaShadowText.value.length !== 0) {
      stopAndPrevent(e)
      textareaModel.value =
        (typeof textareaModel.value === 'string' ? textareaModel.value : '') +
        textareaShadowText.value
    }
  } else if (textareaFillCancelled.value) {
    textareaFillCancelled.value = false
  }
}
</script>
```

### Slots with QBtn type "submit"

> [!IMPORTANT]
> When placing a QBtn with type "submit" in one of the "before", "after", "prepend", or "append" slots of a QField, QInput or QSelect, you should also add a `@click` listener on the QBtn in question. This listener should call the method that submits your form. All "click" events in such slots are not propagated to their parent elements.

### Debouncing model

The role of debouncing is for times when you watch the model and do expensive operations on it. So you want to first let user type out before triggering the model update, rather than updating the model on each keystroke.

Example "Debounce model":

```vue
<template>
  <div style="max-width: 350px">
    <div class="q-gutter-md">
      <div>
        <q-badge color="teal">Model: "{{ search }}"</q-badge>
      </div>

      <q-input
        v-model="search"
        debounce="500"
        filled
        placeholder="Search"
        hint="Debouncing 500ms"
      >
        <template #append>
          <q-icon name="search" />
        </template>
      </q-input>

      <q-input
        v-model="search"
        debounce="1000"
        filled
        placeholder="Search"
        hint="Debouncing 1000ms"
      >
        <template #append>
          <q-icon name="search" />
        </template>
      </q-input>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const search = ref('')
</script>
```

### Lazy model update

Use the `v-model.lazy` modifier when the model should be updated only after the user finishes editing (on the native `change` event or when the input loses focus) instead of on every keystroke, matching the modifier's behavior on a native `<input>`. While this modifier is in use, the `debounce` prop is ignored.

Example "Lazy model":

```vue
<template>
  <div style="max-width: 350px">
    <div class="q-gutter-md">
      <div>
        <q-badge color="teal">Model: "{{ name }}"</q-badge>
      </div>

      <q-input
        v-model.lazy="name"
        filled
        placeholder="Type, then hit Enter or move focus away"
        hint="Updates the model on change only"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const name = ref('')
</script>
```

### Loading state

```vue
<template>
  <div class="q-gutter-y-md column" style="max-width: 300px">
    <q-input :loading="loadingState" filled v-model="text" label="Label" />
    <q-toggle v-model="loadingState" label="Loading state" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const text = ref('')
const loadingState = ref(false)
</script>
```

## Mask

You can force/help the user to input a specific format with help from `mask` prop.

> [!NOTE]
> Mask is only available if the `type` is one of 'text' (default), 'search', 'url', 'tel', or 'password'.

> [!WARNING]
> **Interaction with `maxlength`**
>
> A mask already caps input at its own slots, so you should not combine it with the `maxlength` prop. The native `maxlength` counts the whole displayed value, literals and fill characters included, so anything below the full masked length blocks typing too early. With `fill-mask` the displayed value always has the mask's full length, so such a `maxlength` locks the field entirely.

Below are the default mask tokens. To add your own, see the next section.

| Token | Description |
| --- | --- |
| `#` | Numeric |
| `S` | Letter, a to z, case insensitive |
| `N` | Alphanumeric, case insensitive for letters |
| `A` | Letter, transformed to uppercase |
| `a` | Letter, transformed to lowercase |
| `X` | Alphanumeric, transformed to uppercase for letters |
| `x` | Alphanumeric, transformed to lowercase for letters |

There are **helpers** for QInput `mask` prop: [full list](https://github.com/quasarframework/quasar/blob/dev/ui/src/components/input/use-mask.js#L6). You can use these for convenience (examples: "phone", "card") or write the string specifying your custom needs.

Example "Basic Mask":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-input
        filled
        v-model="id"
        label="Special ID"
        mask="###/##"
        hint="Mask: ###/##"
      />

      <q-input
        filled
        v-model="phone"
        label="Phone"
        mask="(###) ### - ####"
        hint="Mask: (###) ### - ####"
      />

      <q-input
        filled
        v-model="serialNumber"
        label="Serial number"
        mask="AAAA - #### - #### - SSS"
        hint="Mask: AAAA - #### - #### - SSS"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const id = ref(null)
const phone = ref(null)
const serialNumber = ref(null)
</script>
```

Example "Filling the mask":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-input
        filled
        v-model="id"
        label="Special ID"
        mask="###/##"
        fill-mask
        hint="Mask: ###/##"
      />

      <q-input
        filled
        v-model="phone"
        label="Phone"
        mask="(###) ### - ####"
        fill-mask
        hint="Mask: (###) ### - ####"
      />

      <q-input
        filled
        v-model="card"
        label="Card"
        mask="#### #### #### ####"
        fill-mask="#"
        hint="Mask: #### #### #### ####, FillMask: #"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const id = ref(null)
const phone = ref(null)
const card = ref(null)
</script>
```

You can prevent the mask's own handling of a key — the cursor movement over the mask literals, along with the <kbd>BACKSPACE</kbd> / <kbd>DELETE</kbd> boundary logic — by preventing its `keydown` event. Keep in mind that this also cancels the browser's native handling of that key, so `@keydown.left.prevent` leaves the cursor where it is rather than moving it past the literals; repositioning it is then up to your own handler.

The `unmasked-value` is useful if for example you want to force the user type a certain format, but you want the model to contain the raw value:

Example "Unmasked model":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary">Id model: "{{ id }}"</q-badge>
      <q-input
        filled
        v-model="id"
        label="Special Id"
        mask="###/##"
        unmasked-value
        hint="Mask: ###/##"
      />

      <q-badge color="secondary">Phone model: "{{ phone }}"</q-badge>
      <q-input
        filled
        v-model="phone"
        label="Phone"
        mask="(###) ### - ####"
        unmasked-value
        hint="Mask: (###) ### - ####"
      />

      <q-badge color="secondary">Card model: "{{ card }}"</q-badge>
      <q-input
        filled
        v-model="card"
        label="Card"
        mask="#### - #### - #### - ####"
        fill-mask="#"
        unmasked-value
        hint="Mask: #### - #### - #### - ####, FillMask: #"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const id = ref(null)
const phone = ref(null)
const card = ref(null)
</script>
```

The `reverse-fill-mask` is useful if you want to force the user to fill the mask from the end and allow non-fixed length of input:

Example "Filling the mask in reverse":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-input
        filled
        v-model="price"
        label="Price with 2 decimals"
        mask="#.##"
        fill-mask="0"
        reverse-fill-mask
        hint="Mask: #.##"
        input-class="text-right"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const price = ref(null)
</script>
```

### Multiple masks

When one field must accept several formats (a phone number with 8 or 9 local digits, for example), bind `mask` to a computed property that picks the format from the value's length. Two details make the pattern reliable: use `unmasked-value`, so the plain digit count drives the decision regardless of which mask's literals are currently applied, and give the shorter mask one spare token at its end, so the digit that crosses the threshold can be typed at all; the moment it lands, the computed property switches masks and the value is re-laid out, with the caret staying in place.

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-badge color="secondary">Model: "{{ phone }}"</q-badge>

      <q-input
        filled
        v-model="phone"
        label="Phone (BR)"
        :mask="phoneMask"
        unmasked-value
        hint="8 digits: (##) ####-####, 9 digits: (##) #####-####"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const phone = ref('')

// Brazilian landlines have 8 local digits, mobiles 9; pick the mask
// from the unmasked length. Keeping the model unmasked (unmasked-value)
// is what makes this reliable: the plain digit count decides, unaffected
// by the literals of whichever mask is currently applied.
const phoneMask = computed(() =>
  phone.value !== null && phone.value.length > 10
    ? '(##) #####-####'
    : '(##) ####-#####'
)
</script>
```

### Custom mask tokens *(v2.18.4+)*

You can also define custom mask tokens on top of the default ones or even override some/all of the [default ones](https://github.com/quasarframework/quasar/blob/dev/ui/src/components/input/use-mask.js#L15).

The custom mask tokens must have the same syntax as the [default ones](https://github.com/quasarframework/quasar/blob/dev/ui/src/components/input/use-mask.js#L15). Please note that the `transform` property is optional.

Example "Custom tokens":

```vue
<template>
  <div style="max-width: 300px">
    <div class="q-gutter-md">
      <q-input
        filled
        v-model="id"
        label="Special ID"
        mask="AA-CC-XX-CC"
        :mask-tokens="customTokens"
        hint="Mask: AA-CC-XX-CC, example: BC-12-56-E2"
      />

      <div>C (0-4a-eA-E to uppercase), X override (5-8)</div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const id = ref(null)
const customTokens = {
  C: {
    pattern: '[0-4a-eA-E]',
    negate: '[^0-4a-eA-E]',
    transform: v => v.toLocaleUpperCase()
  },
  X: { pattern: '[5-8]', negate: '[^5-8]' }
}
</script>
```

### Using third party mask processors

You can easily use any third party mask processor by doing a few small adjustments to your QInput.

Starting from a QInput like this:

```html
<q-input
  filled
  v-model="price"
  label="Price with 2 decimals"
  mask="#.##"
  fill-mask="#"
  reverse-fill-mask
  hint="Mask: #.00"
  input-class="text-right"
/>
```

You can use v-money directive:

```html
<q-field
  filled
  v-model="price"
  label="Price with v-money directive"
  hint="Mask: $ #,###.00 #"
>
  <template #control="{ id, floatingLabel, modelValue, emitValue }">
    <input
      :id="id"
      class="q-field__input text-right"
      :value="modelValue"
      @change="e => emitValue(e.target.value)"
      v-money="moneyFormatForDirective"
      v-show="floatingLabel"
    />
  </template>
</q-field>
```

```js
moneyFormatForDirective: {
  decimal: '.',
  thousands: ',',
  prefix: '$ ',
  suffix: ' #',
  precision: 2,
  masked: false /* doesn't work with directive */
}
```

Or you can use money component:

```html
<q-field
  filled
  v-model="price"
  label="Price with v-money component"
  hint="Mask: $ #,###.00 #"
>
  <template #control="{ id, floatingLabel, modelValue, emitValue }">
    <money
      :id="id"
      class="q-field__input text-right"
      :model-value="modelValue"
      @update:model-value="emitValue"
      v-bind="moneyFormatForComponent"
      v-show="floatingLabel"
    />
  </template>
</q-field>
```

```js
moneyFormatForComponent: {
  decimal: '.',
  thousands: ',',
  prefix: '$ ',
  suffix: ' #',
  precision: 2,
  masked: true
}
```

## Validation

### Internal validation

You can validate QInput components with `:rules` prop. Specify array of embedded rules or your own validators. Your custom validator will be a function which returns `true` if validator succeeds or `String` with error message if it doesn't succeed.

> [!NOTE]
> By default, for perf reasons, a change in the rules does not trigger a new validation until the model changes. In order to trigger the validation when rules change too, then use `reactive-rules` Boolean prop. The downside is a performance penalty (so use it when you really need this only!) and it can be slightly mitigated by using a computed prop as value for the rules (and not specify them inline in the vue template).

This is so you can write convenient rules of shape like:

```js
value => condition || errorMessage

// example:
value => value.includes('Hello') || 'Field must contain word Hello'
```

You can reset the validation by calling `resetValidation()` method on the QInput.

There are **helpers** for QInput `rules` prop: [full list](https://github.com/quasarframework/quasar/blob/dev/ui/src/utils/patterns/patterns.js). You can use these for convenience (examples: "date", "time", "hexColor", "rgbOrRgbaColor", "anyColor") or write the string specifying your custom needs.

> [!WARNING]
> **Native constraints are separate from rules**
>
> Native HTML constraints (a `type` like "email" or "url", or a `pattern`/`required` attribute passed through to the native input) are enforced by the browser only on a native form submission. The programmatic `validate()` method (on QInput or on a wrapping QForm) evaluates the `rules` only and does not consult them. Express any constraint that `validate()` should catch as a rule too, e.g. `:rules="['email']"`.

Example "Validation required":

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      ref="inputRef"
      filled
      v-model="model"
      label="Required Field"
      :rules="[val => !!val || 'Field is required']"
    />

    <q-btn
      class="q-mt-sm"
      label="Reset Validation"
      @click="reset"
      color="primary"
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const inputRef = useTemplateRef('inputRef')
const model = ref('')

function reset() {
  inputRef.value.resetValidation()
}
</script>
```

Example "Maximum length":

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      ref="inputRef"
      filled
      v-model="model"
      label="Maximum 3 characters"
      :rules="[val => val.length <= 3 || 'Please use maximum 3 characters']"
    />

    <q-btn
      class="q-mt-sm"
      label="Reset Validation"
      @click="reset"
      color="primary"
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const inputRef = useTemplateRef('inputRef')
const model = ref('')

function reset() {
  inputRef.value.resetValidation()
}
</script>
```

If you set `lazy-rules`, validation triggers when the field loses focus (a `readonly` field included, only a `disable`d field is exempt from validation); while an error is displayed, the field re-validates on each change so the error clears as soon as the value becomes valid. A menu or dialog opened from inside the field (a QPopupProxy in the `append` slot, for instance) keeps the field focused for as long as it is open, so it does not count as losing focus. If `lazy-rules` is set to `ondemand` String, then validation will be triggered only when component's validate() method is manually called or when the wrapper QForm submits itself.

Example "Lazy rules":

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      ref="inputRef"
      filled
      v-model="model"
      label="Required field with length < 2"
      hint="Validation starts after first blur"
      counter
      :rules="[
        val => !!val || '* Required',
        val => val.length < 2 || 'Please use maximum 1 character'
      ]"
      lazy-rules
    />

    <q-btn
      class="q-mt-sm"
      label="Reset Validation"
      @click="reset"
      color="primary"
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const inputRef = useTemplateRef('inputRef')
const model = ref('')

function reset() {
  inputRef.value.resetValidation()
}
</script>
```

Example "Form validation":

```vue
<template>
  <div style="max-width: 300px">
    <form
      @submit.prevent.stop="onSubmit"
      @reset.prevent.stop="onReset"
      class="q-gutter-md"
    >
      <q-input
        ref="nameRef"
        filled
        v-model="name"
        label="Your name *"
        hint="Name and surname"
        lazy-rules
        :rules="nameRules"
      />

      <q-input
        ref="ageRef"
        filled
        type="number"
        v-model.number="age"
        label="Your age *"
        lazy-rules
        :rules="ageRules"
      />

      <q-toggle v-model="accept" label="I accept the license and terms" />

      <div>
        <q-btn label="Submit" type="submit" color="primary" />
        <q-btn
          label="Reset"
          type="reset"
          color="primary"
          flat
          class="q-ml-sm"
        />
      </div>
    </form>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { ref, useTemplateRef } from 'vue'

const $q = useQuasar()

const name = ref(null)
const nameRef = useTemplateRef('nameRef')
const nameRules = [val => (val && val.length !== 0) || 'Please type something']

const age = ref(null)
const ageRef = useTemplateRef('ageRef')
const ageRules = [
  val => (val !== null && val !== '') || 'Please type your age',
  val => (val > 0 && val < 100) || 'Please type a real age'
]

const accept = ref(false)

function onSubmit() {
  nameRef.value.validate()
  ageRef.value.validate()

  if (nameRef.value.hasError || ageRef.value.hasError) {
    // form has error
  } else if (accept.value !== true) {
    $q.notify({
      color: 'negative',
      message: 'You need to accept the license and terms first'
    })
  } else {
    $q.notify({
      icon: 'done',
      color: 'positive',
      message: 'Submitted'
    })
  }
}

function onReset() {
  name.value = null
  age.value = null

  nameRef.value.resetValidation()
  ageRef.value.resetValidation()
}
</script>
```

#### Async rules

Rules can be async too, by using async/await or by directly returning a Promise. If the value changes or the field gets blurred while an async validation is still in flight, the field re-validates once it settles, so the displayed verdict always matches the current value.

> [!TIP]
> Consider coupling async rules with `debounce` prop to avoid calling the async rules immediately on each keystroke, which might be detrimental to performance.

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      ref="inputRef"
      filled
      v-model="model"
      label="Required Field *"
      :rules="[myRule]"
    />

    <q-btn
      class="q-mt-sm"
      label="Reset Validation"
      @click="reset"
      color="primary"
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const inputRef = useTemplateRef('inputRef')
const model = ref('')

function myRule(val) {
  // simulating a delay

  return new Promise(resolve => {
    setTimeout(() => {
      // call
      //  resolve(true)
      //     --> content is valid
      //  resolve(false)
      //     --> content is NOT valid, no error message
      //  resolve(error_message)
      //     --> content is NOT valid, we have error message
      resolve(Boolean(val) || '* Required')

      // calling reject(...) will also mark the input
      // as having an error, but there will not be any
      // error message displayed below the input
      // (only in browser console)
    }, 1000)
  })
}

function reset() {
  inputRef.value.resetValidation()
}
</script>
```

### External validation

You can also use external validation and only pass `error` and `error-message` (enable `bottom-slots` to display this error message).

> [!TIP]
> Depending on your needs, you might connect [Regle](https://reglejs.dev/) (our recommended approach) or some other validation library to QInput.

Example "External":

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      filled
      v-model="model"
      label="Type here"
      bottom-slots
      hint="Max 3 characters"
      error-message="Please use maximum 3 characters"
      :error="!isValid"
    />
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const model = ref('')
const isValid = computed(() => model.value.length <= 3)
</script>
```

You can also customize the slot for error message:

Example "Slot for error message":

```vue
<template>
  <div style="max-width: 300px">
    <q-input
      filled
      v-model="model"
      label="Type here"
      bottom-slots
      :error="!isValid"
      hint="Max 3 characters"
    >
      <template #error> Please use maximum 3 characters. </template>
    </q-input>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const model = ref('')
const isValid = computed(() => model.value.length <= 3)
</script>
```

## Accessibility *(v2.25+)*

QInput renders a native `<input>` (or `<textarea>`) inside the QField frame, so everything described in [QField's Accessibility section](field.md#accessibility) applies here: the label association through a generated SSR-safe id, error messages announced with `role="alert"` and referenced from the control through `aria-invalid`/`aria-errormessage`/`aria-describedby`, and the keyboard-operable clear button.

The `label` prop is additionally exposed as `aria-label` on the native element — an `aria-label` or `aria-labelledby` attribute you set yourself takes precedence — while `disable` and `readonly` map to the native `disabled` and `readonly` attributes (so a readonly input stays in the Tab order and shows the field's focused state when reached, as described on the QField page). Any other native attributes (`placeholder`, `autocomplete`, `inputmode`, ...) fall through to the native element as well.

## Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QInput, otherwise formData will not contain it (if it should):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-input
      name="name"
      autocomplete="name"
      v-model="name"
      color="primary"
      label="Full name"
      filled
      clearable
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

const submitResult = ref([])
const name = ref('Jane Doe')

function onSubmit(evt) {
  const formData = new FormData(evt.target)
  const data = []

  for (const [key, value] of formData.entries()) {
    data.push({
      name: key,
      value
    })
  }

  submitResult.value = data
}
</script>
```
