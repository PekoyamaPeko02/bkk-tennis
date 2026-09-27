---
title: Pagination
---
The QPagination component is available for whenever a pagination system is required. It offers the user a simple UI for moving between items or pages.

There are two modes in which QPagination operates: with buttons only or with an inputbox. The latter allows the user to go to a specific page by clicking/tapping on the inputbox, typing the page number then hitting Enter key. If the new page number is within valid limits, the model will be changed accordingly.

## QPagination API

Not inlined here: call the `get_api` tool with `name: "QPagination"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Design

Example "Standard":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination v-model="current" :max="5" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

The following are a few examples, but not an exhaustive list:

Example "Button design":

```vue
<template>
  <div class="q-gutter-md">
    <q-pagination
      v-model="current"
      max="5"
      direction-links
      flat
      color="grey"
      active-color="primary"
    />

    <q-pagination
      v-model="current"
      max="5"
      direction-links
      outline
      color="orange"
      active-design="unelevated"
      active-color="brown"
      active-text-color="orange"
    />

    <q-pagination
      v-model="current"
      max="5"
      direction-links
      push
      color="teal"
      active-design="push"
      active-color="orange"
    />

    <q-pagination
      v-model="current"
      :max="5"
      direction-links
      unelevated
      color="black"
      active-color="purple"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

Example "Gutter":

```vue
<template>
  <div class="q-gutter-md">
    <q-pagination v-model="current" max="5" direction-links />

    <q-pagination v-model="current" max="5" direction-links gutter="sm" />

    <q-pagination v-model="current" max="5" direction-links gutter="md" />

    <q-pagination v-model="current" max="5" direction-links gutter="20px" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(2)
</script>
```

### Custom icons

Example "With icon replacement":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      :max="5"
      direction-links
      boundary-links
      icon-first="skip_previous"
      icon-last="skip_next"
      icon-prev="fast_rewind"
      icon-next="fast_forward"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

### With input

```vue
<template>
  <div class="flex flex-center">
    <q-pagination v-model="current" :max="5" input />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

Example "With input color":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      :max="5"
      input
      input-class="text-orange-10"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

### Max pages shown

Example "Maximum pages shown":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      color="black"
      :max="10"
      :max-pages="6"
      :boundary-numbers="false"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(5)
</script>
```

Example "Removing ellipses":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      color="teal"
      :max="10"
      :max-pages="5"
      :ellipses="false"
      :boundary-numbers="false"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(5)
</script>
```

### Handling boundary

Example "With boundary numbers":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      color="purple"
      :max="10"
      :max-pages="6"
      boundary-numbers
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(6)
</script>
```

Example "With boundary links":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      color="deep-orange"
      :max="5"
      boundary-links
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

Example "With direction links":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination v-model="current" :max="5" direction-links />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const current = ref(3)
</script>
```

### Custom ellipsis *(v2.30+)*

The `ellipsis` slot replaces the "..." buttons. Spread its `btnProps` onto your own QBtn to keep the default look, then attach whatever behavior you need: below, a [QPopupEdit](popup-edit.md) lets the user type the page number instead of jumping to the next hidden page. Bind the slot's `onClick` (or `to` when using `to-fn`) if you also want the default navigation.

Example "Go to page with QPopupEdit":

```vue
<template>
  <div class="flex flex-center">
    <q-pagination
      v-model="current"
      color="teal"
      :max="max"
      :max-pages="5"
      boundary-numbers
    >
      <template #ellipsis="{ btnProps }">
        <q-btn v-bind="btnProps" aria-label="Go to page">
          <q-popup-edit
            v-model="current"
            title="Go to page"
            :cover="false"
            :offset="[0, 8]"
            :validate="validatePage"
            #default="scope"
          >
            <q-input
              v-model.number="scope.value"
              type="number"
              :min="1"
              :max="max"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-btn>
      </template>
    </q-pagination>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const max = 20
const current = ref(10)

function validatePage(page) {
  return Number.isInteger(page) && page >= 1 && page <= max
}
</script>
```

### Localized digits *(v2.31+)*

The page numbers follow the `formatNumber` function of the active [language pack](../options/quasar-language-packs.md), when it defines one (see [QDate's localized digits](date.md#localized-digits)). The `fa` and `fa-IR` packs render Persian digits. The model and the `input` mode stay numeric.

## Accessibility *(v2.25+)*

QPagination renders as a `navigation` landmark. The first/previous/next/last buttons get localized `aria-label`s from the [Quasar Language Pack](../options/quasar-language-packs.md) in use, the numbered buttons are labeled with the page they lead to, and the active page's button is marked with `aria-current="page"`. The landmark itself is named from the same language pack (`pagination.label`); pass your own `aria-label` (it falls through to the root element) to tell several paginations on one page apart. It also carries `aria-disabled` at all times, reporting `true` or `false` according to the `disable` prop.

In input mode, the typed page number is committed when the user hits <kbd>Enter</kbd> or when the field loses focus.
