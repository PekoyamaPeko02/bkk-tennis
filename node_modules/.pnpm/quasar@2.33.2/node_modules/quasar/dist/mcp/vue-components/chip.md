---
title: Chip
related:
  - title: Avatar
    path: avatar.md
  - title: Icon
    path: icon.md
  - title: Badge
    path: badge.md
---
The QChip component is basically a simple UI block entity, representing for example more advanced underlying data, such as a contact, in a compact way.

Chips can contain entities such as an avatar, text or an icon, optionally having a pointer too. They can also be closed or removed if configured so.

> [!NOTE]
> Also check out [QBadge](badge.md).

## QChip API

Not inlined here: call the `get_api` tool with `name: "QChip"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div class="q-gutter-md">
    <div>
      <q-chip icon="event">Add to calendar</q-chip>
      <q-chip icon="bookmark">Bookmark</q-chip>
      <q-chip icon="alarm" label="Set alarm" />
      <q-chip class="glossy" icon="directions">Get directions</q-chip>
    </div>
    <div>
      <q-chip color="primary" text-color="white" icon="event">
        Add to calendar
      </q-chip>
      <q-chip color="teal" text-color="white" icon="bookmark">
        Bookmark
      </q-chip>
      <q-chip
        class="glossy"
        color="orange"
        text-color="white"
        icon-right="star"
      >
        Star
      </q-chip>
      <q-chip color="red" text-color="white" icon="alarm" label="Set alarm" />
      <q-chip color="deep-orange" text-color="white" icon="directions">
        Get directions
      </q-chip>
      <q-chip>
        <q-avatar icon="bookmark" color="red" text-color="white" />
        Bookmark
      </q-chip>
      <q-chip>
        <q-avatar color="red" text-color="white">50</q-avatar>
        Emails
      </q-chip>
      <q-chip>
        <q-avatar>
          <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar5.jpg" />
        </q-avatar>
        John
      </q-chip>
    </div>
  </div>
</template>
```

Example "Dense":

```vue
<template>
  <div class="q-gutter-md">
    <div>
      <q-chip dense icon="event">Add to calendar</q-chip>
      <q-chip dense icon="bookmark">Bookmark</q-chip>
      <q-chip dense icon="alarm" label="Set alarm" />
      <q-chip dense icon="directions">Get directions</q-chip>
    </div>
    <div>
      <q-chip dense color="primary" text-color="white" icon="event">
        Add to calendar
      </q-chip>
      <q-chip dense color="teal" text-color="white" icon="bookmark">
        Bookmark
      </q-chip>
      <q-chip dense color="orange" text-color="white" icon-right="star">
        Star
      </q-chip>
      <q-chip
        dense
        color="red"
        text-color="white"
        icon="alarm"
        label="Set alarm"
      />
      <q-chip dense color="deep-orange" text-color="white" icon="directions">
        Get directions
      </q-chip>
      <q-chip dense>
        <q-avatar icon="bookmark" color="red" text-color="white" />
        Bookmark
      </q-chip>
      <q-chip dense>
        <q-avatar color="red" text-color="white">50</q-avatar>
        Emails
      </q-chip>
      <q-chip dense>
        <q-avatar>
          <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar3.jpg" />
        </q-avatar>
        Mary
      </q-chip>
    </div>
  </div>
</template>
```

Example "Custom size":

```vue
<template>
  <div class="q-gutter-md">
    <div>
      <q-chip size="18px" icon="bookmark"> Bookmark </q-chip>
    </div>

    <div>
      <q-chip size="xs" icon="bookmark"> Bookmark </q-chip>

      <q-chip size="sm" icon="bookmark"> Bookmark </q-chip>

      <q-chip size="md" icon="bookmark"> Bookmark </q-chip>

      <q-chip size="lg" icon="bookmark"> Bookmark </q-chip>

      <q-chip size="xl" icon="bookmark"> Bookmark </q-chip>
    </div>

    <div>
      <q-chip dense size="xs" icon="bookmark"> Bookmark </q-chip>

      <q-chip dense size="sm" icon="bookmark"> Bookmark </q-chip>

      <q-chip dense size="md" icon="bookmark"> Bookmark </q-chip>

      <q-chip dense size="lg" icon="bookmark"> Bookmark </q-chip>

      <q-chip dense size="xl" icon="bookmark"> Bookmark </q-chip>
    </div>
  </div>
</template>
```

Example "Square":

```vue
<template>
  <div class="q-gutter-md">
    <div>
      <q-chip square icon="event">Add to calendar</q-chip>
      <q-chip class="glossy" square icon="bookmark">Bookmark</q-chip>
      <q-chip square icon="alarm" label="Set alarm" />
      <q-chip square icon="directions">Get directions</q-chip>
    </div>
    <div>
      <q-chip square color="primary" text-color="white" icon="event">
        Add to calendar
      </q-chip>
      <q-chip
        class="glossy"
        square
        color="teal"
        text-color="white"
        icon="bookmark"
      >
        Bookmark
      </q-chip>
      <q-chip square color="orange" text-color="white" icon-right="star">
        Star
      </q-chip>
      <q-chip
        square
        color="red"
        text-color="white"
        icon="alarm"
        label="Set alarm"
      />
      <q-chip square color="deep-orange" text-color="white" icon="directions">
        Get directions
      </q-chip>
      <q-chip square>
        <q-avatar icon="bookmark" color="red" text-color="white" />
        Bookmark
      </q-chip>
      <q-chip square>
        <q-avatar color="red" text-color="white">50</q-avatar>
        Emails
      </q-chip>
      <q-chip square>
        <q-avatar>
          <img
            alt="User avatar"
            src="https://cdn.quasar.dev/img/boy-avatar.png"
          />
        </q-avatar>
        John
      </q-chip>
    </div>
  </div>
</template>
```

Example "Outline":

```vue
<template>
  <q-chip outline color="primary" text-color="white" icon="event">
    Add to calendar
  </q-chip>
  <q-chip outline color="teal" text-color="white" icon="bookmark">
    Bookmark
  </q-chip>
  <q-chip outline color="orange" text-color="white" icon-right="star">
    Star
  </q-chip>
  <q-chip
    outline
    square
    color="red"
    text-color="white"
    icon="alarm"
    label="Set alarm"
  />
  <q-chip
    outline
    square
    color="deep-orange"
    text-color="white"
    icon="directions"
  >
    Get directions
  </q-chip>
</template>
```

A QChip with a `@click` listener is clickable by default (v2.29+): it gets the hover effects, keyboard activation and its `click` event without the `clickable` prop. Set `clickable` explicitly only when there is no listener or when you need to toggle the behavior through a boolean; an explicit `clickable="false"` wins over the listener.

Example "Clickable":

```vue
<template>
  <q-chip @click="onClick" color="primary" text-color="white" icon="event">
    Add to calendar
  </q-chip>
  <q-chip @click="onClick" icon="bookmark"> Bookmark </q-chip>
  <q-chip @click="onClick" color="teal" text-color="white" icon="bookmark">
    Bookmark
  </q-chip>
  <q-chip
    @click="onClick"
    color="red"
    text-color="white"
    icon="alarm"
    label="Set alarm"
  />
  <q-chip
    @click="onClick"
    color="orange"
    text-color="white"
    icon="directions"
  >
    Get directions
  </q-chip>
</template>

<script setup>
function onClick() {
  console.log('Clicked on a QChip')
}
</script>
```

Example "Selected":

```vue
<template>
  <div class="q-gutter-xs">
    <q-chip
      v-model:selected="desert.Icecream"
      color="primary"
      text-color="white"
      icon="cake"
    >
      Ice cream
    </q-chip>
    <!-- ... -->
  </div>

  <div class="q-mt-sm"> Your pick: {{ selection }} </div>
</template>

<script setup>
import { computed, reactive } from 'vue'

const desert = reactive({
  Icecream: false,
  Eclair: true,
  Cupcake: false,
  Gingerbread: false
})

const selection = computed(() =>
  Object.keys(desert)
    .filter(type => desert[type])
    .join(', ')
)
</script>
```

Example "Removable":

```vue
<template>
  <div class="q-gutter-xs">
    <q-chip
      removable
      v-model="icecream"
      @remove="log('Icecream')"
      color="primary"
      text-color="white"
      icon="cake"
    >
      Ice cream
    </q-chip>
    <!-- ... -->
    <q-chip
      disable
      removable
      v-model="gingerbread"
      @remove="log('Icecream')"
      color="red"
      text-color="white"
      icon="cake"
    >
      Gingerbread (disable)
    </q-chip>
  </div>

  <q-btn
    color="primary"
    label="Reset"
    @click="onResetClick"
    class="q-mt-sm"
  />
</template>

<script setup>
import { ref } from 'vue'

const icecream = ref(true)
const eclair = ref(true)
const cupcake = ref(true)
const gingerbread = ref(true)

function onResetClick() {
  icecream.value = true
  eclair.value = true
  cupcake.value = true
  gingerbread.value = true
}

function log(desert) {
  console.log(`${desert} has been removed`)
}
</script>
```

Example "Long label truncation":

```vue
<template>
  <div
    class="q-gutter-xs row"
    style="max-width: 300px"
    :class="{ 'truncate-chip-labels': truncate }"
  >
    <q-chip
      removable
      v-model="vanilla"
      color="primary"
      text-color="white"
      icon="cake"
      :label="vanillaLabel"
      :title="vanillaLabel"
    />
    <q-chip
      removable
      v-model="chocolate"
      color="teal"
      text-color="white"
      icon="cake"
      :label="chocolateLabel"
    >
      <q-tooltip>{{ chocolateLabel }}</q-tooltip>
    </q-chip>
    <q-chip
      removable
      v-model="strawberry"
      color="orange"
      text-color="white"
      icon="cake"
    >
      <div class="ellipsis">
        {{ strawberryLabel }}
        <q-tooltip>{{ strawberryLabel }}</q-tooltip>
      </div>
    </q-chip>
    <q-chip removable v-model="cookies" color="red" text-color="white">
      <q-avatar>
        <img
          alt="User avatar"
          src="https://cdn.quasar.dev/img/boy-avatar.png"
        />
      </q-avatar>
      <div class="ellipsis">
        {{ cookiesLabel }}
        <q-tooltip>{{ cookiesLabel }}</q-tooltip>
      </div>
    </q-chip>
  </div>

  <div class="row items-center q-mt-sm">
    <q-btn
      color="primary"
      label="Reset"
      @click="onResetClick"
      class="q-mr-sm"
    />
    <q-toggle v-model="truncate" label="Truncate labels" />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const vanilla = ref(true)
const chocolate = ref(true)
const strawberry = ref(true)
const cookies = ref(true)

const truncate = ref(true)

const vanillaLabel = 'I want vanilla flavoured ice cream'
const chocolateLabel = 'I want chocolate flavoured ice cream'
const strawberryLabel = 'I want strawberry flavoured ice cream'
const cookiesLabel = 'I want cookies flavoured ice cream'

function onResetClick() {
  vanilla.value = true
  chocolate.value = true
  strawberry.value = true
  cookies.value = true
}
</script>

<style lang="sass" scoped>
.truncate-chip-labels > .q-chip
  max-width: 140px
</style>
```

## Accessibility *(v2.25+)*

A clickable chip (through the `clickable` prop, a `@click` listener or a `selected` model) exposes itself as `role="button"` and activates on <kbd>Enter</kbd> or <kbd>Space</kbd>. Only chips driven by a `selected` model additionally expose `aria-pressed` — a plain action chip does not claim toggle semantics. A disabled chip keeps its role (announced as dimmed via `aria-disabled`) but is taken out of the tab order.

The remove icon of a `removable` chip is a keyboard-operable control of its own, named by the localized "Remove" label from the [Quasar Language Pack](../options/quasar-language-packs.md). That generic name says nothing about what would be removed, so set `remove-aria-label` per chip for context — e.g. "Remove tag: Vue".
