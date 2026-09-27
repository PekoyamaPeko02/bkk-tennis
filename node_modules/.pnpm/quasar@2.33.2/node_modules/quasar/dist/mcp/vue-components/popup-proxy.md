---
title: Popup Proxy
related:
  - title: QMenu
    path: menu.md
  - title: Dialog
    path: dialog.md
  - title: v-close-popup directive
    path: ../vue-directives/close-popup.md
---
QPopupProxy should be used when you need either a [QMenu](menu.md) (on bigger screens) or a [QDialog](dialog.md) (on smaller screens) to be displayed. It acts as a proxy which picks either of the two components to use. QPopupProxy also handles context-menus.

## QPopupProxy API

Not inlined here: call the `get_api` tool with `name: "QPopupProxy"` for its definition, or add `part` (`props`, `computedProps`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!TIP]
> Use your browsers development tools to toggle the device between mobile or desktop (with browser refresh after each change) or, physically resize your browser's window to watch the QPopupProxy component switch between either a QMenu or a QDialog before clicking/tapping on its container. The default breakpoint is set at 450px.

### Standard

```vue
<template>
  <q-btn push color="primary" label="Handles click">
    <q-popup-proxy>
      <q-banner>
        <template #avatar>
          <q-icon name="signal_wifi_off" color="primary" />
        </template>
        You have lost connection to the internet. This app is offline.
      </q-banner>
    </q-popup-proxy>
  </q-btn>
</template>
```

### Context menu

Example "Context menu (right click / long tap)":

```vue
<template>
  <q-btn push color="purple" label="Handles right-click">
    <q-popup-proxy context-menu>
      <q-banner>
        <template #avatar>
          <q-icon name="signal_wifi_off" color="primary" />
        </template>
        You have lost connection to the internet. This app is offline.
      </q-banner>
    </q-popup-proxy>
  </q-btn>
</template>
```

### Breakpoint

On the example below, click on the icon in the input. The QInput stays focused for as long as the popup is open, whichever of the two components got rendered.

Example "Breakpoint @600px":

```vue
<template>
  <q-input filled v-model="input" mask="date" :rules="['date']">
    <template #append>
      <q-icon name="event" class="cursor-pointer">
        <q-popup-proxy cover :breakpoint="600">
          <q-date v-model="input" />
        </q-popup-proxy>
      </q-icon>
    </template>
  </q-input>
</template>

<script setup>
import { ref } from 'vue'

const input = ref('')
const date = ref('2018/11/03')
</script>
```

### Pass-through props

Keep in mind that all props from both [QMenu](menu.md) and [QDialog](dialog.md) are passed through via this component. So props like `offset` or `transition-show` (as a mere example) can be used in conjunction with QPopupProxy. The only exception is `separate-close-popup`, which QPopupProxy manages internally.

Example "Props from QMenu or QDialog":

```vue
<template>
  <div class="q-gutter-md" style="font-size: 36px">
    <q-icon name="settings_remote" class="text-brown cursor-pointer">
      <q-popup-proxy transition-show="flip-up" transition-hide="flip-down">
        <q-banner class="bg-brown text-white">
          <template #avatar>
            <q-icon name="signal_wifi_off" />
          </template>
          You have lost connection to the internet. This app is offline.
        </q-banner>
      </q-popup-proxy>
    </q-icon>

    <q-icon name="perm_data_setting" class="text-purple cursor-pointer">
      <q-popup-proxy :offset="[10, 10]">
        <q-banner class="bg-purple text-white">
          <template #avatar>
            <q-icon name="signal_wifi_off" />
          </template>
          You have lost connection to the internet. This app is offline.
        </q-banner>
      </q-popup-proxy>
    </q-icon>
  </div>
</template>
```

> [!NOTE]
> When a Menu is used, QPopupProxy applies a default `max-height` of `99vh` to it. Set the `max-height` prop to override this.

## Accessibility *(v2.25+)*

QPopupProxy has no semantics of its own — it exposes whatever the rendered component provides. Below the breakpoint that is a QDialog (`role="dialog"` with a managed `aria-modal`), above it a QMenu (a positioned container that deliberately claims no ARIA role). See [QDialog's Accessibility section](dialog.md#accessibility) and [QMenu's Accessibility section](menu.md#accessibility) for what each mode announces and how it handles keyboard interaction and focus.

> [!WARNING]
> Just like props, attributes fall through to whichever component is currently active — and that includes `role`. A role you intend for menu mode (e.g. `role="menu"`) would, under the breakpoint, land on the QDialog and replace its `role="dialog"`. If you need to declare a role, put it on an element inside the popup content (such as the wrapping QList) rather than on QPopupProxy itself.
