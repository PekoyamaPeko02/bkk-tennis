---
title: Breadcrumbs
---
The QBreadcrumbs component is used as a navigational aid in UI. It allows users to keep track of their location within programs, documents, or websites. Most common use is in a [QToolbar](toolbar.md), but it's not limited to it.

## QBreadcrumbs API

Not inlined here: call the `get_api` tool with `name: "QBreadcrumbs"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QBreadcrumbsEl API

Not inlined here: call the `get_api` tool with `name: "QBreadcrumbsEl"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs>
      <q-breadcrumbs-el label="Home" icon="home" />
      <q-breadcrumbs-el label="Components" icon="widgets" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs class="text-grey">
      <q-breadcrumbs-el icon="home" />
      <q-breadcrumbs-el icon="widgets" />
      <q-breadcrumbs-el icon="navigation" />
    </q-breadcrumbs>
  </div>
</template>
```

Example "In a QToolbar":

```vue
<template>
  <div class="bg-cyan text-white">
    <q-toolbar>
      <q-btn flat round dense icon="assignment_ind" />

      <q-toolbar-title>Quasar</q-toolbar-title>

      <q-btn flat round dense icon="sim_card" class="q-mr-xs" />
      <q-btn flat round dense icon="gamepad" />
    </q-toolbar>
    <q-toolbar inset>
      <q-breadcrumbs active-color="white" style="font-size: 16px">
        <q-breadcrumbs-el label="Home" icon="home" />
        <q-breadcrumbs-el label="Components" icon="widgets" />
        <q-breadcrumbs-el label="Toolbar" />
      </q-breadcrumbs>
    </q-toolbar>
  </div>
</template>
```

### Design

Example "Custom separators":

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs separator="---" class="text-orange" active-color="secondary">
      <q-breadcrumbs-el icon="home" />
      <q-breadcrumbs-el label="Components" icon="widgets" />
      <q-breadcrumbs-el label="Breadcrumbs" icon="navigation" />
    </q-breadcrumbs>

    <q-breadcrumbs class="text-grey" active-color="purple">
      <template #separator>
        <q-icon size="1.2em" name="arrow_forward" color="purple" />
      </template>

      <q-breadcrumbs-el label="Home" icon="home" />
      <q-breadcrumbs-el label="Components" icon="widgets" />
      <q-breadcrumbs-el label="Breadcrumbs" icon="navigation" />
    </q-breadcrumbs>

    <q-breadcrumbs class="text-brown">
      <template #separator>
        <q-icon size="1.5em" name="chevron_right" color="primary" />
      </template>

      <q-breadcrumbs-el label="Home" icon="home" />
      <q-breadcrumbs-el label="Components" icon="widgets" />
      <q-breadcrumbs-el label="Breadcrumbs" icon="navigation" />
    </q-breadcrumbs>
  </div>
</template>
```

Example "Gutters":

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs gutter="none">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-separator />

    <q-breadcrumbs gutter="xs">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-separator />

    <q-breadcrumbs gutter="sm">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-separator />

    <q-breadcrumbs gutter="md">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-separator />

    <q-breadcrumbs gutter="lg">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-separator />

    <q-breadcrumbs gutter="xl">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>
  </div>
</template>
```

Example "Align":

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs align="left">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs align="center">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs align="right">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs align="between">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>

    <q-breadcrumbs align="around">
      <q-breadcrumbs-el label="Home" />
      <q-breadcrumbs-el label="Components" />
      <q-breadcrumbs-el label="Breadcrumbs" />
    </q-breadcrumbs>
  </div>
</template>
```

### Connecting to Vue Router

The examples below won't work with UMD version (so in Codepen/jsFiddle too) because they depend on Vue Router.

Example "Router links":

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs>
      <q-breadcrumbs-el icon="home" to="/" />
      <q-breadcrumbs-el
        label="Docs"
        icon="widgets"
        to="/start/pick-quasar-flavour"
      />
      <q-breadcrumbs-el
        label="Breadcrumbs"
        icon="navigation"
        to="/vue-components/breadcrumbs"
      />
      <q-breadcrumbs-el label="Build" icon="build" />
    </q-breadcrumbs>
  </div>
</template>
```

You can also delay, cancel or redirect navigation, as seen below. For a more in-depth description of the `@click` event being used below, please refer to QBreadcrumbsEl API card at the top of the page.

Example "Links with delayed, cancelled or redirected navigation":

```vue
<template>
  <div class="q-gutter-sm">
    <q-breadcrumbs>
      <q-breadcrumbs-el
        label="Delayed"
        icon="widgets"
        to="/"
        @click="onDelayedClick"
      />
      <q-breadcrumbs-el
        label="Cancelled"
        icon="navigation"
        to="/"
        @click="onCancelledClick"
      />
      <q-breadcrumbs-el
        label="Redirected"
        icon="build"
        to="/"
        @click="onRedirectedClick"
      />
      <q-breadcrumbs-el label="Page" />
    </q-breadcrumbs>
  </div>
</template>

<script setup>
function onDelayedClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate

  console.log('triggering navigation in 2s')
  setTimeout(() => {
    console.log('navigating as promised 2s ago')
    go()
  }, 2000)
}

function onCancelledClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate
  // then we never call go()
}

function onRedirectedClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate

  // call this at your convenience
  go({
    to: '/start/pick-quasar-flavour' // we pick another route
    // replace: boolean; default is what the tab is configured with
    // returnRouterError: boolean
  })
    .then(_vueRouterResult => {
      /* ... */
    })
    .catch(_vueRouterError => {
      /* ...will not reach here unless returnRouterError === true */
    })
}
</script>
```

## Accessibility *(v2.25+)*

QBreadcrumbs only renders the links themselves — it claims no landmark or list semantics of its own. To follow the [WAI-ARIA breadcrumb pattern](https://www.w3.org/WAI/ARIA/apg/patterns/breadcrumb/), wrap the component in a `<nav aria-label="Breadcrumb">` element and add `aria-current="page"` to the QBreadcrumbsEl representing the current page (attributes fall through to the rendered element) — the component does neither by itself.

Also be aware that the separator text is announced by screen readers between the links; if you want to avoid that, render the separator through the `separator` slot with `aria-hidden="true"` content.
