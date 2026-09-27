---
title: Space
---
The purpose of QSpace (has no props) is to simply fill all available space inside of a flexbox DOM element.

As a sidenote, all Quasar components use flexbox. So what this means, for example (there are numerous examples, just mentioning one), is that you can use QSpace in a QToolbar.

## Usage

These are only two examples so you can get a glimpse on how to use it.

Example "Basic":

```vue
<template>
  <q-toolbar class="bg-primary text-white">
    <q-btn flat round dense icon="menu" />

    <q-space />

    <q-btn flat round dense icon="apps" class="q-mr-xs" />
    <q-btn flat round dense icon="more_vert" />
  </q-toolbar>

  <div
    class="row no-wrap items-center q-mt-md q-pa-sm bg-grey-9 text-white rounded-borders"
  >
    <q-avatar>
      <img
        alt="Quasar logo"
        src="https://cdn.quasar.dev/logo-v2/svg/logo-dark.svg"
      />
    </q-avatar>

    <q-space />

    <div>
      Quasar <q-badge>v{{ $q.version }}</q-badge>
    </div>
  </div>
</template>
```

## Accessibility *(v2.25+)*

QSpace is a pure flexbox filler, invisible to assistive technology — there is nothing to add.
