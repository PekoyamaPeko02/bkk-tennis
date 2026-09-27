---
title: Splitter
related:
  - title: Expansion Item
    path: expansion-item.md
  - title: Slide Item
    path: slide-item.md
  - title: Separator
    path: separator.md
---
The QSplitter component allow containers to be split vertically and/or horizontally through a draggable separator bar.

## QSplitter API

Not inlined here: call the `get_api` tool with `name: "QSplitter"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> The use of the `before` and `after` slots is required.

Click and drag on the splitter separator bar to see results.

### Basic

```vue
<template>
  <q-splitter v-model="splitterModel" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

### Horizontal

```vue
<template>
  <q-splitter v-model="splitterModel" horizontal style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

### Custom dragging limits

Example "Custom dragging limits (50-100)":

```vue
<template>
  <q-splitter
    v-model="splitterModel"
    :limits="[50, 100]"
    style="height: 400px"
  >
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

### Model units

By default, the CSS `unit` used is '%' (percentage). But you can also use 'px' (pixels), as in the example below.

Example "Model in pixels":

```vue
<template>
  <q-splitter v-model="splitterModel" unit="px" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(150) // start at 150px
</script>
```

### Reverse model

By default, the model is connected to the `before` slot size. But you can reverse that and make it connect to the `after` slot, as in the example below. This feature turns out especially useful if your `unit` is set to pixels and you want to control the `after` slot.

```vue
<template>
  <q-splitter v-model="splitterModel" reverse unit="px" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(150) // start at 150px
</script>
```

### Adding content to separator

> [!TIP]
> If you use images as content for the separator slot, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

Example "Adding to separator":

```vue
<template>
  <q-splitter v-model="splitterModel" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #separator>
      <q-avatar
        color="primary"
        text-color="white"
        size="40px"
        icon="drag_indicator"
      />
    </template>

    <template #after>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">After</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

### Dark design

Example "On a dark background with customized separator":

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-splitter
      v-model="splitterModel"
      separator-class="bg-orange"
      separator-style="width: 3px"
      style="height: 400px"
    >
      <template #before>
        <div class="q-pa-md">
          <div class="text-h4 q-mb-md">Before</div>
          <div v-for="n in 20" :key="n" class="q-my-md"
            >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
            Quis praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</div
          >
        </div>
      </template>

      <template #after>
        <div class="q-pa-md">
          <div class="text-h4 q-mb-md">After</div>
          <div v-for="n in 20" :key="n" class="q-my-md"
            >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
            Quis praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</div
          >
        </div>
      </template>
    </q-splitter>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

### Embedded

A QSplitter can be embedded in another QSplitter's `before` and/or `after` slots, like shown in example below.

```vue
<template>
  <q-splitter v-model="splitterModel" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <div class="text-h4 q-mb-md">Before</div>
        <div v-for="n in 20" :key="n" class="q-my-md"
          >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing elit.
          Quis praesentium cumque magnam odio iure quidem, quod illum numquam
          possimus obcaecati commodi minima assumenda consectetur culpa fuga
          nulla ullam. In, libero.</div
        >
      </div>
    </template>

    <template #after>
      <q-splitter v-model="insideModel" horizontal>
        <template #before>
          <div class="q-pa-md">
            <div class="text-h4 q-mb-md">Before</div>
            <div v-for="n in 20" :key="n" class="q-my-md"
              >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing
              elit. Quis praesentium cumque magnam odio iure quidem, quod
              illum numquam possimus obcaecati commodi minima assumenda
              consectetur culpa fuga nulla ullam. In, libero.</div
            >
          </div>
        </template>

        <template #after>
          <div class="q-pa-md">
            <div class="text-h4 q-mb-md">After</div>
            <div v-for="n in 20" :key="n" class="q-my-md"
              >{{ n }}. Lorem ipsum dolor sit, amet consectetur adipisicing
              elit. Quis praesentium cumque magnam odio iure quidem, quod
              illum numquam possimus obcaecati commodi minima assumenda
              consectetur culpa fuga nulla ullam. In, libero.</div
            >
          </div>
        </template>
      </q-splitter>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
const insideModel = ref(50)
</script>
```

### Fun examples

Example "Image Fun":

```vue
<template>
  <div class="overflow-hidden">
    <q-resize-observer @resize="onResize" :debounce="0" />

    <q-splitter
      id="photos"
      v-model="splitterModel"
      :limits="[0, 100]"
      :style="splitterStyle"
      before-class="overflow-hidden"
      after-class="overflow-hidden"
    >
      <template #before>
        <img
          alt="Landscape photo"
          src="https://cdn.quasar.dev/img/parallax1.jpg"
          :width="width"
          class="absolute-top-left"
        />
      </template>

      <template #after>
        <img
          alt="Landscape photo in black and white"
          src="https://cdn.quasar.dev/img/parallax1-bw.jpg"
          :width="width"
          class="absolute-top-right"
        />
      </template>
    </q-splitter>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const width = ref(400)
const splitterModel = ref(50) // start at 50%

const splitterStyle = computed(() => ({
  height: Math.min(600, 0.66 * width.value) + 'px',
  width: width.value + 'px'
}))

function onResize(info) {
  width.value = info.width
}
</script>
```

Example "Reactive Images":

```vue
<template>
  <q-splitter
    v-model="splitterModel"
    style="height: 300px"
    :limits="[0, 100]"
    before-class="overflow-hidden"
    after-class="overflow-hidden"
    separator-class="bg-black"
  >
    <template #before>
      <q-img src="https://cdn.quasar.dev/img/parallax1.jpg" :ratio="16 / 9" />
    </template>

    <template #after>
      <q-img
        src="https://cdn.quasar.dev/img/parallax1-inverted.jpg"
        :ratio="16 / 9"
      />
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50) // start at 50%
</script>
```

## Accessibility *(v2.25+)*

The separator bar implements the [WAI-ARIA window splitter pattern](https://www.w3.org/WAI/ARIA/apg/patterns/windowsplitter/): it carries `role="separator"` with an `aria-orientation` matching the splitter's direction, `aria-controls` pointing at the panel the model resizes, and `aria-valuemin`/`aria-valuemax`/`aria-valuenow` tracking the split as it moves. A disabled QSplitter exposes `aria-disabled` on the separator and removes it from the Tab order.

Its accessible name defaults to the `label.resize` entry of the [Quasar Language Pack](../options/quasar-language-packs.md), since a separator's children are presentational in ARIA — whatever you put in the `separator` slot can never name it. Use the `separator-aria-label` prop (v2.25+) to replace that generic name with one that says which panels are being resized, which is what you want as soon as a page holds more than one splitter.

### Keyboard navigation

QSplitter follows the [WAI-ARIA window splitter pattern](https://www.w3.org/WAI/ARIA/apg/patterns/windowsplitter/): the separator bar is a Tab stop exposed to assistive technology as a `separator` with the model as its value. While it has focus, the arrow keys matching the splitter's orientation (left/right, or up/down when in `horizontal` mode) move it by 1% (or 10px when `unit` is set to pixels), while <kbd>Home</kbd>/<kbd>End</kbd> jump to the model's limits. Arrow keys account for the `reverse` prop and RTL language packs, so a given key always moves the separator in the direction it points to. Pressing <kbd>Enter</kbd> collapses the model-controlled panel to its minimum limit, and pressing it again restores the previous position.
