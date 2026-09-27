---
title: Layout Page
related:
  - title: Layout
    path: layout.md
---
We will be talking about encapsulating pages within a QLayout. If you haven’t already, please read [QLayout](layout.md) documentation page first.

## QPageContainer API

Not inlined here: call the `get_api` tool with `name: "QPageContainer"` for its `slots` definition.

## QPage API

Not inlined here: call the `get_api` tool with `name: "QPage"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

A QPage must be encapsulated by QPageContainer, which in turn must be a child of QLayout.

```html
<q-layout>
  ...
  <q-page-container>
    <q-page>
      <!-- page content -->
    </q-page>
  </q-page-container>
  ...
</q-layout>
```

Usually, the QPageContainer is part of the Layout template (where it contains a `<router-view />` child only), and its content goes into separate vue files under `/src/pages`. If you haven't already, please read [Routing with Layouts and Pages](routing-with-layouts-and-pages.md).

```html
<!-- vue file for Layout: -->
<q-layout>
  ...
  <q-page-container>
    <router-view />
  </q-page-container>
  ...
</q-layout>

<!-- vue file for a Page: -->
<q-page padding>
  <!-- page content -->
</q-page>
```

### Example

> [!NOTE]
> Since QPageContainer and QPage need a layout and QLayout by default manages the entire window, then for demoing purposes we are going to use containerized QLayouts. But remember that by no means you are required to use containerized QLayouts for QPageContainer and QPage.

Example "Basic":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated>
      <q-toolbar>
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          />
        </q-avatar>

        <q-toolbar-title> Quasar Framework </q-toolbar-title>

        <q-btn aria-label="Trending" flat round dense icon="whatshot" />
      </q-toolbar>
    </q-header>

    <q-page-container>
      <q-page padding>
        <p v-for="n in 15" :key="n">
          Lorem ipsum dolor sit amet consectetur adipisicing elit. Fugit nihil
          praesentium molestias a adipisci, dolore vitae odit, quidem
          consequatur optio voluptates asperiores pariatur eos numquam rerum
          delectus commodi perferendis voluptate?
        </p>
      </q-page>
    </q-page-container>
  </q-layout>
</template>
```

### Style-fn

A QPage needs a QLayout because QLayout controls all the offsets of a page, keeping account of the space that header/footer/drawer use, according to its `view` property configuration. By default, your QPage component will have a `min-height` CSS property set on it to ensure that the content fills the screen at all times, even when the content is just a few lines.

QPage also establishes its own block formatting context (`display: flow-root`), so the vertical margins of its children (for example a `q-mt-md` on the first card) stay inside the page instead of collapsing through it. Otherwise the page would get pushed below its `min-height` and the window would show a scrollbar even when the content fits the screen.

If you wish to tweak, or even remove this property, you can do so by using the `style-fn` property:

```html
<template>
  <q-page :style-fn="myTweak">...</q-page>
</template>

<script setup>
  function myTweak(offset) {
    // "offset" is a Number (pixels) that refers to the total
    // height of header + footer that occupies on screen,
    // based on the QLayout "view" prop configuration

    // this is actually what the default style-fn does in Quasar
    return { minHeight: offset ? `calc(100vh - ${offset}px)` : '100vh' }
  }
</script>
```

## Accessibility *(v2.25+)*

QPage renders a real `<main>` element, so the main landmark of your [QLayout](layout.md#accessibility) comes for free — exactly one per page, as long as you use a single QPage. Do not add a `<main>` of your own around or inside it, which would leave the page with duplicate main landmarks. QPageContainer is a plain container with no semantics of its own.
