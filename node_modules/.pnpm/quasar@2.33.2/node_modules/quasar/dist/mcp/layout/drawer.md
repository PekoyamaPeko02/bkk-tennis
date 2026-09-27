---
title: Layout Drawer
related:
  - title: Layout
    path: layout.md
  - title: List and List Items
    path: ../vue-components/list-and-list-items.md
---
QLayout allows you to configure your views as a 3x3 matrix, containing optional left-side and/or right-side Drawers. If you haven’t already, please read [QLayout](layout.md) documentation page first.

QDrawer is the sidebar part of your QLayout.

## QDrawer API

Not inlined here: call the `get_api` tool with `name: "QDrawer"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!TIP]
>
> - Since QDrawer needs a layout and QLayout by default manages the entire window, then for demoing purposes we are going to use containerized QLayouts. But remember that by no means you are required to use containerized QLayouts for QDrawer.
> - If the QDrawer content also has images and you want to use touch actions to close it, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

> [!NOTE]
> **Touch gestures**
>
> While the drawer is in its "mobile" behavior (the layout is below the drawer's `breakpoint`, or `behavior` is forced to "mobile"), QDrawer attaches three touch gestures, each with its own opt-out:
>
> - swiping the drawer content sideways to close it, disabled by `no-swipe-close`
> - swiping the backdrop to close it, disabled by `no-swipe-backdrop`
> - swiping in from that edge of the screen to open it, disabled by `no-swipe-open`
>
> Reach for `no-swipe-close` when your drawer holds components of your own that need to be swiped or panned. None of these gestures are attached while the drawer is in its "desktop" behavior, so setting these props there changes nothing.

> [!WARNING]
> The swipe-to-open gesture is served by an invisible strip (`.q-drawer__opener`) which is 15px wide, spans the full height of that side of the screen and sits above your page content, so it captures the pointer events landing in that band. If you have something anchored to the same edge (a QPageSticky, for instance), disable the strip with `no-swipe-open`.
>
> The strip and the backdrop claim the horizontal swipe for themselves (v2.31+): once the pan direction is known, the browser no longer runs its own edge gesture alongside it, so on iOS Safari the page stops sliding towards the previous or the next history entry while the drawer moves. A vertical swipe starting in the strip still scrolls the page.

> [!NOTE]
> While the drawer is shown in "mobile" behavior it locks the scrolling of the `<body>` element, the same way a modal does. Containerized [QLayouts](layout.md) are exempt, since they scroll their own container rather than the page.

> [!WARNING]
> When QDrawer is set into overlay mode, **it will force it to go into fixed position**, regardless if QLayout's "view" prop is configured with "l/r" or "L/R". Also, **if on iOS platform and QLayout is containerized**, the fixed position will also be forced upon QDrawer due to platform limitations that cannot be overcome.

### Basic

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle left drawer"
          flat
          @click="drawerLeft = !drawerLeft"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
        <q-btn
          aria-label="Toggle right drawer"
          flat
          @click="drawerRight = !drawerRight"
          round
          dense
          icon="menu"
        />
      </q-toolbar>
    </q-header>

    <q-drawer
      aria-label="Left drawer"
      v-model="drawerLeft"
      show-if-above
      :width="200"
      :breakpoint="700"
      elevated
      class="bg-primary text-white"
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
      show-if-above
      bordered
      :width="200"
      :breakpoint="500"
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const drawerLeft = ref(false)
const drawerRight = ref(false)
</script>
```

Consider using QItems with routing props (like `to`) below. For demoing purposes these props have not been added as it would break the UMD version.

Example "With navigation menu":

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      :width="200"
      :breakpoint="500"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <q-list role="none">
          <template v-for="(menuItem, index) in menuList" :key="index">
            <q-item clickable :active="menuItem.label === 'Outbox'" v-ripple>
              <q-item-section avatar>
                <q-icon :name="menuItem.icon" />
              </q-item-section>
              <q-item-section>
                {{ menuItem.label }}
              </q-item-section>
            </q-item>
            <q-separator :key="'sep' + index" v-if="menuItem.separator" />
          </template>
        </q-list>
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const menuList = [
  // ...
]

const drawer = ref(false)
</script>
```

Example "Seamless menu":

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated :class="$q.dark.isActive ? 'bg-primary' : 'bg-black'">
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="drawer" show-if-above :width="200" :breakpoint="500">
      <q-scroll-area class="fit">
        <q-list padding class="menu-list" role="none">
          <q-item clickable v-ripple>
            <q-item-section avatar>
              <q-icon name="inbox" />
            </q-item-section>

            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const drawer = ref(false)
</script>

<style lang="sass" scoped>
.menu-list .q-item
  border-radius: 0 32px 32px 0
</style>
```

Example "Header Picture":

```vue
<template>
  <q-layout
    view="lHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header elevated class="bg-cyan-8">
      <q-toolbar>
        <q-toolbar-title>Header</q-toolbar-title>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
      </q-toolbar>
    </q-header>

    <q-drawer v-model="drawer" show-if-above :width="200" :breakpoint="400">
      <q-scroll-area
        style="
          height: calc(100% - 150px);
          margin-top: 150px;
          border-right: 1px solid #ddd;
        "
      >
        <q-list padding role="none">
          <q-item clickable v-ripple>
            <q-item-section avatar>
              <q-icon name="inbox" />
            </q-item-section>

            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
      </q-scroll-area>

      <q-img
        class="absolute-top"
        src="https://cdn.quasar.dev/img/material.png"
        style="height: 150px"
      >
        <div class="absolute-bottom bg-transparent">
          <q-avatar size="56px" class="q-mb-sm">
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
          <div class="text-weight-bold">Razvan Stoenescu</div>
          <div>@rstoenescu</div>
        </div>
      </q-img>
    </q-drawer>

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

<script setup>
import { ref } from 'vue'

const drawer = ref(false)
</script>
```

### Desktop and mobile behavior

QDrawer runs in one of two behaviors, and a number of its props mean different things in each:

|  | "desktop" behavior | "mobile" behavior |
| --- | --- | --- |
| Space on the layout | occupies it, unless `overlay` is set | never occupies it |
| Backdrop | none | shown for as long as the drawer is |
| Touch gestures | none | attached, as described above |
| Body scroll | untouched | locked while the drawer is shown |
| `mini` mode | applies | ignored |

The `breakpoint` prop (default: 1023) decides which of the two is used: the drawer is in "mobile" behavior for as long as the width of the layout is smaller than or equal to it. Note that this is the width of the layout: the width of the window (the same one that `$q.screen` and the CSS breakpoints see, whether or not a page scrollbar is showing) or, for a containerized QLayout, the width of the container.

Set the `behavior` prop to "desktop" or "mobile" to pin the drawer into one of them regardless of the width. Its default value ("default") is the dynamic switch described above.

> [!WARNING]
> A layout can hold one drawer per side, but the two cannot be on screen at the same time while both are in "mobile" behavior, where each of them covers the page with a backdrop of its own. Showing one closes the other, which also syncs its `v-model` to `false`. In "desktop" behavior they coexist.

#### Showing it above the breakpoint

The `show-if-above` Boolean prop shows the drawer whenever the layout is in "desktop" behavior, even though its `v-model` is `false`, and it syncs that `v-model` back to `true` when it does so on the first render.

It also takes part in what happens when the layout crosses the breakpoint. Going into "mobile" behavior always hides the drawer and remembers whether it was shown, and coming back into "desktop" behavior shows it again if it was. What `show-if-above` adds is that a drawer which the user never opened comes back too. This restore is skipped when the drawer has `overlay` set or its `behavior` pinned to "mobile".

#### Persistence

While the drawer is in a dismissible state (in "mobile" behavior, or shown while in `overlay` mode), it can close itself in three ways beyond your `v-model`: the <kbd>Escape</kbd> key, a change of the app's route, and the Cordova/Capacitor back button. The `persistent` Boolean prop turns off all three.

It does not affect closing through a click on the backdrop or through a swipe, so pair it with `no-swipe-close` and `no-swipe-backdrop` for a drawer that only your `v-model` can close.

### Mini-mode

Drawer can operate in two modes: 'normal' and 'mini', and you can switch between them by using the Boolean `mini` property on QDrawer.

> [!NOTE]
> Please note that **`mini` mode** does not apply when in **mobile** behavior.

The transition played while switching between the two modes can be turned off with the `no-mini-animation` Boolean prop.

There are some CSS classes that will help you customize the drawer when dealing with "mini" mode. These are very useful especially when using the "click" trigger:

| CSS Class | Description |
| --- | --- |
| `q-mini-drawer-hide` | Hide when drawer is in "mini" mode or in "mobile" mode. |
| `q-mini-drawer-only` | Show only when drawer is in "mini" mode. |

You can also write your own CSS classes based on the fact that QDrawer has `q-drawer--standard` CSS class when in "normal" mode and `q-drawer--mini` when in "mini" mode. Also, when drawer is in "mobile" behavior, it gets `q-drawer--mobile` CSS class.

QDrawer also exposes its current width (the `width` or the `mini-width` prop, whichever applies) as the `--q-drawer-width` CSS custom property on its root element, so your own CSS can size things relative to it.

#### Mouseover/mouseout trigger

Consider using QItems with routing props (like `to`) below. For demoing purposes these props have not been added as it would break the UMD version.

Example "Mini-mode with mouseover/mouseout trigger":

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      :mini="miniState"
      @mouseenter="miniState = false"
      @mouseleave="miniState = true"
      :width="200"
      :breakpoint="500"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit" :horizontal-thumb-style="{ opacity: 0 }">
        <q-list padding role="none">
          <q-item clickable v-ripple aria-label="Inbox">
            <q-item-section avatar>
              <q-icon name="inbox" />
            </q-item-section>

            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const drawer = ref(false)
const miniState = ref(true)
</script>
```

#### Mini to overlay

The `mini-to-overlay` Boolean property will always set your drawer with fixed position, regardless of your configuration from the `view` prop, but will occupy space on the layout only as wide as when in mini-mode.

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      :mini="miniState"
      @mouseenter="miniState = false"
      @mouseleave="miniState = true"
      mini-to-overlay
      :width="200"
      :breakpoint="500"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit" :horizontal-thumb-style="{ opacity: 0 }">
        <q-list padding role="none">
          <q-item clickable v-ripple aria-label="Inbox">
            <q-item-section avatar>
              <q-icon name="inbox" />
            </q-item-section>

            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const drawer = ref(false)
const miniState = ref(true)
</script>
```

#### Click trigger

In the example below, when in "mini" mode, if the user clicks on Drawer then we switch to normal mode.

Consider using QItems with routing props (like `to`) below. For demoing purposes these props have not been added as it would break the UMD version.

Example "Mini-mode with click trigger":

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      :mini="!drawer || miniState"
      @click.capture="drawerClick"
      :width="200"
      :breakpoint="500"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit" :horizontal-thumb-style="{ opacity: 0 }">
        <q-list padding role="none">
          <q-item clickable v-ripple aria-label="Inbox">
            <q-item-section avatar>
              <q-icon name="inbox" />
            </q-item-section>

            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
      </q-scroll-area>

      <!--
        in this case, we use a button (can be anything)
        so that user can switch back
        to mini-mode
      -->
      <div
        class="q-mini-drawer-hide absolute"
        style="top: 15px; right: -17px"
      >
        <q-btn
          aria-label="Previous"
          dense
          round
          unelevated
          color="accent"
          icon="chevron_left"
          @click="miniState = true"
        />
      </div>
    </q-drawer>

    <q-page-container>
      <q-page class="q-px-lg q-py-md">
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

const drawer = ref(false)
const miniState = ref(false)

function drawerClick(e) {
  // if in "mini" state and user
  // click on drawer, we switch it to "normal" mode
  if (miniState.value) {
    miniState.value = false

    // notice we have registered an event with capture flag;
    // we need to stop further propagation as this click is
    // intended for switching drawer to "normal" mode only
    e.stopPropagation()
  }
}
</script>
```

#### Slots

By default, when in "mini" mode, Quasar CSS hides a few DOM elements to provide a neat narrow drawer. But there may certainly be use-cases where you need a deep tweak. You can use the "mini" Vue slot of QDrawer just for that. The content of this slot will replace your drawer's default content when in "mini" mode.

Example "Mini-mode with slot":

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      :mini="!drawer || miniState"
      @click.capture="drawerClick"
      :width="200"
      :breakpoint="500"
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <template #mini>
        <q-scroll-area class="fit mini-slot cursor-pointer">
          <div class="q-py-lg">
            <div class="column items-start">
              <q-icon name="inbox" color="blue" class="mini-icon" />
              <!-- ... -->
            </div>
          </div>
        </q-scroll-area>
      </template>

      <q-scroll-area class="fit">
        <q-list padding role="none">
          <q-item clickable v-ripple>
            <q-item-section> Inbox </q-item-section>
          </q-item>

          <!-- ... -->
        </q-list>
      </q-scroll-area>

      <!--
        in this case, we use a button (can be anything)
        so that user can switch back
        to mini-mode
      -->
      <div
        class="q-mini-drawer-hide absolute"
        style="top: 15px; right: -17px"
      >
        <q-btn
          aria-label="Previous"
          dense
          round
          unelevated
          color="accent"
          icon="chevron_left"
          @click="miniState = true"
        />
      </div>
    </q-drawer>

    <q-page-container>
      <q-page class="q-px-lg q-py-md">
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

const drawer = ref(false)
const miniState = ref(true)

function drawerClick(e) {
  // if in "mini" state and user
  // click on drawer, we switch it to "normal" mode
  if (miniState.value) {
    miniState.value = false

    // notice we have registered an event with capture flag;
    // we need to stop further propagation as this click is
    // intended for switching drawer to "normal" mode only
    e.stopPropagation()
  }
}
</script>

<style lang="sass" scoped>
.mini-slot
  transition: background-color .28s
  &:hover
    background-color: rgba(0, 0, 0, .04)

.mini-icon
  font-size: 1.718em
  padding: 2px 16px

  & + &
    margin-top: 18px
</style>
```

### Overlay mode

The overlay mode prevents the drawer from occupying space on the layout and rather hover over the page instead. This will always set your drawer with fixed position, regardless of your configuration from the `view` prop.

On the example below, click the menu icon to see the drawer in action. It's best viewed on a desktop with a window of at least 500px width (this is the breakpoint that is set on this demo).

```vue
<template>
  <q-layout
    view="hHh Lpr lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header
      elevated
      :class="$q.dark.isActive ? 'bg-secondary' : 'bg-black'"
    >
      <q-toolbar>
        <q-btn
          aria-label="Toggle drawer"
          flat
          @click="drawer = !drawer"
          round
          dense
          icon="menu"
        />
        <q-toolbar-title>Header</q-toolbar-title>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      :width="200"
      :breakpoint="500"
      overlay
      bordered
      :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
    >
      <q-scroll-area class="fit">
        <q-list role="none">
          <template v-for="(menuItem, index) in menuList" :key="index">
            <q-item clickable :active="menuItem.label === 'Outbox'" v-ripple>
              <q-item-section avatar>
                <q-icon :name="menuItem.icon" />
              </q-item-section>
              <q-item-section>
                {{ menuItem.label }}
              </q-item-section>
            </q-item>
            <q-separator :key="'sep' + index" v-if="menuItem.separator" />
          </template>
        </q-list>
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
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const menuList = [
  // ...
]

const drawer = ref(false)
</script>
```

## Accessibility *(v2.25+)*

QDrawer renders its panel as a real `<aside>` element, so it is exposed to assistive technology as a complementary landmark of your [QLayout](layout.md#accessibility). The backdrop shown in its overlay states and the invisible swipe-opener strip along the screen edge are hidden from assistive technology, since they are redundant, pointer-only affordances. A closed drawer leaves the Tab order and the accessibility tree entirely, so nothing invisible stays reachable.

A `role` or any `aria-*` attribute set on QDrawer is applied to the `<aside>` element itself, since that is the element assistive technology interacts with. Give it an `aria-label` (or `aria-labelledby`) so that multiple drawers can be told apart, or a `role` when complementary does not fit, one [allowed on `aside`](https://www.w3.org/TR/html-aria/#el-aside), like `region`, `search` or `none`. All other fall-through attributes keep targeting the inner scrolling element.

### Keyboard dismissal

While the drawer is in a dismissible state (below its breakpoint or shown in overlay mode), hitting the <kbd>Escape</kbd> key closes it, the keyboard counterpart of the backdrop click and the swipe gesture. The `persistent` prop opts out of it, and an `escape-key` event is emitted whenever the key is handled.

### Your responsibilities

Be aware that in its overlay states the drawer only looks modal: it does not trap or move keyboard focus, so the page behind the backdrop remains keyboard-reachable and readable by screen readers. If your use case calls for it, move focus into the drawer yourself when opening it. The toggle button is app-provided too, so it should manage its own `aria-expanded` state.

When a drawer holds your primary navigation, wrap the menu inside it in a `<nav>` element (or add `role="navigation"`) and give it an `aria-label`, so it is announced as a navigation landmark distinct from the surrounding `aside`.
