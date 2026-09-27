---
title: Markup Table
related:
  - title: Table
    path: table.md
---
The QMarkupTable is a way for you to simply wrap a native `<table>` in order to make it look like a Material Design table.

> [!TIP]
> For advanced functionality like pagination, sorting, filtering, and many more, you may want to check out [QTable](table.md) component instead.

## QMarkupTable API

Not inlined here: call the `get_api` tool with `name: "QMarkupTable"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> Notice that the content of `QMarkupTable` reflects an accurate markup representation of a native HTML `<table>`, having a `<thead>` and `<tbody>` to wrap header and table body. This is required.

> [!WARNING]
> **UMD developers**
>
> This component will *NOT* work as-is within the UMD version of Quasar. Browsers parse the template HTML before Vue kicks in and renders it, so the markup needs to be correct. `<q-markup-table> <thead>` or `<q-markup-table> <tbody>` is not. The solution is to wrap the content in a `<template>` like the following:
>
>  ```html
> <q-markup-table>
>   <template>
>     <!-- your content -->
>   </template>
> </q-markup-table>
> ```

Example "Basic":

```vue
<template>
  <q-markup-table>
    <thead>
      <tr>
        <th class="text-left">Dessert (100g serving)</th>
        <th class="text-right">Calories</th>
        <th class="text-right">Fat (g)</th>
        <th class="text-right">Carbs (g)</th>
        <th class="text-right">Protein (g)</th>
        <th class="text-right">Sodium (mg)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="text-left">Frozen Yogurt</td>
        <td class="text-right">159</td>
        <td class="text-right">6</td>
        <td class="text-right">24</td>
        <td class="text-right">4</td>
        <td class="text-right">87</td>
      </tr>
      <!-- ... -->
    </tbody>
  </q-markup-table>
</template>
```

Example "Separators":

```vue
<template>
  <q-option-group
    v-model="separator"
    inline
    class="q-mb-md"
    :options="separatorOptions"
  />

  <q-markup-table :separator="separator" flat bordered>
    <thead>
      <tr>
        <th class="text-left">Dessert (100g serving)</th>
        <th class="text-right">Calories</th>
        <th class="text-right">Fat (g)</th>
        <th class="text-right">Carbs (g)</th>
        <th class="text-right">Protein (g)</th>
        <th class="text-right">Sodium (mg)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="text-left">Frozen Yogurt</td>
        <td class="text-right">159</td>
        <td class="text-right">6</td>
        <td class="text-right">24</td>
        <td class="text-right">4</td>
        <td class="text-right">87</td>
      </tr>
      <!-- ... -->
    </tbody>
  </q-markup-table>
</template>

<script setup>
import { ref } from 'vue'

const separator = ref('vertical')
const separatorOptions = [
  { label: 'Horizontal', value: 'horizontal' },
  { label: 'Vertical', value: 'vertical' },
  { label: 'Cell', value: 'cell' },
  { label: 'None', value: 'none' }
]
</script>
```

Example "Force dark mode":

```vue
<template>
  <q-markup-table dark class="bg-indigo-8">
    <thead>
      <tr>
        <th class="text-left">Dessert (100g serving)</th>
        <th class="text-right">Calories</th>
        <th class="text-right">Fat (g)</th>
        <th class="text-right">Carbs (g)</th>
        <th class="text-right">Protein (g)</th>
        <th class="text-right">Sodium (mg)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td class="text-left">Frozen Yogurt</td>
        <td class="text-right">159</td>
        <td class="text-right">6</td>
        <td class="text-right">24</td>
        <td class="text-right">4</td>
        <td class="text-right">87</td>
      </tr>
      <!-- ... -->
    </tbody>
  </q-markup-table>
</template>
```

Example "Customization":

```vue
<template>
  <q-markup-table flat bordered>
    <thead class="bg-teal">
      <tr>
        <th colspan="5">
          <div class="row no-wrap items-center">
            <q-img
              style="width: 70px"
              :ratio="1"
              class="rounded-borders"
              src="https://cdn.quasar.dev/img/donuts.png"
            />

            <div class="text-h4 q-ml-md text-white">Treats</div>
          </div>
        </th>
      </tr>
      <tr>
        <th class="text-left">Dessert (100g serving)</th>
        <th class="text-right">Calories</th>
        <th class="text-right">Fat (g)</th>
        <th class="text-right">Carbs (g)</th>
        <th class="text-right">Protein (g)</th>
      </tr>
    </thead>
    <tbody :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'">
      <tr>
        <td class="text-left">Frozen Yogurt</td>
        <td class="text-right">159</td>
        <td class="text-right">6</td>
        <td class="text-right">24</td>
        <td class="text-right">4</td>
      </tr>
      <!-- ... -->
    </tbody>
  </q-markup-table>
</template>
```

> [!TIP]
> If you want to remove the hover effect on some rows or some cells add a `q-tr--no-hover` or `q-td--no-hover` class to them.

## Accessibility *(v2.25+)*

QMarkupTable renders a plain native `<table>` around your content and adds no semantics of its own, so all the accessibility work of a hand-written table — `scope` on header cells, a `<caption>`, header/data associations — is yours to author in the markup.

The wrapper that provides the horizontal scrolling is a Tab stop, so a table wider than its container can be scrolled with the arrow keys (WCAG 2.1.1).
