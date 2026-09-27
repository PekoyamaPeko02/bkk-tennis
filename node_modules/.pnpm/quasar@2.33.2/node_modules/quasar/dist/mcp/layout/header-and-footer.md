---
title: Layout Header and Footer
related:
  - title: Layout
    path: layout.md
  - title: Layout Page
    path: page.md
  - title: Toolbar
    path: ../vue-components/toolbar.md
  - title: Breadcrumbs
    path: ../vue-components/breadcrumbs.md
  - title: Tabs
    path: ../vue-components/tabs.md
  - title: Bar
    path: ../vue-components/bar.md
---
QLayout allows you to configure your views as a 3x3 matrix, containing an optional Header and/or Footer (mostly used for navbar, but can be anything). If you haven’t already, please read [QLayout](layout.md) documentation page first.

## QHeader API

Not inlined here: call the `get_api` tool with `name: "QHeader"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## QFooter API

Not inlined here: call the `get_api` tool with `name: "QFooter"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

> [!NOTE]
> Since the header and footer needs a layout and QLayout by default manages the entire window, then for demoing purposes we are going to use containerized QLayouts. But remember that by no means you are required to use containerized QLayouts for QHeader or QFooter.

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
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          />
        </q-avatar>

        <q-toolbar-title>Quasar Framework</q-toolbar-title>

        <q-btn aria-label="Trending" flat round dense icon="whatshot" />
      </q-toolbar>
    </q-header>

    <q-footer elevated>
      <q-toolbar>
        <q-toolbar-title>Footer</q-toolbar-title>
      </q-toolbar>
    </q-footer>

    <q-page-container>
      <q-page class="q-pa-md">
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

You can use `glossy` class on toolbars in header and footer.

Example "Glossy":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated>
      <q-toolbar class="glossy">
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          />
        </q-avatar>

        <q-toolbar-title>Quasar Framework</q-toolbar-title>

        <q-btn aria-label="Trending" flat round dense icon="whatshot" />
      </q-toolbar>
    </q-header>

    <q-footer elevated>
      <q-toolbar class="glossy">
        <q-toolbar-title>Footer</q-toolbar-title>
      </q-toolbar>
    </q-footer>

    <q-page-container>
      <q-page class="q-pa-md">
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

### Various content

Example "Playing with QToolbar":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated class="bg-purple">
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-space></q-space>
        <q-btn
          aria-label="Search"
          flat
          round
          dense
          icon="search"
          class="q-mr-xs"
        />
        <q-btn aria-label="Add group" flat round dense icon="group_add" />
      </q-toolbar>
      <q-toolbar inset>
        <q-toolbar-title> <strong>Quasar</strong> Framework </q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <q-page class="q-pa-md">
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

Example "Playing with QBreadcrumb":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated class="bg-cyan">
      <q-toolbar>
        <q-btn aria-label="Profile" flat round dense icon="assignment_ind" />

        <q-space />

        <q-btn
          aria-label="SIM settings"
          flat
          round
          dense
          icon="sim_card"
          class="q-mr-xs"
        />
        <q-btn aria-label="Games" flat round dense icon="gamepad" />
      </q-toolbar>

      <q-toolbar inset>
        <q-breadcrumbs active-color="white" style="font-size: 16px">
          <q-breadcrumbs-el label="Home" icon="home" />
          <q-breadcrumbs-el label="Components" icon="widgets" />
          <q-breadcrumbs-el label="Toolbar" />
        </q-breadcrumbs>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <q-page class="q-pa-md">
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

Example "Playing with QTabs":

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
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          />
        </q-avatar>

        <q-toolbar-title>Quasar Framework</q-toolbar-title>

        <q-btn aria-label="Trending" flat round dense icon="whatshot" />
      </q-toolbar>

      <q-tabs v-model="tab">
        <q-tab name="images" label="Images" />
        <q-tab name="videos" label="Videos" />
        <q-tab name="articles" label="Articles" />
      </q-tabs>
    </q-header>

    <q-page-container>
      <q-page class="q-pa-md">
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

<script setup>
import { ref } from 'vue'

const tab = ref('images')
</script>
```

### Reveal property

In the example below, scroll the page to see the QHeader and QFooter behavior.

Example "Reveal":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header reveal elevated>
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-avatar>
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          />
        </q-avatar>

        <q-toolbar-title>Quasar Framework</q-toolbar-title>

        <q-btn aria-label="Trending" flat round dense icon="whatshot" />
      </q-toolbar>
    </q-header>

    <q-footer reveal elevated>
      <q-toolbar>
        <q-toolbar-title>Footer</q-toolbar-title>
      </q-toolbar>
    </q-footer>

    <q-page-container>
      <q-page class="q-pa-md">
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

### iOS look and feel

In the example below, you could use Ionicons icons (v4) with `ion-ios-` prefix for QTabs, which would perfectly match the iOS look and feel.

Example "iOS-like":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header bordered class="bg-grey-3 text-primary">
      <q-toolbar>
        <q-toolbar-title class="text-center">
          <q-avatar>
            <img
              alt="Quasar logo"
              src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
            />
          </q-avatar>
          Quasar Framework
        </q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-footer bordered class="bg-grey-3 text-primary">
      <q-tabs
        no-caps
        active-color="primary"
        indicator-color="transparent"
        class="text-grey-8"
        v-model="tab"
      >
        <q-tab name="images" label="Images" />
        <q-tab name="videos" label="Videos" />
        <q-tab name="articles" label="Articles" />
      </q-tabs>
    </q-footer>

    <q-page-container>
      <q-page class="q-pa-md">
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

<script setup>
import { ref } from 'vue'

const tab = ref('images')
</script>
```

### Desktop app look and feel

The example below is especially useful if you build an Electron app and you hide the default app frame.

Example "Desktop app-like":

```vue
<template>
  <q-layout
    view="lHh lpr lFf"
    container
    style="height: 400px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated>
      <q-bar>
        <q-icon name="laptop_chromebook" />
        <div>Google Chrome</div>

        <q-space />

        <q-btn aria-label="Minimize" dense flat icon="minimize" />
        <q-btn aria-label="Maximize" dense flat icon="crop_square" />
        <q-btn aria-label="Close" dense flat icon="close" />
      </q-bar>

      <div class="q-pa-sm q-pl-md row items-center">
        <div class="cursor-pointer non-selectable">
          File
          <q-menu>
            <q-list dense style="min-width: 100px">
              <q-item clickable v-close-popup>
                <q-item-section>Open...</q-item-section>
              </q-item>
              <q-item clickable v-close-popup>
                <q-item-section>New</q-item-section>
              </q-item>

              <q-separator />

              <q-item clickable>
                <q-item-section>Preferences</q-item-section>
                <q-item-section side>
                  <q-icon name="keyboard_arrow_right" />
                </q-item-section>

                <q-menu anchor="top end" self="top start">
                  <q-list>
                    <q-item v-for="n in 3" :key="n" dense clickable>
                      <q-item-section>Submenu Label</q-item-section>
                      <q-item-section side>
                        <q-icon name="keyboard_arrow_right" />
                      </q-item-section>
                      <q-menu auto-close anchor="top end" self="top start">
                        <q-list>
                          <q-item v-for="n in 3" :key="n" dense clickable>
                            <q-item-section>3rd level Label</q-item-section>
                          </q-item>
                        </q-list>
                      </q-menu>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-item>

              <q-separator />

              <q-item clickable v-close-popup>
                <q-item-section>Quit</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </div>

        <div class="q-ml-md cursor-pointer non-selectable">
          Edit
          <q-menu auto-close>
            <q-list dense style="min-width: 100px">
              <q-item clickable>
                <q-item-section>Cut</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Copy</q-item-section>
              </q-item>
              <q-item clickable>
                <q-item-section>Paste</q-item-section>
              </q-item>
              <q-separator />
              <q-item clickable>
                <q-item-section>Select All</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </div>
      </div>
    </q-header>

    <q-page-container>
      <q-page class="q-pa-md">
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

## Accessibility *(v2.25+)*

QHeader and QFooter render real `<header>` and `<footer>` elements, so they are exposed to assistive technology as the banner and footer landmarks of your [QLayout](layout.md#accessibility) with no extra work. A header or footer hidden through its `v-model` also leaves the Tab order and the accessibility tree entirely, so nothing invisible stays reachable.

One hidden by the `reveal` behavior is only translated off-screen instead; should keyboard focus land inside it (e.g. by tabbing), it reveals itself again, so focus never sits in an invisible bar.
