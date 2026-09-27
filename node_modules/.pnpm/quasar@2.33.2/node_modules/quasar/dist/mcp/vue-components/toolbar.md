---
title: Toolbar
related:
  - title: Layout Header and Footer
    path: ../layout/header-and-footer.md
  - title: Icon
    path: icon.md
  - title: Avatar
    path: avatar.md
  - title: Breadcrumbs
    path: breadcrumbs.md
  - title: Separator
    path: separator.md
---
QToolbar is a component usually part of Layout Header and Footer, but it can be used anywhere on the page.

## QToolbar API

Not inlined here: call the `get_api` tool with `name: "QToolbar"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QToolbarTitle API

Not inlined here: call the `get_api` tool with `name: "QToolbarTitle"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div class="q-gutter-y-sm">
    <q-toolbar class="text-primary">
      <q-btn aria-label="Toggle drawer" flat round dense icon="menu" />
      <q-toolbar-title> Toolbar </q-toolbar-title>
      <q-btn aria-label="More options" flat round dense icon="more_vert" />
    </q-toolbar>

    <q-toolbar class="bg-grey-9 text-white">
      <q-btn aria-label="Toggle drawer" flat round dense>
        <q-icon name="menu" />
      </q-btn>
      <q-toolbar-title> Toolbar </q-toolbar-title>
      <q-btn aria-label="More options" flat round dense>
        <q-icon name="more_vert" />
      </q-btn>
    </q-toolbar>

    <q-toolbar class="bg-purple text-white">
      <q-btn aria-label="Profile" flat round dense icon="assignment_ind" />
      <q-toolbar-title> Toolbar </q-toolbar-title>
      <q-btn aria-label="Apps" flat round dense icon="apps" class="q-mr-xs" />
      <q-btn aria-label="More options" flat round dense icon="more_vert" />
    </q-toolbar>

    <q-toolbar class="bg-black text-white">
      <q-btn aria-label="Profile" flat round dense icon="assignment_ind">
        <q-badge floating color="red">2</q-badge>
      </q-btn>
      <q-toolbar-title> Toolbar </q-toolbar-title>
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
  </div>
</template>
```

Example "With Avatar":

```vue
<template>
  <q-toolbar class="bg-primary text-white">
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
</template>
```

You can use the `glossy` class to make the toolbar glossy.

Example "Glossy":

```vue
<template>
  <q-toolbar class="bg-primary glossy text-white">
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
</template>
```

Example "Grouped vertically":

```vue
<template>
  <div class="q-gutter-y-sm">
    <div class="bg-orange text-white">
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          round
          dense
          icon="menu"
          class="q-mr-sm"
        />
        <q-space />
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
        <q-toolbar-title><strong>Quasar</strong> Framework</q-toolbar-title>
      </q-toolbar>
    </div>

    <div class="bg-cyan text-white">
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
    </div>
  </div>
</template>
```

Example "Grouped horizontally":

```vue
<template>
  <div class="row no-wrap shadow-1">
    <q-toolbar
      class="col-8"
      :class="$q.dark.isActive ? 'bg-grey-9 text-white' : 'bg-grey-3'"
    >
      <q-btn aria-label="Toggle drawer" flat round dense icon="menu" />
      <q-toolbar-title>Title</q-toolbar-title>
      <q-btn aria-label="Search" flat round dense icon="search" />
    </q-toolbar>
    <q-toolbar class="col-4 bg-primary text-white">
      <q-space />
      <q-btn
        aria-label="Bluetooth settings"
        flat
        round
        dense
        icon="bluetooth"
        class="q-mr-sm"
      />
      <q-btn aria-label="More options" flat round dense icon="more_vert" />
    </q-toolbar>
  </div>
</template>
```

Example "With Tabs":

```vue
<template>
  <q-toolbar class="bg-purple text-white shadow-2 rounded-borders">
    <q-btn flat label="Homepage" />
    <q-space />

    <!--
      notice shrink property since we are placing it
      as child of QToolbar
    -->
    <q-tabs v-model="tab" shrink>
      <q-tab name="tab1" label="Tab 1" />
      <q-tab name="tab2" label="Tab 2" />
      <q-tab name="tab3" label="Tab 3" />
    </q-tabs>
  </q-toolbar>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('')
</script>
```

Example "With Button Dropdown":

```vue
<template>
  <q-toolbar class="bg-primary text-white q-my-md shadow-2">
    <q-btn
      aria-label="Toggle drawer"
      flat
      round
      dense
      icon="menu"
      class="q-mr-sm"
    />
    <q-separator dark vertical inset />
    <q-btn stretch flat label="Link" />

    <q-space />

    <q-btn-dropdown stretch flat label="Dropdown">
      <q-list>
        <q-item-label header>Folders</q-item-label>
        <q-item v-for="n in 3" :key="`x.${n}`" clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar icon="folder" color="secondary" text-color="white" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" />
          </q-item-section>
        </q-item>
        <q-separator inset spaced />
        <q-item-label header>Files</q-item-label>
        <q-item v-for="n in 3" :key="`y.${n}`" clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar icon="assignment" color="primary" text-color="white" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Vacation</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" />
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>
    <q-separator dark vertical />
    <q-btn stretch flat label="Link" />
    <q-separator dark vertical />
    <q-btn stretch flat label="Link" />
  </q-toolbar>
</template>
```

Example "With Button Toggle":

```vue
<template>
  <q-toolbar class="bg-secondary text-white q-my-md shadow-2">
    <q-btn
      aria-label="Toggle drawer"
      flat
      round
      dense
      icon="menu"
      class="q-mr-sm"
    />

    <q-space />

    <q-btn-toggle
      v-model="model"
      flat
      stretch
      toggle-color="yellow"
      :options="options"
    />
  </q-toolbar>
</template>

<script setup>
import { ref } from 'vue'

const model = ref('one')
const options = [
  { label: 'One', value: 'one' },
  { label: 'Two', value: 'two' },
  { label: 'Three', value: 'three' }
]
</script>
```

## Accessibility *(v2.25+)*

QToolbar carries `role="toolbar"`. Give each toolbar an `aria-label` (it falls through to the root element) when a page contains more than one, so screen reader users can tell them apart. Note that the controls inside remain independent Tab stops — QToolbar does not implement the single-Tab-stop, arrow-key navigation that the [WAI-ARIA toolbar pattern](https://www.w3.org/WAI/ARIA/apg/patterns/toolbar/) describes.
