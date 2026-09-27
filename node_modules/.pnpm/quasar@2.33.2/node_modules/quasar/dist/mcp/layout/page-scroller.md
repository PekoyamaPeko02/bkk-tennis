---
title: Layout QPageScroller
related:
  - title: Layout
    path: layout.md
  - title: Layout Page
    path: page.md
---
The QPageScroller component helps in placing DOM elements / components wrapped by it into a static position within the content area of your QPage, no matter where the user scrolls.

The great advantage of this is that the elements wrapped by this component will never overlap the layout header, footer or drawer(s), even if those are not configured to be fixed. In the latter case, the position will be offset so that the overlap won't occur.
Try it out with a non-fixed footer for example. When user reaches bottom of screen and footer comes into view, the component will shift up so it won't overlap with the footer.

Essentially QPageScroller is very similar to QPageSticky. Whereas a QPageSticky component is always visible, a QPageScroller component only appears after a `scroll-offset` (property) is reached. Once visible, the user can click on it to quickly get back to the top of the page via `duration` property.

## QPageScroller API

Not inlined here: call the `get_api` tool with `name: "QPageScroller"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!NOTE]
> Since QPageScroller needs a layout and QLayout by default manages the entire window, then for demoing purposes we are going to use containerized QLayouts. But remember that by no means you are required to use containerized QLayouts for QPageScroller.

> [!IMPORTANT]
>
> - In order for QPageScroller to work, it must be placed within a QLayout component.
> - QPageScroller must be the last child element within its parent, so it can display on top of other content

### Basic

```vue
<template>
  <q-layout
    view="lHh Lpr lFf"
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
        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>
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

        <!-- place QPageScroller at end of page -->
        <q-page-scroller
          position="bottom-right"
          :scroll-offset="150"
          :offset="[18, 18]"
        >
          <q-btn
            aria-label="Scroll to top"
            fab
            icon="keyboard_arrow_up"
            color="accent"
          />
        </q-page-scroller>
      </q-page>
    </q-page-container>
  </q-layout>
</template>
```

### Expanded

```vue
<template>
  <q-layout
    view="lHh Lpr lFf"
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
        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>
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

        <!-- place QPageScroller at end of page -->
        <q-page-scroller
          expand
          position="top"
          :scroll-offset="150"
          :offset="[0, 0]"
        >
          <div
            class="col cursor-pointer q-pa-sm bg-accent text-white text-center"
          >
            Scroll back up...
          </div>
        </q-page-scroller>
      </q-page>
    </q-page-container>
  </q-layout>
</template>
```

### Reverse

```vue
<template>
  <q-layout
    view="lHh Lpr lFf"
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
        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>
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

        <!-- place QPageScroller at end of page -->
        <q-page-scroller
          reverse
          position="top"
          :scroll-offset="20"
          :offset="[0, 18]"
        >
          <q-btn
            aria-label="Scroll to bottom"
            fab
            icon="keyboard_arrow_down"
            color="accent"
          />
        </q-page-scroller>
      </q-page>
    </q-page-container>
  </q-layout>
</template>
```

## Accessibility *(v2.25+)*

QPageScroller itself is a plain clickable container with no role or keyboard handling of its own — keyboard operability comes entirely from what you put in its slot. Use a real button ([QBtn](../vue-components/button.md)) as the slot content, like the examples on this page do, so keyboard users can reach and activate it too.
