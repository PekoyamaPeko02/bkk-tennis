---
title: Layout QPageSticky
related:
  - title: Layout
    path: layout.md
  - title: Layout Page
    path: page.md
---
The QPageSticky component helps in placing DOM elements / components wrapped by it into a static position within the content area of your QPage, no matter where the user scrolls.

The great advantage of this is that the elements wrapped by this component will never overlap the layout header, footer or drawer(s), even if those are not configured to be fixed. In the latter case, the position will be offset so that the overlap won't occur.
Try it out with a non-fixed footer for example. When user reaches bottom of screen and footer comes into view, the component will shift up so it won't overlap with the footer.

## QPageSticky API

Not inlined here: call the `get_api` tool with `name: "QPageSticky"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!NOTE]
> Since QPageSticky needs a layout and QLayout by default manages the entire window, then for demoing purposes we are going to use containerized QLayouts. But remember that by no means you are required to use containerized QLayouts for QPageSticky.

> [!IMPORTANT]
>
> - In order for QPageSticky to work, it must be placed within a QLayout component.
> - QPageSticky must be the last child element within its parent, so it can display on top of other content

### Basic

In the example below, click on the menu buttons to show/hide Drawers, scroll the inner page, and resize the browser window so that the enclosing QLayout hits the Drawer's 700px and 500px breakpoints.

```vue
<template>
  <q-layout
    view="lHh Lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header reveal elevated>
      <q-toolbar>
        <q-btn
          aria-label="Toggle left drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerLeft = !drawerLeft"
        />

        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>

        <q-btn
          aria-label="Toggle right drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerRight = !drawerRight"
        />
      </q-toolbar>
    </q-header>

    <q-footer reveal elevated>
      <q-toolbar>
        <q-btn
          aria-label="Toggle left drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerLeft = !drawerLeft"
        />

        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>

        <q-btn
          aria-label="Toggle right drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerRight = !drawerRight"
        />
      </q-toolbar>
    </q-footer>

    <q-drawer
      aria-label="Left drawer"
      v-model="drawerLeft"
      :width="150"
      :breakpoint="700"
      behavior="desktop"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <div class="q-pa-sm">
          <div v-for="n in 50" :key="n">Drawer {{ n }} / 50</div>
        </div>
      </q-scroll-area>
    </q-drawer>

    <q-drawer
      aria-label="Right drawer"
      side="right"
      v-model="drawerRight"
      bordered
      :width="150"
      :breakpoint="500"
      behavior="desktop"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <div class="q-pa-sm">
          <div v-for="n in 50" :key="n">Drawer {{ n }} / 50</div>
        </div>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <q-page padding>
        <p v-for="n in 15" :key="n">
          Lorem ipsum dolor sit amet consectetur adipisicing elit. Fugit nihil
          praesentium molestias a adipisci, dolore vitae odit, quidem
          consequatur optio voluptates asperiores pariatur eos numquam rerum
          delectus commodi perferendis voluptate?
        </p>

        <!-- place QPageSticky at end of page -->
        <q-page-sticky position="top-left" :offset="[18, 18]">
          <q-btn
            aria-label="Sticky top left"
            round
            color="accent"
            icon="arrow_back"
            class="rotate-45"
          />
        </q-page-sticky>
        <q-page-sticky position="top" :offset="[0, 18]">
          <q-btn
            aria-label="Sticky top"
            round
            color="accent"
            icon="arrow_back"
            class="rotate-90"
          />
        </q-page-sticky>
        <q-page-sticky position="top-right" :offset="[18, 18]">
          <q-btn
            aria-label="Sticky top right"
            round
            color="accent"
            icon="arrow_upward"
            class="rotate-45"
          />
        </q-page-sticky>
        <q-page-sticky position="right" :offset="[18, 0]">
          <q-btn
            aria-label="Sticky right"
            round
            color="accent"
            icon="arrow_upward"
            class="rotate-90"
          />
        </q-page-sticky>
        <q-page-sticky position="left" :offset="[18, 0]">
          <q-btn
            aria-label="Sticky left"
            round
            color="accent"
            icon="arrow_back"
          />
        </q-page-sticky>
        <q-page-sticky position="bottom-left" :offset="[18, 18]">
          <q-btn
            aria-label="Sticky bottom left"
            round
            color="accent"
            icon="arrow_forward"
            class="rotate-135"
          />
        </q-page-sticky>
        <q-page-sticky position="bottom" :offset="[0, 18]">
          <q-btn
            aria-label="Sticky bottom"
            round
            color="accent"
            icon="arrow_forward"
            class="rotate-90"
          />
        </q-page-sticky>
        <q-page-sticky position="bottom-right" :offset="[18, 18]">
          <q-btn
            aria-label="Sticky bottom right"
            round
            color="accent"
            icon="arrow_forward"
            class="rotate-45"
          />
        </q-page-sticky>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { ref } from 'vue'

const $q = useQuasar()

const drawerLeft = ref($q.screen.width > 700)
const drawerRight = ref($q.screen.width > 500)
</script>
```

### Expanded

In the example below, click on the menu buttons to show/hide Drawers, scroll the inner page, and resize the browser window so that the enclosing QLayout hits the Drawer's 700px and 500px breakpoints.

By using expanded QPageSticky you can, for example, have a page-specific QToolbar as below.

```vue
<template>
  <q-layout
    view="lHh Lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header reveal elevated>
      <q-toolbar>
        <q-btn
          aria-label="Toggle left drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerLeft = !drawerLeft"
        />

        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>

        <q-btn
          aria-label="Toggle right drawer"
          flat
          round
          dense
          icon="menu"
          @click="drawerRight = !drawerRight"
        />
      </q-toolbar>
    </q-header>

    <q-drawer
      aria-label="Left drawer"
      v-model="drawerLeft"
      :width="150"
      :breakpoint="700"
      behavior="desktop"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <div class="q-pa-sm">
          <div v-for="n in 50" :key="n">Drawer {{ n }} / 50</div>
        </div>
      </q-scroll-area>
    </q-drawer>

    <q-drawer
      aria-label="Right drawer"
      side="right"
      v-model="drawerRight"
      bordered
      :width="150"
      :breakpoint="500"
      behavior="desktop"
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <div class="q-pa-sm">
          <div v-for="n in 50" :key="n">Drawer {{ n }} / 50</div>
        </div>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <q-page padding style="padding-top: 66px">
        <p v-for="n in 15" :key="n">
          Lorem ipsum dolor sit amet consectetur adipisicing elit. Fugit nihil
          praesentium molestias a adipisci, dolore vitae odit, quidem
          consequatur optio voluptates asperiores pariatur eos numquam rerum
          delectus commodi perferendis voluptate?
        </p>

        <!-- place QPageSticky at end of page -->
        <q-page-sticky expand position="top">
          <q-toolbar class="bg-accent text-white">
            <q-avatar>
              <img
                alt="Quasar logo"
                src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
              />
            </q-avatar>
            <q-toolbar-title> Page Title </q-toolbar-title>
          </q-toolbar>
        </q-page-sticky>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { ref } from 'vue'

const $q = useQuasar()

const drawerLeft = ref($q.screen.width > 700)
const drawerRight = ref($q.screen.width > 500)
</script>
```

## Accessibility *(v2.25+)*

QPageSticky is a positioning-only wrapper with no accessibility surface of its own — the accessibility of whatever you place inside it is entirely up to that content.
