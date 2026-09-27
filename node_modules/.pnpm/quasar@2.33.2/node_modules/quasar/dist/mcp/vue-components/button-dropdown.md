---
title: Dropdown Button
related:
  - title: Button
    path: button.md
  - title: Button Group
    path: button-group.md
---
QBtnDropdown is a very convenient dropdown button. Goes very well with [QList](list-and-list-items.md) as dropdown content, but it's by no means limited to it.

In case you are looking for a dropdown "input" instead of "button" use [Select](select.md) instead.

## QBtnDropdown API

Not inlined here: call the `get_api` tool with `name: "QBtnDropdown"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <q-btn-dropdown
    color="primary"
    label="Dropdown Button"
    aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Photos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Videos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Articles</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

Example "Various content":

```vue
<template>
  <q-btn-dropdown class="glossy" color="purple" label="Account Settings">
    <div class="row no-wrap q-pa-md">
      <div class="column">
        <div class="text-h6 q-mb-md">Settings</div>
        <q-toggle v-model="mobileData" label="Use Mobile Data" />
        <q-toggle v-model="bluetooth" label="Bluetooth" />
      </div>

      <q-separator vertical inset class="q-mx-lg" />

      <div class="column items-center">
        <q-avatar size="72px">
          <img
            alt="User avatar"
            src="https://cdn.quasar.dev/img/boy-avatar.png"
          />
        </q-avatar>

        <div class="text-subtitle1 q-mt-md q-mb-xs">John Doe</div>

        <q-btn color="primary" label="Logout" push size="sm" v-close-popup />
      </div>
    </div>
  </q-btn-dropdown>
</template>

<script setup>
import { ref } from 'vue'

const mobileData = ref(false)
const bluetooth = ref(false)
</script>
```

### Split

```vue
<template>
  <q-btn-dropdown
    split
    class="glossy"
    color="teal"
    label="Folders"
    @click="onMainClick"
    toggle-aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="folder" color="primary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Photos</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="assignment" color="secondary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Vacation</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onMainClick() {
  console.log('Clicked on main button')
}

function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

### Hover *(v2.26+)*

The `hover` prop also opens the dropdown when the pointer hovers the button (in `split` mode: either one of the two buttons) and closes it once the pointer has left both the button and the menu. Click/tap and keyboard interactions keep toggling the dropdown as usual, so touch devices (which have no hover) simply fall back to them; this also means that clicking the toggle while the dropdown is hover-shown closes it. The one exception is a click that lands while the dropdown is still animating into view: it is ignored, so a single move-and-click gesture on the button cannot close the dropdown that the very same gesture just opened. A hover-opened dropdown does not move keyboard focus onto itself. The `hover-delay` and `hover-hide-delay` props tune the timings (see [QMenu's Hover section](menu.md#hover)).

Example "Hover":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn-dropdown color="primary" label="Hover me" hover aria-haspopup="menu">
      <q-list role="menu">
        <q-item clickable v-close-popup>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-close-popup>
          <q-item-section>
            <q-item-label>Videos</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-close-popup>
          <q-item-section>
            <q-item-label>Articles</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>

    <q-btn-dropdown
      color="secondary"
      label="Split hover"
      split
      hover
      aria-haspopup="menu"
      @click="onMainClick"
    >
      <q-list role="menu">
        <q-item clickable v-close-popup>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
          </q-item-section>
        </q-item>

        <q-item clickable v-close-popup>
          <q-item-section>
            <q-item-label>Videos</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>
  </div>
</template>

<script setup>
function onMainClick() {
  console.log('Clicked on main button')
}
</script>
```

### Customization and slots

Example "Custom button":

```vue
<template>
  <q-btn-dropdown
    split
    color="orange"
    push
    glossy
    no-caps
    icon="folder"
    label="Dropdown Button"
    @click="onMainClick"
    toggle-aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="folder" color="primary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Photos</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="assignment" color="secondary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Vacation</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onMainClick() {
  console.log('Clicked on main button')
}

function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

Example "Custom dropdown icon":

```vue
<template>
  <q-btn-dropdown
    color="pink"
    label="Dropdown Button"
    dropdown-icon="change_history"
    aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Photos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Videos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section>
          <q-item-label>Articles</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

Example "Label slot":

```vue
<template>
  <q-btn-dropdown
    split
    color="cyan"
    push
    no-caps
    @click="onMainClick"
    toggle-aria-haspopup="menu"
  >
    <template #label>
      <div class="row items-center no-wrap">
        <q-icon left name="map" />
        <div class="text-center"> Custom<br />Content </div>
      </div>
    </template>

    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="folder" color="primary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Photos</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="assignment" color="secondary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Vacation</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onMainClick() {
  console.log('Clicked on main button')
}

function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

The `toggle` slot (v2.25+) adds content to the dropdown toggle itself, next to the arrow icon. In `split` mode it is the only way to reach the toggle button — attach a [QTooltip](tooltip.md) to it below (the `label` slot covers the main button):

Example "Toggle slot":

```vue
<template>
  <q-btn-dropdown
    split
    color="primary"
    label="Save"
    @click="onMainClick"
    toggle-aria-haspopup="menu"
  >
    <template #toggle>
      <q-tooltip>More save options</q-tooltip>
    </template>

    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section>Save as...</q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section>Save a copy</q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
function onMainClick() {
  console.log('Clicked on main button')
}

function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

### Other

Example "Using v-model":

```vue
<template>
  <q-toggle v-model="menu" label="Menu state" />

  <q-btn-dropdown
    v-model="menu"
    class="glossy q-ml-lg"
    color="primary"
    label="Dropdown"
    aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="folder" color="primary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Photos</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>

      <q-item v-close-popup @click="onItemClick">
        <q-item-section avatar>
          <q-avatar icon="assignment" color="secondary" text-color="white" />
        </q-item-section>
        <q-item-section>
          <q-item-label>Vacation</q-item-label>
          <q-item-label caption>February 22, 2016</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-icon name="info" color="amber" />
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>

<script setup>
import { ref } from 'vue'

const menu = ref(false)
function onItemClick() {
  console.log('Clicked on an Item')
}
</script>
```

Example "Disable":

```vue
<template>
  <div class="row q-gutter-sm">
    <q-btn-dropdown
      disable
      class="glossy"
      color="primary"
      label="Default"
      aria-haspopup="menu"
    >
      <q-list role="menu">
        <q-item clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar icon="folder" color="primary" text-color="white" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" color="amber" />
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>

    <q-btn-dropdown
      split
      disable-main-btn
      class="glossy"
      color="primary"
      label="Only main btn"
      toggle-aria-haspopup="menu"
    >
      <q-list role="menu">
        <q-item clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar icon="folder" color="primary" text-color="white" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" color="amber" />
          </q-item-section>
        </q-item>

        <q-item clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar
              icon="assignment"
              color="secondary"
              text-color="white"
            />
          </q-item-section>
          <q-item-section>
            <q-item-label>Vacation</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" color="amber" />
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>

    <q-btn-dropdown
      split
      disable-dropdown
      class="glossy"
      color="primary"
      label="Only dropdown"
      toggle-aria-haspopup="menu"
    >
      <q-list role="menu">
        <q-item clickable v-close-popup>
          <q-item-section avatar>
            <q-avatar icon="folder" color="primary" text-color="white" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Photos</q-item-label>
            <q-item-label caption>February 22, 2016</q-item-label>
          </q-item-section>
          <q-item-section side>
            <q-icon name="info" color="amber" />
          </q-item-section>
        </q-item>
      </q-list>
    </q-btn-dropdown>
  </div>
</template>
```

The following example won't work with UMD version (so in Codepen/jsFiddle too) because it relies on the existence of Vue Router.

Example "Split and router link on main":

```vue
<template>
  <q-btn-dropdown
    split
    to="/start/pick-quasar-flavour"
    color="teal"
    rounded
    label="Go to Docs Index"
    toggle-aria-haspopup="menu"
  >
    <q-list role="menu">
      <q-item clickable v-close-popup>
        <q-item-section>
          <q-item-label>Photos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item clickable v-close-popup>
        <q-item-section>
          <q-item-label>Videos</q-item-label>
        </q-item-section>
      </q-item>

      <q-item clickable v-close-popup>
        <q-item-section>
          <q-item-label>Articles</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </q-btn-dropdown>
</template>
```

## Accessibility *(v2.25+)*

The toggle button follows the [WAI-ARIA disclosure pattern](https://www.w3.org/WAI/ARIA/apg/patterns/disclosure/): it exposes `aria-expanded` (plus `aria-controls` while the popup exists — the reference must not point to a missing id) and deliberately claims no `aria-haspopup` — that attribute's value must name the popup's ARIA role, and the dropdown content (which has no default role) can be anything. Once you give the content an actual role, mirror it with the `toggle-aria-haspopup` prop — for instance when declaring `role="menu"` on a wrapped [QList](list-and-list-items.md) (see [QMenu's Accessibility section](menu.md#accessibility)):

```html
<q-btn-dropdown label="Actions" toggle-aria-haspopup="menu">
  <q-list role="menu">
    <!-- clickable QItems become menuitems automatically -->
  </q-list>
</q-btn-dropdown>
```

The prop is the reliable way to do this in both designs: in `split` mode the fall-through attributes land on the wrapping button group rather than on the toggle button, so setting `aria-haspopup` as a plain attribute would never reach the control that opens the popup.

The toggle also carries an accessible name of its own, built from the `label` prop and the active [Quasar Language Pack](../options/quasar-language-packs.md) and following the state — `Expand "Actions"` while collapsed, `Collapse "Actions"` once open (a dropdown without a `label` falls back to a bare `Expand`/`Collapse`). Since it describes the disclosure rather than the action behind it, override it with the `toggle-aria-label` prop whenever the label alone doesn't tell the story — and note that, like `toggle-aria-haspopup`, the prop is what reaches the toggle button in `split` mode.
