---
title: Bottom Sheet Plugin
related:
  - title: Dialog Plugin
    path: dialog.md
  - title: Dialog
    path: ../vue-components/dialog.md
---
Bottom Sheets slide up from the bottom edge of the device screen, and display a set of options with the ability to confirm or cancel an action. Bottom Sheets can sometimes be used as an alternative to menus, however, they should not be used for navigation.

The Bottom Sheet always appears above any other components on the page, and must be dismissed in order to interact with the underlying content. When it is triggered, the rest of the page darkens to give more focus to the Bottom Sheet options.

Bottom Sheets can be displayed as a list or as a grid, with icons or with avatars. They can be used either as a component in your Vue file templates, or as a globally available method.

## BottomSheet API

Not inlined here: call the `get_api` tool with `name: "BottomSheet"` for its definition, or add `part` (`methods`, `injection`) for one of them.

## Installation

Add to `quasar.config.js`:

```js
framework: {
    plugins: [
      'BottomSheet'
    ]
}
```

## Usage

Example "Outside of a Vue file":

```js
import { BottomSheet } from 'quasar'
BottomSheet.create({ ... }) // returns Object

// inside of a Vue file
import { useQuasar } from 'quasar'
setup () {
  const $q = useQuasar()
  $q.bottomSheet({ ... }) // returns Object
}
```

> [!NOTE]
> When user hits the phone/tablet back button (only for Cordova apps), the Action Sheet will get closed automatically.
>
> Also, when on a desktop browser, hitting the `ESCAPE` key also closes the Action Sheet.

Starting with Quasar v2.28, the `onCancel` callback (and `onDismiss`, when no action was picked) receives the reason for the dismissal: `backdrop`, `escape` (the ESC key) or `programmatic` (hidden through code, which includes an app route change).

Example "List and Grid":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      no-caps
      push
      color="primary"
      label="List BottomSheet"
      @click="show()"
    />
    <q-btn
      no-caps
      push
      color="white"
      text-color="primary"
      label="Grid BottomSheet"
      @click="show(true)"
    />
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'

const $q = useQuasar()

function show(grid) {
  $q.bottomSheet({
    message: 'Bottom Sheet message',
    grid,
    actions: [
      {
        label: 'Drive',
        img: 'https://cdn.quasar.dev/img/logo_drive_128px.png',
        id: 'drive'
      },
      // ...
      {},
      {
        label: 'Share',
        icon: 'share',
        id: 'share'
      },
      {
        label: 'Upload',
        icon: 'cloud_upload',
        color: 'primary',
        id: 'upload'
      },
      {},
      {
        label: 'John',
        avatar: 'https://cdn.quasar.dev/img/boy-avatar.png',
        id: 'john'
      }
    ]
  })
    .onOk(action => {
      console.log('Action chosen:', action.id)
    })
    .onCancel(reason => {
      // reason (Quasar v2.28+) is 'backdrop',
      // 'escape' or 'programmatic'
      console.log('Dismissed:', reason)
    })
    .onDismiss(() => {
      console.log('I am triggered on both OK and Cancel')
    })
}
</script>
```

Example "Force dark mode":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      no-caps
      push
      color="primary"
      label="List BottomSheet"
      @click="show()"
    />
    <q-btn
      no-caps
      push
      color="white"
      text-color="primary"
      label="Grid BottomSheet"
      @click="show(true)"
    />
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'

const $q = useQuasar()

function show(grid) {
  $q.bottomSheet({
    dark: true,
    message: 'Bottom Sheet message',
    grid,
    actions: [
      {
        label: 'Drive',
        img: 'https://cdn.quasar.dev/img/logo_drive_128px.png',
        id: 'drive'
      },
      // ...
      {},
      {
        label: 'Share',
        icon: 'share',
        id: 'share'
      },
      {
        label: 'Upload',
        icon: 'cloud_upload',
        color: 'primary',
        id: 'upload'
      },
      {},
      {
        label: 'John',
        avatar: 'https://cdn.quasar.dev/img/boy-avatar.png',
        id: 'john'
      }
    ]
  })
    .onOk(action => {
      console.log('Action chosen:', action.id)
    })
    .onCancel(() => {
      console.log('Dismissed')
    })
    .onDismiss(() => {
      console.log('I am triggered on both OK and Cancel')
    })
}
</script>
```

> [!NOTE]
> For an exhaustive list of options, please check API section.
