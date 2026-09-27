---
title: Rating
---
Quasar Rating is a Component which allows users to rate items, usually known as “Star Rating”.

## QRating API

Not inlined here: call the `get_api` tool with `name: "QRating"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating v-model="ratingModel" size="1.5em" icon="thumb_up" />
    <q-rating
      v-model="ratingModel"
      size="2em"
      color="red-7"
      icon="favorite_border"
    />
    <q-rating
      v-model="ratingModel"
      size="2.5em"
      color="purple-4"
      icon="create"
    />
    <q-rating v-model="ratingModel" size="3em" color="brown-5" icon="pets" />
    <q-rating
      v-model="ratingModel"
      size="3.5em"
      color="green-5"
      icon="star_border"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

Example "Custom number of choices":

```vue
<template>
  <q-rating v-model="ratingModel" size="2em" :max="10" color="primary" />
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

### Accessibility *(v2.25+)*

QRating exposes the [WAI-ARIA radio group pattern](https://www.w3.org/WAI/ARIA/apg/patterns/radio/): the component is a `radiogroup` whose stars are `role="radio"` elements, with `aria-checked` set on the star matching the current model and a roving tabindex (the selected star — or the first one — is the group's single Tab stop). Each star's accessible name comes from the `icon-aria-label` prop (a string gets the star's value appended, an array names each star individually; the icon name is the fallback), and `aria-readonly` / `aria-disabled` are set on the group when applicable.

Note that half values (see "Floating number" below) are not conveyed to screen readers: `aria-checked="true"` requires an exact integer match, so a model of e.g. 3.5 announces no star as checked.

#### Keyboard navigation

QRating uses radio-group keyboard behavior:

- <kbd>Space</kbd> or <kbd>Enter</kbd> update the model.
- <kbd>Arrow Right</kbd> and <kbd>Arrow Down</kbd> move focus to the next value (<kbd>Space</kbd>/<kbd>Enter</kbd> required to be pressed to update the model).
- <kbd>Arrow Left</kbd> and <kbd>Arrow Up</kbd> move focus to the previous value (<kbd>Space</kbd>/<kbd>Enter</kbd> required to be pressed to update the model).

### Icons

Example "Image icons":

```vue
<template>
  <q-rating
    v-model="ratingModel"
    size="3.5em"
    icon="img:https://cdn.quasar.dev/logo-v2/svg/logo.svg"
  />
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

In the example below, when using the `icon-selected` prop, notice we can still use `icon` as well. The latter becomes the icon(s) when they are not selected.

Example "Different icon when selected":

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating
      v-model="ratingModel"
      size="3.5em"
      color="green-5"
      icon="star_border"
      icon-selected="star"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

Example "Different icon for each rating":

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating
      v-model="ratingModel"
      :max="4"
      size="3.5em"
      color="green-5"
      :icon="icons"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
const icons = [
  'sentiment_very_dissatisfied',
  'sentiment_dissatisfied',
  'sentiment_satisfied',
  'sentiment_very_satisfied'
]
</script>
```

### Colors

When using the `color-selected` prop, notice we can still use `color` as well. The latter becomes the color(s) of the icons when they are not selected.

Example "Different color for each rating":

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating
      v-model="ratingModel"
      size="3.5em"
      color="grey"
      :color-selected="ratingColors"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(4)
const ratingColors = [
  'light-green-3',
  'light-green-6',
  'green',
  'green-9',
  'green-10'
]
</script>
```

### Floating number

Example "Different icon and color when half selected":

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating
      v-model="model1"
      max="7"
      size="3em"
      color="green-5"
      icon="star_border"
      icon-selected="star"
      icon-half="star_half"
    />

    <q-rating
      v-model="model2"
      max="7"
      size="3em"
      color="yellow"
      icon="star_border"
      icon-selected="star"
      icon-half="star_half"
      no-dimming
    />

    <q-rating
      v-model="model3"
      max="7"
      size="3em"
      color="red"
      color-selected="red-9"
      icon="favorite_border"
      icon-selected="favorite"
      icon-half="favorite"
      no-dimming
    />

    <div>
      <q-btn color="grey" no-caps label="Reset" @click="resetModels" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const model1 = ref(3.5)
const model2 = ref(2.3)
const model3 = ref(4.5)

function resetModels() {
  model1.value = 3.5
  model2.value = 2.3
  model3.value = 4.5
}
</script>
```

### No dimming

```vue
<template>
  <q-rating
    v-model="model"
    max="5"
    size="3.5em"
    color="yellow"
    icon="star_border"
    icon-selected="star"
    icon-half="star_half"
    no-dimming
  />
</template>

<script setup>
import { ref } from 'vue'

const model = ref(2.3)
</script>
```

### Tooltips

Notice how we can add tooltips to each icon in the example below.

Example "With QTooltip":

```vue
<template>
  <q-rating v-model="ratingModel" size="2em" :max="3" color="primary">
    <template #tip-1>
      <q-tooltip>Not bad</q-tooltip>
    </template>
    <template #tip-2>
      <q-tooltip>Good</q-tooltip>
    </template>
    <template #tip-3>
      <q-tooltip>Very good!</q-tooltip>
    </template>
  </q-rating>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(2)
</script>
```

### Sizes

Apart from the standard sizes below, you can define your own through the `size` property.

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating
      v-for="size in ['xs', 'sm', 'md', 'lg', 'xl']"
      :key="size"
      :size="size"
      v-model="ratingModel"
      icon="stars"
      color="primary"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

### Readonly and disable

```vue
<template>
  <div class="q-gutter-y-md column">
    <q-rating v-model="ratingModel" size="2em" color="orange" readonly />

    <q-rating v-model="ratingModel" size="2em" color="purple" disable />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ratingModel = ref(3)
</script>
```

### Native form submit

When dealing with a native form which has an `action` and a `method` (eg. when using Quasar with ASP.NET controllers), you need to specify the `name` property on QRating, otherwise formData will not contain it (if it should):

Example "Native form":

```vue
<template>
  <q-form @submit="onSubmit" class="q-gutter-md">
    <q-rating
      name="quality"
      v-model="quality"
      max="5"
      size="3.5em"
      color="yellow"
      icon="star_border"
      icon-selected="star"
      no-dimming
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

const quality = ref(3)
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
