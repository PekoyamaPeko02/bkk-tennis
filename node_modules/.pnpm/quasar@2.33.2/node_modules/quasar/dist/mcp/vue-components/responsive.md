---
title: Responsive
---
QResponsive is a component which forces the content to maintain an aspect ratio based on its width.

## QResponsive API

Not inlined here: call the `get_api` tool with `name: "QResponsive"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
>
> - The component can be used with any content, as long you specify **only one direct child**. If you need multiple elements inside of it, wrap them in a `<div>`.
> - It is your responsibility to make sure that your content won't overflow the container.

> [!WARNING]
> Do not use it on Quasar components that already have a `ratio` property, like QImg or QVideo, or on components that have a forced height.

### Basic

Example "Basic usage":

```vue
<template>
  <div class="q-gutter-md" style="max-width: 300px">
    <q-responsive :ratio="16 / 9">
      <div class="rounded-borders bg-primary text-white flex flex-center">
        Ratio 16:9
      </div>
    </q-responsive>

    <q-responsive :ratio="1">
      <div class="rounded-borders bg-primary text-white flex flex-center">
        Ratio 1:1
      </div>
    </q-responsive>
  </div>
</template>
```

### Flex row

Note below that we are using a vertical alignment (`items-start`) other than the default (`stretch`), so that flexbox won't force the height on each QResponsive component.

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-responsive :ratio="16 / 9" class="col">
      <div class="rounded-borders bg-primary text-white flex flex-center">
        Ratio 16:9
      </div>
    </q-responsive>

    <q-responsive :ratio="1" class="col">
      <div class="rounded-borders bg-primary text-white flex flex-center">
        Ratio 1:1
      </div>
    </q-responsive>
  </div>
</template>
```

### On some components

Below are just a few examples. QResponsive is not restricted to only QCard and QCarousel.

Example "On QCard":

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-responsive :ratio="16 / 9" class="col">
      <q-card class="column" flat bordered>
        <q-img class="col" src="https://cdn.quasar.dev/img/parallax2.jpg" />

        <q-card-section>
          <div>Ratio 16:9</div>
        </q-card-section>
      </q-card>
    </q-responsive>

    <q-responsive :ratio="1" class="col">
      <q-card class="column" flat bordered>
        <q-img class="col" src="https://cdn.quasar.dev/img/parallax1.jpg" />

        <q-card-section>
          <div>Ratio 1:1</div>
        </q-card-section>
      </q-card>
    </q-responsive>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const slide = ref(1)
</script>
```

Example "On QCardSection":

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-card flat bordered class="col">
      <q-item>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>Title</q-item-label>
          <q-item-label caption> Subhead </q-item-label>
        </q-item-section>
      </q-item>

      <q-separator />

      <q-responsive :ratio="16 / 9">
        <!-- notice "border-radius-inherit" below; it's important when in a QCard -->
        <q-card-section class="border-radius-inherit flex flex-center">
          <div>QCardSection with ratio 16:9</div>
        </q-card-section>
      </q-responsive>
    </q-card>

    <q-card flat bordered class="col">
      <q-item>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>Title</q-item-label>
          <q-item-label caption> Subhead </q-item-label>
        </q-item-section>
      </q-item>

      <q-separator />

      <q-responsive :ratio="1">
        <!-- notice "border-radius-inherit" below; it's important when in a QCard -->
        <q-card-section class="border-radius-inherit flex flex-center">
          <div>QCardSection with ratio 1:1</div>
        </q-card-section>
      </q-responsive>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const slide = ref(1)
</script>
```

Example "On QTable":

```vue
<template>
  <q-responsive :ratio="16 / 9">
    <q-table
      class="my-sticky-table"
      virtual-scroll
      v-model:pagination="pagination"
      :rows-per-page-options="[0]"
      :virtual-scroll-sticky-size-start="48"
      row-key="index"
      title="Table aspect ratio: 4/3"
      :rows="rows"
      :columns="columns"
    />
  </q-responsive>
</template>

<script setup>
import { ref } from 'vue'

const seed = [
  // ...
]

// we generate lots of rows here
const rowsData = []
for (let i = 0; i < 100; i++) {
  rowsData.push(...seed.map(r => ({ ...r })))
}
rowsData.forEach((row, index) => {
  row.index = index
})

const rows = ref(rowsData)

const columns = [
  // ...
]

const pagination = ref({
  rowsPerPage: 0
})
</script>

<style lang="sass">
.my-sticky-table
  .q-table__top,
  .q-table__bottom,
  thead tr:first-child th /* bg color is important for th; just specify one */
    background-color: #00b4ff

  thead tr th
    position: sticky
    z-index: 1
    /* covers any sub-pixel gap between sticky cells */
    box-shadow: -1px 0 0 #00b4ff
  /* this will be the loading indicator */
  thead tr:last-child th
    /* height of all previous header rows */
    top: 48px
  thead tr:first-child th
    top: 0

  /* prevent scrolling behind sticky top row on focus */
  tbody
    /* height of all previous header rows */
    scroll-margin-top: 48px
</style>
```

Notice that we will not supply a `height` prop to QCarousel when we use QResponsive on it, since it's QResponsive who will take care of that.

Example "On QCarousel":

```vue
<template>
  <q-responsive :ratio="16 / 9" style="width: 500px; max-width: 100%">
    <q-carousel swipeable animated arrows v-model="slide" infinite>
      <q-carousel-slide
        :name="1"
        img-src="https://cdn.quasar.dev/img/mountains.jpg"
      />
      <q-carousel-slide
        :name="2"
        img-src="https://cdn.quasar.dev/img/parallax1.jpg"
      />
      <q-carousel-slide
        :name="3"
        img-src="https://cdn.quasar.dev/img/parallax2.jpg"
      />
      <q-carousel-slide
        :name="4"
        img-src="https://cdn.quasar.dev/img/quasar.jpg"
      />

      <template #control>
        <q-carousel-control
          position="bottom"
          :offset="[16, 8]"
          class="text-white text-center rounded-borders"
          style="background: rgba(255, 255, 255, 0.2); padding: 4px 8px"
        >
          Ratio 16:9
        </q-carousel-control>
      </template>
    </q-carousel>
  </q-responsive>
</template>

<script setup>
import { ref } from 'vue'

const slide = ref(1)
</script>
```

### Maximum height

Apply the max height (or max width, etc etc) directly on the QResponsive component through a CSS class or inline. Remember that it is still your responsibility to ensure that the content won't overflow the container.

Example "Max height":

```vue
<template>
  <div class="row items-start q-gutter-md">
    <q-responsive :ratio="4 / 3" class="col" style="max-height: 100px">
      <q-card class="column" flat bordered>
        <q-img class="col" src="https://cdn.quasar.dev/img/parallax1.jpg" />

        <q-card-section>
          <div>Ratio 4:3, but max height of 100px</div>
        </q-card-section>
      </q-card>
    </q-responsive>
  </div>
</template>
```

## Accessibility *(v2.25+)*

QResponsive is a purely presentational aspect-ratio wrapper with no accessibility surface of its own.
