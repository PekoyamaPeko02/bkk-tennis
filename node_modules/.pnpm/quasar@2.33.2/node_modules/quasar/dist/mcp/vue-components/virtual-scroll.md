---
title: Virtual Scroll
related:
  - title: Intersection
    path: intersection.md
  - title: Infinite Scroll
    path: infinite-scroll.md
---
The QVirtualScroll component allows you to display only a part of a long list of items and update the visible items as the user scrolls in the container. This has several advantages: only visible items are rendered, so the smallest number of nodes are in the DOM tree at any given point in time and the memory consumption is kept at its lowest.

There are currently two types of QVirtualScroll: "list" (using QItems) and "table" (using a tabular style to display rows of data). The props of the underlying component ([QList](list-and-list-items.md) for "list", [QMarkupTable](markup-table.md) for "table") are passed through via this component.

## QVirtualScroll API

Not inlined here: call the `get_api` tool with `name: "QVirtualScroll"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!TIP]
>
> - (Composition API) To get the best performance while using large lists, do not wrap the array that you are passing in the `items` prop with ref()/computed()/reactive()/etc. This allows Vue to skip making the list "responsive" to changes.
> - (Options API) To get the best performance while using large lists, freeze the array that you are passing in the `items` prop using `Object.freeze(items)`. This allows Vue to skip making the list "responsive" to changes.
> - The number of items that will be rendered will be calculated based on the `virtual-scroll-item-size` prop and the size of the scrollable area, but you can fit it to your needs using the `virtual-scroll-slice-size` prop.
> - Use the `virtual-scroll-item-size` to specify the size of elements (pixels of height, or width if horizontal). After an element is rendered on screen its size is updated automatically, but if you specify an element size close to the real size you'll get a better initial indication of the scroll position. Regardless if you will be using this property or not, QVirtualScroll will still work, but without it you may experience the scrollbar not following the mouse grab position while continuously scrolling (on desktop) or the actual scroll of the container getting slightly off by one or two elements when on mobile and continuously scrolling.

> [!WARNING]
> Each browser imposes a maximum height on the scrolling container (Chromium caps it at 2^24 CSS pixels, roughly 16.7 million), so very large lists still hit a limit.

> [!IMPORTANT]
> **SSR**
>
> On SSR, the initial window of items (sized by `virtual-scroll-slice-size`, since the viewport cannot be measured on the server) is rendered into the server payload; the real viewport re-slices after hydration. Your item template therefore runs on the server too, so keep it SSR-safe — the same requirement any server-rendered content has. This also applies to QTable's `virtual-scroll` mode.

Scroll the examples below to see QVirtualScroll in action.

### Basic

```vue
<template>
  <q-virtual-scroll
    style="max-height: 300px"
    :items="heavyList"
    separator
    #default="{ item, index }"
  >
    <q-item :key="index" dense>
      <q-item-section>
        <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
      </q-item-section>
    </q-item>
  </q-virtual-scroll>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1)
  })
}
</script>
```

### Horizontal

```vue
<template>
  <q-virtual-scroll
    :items="heavyList"
    virtual-scroll-horizontal
    #default="{ item, index }"
  >
    <div :key="index" :class="item.class">
      #{{ index }} - {{ item.label }}
    </div>
  </q-virtual-scroll>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1),
    class:
      i % 2 === 0
        ? 'q-pa-md self-center bg-grey-2 text-black'
        : 'q-pa-lg bg-black text-white'
  })
}
</script>
```

### Different templates

Example "Different templates for items":

```vue
<template>
  <q-virtual-scroll
    style="max-height: 300px"
    :items="heavyList"
    separator
    #default="{ item, index }"
  >
    <q-banner
      v-if="item.banner"
      class="bg-black text-white q-py-xl"
      :key="'a' + index"
    >
      #{{ index }} - {{ item.label }}
    </q-banner>

    <q-item v-else :key="'b' + index" dense clickable>
      <q-item-section>
        <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
      </q-item-section>
    </q-item>
  </q-virtual-scroll>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1),
    banner: i % 5 === 0
  })
}
</script>
```

Example "Different templates for horizontal items":

```vue
<template>
  <q-virtual-scroll
    :items="heavyList"
    virtual-scroll-horizontal
    #default="{ item, index }"
  >
    <div :key="index" class="row items-center">
      <q-separator v-if="index === 0" vertical spaced />

      <q-avatar v-if="item.avatar" class="bg-black text-white q-my-md">
        {{ (index % 10) + 1 }}
      </q-avatar>

      <q-item v-else dense clickable>
        <q-item-section>
          <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
        </q-item-section>
      </q-item>

      <q-separator vertical spaced />
    </div>
  </q-virtual-scroll>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1),
    avatar: i % 5 === 0
  })
}
</script>
```

### Table type

Notice the `type="table"` property.

Example "Basic table":

```vue
<template>
  <q-virtual-scroll
    type="table"
    style="max-height: 70vh"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    :virtual-scroll-sticky-size-end="32"
    :items="heavyList"
    #default="{ item: row, index }"
  >
    <tr :key="index">
      <td>#{{ index }}</td>
      <td v-for="col in columns" :key="index + '-' + col">
        {{ row[col] }}
      </td>
    </tr>
  </q-virtual-scroll>
</template>

<script setup>
const rows = [
  // ...
]

const columns = [
  // ...
]

const heavyList = []

// adding same data multiple times to
// create a huge list
for (let i = 0; i <= 1000; i++) {
  Array.prototype.push.apply(heavyList, rows)
}
</script>
```

With header that scrolls along with content (doesn't stay in place).

Example "Table with scrolling header/footer":

```vue
<template>
  <q-virtual-scroll
    type="table"
    style="max-height: 70vh"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    :virtual-scroll-sticky-size-end="32"
    :items="heavyList"
  >
    <template #before>
      <thead class="text-left">
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'1--' + col.name">
            {{ col.name }}
          </th>
        </tr>
      </thead>
    </template>

    <template #after>
      <tfoot class="text-left">
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'2--' + col.name">
            {{ col.name }}
          </th>
        </tr>
      </tfoot>
    </template>

    <template #default="{ item: row, index }">
      <tr :key="index">
        <td>#{{ index }}</td>
        <td v-for="column in columns" :key="index + '-' + column.name">
          {{ row[column.prop] }}
        </td>
      </tr>
    </template>
  </q-virtual-scroll>
</template>

<script setup>
const rows = [
  // ...
]

const columns = [
  // ...
]

const heavyList = []

// adding same data multiple times to
// create a huge list
for (let i = 0; i <= 1000; i++) {
  Array.prototype.push.apply(heavyList, rows)
}
</script>
```

Notice (in the example below) the CSS required to make the table header and footer "sticky". Also note the additional scoped slots which define the header and footer content.

Example "Sticky headers table":

```vue
<template>
  <q-virtual-scroll
    type="table"
    style="max-height: 70vh"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    :virtual-scroll-sticky-size-end="32"
    :items="heavyList"
  >
    <template #before>
      <thead class="thead-sticky text-left">
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'1--' + col.name">
            {{ col.name }}
          </th>
        </tr>
      </thead>
    </template>

    <template #after>
      <tfoot class="tfoot-sticky text-left">
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'2--' + col.name">
            {{ col.name }}
          </th>
        </tr>
      </tfoot>
    </template>

    <template #default="{ item: row, index }">
      <tr :key="index">
        <td>#{{ index }}</td>
        <td v-for="col in columns" :key="index + '-' + col.name">
          {{ row[col.prop] }}
        </td>
      </tr>
    </template>
  </q-virtual-scroll>
</template>

<style lang="sass">
.thead-sticky tr > *,
.tfoot-sticky tr > *
  position: sticky
  opacity: 1
  z-index: 1
  background: black
  color: white
  /* covers any sub-pixel gap between sticky cells */
  box-shadow: -1px 0 0 black

.thead-sticky tr:last-child > *
  top: 0

.tfoot-sticky tr:first-child > *
  bottom: 0
</style>

<script setup>
const rows = [
  // ...
]

const columns = [
  // ...
]

const heavyList = []

// adding same data multiple times to
// create a huge list
for (let i = 0; i <= 1000; i++) {
  Array.prototype.push.apply(heavyList, rows)
}
</script>
```

A more involved example below, playing with sticky headers and footers.

Example "Playing with sticky headers":

```vue
<template>
  <q-virtual-scroll
    type="table"
    style="max-height: 70vh"
    :virtual-scroll-item-size="48"
    :virtual-scroll-sticky-size-start="48"
    :virtual-scroll-sticky-size-end="32"
    :items="heavyList"
  >
    <template #before>
      <thead class="thead-custom-sticky text-left">
        <tr>
          <th>#</th>
          <th v-for="col in columns" :key="'1--' + col.name2">
            {{ col.name1 }}
          </th>
        </tr>
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'2--' + col.name2">
            {{ col.name2 }}
          </th>
        </tr>
      </thead>
    </template>

    <template #after>
      <tfoot class="tfoot-custom-sticky text-left">
        <tr>
          <th>#</th>
          <th v-for="col in columns" :key="'3--' + col.name2">
            {{ col.name1 }}
          </th>
        </tr>
        <tr>
          <th>Index</th>
          <th v-for="col in columns" :key="'4--' + col.name2">
            {{ col.name2 }}
          </th>
        </tr>
      </tfoot>
    </template>

    <template #default="{ item: row, index }">
      <tr :key="index">
        <td>#{{ index }}</td>
        <td v-for="col in columns" :key="index + '-' + col.name2">
          {{ row[col.prop] }}
        </td>
      </tr>
    </template>
  </q-virtual-scroll>
</template>

<style lang="sass">
.thead-custom-sticky tr > *,
.tfoot-custom-sticky tr > *
  position: sticky
  opacity: 1
  z-index: 1
  background-color: black
  color: white
  /* covers any sub-pixel gap between sticky cells */
  box-shadow: -1px 0 0 black

.thead-custom-sticky tr:last-child > *
  top: 0

.tfoot-custom-sticky tr:first-child > *
  bottom: 0
</style>

<script setup>
const rows = [
  // ...
]

const columns = [
  // ...
]

const heavyList = []

// adding same data multiple times to
// create a huge list
for (let i = 0; i <= 1000; i++) {
  Array.prototype.push.apply(heavyList, rows)
}
</script>
```

### Scroll target

If you need to specify the scroll target (because the auto detected one is not the desired one) pass a CSS selector (as string), the DOM element or a Vue component reference (which stands for its root element) to the `scroll-target` prop.

If you need to use the virtual list with the whole page as the scrolling element then please set `scroll-target="body"`.

> [!IMPORTANT]
>
> - If you pass a custom scroll target container with `scroll-target` prop you must make sure that the element exists and that it can be overflowed (it must have a maximum height and an overflow that allows scrolling).
> - If the scroll target container cannot be overflowed you'll get the whole list rendered.

> [!IMPORTANT]
> If you want to use a Vue reference for `scroll-target`, please take care to set it after mounting the component, like in the example below.

Example "Custom scroll target by id":

```vue
<template>
  <div id="virtual-scroll-target" class="scroll" style="max-height: 230px">
    <div class="q-pa-md bg-purple text-white">
      Above the list - scrolls with the list
    </div>

    <q-virtual-scroll
      scroll-target="#virtual-scroll-target"
      :items="heavyList"
      separator
      #default="{ item, index }"
    >
      <q-item :key="index" dense>
        <q-item-section>
          <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
        </q-item-section>
      </q-item>
    </q-virtual-scroll>

    <div class="q-pa-md bg-purple text-white">
      Below the list - scrolls with the list
    </div>
  </div>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1)
  })
}
</script>
```

Example "Custom scroll target by ref":

```vue
<template>
  <div
    ref="virtualListScrollTargetRef"
    class="scroll"
    style="max-height: 230px"
  >
    <div class="q-pa-md bg-purple text-white">
      Above the list - scrolls with the list
    </div>

    <q-virtual-scroll
      :scroll-target="scrollTarget"
      :items="heavyList"
      separator
      #default="{ item, index }"
    >
      <q-item :key="index" dense>
        <q-item-section>
          <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
        </q-item-section>
      </q-item>
    </q-virtual-scroll>

    <div class="q-pa-md bg-purple text-white">
      Below the list - scrolls with the list
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, useTemplateRef } from 'vue'

const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1)
  })
}

Object.freeze(heavyList)

const virtualListScrollTargetRef = useTemplateRef('virtualListScrollTargetRef')
const scrollTarget = ref(null)

onMounted(() => {
  scrollTarget.value = virtualListScrollTargetRef.value
})
</script>
```

Example "Using QScrollArea":

```vue
<template>
  <div class="q-ma-md">
    <q-scroll-area
      :horizontal-offset="[0, 3]"
      :thumb-style="thumbStyle"
      :bar-style="barStyle"
      style="height: 200px"
      id="scroll-area-with-virtual-scroll-1"
    >
      <q-virtual-scroll
        scroll-target="#scroll-area-with-virtual-scroll-1 > .scroll"
        :items="heavyList"
        :virtual-scroll-item-size="32"
        separator
        #default="{ item, index }"
      >
        <q-item :key="index" dense>
          <q-item-section>
            <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
          </q-item-section>
        </q-item>
      </q-virtual-scroll>
    </q-scroll-area>
  </div>
</template>

<script setup>
const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1)
  })
}

const thumbStyle = {
  borderRadius: '8px',
  backgroundColor: '#027be3',
  width: '8px',
  opacity: 0.75
}

const barStyle = {
  borderRadius: '14px',
  backgroundColor: '#027be3',
  width: '14px',
  opacity: 0.2
}
</script>
```

### Scroll to position

```vue
<template>
  <div class="q-pa-md row justify-center">
    <q-input
      style="min-width: 10em"
      type="number"
      v-model.number="virtualListIndex"
      :min="0"
      :max="9999"
      label="Scroll to index"
      input-class="text-right"
      outlined
    />
    <q-btn
      class="q-ml-sm"
      label="Go"
      no-caps
      color="primary"
      @click="executeScroll"
    />
  </div>

  <q-separator />

  <q-virtual-scroll
    ref="virtualListRef"
    style="max-height: 300px"
    component="q-list"
    :items="heavyList"
    separator
    @virtual-scroll="onVirtualScroll"
    #default="{ item, index }"
  >
    <q-item
      :key="index"
      dense
      :class="{ 'bg-black text-white': index === virtualListIndex }"
    >
      <q-item-section>
        <q-item-label> #{{ index }} - {{ item.label }} </q-item-label>
      </q-item-section>
    </q-item>
  </q-virtual-scroll>
</template>

<script setup>
import { onMounted, ref, useTemplateRef } from 'vue'

const maxSize = 10_000
const heavyList = []

for (let i = 0; i < maxSize; i++) {
  heavyList.push({
    label: 'Option ' + (i + 1)
  })
}

const virtualListRef = useTemplateRef('virtualListRef')
const virtualListIndex = ref(1200)

onMounted(() => {
  virtualListRef.value.scrollTo(virtualListIndex.value)
})

function onVirtualScroll({ index }) {
  virtualListIndex.value = index
}

function executeScroll() {
  virtualListRef.value.scrollTo(virtualListIndex.value, 'start-force')
}
</script>
```

### Sync and async

You can also generate the items to be displayed on the list by using the `items-fn` prop.

> [!IMPORTANT]
> Make sure to use a synchronous function that returns the list of items to be displayed.

If you need async data use a component that retrieves and renders the data.

Example "Generate items on the fly":

```vue
<template>
  <q-virtual-scroll
    style="max-height: 300px; overflow-x: hidden"
    :items-size="size"
    :items-fn="getItems"
    :virtual-scroll-item-size="78"
    separator
    #default="{ item, index }"
  >
    <async-component
      :key="index"
      :index="item.index"
      :sent="item.sent"
    ></async-component>
  </q-virtual-scroll>
</template>

<script setup>
import { QChatMessage, QSkeleton } from 'quasar'
import { defineComponent, h, onBeforeMount, onBeforeUnmount, ref } from 'vue'

const AsyncComponent = defineComponent({
  props: {
    index: Number,
    sent: Boolean
  },

  setup(props) {
    const asyncContent = ref(null)

    let timer

    onBeforeMount(() => {
      timer = setTimeout(
        () => {
          asyncContent.value = {
            sent: props.sent,
            name: props.sent ? 'me' : 'Someone else',
            avatar: props.sent
              ? 'https://cdn.quasar.dev/img/avatar4.jpg'
              : 'https://cdn.quasar.dev/img/avatar3.jpg',
            stamp: `${Math.floor(props.index / 1000)} minutes ago`,
            text: [`Message with id ${props.index}`]
          }
        },
        300 + Math.random() * 2000
      )
    })

    onBeforeUnmount(() => {
      clearTimeout(timer)
    })

    return () => {
      if (asyncContent.value === Object(asyncContent.value)) {
        return h(QChatMessage, {
          class: 'q-mx-sm',
          key: props.index,
          ...asyncContent.value
        })
      }

      const content = [
        h(QSkeleton, {
          class: 'on-left on-right',
          animation: 'none',
          type: 'text',
          width: '150px',
          height: '100px'
        })
      ]

      content[props.sent ? 'push' : 'unshift'](
        h(QSkeleton, {
          animation: 'none',
          type: 'QAvatar'
        })
      )

      return h(
        'div',
        {
          class: `row no-wrap items-center q-mx-sm justify-${props.sent ? 'end' : 'start'}`,
          style: 'height: 78px',
          key: props.index
        },
        content
      )
    }
  }
})

const size = ref(100_000)
const allItems = Array.from({ length: size.value }, (_, index) => ({
  index,
  sent: Math.random() > 0.5
}))

function getItems(from, curSize) {
  const items = []

  for (let i = 0; i < curSize; i++) {
    items.push(allItems[from + i])
  }

  return Object.freeze(items)
}
</script>
```

### Utility classes

There are two CSS classes that you can use (should you need to) to control VirtualScroll size calculation:

- Use `q-virtual-scroll--with-prev` class on an element rendered by the VirtualScroll to indicate the element should be grouped with the previous one (main use case is for multiple table rows generated from the same row of data).
- Use `q-virtual-scroll--skip` class on an element rendered by the VirtualScroll to indicate the element size should be ignored in size calculations.

Example "Virtual scroll with multiple rows for a data row":

```vue
<template>
  <q-table
    style="height: 400px"
    flat
    bordered
    ref="tableRef"
    title="Treats"
    :rows="rows"
    :columns="columns"
    :table-colspan="9"
    row-key="index"
    virtual-scroll
    :virtual-scroll-item-size="48"
    :pagination="pagination"
    :rows-per-page-options="[0]"
  >
    <template #header="props">
      <q-tr :props="props">
        <q-th />

        <q-th v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.label }}
        </q-th>
      </q-tr>
    </template>

    <template #body="props">
      <q-tr :props="props" :key="`m_${props.row.index}`">
        <q-td> Index: {{ props.row.index }} </q-td>

        <q-td v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.value }}
        </q-td>
      </q-tr>
      <q-tr
        :props="props"
        :key="`e_${props.row.index}`"
        class="q-virtual-scroll--with-prev"
      >
        <q-td colspan="100%">
          <div class="text-left"
            >This is the second row generated from the same data:
            {{ props.row.name }} (Index: {{ props.row.index }}).</div
          >
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { onMounted, ref, useTemplateRef } from 'vue'

const columns = [
  // ...
]

const seed = [
  // ...
]

const seedSize = seed.length

const rows = []
for (let i = 0; i < 1000; i++) {
  rows.push(...seed.map((r, j) => ({ ...r, index: i * seedSize + j + 1 })))
}

const tableRef = useTemplateRef('tableRef')
const pagination = { rowsPerPage: 0 }

onMounted(() => {
  tableRef.value.scrollTo(5000)
})
</script>
```

Example "Virtual scroll with expansion model":

```vue
<template>
  <q-table
    style="height: 400px"
    flat
    bordered
    ref="tableRef"
    title="Treats"
    :rows="rows"
    :columns="columns"
    :table-colspan="9"
    row-key="index"
    virtual-scroll
    :virtual-scroll-item-size="48"
    :pagination="pagination"
    :rows-per-page-options="[0]"
    v-model:expanded="expanded"
  >
    <template #header="props">
      <q-tr :props="props">
        <q-th auto-width />

        <q-th v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.label }}
        </q-th>
      </q-tr>
    </template>

    <template #body="props">
      <q-tr :props="props" :key="`m_${props.row.index}`">
        <q-td auto-width>
          <q-toggle
            v-model="props.expand"
            checked-icon="add"
            unchecked-icon="remove"
            :label="`Index: ${props.row.index}`"
          />
        </q-td>

        <q-td v-for="col in props.cols" :key="col.name" :props="props">
          {{ col.value }}
        </q-td>
      </q-tr>
      <q-tr
        v-show="props.expand"
        :props="props"
        :key="`e_${props.row.index}`"
        class="q-virtual-scroll--with-prev"
      >
        <q-td colspan="100%">
          <div class="text-left"
            >This is expand slot for row above: {{ props.row.name }} (Index:
            {{ props.row.index }}).</div
          >
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { onMounted, ref, useTemplateRef } from 'vue'

const columns = [
  // ...
]

const seed = [
  // ...
]

const seedSize = seed.length

const rows = []
for (let i = 0; i < 1000; i++) {
  rows.push(...seed.map((r, j) => ({ ...r, index: i * seedSize + j + 1 })))
}

const initialExpanded = rows.filter((r, i) => i % 3 === 0).map(r => r.index)

const expanded = ref(initialExpanded)
const tableRef = useTemplateRef('tableRef')
const pagination = { rowsPerPage: 0 }

onMounted(() => {
  tableRef.value.scrollTo(5000)
})
</script>
```

## Accessibility *(v2.25+)*

Items outside the rendered slice are not in the DOM, so they do not exist for assistive technology either — screen readers perceive only the visible window of the list, never its full length. When that matters, add per-item `aria-setsize`/`aria-posinset` attributes yourself (this is the compensation pattern [QTree](tree.md#accessibility)'s `virtual-scroll` mode uses). The padding around the rendered slice is hidden from assistive technology, and when a focused item scrolls out of the slice, focus is transferred to the container instead of falling to `<body>`. The container is also keyboard-scrollable when it owns its own scrolling (no `scroll-target` set).
