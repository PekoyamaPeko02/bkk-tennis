---
title: Floating Action Button
related:
  - title: Layout
    path: ../layout/layout.md
  - title: Layout Page
    path: ../layout/page.md
---
A Floating Action Button (FAB) represents the primary action in a Page. But, it's not limited to only a single action. It can contain any number of sub-actions too. And more importantly, it can also be used inline in your Pages or Layouts.

Note that you don’t need a QLayout to use FABs.

## QFab API

Not inlined here: call the `get_api` tool with `name: "QFab"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## QFabAction API

Not inlined here: call the `get_api` tool with `name: "QFabAction"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

There are two types of FABs: expandable (has sub-actions) and non-expandable.

> [!NOTE]
> For an exhausting list of options, please read the API cards (at the top of this page).

### Non-Expandable

If you want a non-expandable FAB, all you need is a round button – wrapped in QPageSticky if used on a QLayout.

```vue
<template>
  <q-layout
    view="lhh LpR lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header class="bg-black">
      <q-toolbar>
        <q-btn flat round dense icon="menu" />
        <q-toolbar-title>Header</q-toolbar-title>
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

        <q-page-sticky position="bottom-right" :offset="[18, 18]">
          <q-btn fab icon="add" color="accent" />
        </q-page-sticky>
      </q-page>
    </q-page-container>
  </q-layout>
</template>
```

### Expandable

```vue
<template>
  <div
    class="column items-center"
    style="margin-top: 100px; margin-bottom: 100px"
  >
    <q-fab color="purple" icon="keyboard_arrow_up" direction="up">
      <q-fab-action color="primary" @click="onClick" icon="mail" />
      <q-fab-action color="secondary" @click="onClick" icon="alarm" />
    </q-fab>

    <br />

    <q-fab
      color="amber"
      text-color="black"
      icon="keyboard_arrow_left"
      direction="left"
    >
      <q-fab-action
        color="amber"
        text-color="black"
        @click="onClick"
        icon="mail"
      />
      <q-fab-action
        color="amber"
        text-color="black"
        @click="onClick"
        icon="alarm"
      />
    </q-fab>

    <br />

    <q-fab
      color="secondary"
      push
      icon="keyboard_arrow_right"
      direction="right"
    >
      <q-fab-action color="primary" @click="onClick" icon="mail" />
      <q-fab-action color="accent" @click="onClick" icon="alarm" />
    </q-fab>

    <br />

    <q-fab color="accent" glossy icon="keyboard_arrow_down" direction="down">
      <q-fab-action
        color="amber"
        text-color="black"
        @click="onClick"
        icon="mail"
      />
      <q-fab-action
        color="amber"
        text-color="black"
        @click="onClick"
        icon="alarm"
      />
    </q-fab>
  </div>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Hover *(v2.27+)*

With the `hover` prop the FAB also opens when the pointer hovers it and closes once the pointer has left both the main button and the actions. The `hover-hide-delay` prop controls the grace period in which the pointer can travel between the two (or return) before the FAB closes, while `hover-delay` postpones the opening.

Click/tap and keyboard interactions keep toggling the FAB as usual, so touch devices (which have no hover) simply fall back to them; this also means that clicking the main button (or activating it with <kbd>Enter</kbd>) while the FAB is hover-shown closes it. The one exception is a click that lands while the actions are still animating into view: it is ignored, so a single move-and-click gesture cannot close the FAB that the very same gesture just opened.

Example "Hover":

```vue
<template>
  <div class="row justify-center q-gutter-xl" style="margin: 100px 0">
    <q-fab hover color="primary" icon="keyboard_arrow_left" direction="left">
      <q-fab-action color="primary" @click="onClick" icon="mail" />
      <q-fab-action color="secondary" @click="onClick" icon="alarm" />
    </q-fab>

    <q-fab
      hover
      :hover-delay="300"
      :hover-hide-delay="600"
      color="purple"
      icon="keyboard_arrow_up"
      direction="up"
    >
      <q-fab-action color="primary" @click="onClick" icon="mail" />
      <q-fab-action color="secondary" @click="onClick" icon="alarm" />
    </q-fab>
  </div>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Internal labels

Example "Internal label":

```vue
<template>
  <div style="padding-bottom: 220px">
    <div>
      <q-fab
        v-model="fab1"
        label="Actions"
        label-position="left"
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
      >
        <q-fab-action
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <q-fab-action
          color="secondary"
          @click="onClick"
          icon="alarm"
          label="Alarm"
        />
      </q-fab>
    </div>

    <div class="q-mt-md">
      <q-fab
        v-model="fab2"
        label="Actions"
        vertical-actions-align="left"
        color="purple"
        icon="keyboard_arrow_down"
        direction="down"
      >
        <q-fab-action
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

Example "Toggling internal label":

```vue
<template>
  <div style="padding-bottom: 240px">
    <q-toggle v-model="hideLabels" label="Hide labels" />

    <div class="q-mt-md">
      <q-fab
        v-model="fab1"
        label="Actions"
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
        :hide-label="hideLabels"
      >
        <q-fab-action
          :hide-label="hideLabels"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <q-fab-action
          :hide-label="hideLabels"
          color="secondary"
          @click="onClick"
          icon="alarm"
          label="Alarm"
        />
      </q-fab>
    </div>

    <div class="q-mt-md row justify-center">
      <q-fab
        v-model="fab2"
        label="Actions"
        label-position="bottom"
        glossy
        color="purple"
        icon="keyboard_arrow_down"
        direction="down"
        :hide-label="hideLabels"
      >
        <q-fab-action
          :hide-label="hideLabels"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)
const hideLabels = ref(false)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

When the labels are internal and your QFab opens up vertically (up or down) then you also have the ability to choose how to vertically align the sub-actions:

Example "Vertical actions alignment":

```vue
<template>
  <div style="padding-top: 220px">
    <div class="row justify-between">
      <q-fab
        v-model="fabLeft"
        vertical-actions-align="left"
        color="primary"
        glossy
        icon="keyboard_arrow_up"
        direction="up"
      >
        <q-fab-action
          label-position="right"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>

      <q-fab
        v-model="fabCenter"
        vertical-actions-align="center"
        color="primary"
        glossy
        icon="keyboard_arrow_up"
        direction="up"
      >
        <q-fab-action
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>

      <q-fab
        v-model="fabRight"
        vertical-actions-align="right"
        color="primary"
        glossy
        icon="keyboard_arrow_up"
        direction="up"
      >
        <q-fab-action
          label-position="left"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fabLeft = ref(true)
const fabCenter = ref(true)
const fabRight = ref(true)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### External labels

By default, when the label is external on the main QFab (not the sub-actions), it gets shown only when QFab is opened. However, you can override that by setting a Boolean value for `hide-label` prop.

Example "External label":

```vue
<template>
  <div style="padding-top: 48px; padding-bottom: 220px">
    <div>
      <q-fab
        v-model="fab1"
        label="Actions"
        label-position="top"
        external-label
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
      >
        <q-fab-action
          external-label
          label-position="top"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>

    <div class="q-mt-md">
      <q-fab
        v-model="fab2"
        label="Actions"
        external-label
        vertical-actions-align="left"
        color="purple"
        icon="keyboard_arrow_down"
        direction="down"
      >
        <q-fab-action
          external-label
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

Example "Custom styled external label":

```vue
<template>
  <div style="padding-top: 48px; padding-bottom: 220px">
    <div>
      <q-fab
        v-model="fab1"
        label="Actions"
        label-position="top"
        label-class="bg-grey-3 text-purple"
        external-label
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
      >
        <q-fab-action
          label-class="bg-grey-3 text-grey-8"
          external-label
          label-position="top"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>

    <div class="q-mt-md">
      <q-fab
        v-model="fab2"
        label="Actions"
        external-label
        label-class="bg-grey-3 text-purple"
        vertical-actions-align="left"
        color="purple"
        icon="keyboard_arrow_down"
        direction="down"
      >
        <q-fab-action
          label-class="bg-grey-3 text-grey-8"
          external-label
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

Example "Toggling external label":

```vue
<template>
  <div style="padding-bottom: 220px">
    <q-toggle v-model="hideLabels" label="Hide labels" />

    <div style="padding-top: 48px">
      <q-fab
        v-model="fab1"
        label="Actions"
        label-position="top"
        external-label
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
        :hide-label="hideLabels"
      >
        <q-fab-action
          :hide-label="hideLabels"
          external-label
          label-position="top"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>

    <div class="q-mt-md">
      <q-fab
        v-model="fab2"
        label="Actions"
        external-label
        vertical-actions-align="left"
        color="purple"
        icon="keyboard_arrow_down"
        direction="down"
        :hide-label="hideLabels"
      >
        <q-fab-action
          :hide-label="hideLabels"
          external-label
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)
const hideLabels = ref(false)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Hide icons

If we hide the icon (through specific prop), we should at least use an internal label:

Example "Hide icon":

```vue
<template>
  <q-fab
    v-model="fab"
    label="Actions"
    label-position="left"
    color="purple"
    hide-icon
    direction="right"
  >
    <q-fab-action color="primary" @click="onClick" hide-icon label="Email" />
    <q-fab-action
      color="secondary"
      @click="onClick"
      hide-icon
      label="Alarm"
    />
  </q-fab>
</template>

<script setup>
import { ref } from 'vue'

const fab = ref(true)
function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Padding

The default padding for QFab is "md" and for QFabAction is "sm". However, you can use `padding` prop to customize it (accepts CSS units too):

Example "Playing with padding":

```vue
<template>
  <div style="padding-top: 48px; padding-bottom: 220px">
    <div>
      <q-fab
        v-model="fab1"
        label="Actions"
        label-position="top"
        external-label
        color="purple"
        icon="keyboard_arrow_right"
        direction="right"
        padding="xs"
      >
        <q-fab-action
          padding="5px"
          external-label
          label-position="top"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <q-fab-action
          padding="5px"
          external-label
          label-position="top"
          color="orange"
          @click="onClick"
          icon="room"
          label="Map"
        />
      </q-fab>
    </div>

    <div class="q-mt-lg">
      <q-fab
        v-model="fab2"
        label="Actions"
        vertical-actions-align="left"
        color="purple"
        padding="none xl"
        icon="keyboard_arrow_down"
        direction="down"
      >
        <q-fab-action
          padding="3px"
          external-label
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(false)
const fab2 = ref(false)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Square style

```vue
<template>
  <div style="padding-top: 48px; padding-bottom: 220px">
    <div>
      <q-fab
        v-model="fab1"
        color="primary"
        glossy
        icon="keyboard_arrow_right"
        direction="right"
      >
        <q-fab-action
          square
          external-label
          label-position="top"
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
        />
        <!-- ... -->
      </q-fab>
    </div>

    <div class="q-mt-md row justify-center">
      <q-fab
        v-model="fab2"
        square
        vertical-actions-align="right"
        color="secondary"
        glossy
        icon="keyboard_arrow_down"
        direction="down"
      >
        <q-fab-action
          square
          color="primary"
          @click="onClick"
          icon="mail"
          label="Email"
          label-position="left"
        />
        <!-- ... -->
        <q-fab-action
          glossy
          square
          color="orange"
          @click="onClick"
          icon="airplay"
          label="Airplay"
          label-position="left"
        />
        <!-- ... -->
      </q-fab>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const fab1 = ref(true)
const fab2 = ref(true)

function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Animating the actions *(v2.30+)*

The sub-actions stay in the DOM at all times. Opening the FAB toggles an `--opened` / `--closed` state class on their container and each action fades and scales into place from there, one after the other. The `stagger` prop sets how many milliseconds apart consecutive actions animate: the cascade runs outwards from the main button when opening and back towards it when closing, whatever the `direction` is. Use `0` to animate them all at once.

Example "Staggered actions":

```vue
<template>
  <div class="row justify-center q-gutter-xl" style="margin: 260px 0 20px">
    <q-fab hover color="purple" icon="keyboard_arrow_up" direction="up">
      <q-fab-action
        color="primary"
        @click="onClick"
        icon="mail"
        label="Email"
      />
      <q-fab-action
        color="secondary"
        @click="onClick"
        icon="alarm"
        label="Alarm"
      />
      <q-fab-action
        color="orange"
        @click="onClick"
        icon="airplay"
        label="Airplay"
      />
      <q-fab-action color="accent" @click="onClick" icon="room" label="Map" />
    </q-fab>

    <q-fab
      :stagger="0"
      hover
      color="purple"
      icon="keyboard_arrow_up"
      direction="up"
    >
      <q-fab-action
        color="primary"
        @click="onClick"
        icon="mail"
        label="Email"
      />
      <q-fab-action
        color="secondary"
        @click="onClick"
        icon="alarm"
        label="Alarm"
      />
      <q-fab-action
        color="orange"
        @click="onClick"
        icon="airplay"
        label="Airplay"
      />
      <q-fab-action color="accent" @click="onClick" icon="room" label="Map" />
    </q-fab>

    <q-fab
      :stagger="120"
      hover
      color="purple"
      icon="keyboard_arrow_up"
      direction="up"
    >
      <q-fab-action
        color="primary"
        @click="onClick"
        icon="mail"
        label="Email"
      />
      <q-fab-action
        color="secondary"
        @click="onClick"
        icon="alarm"
        label="Alarm"
      />
      <q-fab-action
        color="orange"
        @click="onClick"
        icon="airplay"
        label="Airplay"
      />
      <q-fab-action color="accent" @click="onClick" icon="room" label="Map" />
    </q-fab>
  </div>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Slots

Notice the slots for QFab and the slots for QFabAction below:

Example "Slots: icon, active-icon and label":

```vue
<template>
  <div
    class="column items-center"
    style="margin-top: 100px; margin-bottom: 100px"
  >
    <q-fab color="purple" direction="up">
      <template #icon="{ opened }">
        <q-icon
          :class="{ 'example-fab-animate--hover': !opened }"
          name="keyboard_arrow_up"
        />
      </template>

      <template #active-icon="{ opened }">
        <q-icon :class="{ 'example-fab-animate': opened }" name="close" />
      </template>

      <q-fab-action color="primary" external-label @click="onClick">
        <template #icon>
          <q-icon name="mail" />
        </template>
        <template #label> Mail </template>
      </q-fab-action>

      <q-fab-action
        color="secondary"
        external-label
        @click="onClick"
        icon="alarm"
        label="Alarm"
      />
    </q-fab>

    <br />

    <q-fab
      color="amber"
      text-color="black"
      icon="keyboard_arrow_left"
      direction="left"
    >
      <template #label="{ opened }">
        <div :class="{ 'example-fab-animate--hover': !opened }">
          {{ opened ? 'Close' : 'Open' }}
        </div>
      </template>

      <q-fab-action color="primary" @click="onClick" icon="mail" />
      <q-fab-action color="secondary" @click="onClick" icon="alarm" />
    </q-fab>
  </div>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a fab action')
}
</script>

<style lang="sass" scoped>
.example-fab-animate,
.q-fab:hover .example-fab-animate--hover
  animation: example-fab-animate 0.82s cubic-bezier(.36,.07,.19,.97) both
  transform: translate3d(0, 0, 0)
  backface-visibility: hidden
  perspective: 1000px

@keyframes example-fab-animate
  10%, 90%
    transform: translate3d(-1px, 0, 0)

  20%, 80%
    transform: translate3d(2px, 0, 0)

  30%, 50%, 70%
    transform: translate3d(-4px, 0, 0)

  40%, 60%
    transform: translate3d(4px, 0, 0)
</style>
```

### With QPageSticky

```vue
<template>
  <q-layout
    view="lhh LpR lff"
    container
    style="height: 300px"
    class="shadow-2 rounded-borders"
  >
    <q-header reveal class="bg-black">
      <q-toolbar>
        <q-btn flat round dense icon="menu" />
        <q-toolbar-title>Header</q-toolbar-title>
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

        <q-page-sticky position="bottom-right" :offset="[18, 18]">
          <q-fab icon="add" direction="up" color="accent">
            <q-fab-action
              @click="onClick"
              color="primary"
              icon="person_add"
            />
            <q-fab-action @click="onClick" color="primary" icon="mail" />
          </q-fab>
        </q-page-sticky>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a fab action')
}
</script>
```

### Draggable

Below is a nice example of using [TouchPan](../vue-directives/touch-pan.md) for making the QFab draggable across the screen.

```vue
<template>
  <q-layout
    view="lhh LpR lff"
    container
    style="height: 500px"
    class="shadow-2 rounded-borders"
  >
    <q-page-container>
      <q-page padding>
        <p v-for="n in 15" :key="n">
          Lorem ipsum dolor sit amet consectetur adipisicing elit. Fugit nihil
          praesentium molestias a adipisci, dolore vitae odit, quidem
          consequatur optio voluptates asperiores pariatur eos numquam rerum
          delectus commodi perferendis voluptate?
        </p>

        <q-page-sticky position="bottom-right" :offset="fabPos">
          <q-fab
            icon="add"
            direction="up"
            color="accent"
            :disable="draggingFab"
            v-touch-pan.prevent.mouse="moveFab"
          >
            <q-fab-action
              @click="onClick"
              color="primary"
              icon="person_add"
              :disable="draggingFab"
            />
            <q-fab-action
              @click="onClick"
              color="primary"
              icon="mail"
              :disable="draggingFab"
            />
          </q-fab>
        </q-page-sticky>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import { ref } from 'vue'

const fabPos = ref([18, 18])
const draggingFab = ref(false)

function onClick() {
  console.log('Clicked on a fab action')
}

function moveFab(ev) {
  draggingFab.value = ev.isFirst !== true && ev.isFinal !== true

  fabPos.value = [fabPos.value[0] - ev.delta.x, fabPos.value[1] - ev.delta.y]
}
</script>
```

## Accessibility *(v2.25+)*

The QFab trigger is a button carrying `aria-expanded` and `aria-controls` pointing at its actions container, so screen readers can tell whether the sub-actions are open. While closed, the actions are hidden from assistive technology and removed from the tab order; activating an action closes the FAB and returns focus to the trigger.

A visible `label` doubles as the accessible name of the trigger or of an action; give icon-only ones an `aria-label`, which falls through to the rendered button. Keyboard support covers opening/closing the FAB with <kbd>Enter</kbd> / <kbd>Space</kbd> and Tab-bing through the open actions only — there is no <kbd>Escape</kbd>-to-close and no arrow-key navigation between actions.
