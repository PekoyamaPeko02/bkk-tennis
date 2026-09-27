---
title: Expansion Item
related:
  - title: List and List Items
    path: list-and-list-items.md
  - title: Slide Item
    path: slide-item.md
  - title: Slide Transition
    path: slide-transition.md
---
The QExpansionItem component allows the hiding of content that is not immediately relevant to the user. Think of them as accordion elements that expand when clicked on. It's also known as a collapsible.

They are basically [QItem](list-and-list-items.md) components wrapped with additional functionality. So they can be included in QLists and inherit QItem component properties.

## QExpansionItem API

Not inlined here: call the `get_api` tool with `name: "QExpansionItem"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered class="rounded-borders">
      <q-expansion-item
        expand-separator
        icon="perm_identity"
        label="Account settings"
        caption="John Doe"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        expand-separator
        icon="signal_wifi_off"
        label="Wifi settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        expand-separator
        icon="drafts"
        label="Drafts"
        header-class="text-purple"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item icon="assessment" label="Disabled" disable>
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

### Controlling expansion state

```vue
<template>
  <div style="max-width: 350px">
    <q-toggle v-model="expanded" label="Expanded" class="q-mb-md" />

    <q-expansion-item
      v-model="expanded"
      icon="perm_identity"
      label="Account settings"
      caption="John Doe"
    >
      <q-card>
        <q-card-section>
          Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem, eius
          reprehenderit eos corrupti commodi magni quaerat ex numquam, dolorum
          officiis modi facere maiores architecto suscipit iste eveniet
          doloribus ullam aliquid.
        </q-card-section>
      </q-card>
    </q-expansion-item>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const expanded = ref(false)
</script>
```

### Style

Example "Dense":

```vue
<template>
  <div style="max-width: 350px">
    <q-list padding bordered class="rounded-borders">
      <q-expansion-item
        dense
        dense-toggle
        expand-separator
        icon="perm_identity"
        label="Account settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        dense
        dense-toggle
        expand-separator
        icon="signal_wifi_off"
        label="Wifi settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        dense
        dense-toggle
        expand-separator
        icon="drafts"
        label="Drafts"
        header-class="text-purple"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

Example "Force dark mode":

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-list
      dark
      padding
      bordered
      class="rounded-borders"
      style="max-width: 328px"
    >
      <q-expansion-item icon="perm_identity" label="Account settings">
        <q-card class="bg-grey-9">
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item icon="signal_wifi_off" label="Wifi settings">
        <q-card class="bg-grey-9">
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item icon="drafts" label="Drafts" header-class="text-orange">
        <q-card class="bg-grey-9">
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

### Options

Example "Switch toggle side":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered class="rounded-borders">
      <q-expansion-item
        switch-toggle-side
        expand-separator
        icon="perm_identity"
        label="Account settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        switch-toggle-side
        expand-separator
        icon="signal_wifi_off"
        label="Wifi settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>

    <q-list bordered class="rounded-borders q-mt-md">
      <q-expansion-item
        dense-toggle
        switch-toggle-side
        expand-separator
        icon="perm_identity"
        label="Account settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        dense-toggle
        switch-toggle-side
        expand-separator
        icon="signal_wifi_off"
        label="Wifi settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

Example "Header slot":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered class="rounded-borders">
      <q-expansion-item>
        <template #header>
          <q-item-section avatar>
            <q-avatar icon="bluetooth" color="primary" text-color="white" />
          </q-item-section>

          <q-item-section> Bluetooth technology </q-item-section>

          <q-item-section side>
            <div class="row items-center">
              <q-icon name="star" color="red" size="24px" />
              <q-icon name="star" color="red" size="24px" />
              <q-icon name="star" color="red" size="24px" />
            </div>
          </q-item-section>
        </template>

        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-separator />

      <q-expansion-item>
        <template #header="{ expanded }">
          <q-item-section avatar>
            <q-avatar>
              <img
                alt="User avatar"
                src="https://cdn.quasar.dev/img/boy-avatar.png"
              />
            </q-avatar>
          </q-item-section>

          <q-item-section>
            Item {{ expanded ? 'expanded' : 'collapsed' }}
          </q-item-section>
        </template>

        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-separator />

      <q-expansion-item>
        <template #header>
          <q-item-section avatar>
            <q-icon color="purple" name="signal_wifi_off" />
          </q-item-section>

          <q-item-section> Wifi settings </q-item-section>
        </template>

        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

Example "Handling events":

```vue
<template>
  <div style="max-width: 350px">
    <q-expansion-item
      class="shadow-1 overflow-hidden"
      style="border-radius: 30px"
      icon="explore"
      label="Counter"
      @show="startCounting"
      @hide="stopCounting"
      header-class="bg-primary text-white"
      expand-icon-class="text-white"
    >
      <q-card>
        <q-card-section>
          Counting: <q-badge color="secondary">{{ counter }}</q-badge
          >. Will only count when opened, using the show/hide events to control
          count timer.
        </q-card-section>
      </q-card>
    </q-expansion-item>
  </div>
</template>

<script setup>
import { onBeforeUnmount, ref } from 'vue'

const counter = ref(0)
let timer

function stopCounting() {
  clearInterval(timer)
}

function startCounting() {
  timer = setInterval(() => {
    counter.value++
  }, 1000)
}

onBeforeUnmount(stopCounting)
</script>
```

When dealing with inset levels, a general rule of thumb is that `header-inset-level` adds left padding to header while it doesn't do anything with the content, while `content-inset-level` adds left padding to the content.

Example "Playing with inset levels":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered class="rounded-borders">
      <q-expansion-item
        expand-separator
        icon="mail"
        label="Inbox"
        caption="5 unread emails"
        default-opened
      >
        <q-expansion-item
          :header-inset-level="1"
          expand-separator
          icon="receipt"
          label="Receipts"
          default-opened
        >
          <q-expansion-item
            switch-toggle-side
            dense-toggle
            label="Today"
            :header-inset-level="1"
            :content-inset-level="2"
          >
            <q-card>
              <q-card-section>
                Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                Quidem, eius reprehenderit eos corrupti commodi magni quaerat ex
                numquam, dolorum officiis modi facere maiores architecto
                suscipit iste eveniet doloribus ullam aliquid.
              </q-card-section>
            </q-card>
          </q-expansion-item>

          <q-expansion-item
            switch-toggle-side
            dense-toggle
            label="Yesterday"
            :header-inset-level="1"
            :content-inset-level="2"
          >
            <q-card>
              <q-card-section>
                Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                Quidem, eius reprehenderit eos corrupti commodi magni quaerat ex
                numquam, dolorum officiis modi facere maiores architecto
                suscipit iste eveniet doloribus ullam aliquid.
              </q-card-section>
            </q-card>
          </q-expansion-item>
        </q-expansion-item>

        <q-expansion-item
          :header-inset-level="1"
          :content-inset-level="1"
          expand-separator
          icon="schedule"
          label="Postponed"
        >
          <q-card>
            <q-card-section>
              Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
              eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
              dolorum officiis modi facere maiores architecto suscipit iste
              eveniet doloribus ullam aliquid.
            </q-card-section>
          </q-card>
        </q-expansion-item>
      </q-expansion-item>

      <q-expansion-item
        :content-inset-level="0.5"
        expand-separator
        icon="mail"
        label="Inbox"
        caption="5 unread emails"
        default-opened
      >
        <q-expansion-item
          expand-separator
          :content-inset-level="0.5"
          icon="receipt"
          label="Receipts"
        >
          <q-expansion-item label="Today" :content-inset-level="0.5">
            <q-card>
              <q-card-section>
                Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                Quidem, eius reprehenderit eos corrupti commodi magni quaerat ex
                numquam, dolorum officiis modi facere maiores architecto
                suscipit iste eveniet doloribus ullam aliquid.
              </q-card-section>
            </q-card>
          </q-expansion-item>

          <q-expansion-item label="Yesterday" :content-inset-level="0.5">
            <q-card>
              <q-card-section>
                Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                Quidem, eius reprehenderit eos corrupti commodi magni quaerat ex
                numquam, dolorum officiis modi facere maiores architecto
                suscipit iste eveniet doloribus ullam aliquid.
              </q-card-section>
            </q-card>
          </q-expansion-item>
        </q-expansion-item>

        <q-expansion-item
          :content-inset-level="0.5"
          expand-separator
          icon="schedule"
          label="Postponed"
        >
          <q-card>
            <q-card-section>
              Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
              eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
              dolorum officiis modi facere maiores architecto suscipit iste
              eveniet doloribus ullam aliquid.
            </q-card-section>
          </q-card>
        </q-expansion-item>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

### Behavior

> [!TIP]
> The behavior below of toggling by expand icon only is especially useful when having a route attached to the header of QExpansionItem. This way by clicking header it will activate the route and by clicking the expand icon it will, well, expand the content. You can't have both actions attached to the whole header, obviously.

Example "Toggle by expand icon only":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered class="rounded-borders">
      <q-expansion-item
        expand-icon-toggle
        expand-separator
        icon="perm_identity"
        label="Account settings"
        caption="John Doe"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        expand-icon-toggle
        expand-separator
        icon="signal_wifi_off"
        label="Wifi settings"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <q-expansion-item
        expand-icon-toggle
        expand-separator
        icon="drafts"
        label="Drafts"
        header-class="text-purple"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

Example "Accordion mode":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered>
      <q-expansion-item
        group="somegroup"
        icon="explore"
        label="First"
        default-opened
        header-class="text-primary"
      >
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>

      <!-- ... -->
    </q-list>
  </div>
</template>
```

Example "Popup mode":

```vue
<template>
  <div style="max-width: 350px">
    <q-list>
      <q-expansion-item
        popup
        default-opened
        icon="mail"
        label="Inbox"
        caption="5 unread emails"
      >
        <q-separator />
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
      <q-expansion-item popup icon="send" label="Outbox" caption="Empty">
        <q-separator />
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
      <q-expansion-item
        popup
        icon="drafts"
        label="Draft"
        caption="Draft a new email"
      >
        <q-separator />
        <q-card>
          <q-card-section>
            Lorem ipsum dolor sit amet, consectetur adipisicing elit. Quidem,
            eius reprehenderit eos corrupti commodi magni quaerat ex numquam,
            dolorum officiis modi facere maiores architecto suscipit iste
            eveniet doloribus ullam aliquid.
          </q-card-section>
        </q-card>
      </q-expansion-item>
    </q-list>
  </div>
</template>
```

## Accessibility *(v2.25+)*

The header follows the [WAI-ARIA disclosure pattern](https://www.w3.org/WAI/ARIA/apg/patterns/disclosure/): it exposes `role="button"` with `aria-expanded` reflecting the current state and `aria-controls` pointing at the content container, and <kbd>Enter</kbd> or <kbd>Space</kbd> toggles it. Its accessible name is a localized "Expand"/"Collapse" label built from the `label` prop (see [Quasar Language Packs](../options/quasar-language-packs.md)); the `toggle-aria-label` prop overrides the generated label. Collapsed content is truly hidden from assistive technology, not just visually.

When the header is a link (`to`/`href` props — see "Toggle by expand icon only" above), the header stays a native link and the toggle ARIA attributes move to the expand icon instead, which becomes its own Tab stop — so navigating and expanding remain separately reachable by keyboard.

Note that rich custom header content is not part of the generated accessible name — screen reader users still hear only the "Expand"/"Collapse" label built from the `label` prop (or a generic one without it). Set `toggle-aria-label` whenever the `label` prop alone doesn't tell the story.
