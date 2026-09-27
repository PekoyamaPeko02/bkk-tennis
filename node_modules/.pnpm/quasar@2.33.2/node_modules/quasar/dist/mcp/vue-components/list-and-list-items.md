---
title: List and List Items
related:
  - title: Expansion Item
    path: expansion-item.md
  - title: Slide Item
    path: slide-item.md
  - title: Separator
    path: separator.md
---
The QList and QItem are a group of components which can work together to present multiple line items vertically as a single continuous element. They are best suited for displaying similar data types as rows of information, such as a contact list, a playlist or menu. Each row is called an Item. QItem can also be used outside of a QList too.

Lists can encapsulate Items or Item-like components, for example [QExpansionItem](expansion-item.md) or [QSlideItem](slide-item.md). Also [QSeparator](separator.md) can be used to split up sections, where needed.

List Items have the following pre-built child components:

- **QItemSection** - An item section can have several uses for particular content. They are controlled via the `avatar`, `thumbnail` and `side` props. With no props, it will render the main section of your QItem (which spans to the fullest of available space).
- **QItemLabel** - An item label is useful for predefined text content type within a QItemSection, or for header-like content of the QList itself.

## QList API

Not inlined here: call the `get_api` tool with `name: "QList"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QItem API

Not inlined here: call the `get_api` tool with `name: "QItem"` for its definition, or add `part` (`props`, `events`, `slots`) for one of them.

## QItemSection API

Not inlined here: call the `get_api` tool with `name: "QItemSection"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QItemLabel API

Not inlined here: call the `get_api` tool with `name: "QItemLabel"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered separator>
      <q-item clickable v-ripple>
        <q-item-section>Single line item</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label>Item with caption</q-item-label>
          <q-item-label caption>Caption</q-item-label>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label overline>OVERLINE</q-item-label>
          <q-item-label>Item with overline</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

> [!NOTE]
> A QItem with a `@click` listener is clickable by default (v2.29+): it gets the hover effects, keyboard activation and its `click` event without the `clickable` prop. Set `clickable` explicitly only when there is no listener (a `v-close-popup` entry, for example) or when you need to toggle the behavior through a boolean; an explicit `clickable="false"` wins over the listener.

Example "Force dark mode":

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-list dark bordered separator style="max-width: 318px">
      <q-item clickable v-ripple>
        <q-item-section>Single line item</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label>Item with caption</q-item-label>
          <q-item-label caption>Caption</q-item-label>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label overline>OVERLINE</q-item-label>
          <q-item-label>Item with overline</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

Example "Dense":

```vue
<template>
  <div style="max-width: 350px">
    <q-list dense bordered padding class="rounded-borders">
      <q-item clickable v-ripple>
        <q-item-section> Item </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section> Item </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section> Item </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

### QItemSection

Example "Left avatar/thumbnail QItemSection":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered>
      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-icon color="primary" name="bluetooth" />
        </q-item-section>

        <q-item-section>Icon as avatar</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar color="teal" text-color="white" icon="bluetooth" />
        </q-item-section>

        <q-item-section>Avatar-type icon</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar
            rounded
            color="purple"
            text-color="white"
            icon="bluetooth"
          />
        </q-item-section>

        <q-item-section>Rounded avatar-type icon</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar color="primary" text-color="white"> R </q-avatar>
        </q-item-section>

        <q-item-section>Letter avatar-type</q-item-section>
      </q-item>

      <q-separator />

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
        <q-item-section>Image avatar</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar square>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
        <q-item-section>Image square avatar</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar rounded>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
        <q-item-section>Image rounded avatar</q-item-section>
      </q-item>

      <q-separator />

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar rounded>
            <img
              alt="Mountains"
              src="https://cdn.quasar.dev/img/mountains.jpg"
            />
          </q-avatar>
        </q-item-section>
        <q-item-section>List item</q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section thumbnail>
          <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />
        </q-item-section>
        <q-item-section>List item</q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

Example "Right avatar/thumbnail QItemSection":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered>
      <q-item clickable v-ripple>
        <q-item-section>Icon as avatar</q-item-section>
        <q-item-section avatar>
          <q-icon color="primary" name="bluetooth" />
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>Avatar-type icon</q-item-section>
        <q-item-section avatar>
          <q-avatar color="teal" text-color="white" icon="bluetooth" />
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>Rounded avatar-type icon</q-item-section>
        <q-item-section avatar>
          <q-avatar
            rounded
            color="purple"
            text-color="white"
            icon="bluetooth"
          />
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>Letter avatar-type</q-item-section>
        <q-item-section avatar>
          <q-avatar color="primary" text-color="white"> R </q-avatar>
        </q-item-section>
      </q-item>

      <q-separator />

      <q-item clickable v-ripple>
        <q-item-section>Image avatar</q-item-section>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>Image square avatar</q-item-section>
        <q-item-section avatar>
          <q-avatar square>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>Image rounded avatar</q-item-section>
        <q-item-section avatar>
          <q-avatar rounded>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>
      </q-item>

      <q-separator />

      <q-item clickable v-ripple>
        <q-item-section>List item</q-item-section>
        <q-item-section avatar>
          <q-avatar rounded>
            <img
              alt="Mountains"
              src="https://cdn.quasar.dev/img/mountains.jpg"
            />
          </q-avatar>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>List item</q-item-section>
        <q-item-section thumbnail>
          <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

> [!TIP]
> When you have multi-line items, you could use `top` property on QItemSection side/avatar to align the sections to top, overriding default middle alignment.

Example "Side QItemSection":

```vue
<template>
  <div style="max-width: 350px">
    <q-list>
      <q-item>
        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption lines="2"
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>5 min ago</q-item-label>
          <q-icon name="star" color="yellow" />
        </q-item-section>
      </q-item>

      <q-separator spaced inset />

      <q-item>
        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>Voted!</q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced inset />

      <q-item>
        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-badge color="teal" label="10k" />
        </q-item-section>
      </q-item>

      <q-separator spaced inset />

      <q-item>
        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>2 min ago</q-item-label>
          <div class="text-orange">
            <q-icon name="star" />
            <q-icon name="star" />
            <q-icon name="star" />
          </div>
        </q-item-section>
      </q-item>

      <q-separator spaced inset />

      <q-item>
        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>meta</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

### Active state

Example "Active prop":

```vue
<template>
  <div class="q-gutter-md" style="max-width: 350px">
    <q-toggle v-model="active" label="Active" />

    <q-list bordered separator>
      <q-item clickable v-ripple :active="active">
        <q-item-section avatar>
          <q-icon name="signal_wifi_off" />
        </q-item-section>
        <q-item-section>Active</q-item-section>
        <q-item-section side>Side</q-item-section>
      </q-item>

      <q-item clickable v-ripple :active="active" active-class="text-orange">
        <q-item-section avatar>
          <q-icon name="signal_wifi_off" />
        </q-item-section>
        <q-item-section>Active, Active class</q-item-section>
        <q-item-section side>Side</q-item-section>
      </q-item>

      <q-item
        clickable
        v-ripple
        :active="active"
        active-class="bg-teal-1 text-grey-8"
      >
        <q-item-section avatar>
          <q-icon name="signal_wifi_off" />
        </q-item-section>
        <q-item-section>Active, Active class</q-item-section>
        <q-item-section side>Side</q-item-section>
      </q-item>
    </q-list>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const active = ref(true)
</script>
```

### QItemLabel

> [!WARNING]
> Notice you can handle label overflow with `lines` prop, telling it how many lines it can span. However, this feature uses Webkit specific CSS so won't work in IE/Edge.

Example "ItemLabel":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered padding>
      <q-item>
        <q-item-section>
          <q-item-label overline>OVERLINE</q-item-label>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>5 min ago</q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced />
      <q-item-label header>List Header</q-item-label>

      <q-item>
        <q-item-section avatar>
          <q-icon color="primary" name="bluetooth" />
        </q-item-section>
        <q-item-section>List item</q-item-section>
        <q-item-section side>
          <q-item-label caption>meta</q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced inset="item" />

      <q-item>
        <q-item-section top avatar>
          <q-avatar color="primary" text-color="white" icon="bluetooth" />
        </q-item-section>

        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption lines="2"
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>5 min ago</q-item-label>
          <q-icon name="star" color="yellow" />
        </q-item-section>
      </q-item>

      <q-separator spaced inset="item" />

      <q-item>
        <q-item-section top avatar>
          <q-avatar
            color="primary"
            text-color="white"
            square
            icon="bluetooth"
          />
        </q-item-section>

        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>meta</q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced inset="item" />

      <q-item>
        <q-item-section top avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-badge label="10k" />
        </q-item-section>
      </q-item>

      <q-separator spaced inset="item" />

      <q-item>
        <q-item-section top avatar>
          <q-avatar rounded>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/boy-avatar.png"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>meta</q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced />

      <q-item>
        <q-item-section top thumbnail class="q-ml-none">
          <img alt="Mountains" src="https://cdn.quasar.dev/img/mountains.jpg" />
        </q-item-section>

        <q-item-section>
          <q-item-label>Single line item</q-item-label>
          <q-item-label caption
            >Secondary line text. Lorem ipsum dolor sit amet, consectetur
            adipiscit elit.</q-item-label
          >
        </q-item-section>

        <q-item-section side top>
          <q-item-label caption>meta</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>
```

### More involved examples

Example "Contact list":

```vue
<template>
  <div style="max-width: 350px">
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Contacts</q-toolbar-title>
    </q-toolbar>

    <q-list bordered>
      <q-item
        v-for="contact in contacts"
        :key="contact.id"
        class="q-my-sm"
        clickable
        v-ripple
      >
        <q-item-section avatar>
          <q-avatar color="primary" text-color="white">
            {{ contact.letter }}
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>{{ contact.name }}</q-item-label>
          <q-item-label caption lines="1">{{ contact.email }}</q-item-label>
        </q-item-section>

        <q-item-section side>
          <q-icon name="chat_bubble" color="green" />
        </q-item-section>
      </q-item>

      <q-separator />
      <q-item-label header>Offline</q-item-label>

      <q-item
        v-for="contact in offline"
        :key="contact.id"
        class="q-mb-sm"
        clickable
        v-ripple
      >
        <q-item-section avatar>
          <q-avatar>
            <img
              :alt="`${contact.name} avatar`"
              :src="`https://cdn.quasar.dev/img/${contact.avatar}`"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>{{ contact.name }}</q-item-label>
          <q-item-label caption lines="1">{{ contact.email }}</q-item-label>
        </q-item-section>

        <q-item-section side>
          <q-icon name="chat_bubble" color="grey" />
        </q-item-section>
      </q-item>
    </q-list>
  </div>
</template>

<script setup>
const contacts = [
  {
    id: 1,
    name: 'Ruddy Jedrzej',
    email: 'rjedrzej0@discuz.net',
    letter: 'R'
  },
  // ...
]

const offline = [
  {
    id: 5,
    name: 'Brunhilde Panswick',
    email: 'bpanswick4@csmonitor.com',
    avatar: 'avatar2.jpg'
  },
  {
    id: 6,
    name: 'Winfield Stapforth',
    email: 'wstapforth5@pcworld.com',
    avatar: 'avatar6.jpg'
  }
]
</script>
```

Example "Settings":

```vue
<template>
  <div style="max-width: 350px">
    <q-list bordered padding>
      <q-item-label header>User Controls</q-item-label>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label>Content filtering</q-item-label>
          <q-item-label caption>
            Set the content filtering level to restrict apps that can be
            downloaded
          </q-item-label>
        </q-item-section>
      </q-item>

      <q-item clickable v-ripple>
        <q-item-section>
          <q-item-label>Password</q-item-label>
          <q-item-label caption>
            Require password for purchase or use password to restrict purchase
          </q-item-label>
        </q-item-section>
      </q-item>

      <q-separator spaced />
      <q-item-label header>General</q-item-label>

      <q-item tag="label" v-ripple>
        <q-item-section side top>
          <q-checkbox v-model="check1" />
        </q-item-section>

        <q-item-section>
          <q-item-label>Notifications</q-item-label>
          <q-item-label caption>
            Notify me about updates to apps or games that I downloaded
          </q-item-label>
        </q-item-section>
      </q-item>

      <!-- ... -->

      <q-separator spaced />
      <q-item-label header>Notifications</q-item-label>

      <q-item tag="label" v-ripple>
        <q-item-section>
          <q-item-label>Battery too low</q-item-label>
        </q-item-section>
        <q-item-section side>
          <q-toggle color="blue" v-model="notif1" val="battery" />
        </q-item-section>
      </q-item>

      <q-item tag="label" v-ripple>
        <q-item-section>
          <q-item-label>Friend request</q-item-label>
          <q-item-label caption>Allow notification</q-item-label>
        </q-item-section>
        <q-item-section side top>
          <q-toggle color="green" v-model="notif2" val="friend" />
        </q-item-section>
      </q-item>

      <!-- ... -->

      <q-separator spaced />
      <q-item-label header>Other settings</q-item-label>

      <q-item>
        <q-item-section side>
          <q-icon color="teal" name="volume_down" />
        </q-item-section>
        <q-item-section>
          <q-slider v-model="volume" :min="0" :max="10" label color="teal" />
        </q-item-section>
        <q-item-section side>
          <q-icon color="teal" name="volume_up" />
        </q-item-section>
      </q-item>

      <q-item>
        <q-item-section side>
          <q-icon color="deep-orange" name="brightness_medium" />
        </q-item-section>
        <q-item-section>
          <q-slider
            v-model="brightness"
            :min="0"
            :max="10"
            label
            color="deep-orange"
          />
        </q-item-section>
      </q-item>

      <!-- ... -->
    </q-list>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const check1 = ref(true)
const check2 = ref(false)
const check3 = ref(false)

const notif1 = ref(true)
const notif2 = ref(true)
const notif3 = ref(false)

const volume = ref(6)
const brightness = ref(3)
const mic = ref(8)
</script>
```

Example "Emails":

```vue
<template>
  <div class="q-gutter-md">
    <q-list bordered class="rounded-borders" style="max-width: 350px">
      <q-item-label header>Friends</q-item-label>

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/avatar2.jpg"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label lines="1">Brunch this weekend?</q-item-label>
          <q-item-label caption lines="2">
            <span class="text-weight-bold">Janet</span>
            -- I'll be in your neighborhood doing errands this weekend. Do you
            want to grab brunch?
          </q-item-label>
        </q-item-section>

        <q-item-section side top> 1 min ago </q-item-section>
      </q-item>

      <q-separator inset="item" />

      <q-item clickable v-ripple>
        <q-item-section avatar>
          <q-avatar>
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/avatar4.jpg"
            />
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label lines="1">Linear Project</q-item-label>
          <q-item-label caption lines="2">
            <span class="text-weight-bold">John</span>
            -- Can we schedule a call for tomorrow?
          </q-item-label>
        </q-item-section>

        <q-item-section side top> 1 min ago </q-item-section>
      </q-item>
    </q-list>

    <q-list bordered class="rounded-borders" style="max-width: 600px">
      <q-item-label header>Google Inbox style</q-item-label>

      <q-item>
        <q-item-section avatar top>
          <q-icon name="account_tree" color="black" size="34px" />
        </q-item-section>

        <q-item-section top class="col-2 gt-sm">
          <q-item-label class="q-mt-sm">GitHub</q-item-label>
        </q-item-section>

        <q-item-section top>
          <q-item-label lines="1">
            <span class="text-weight-medium">[quasarframework/quasar]</span>
            <span class="text-grey-8"> - GitHub repository</span>
          </q-item-label>
          <q-item-label caption lines="1">
            @rstoenescu in #3: > Generic type parameter for props
          </q-item-label>
          <q-item-label
            lines="1"
            class="q-mt-xs text-body2 text-weight-bold text-primary text-uppercase"
          >
            <span class="cursor-pointer">Open in GitHub</span>
          </q-item-label>
        </q-item-section>

        <q-item-section top side>
          <div class="text-grey-8 q-gutter-xs">
            <q-btn class="gt-xs" size="12px" flat dense round icon="delete" />
            <q-btn class="gt-xs" size="12px" flat dense round icon="done" />
            <q-btn size="12px" flat dense round icon="more_vert" />
          </div>
        </q-item-section>
      </q-item>

      <!-- ... -->
    </q-list>
  </div>
</template>
```

Example "Folder listing":

```vue
<template>
  <div class="q-gutter-md">
    <q-list bordered padding class="rounded-borders" style="max-width: 350px">
      <q-item-label header>Folders</q-item-label>

      <q-item clickable v-ripple>
        <q-item-section avatar top>
          <q-avatar icon="folder" color="primary" text-color="white" />
        </q-item-section>

        <q-item-section>
          <q-item-label lines="1">Photos</q-item-label>
          <q-item-label caption>February 22nd, 2019</q-item-label>
        </q-item-section>

        <q-item-section side>
          <q-icon name="info" color="green" />
        </q-item-section>
      </q-item>

      <!-- ... -->

      <q-separator spaced />
      <q-item-label header>Files</q-item-label>

      <q-item clickable v-ripple>
        <q-item-section avatar top>
          <q-avatar icon="assignment" color="grey" text-color="white" />
        </q-item-section>

        <q-item-section>
          <q-item-label lines="1">Expenses spreadsheet</q-item-label>
          <q-item-label caption>March 2nd, 2019</q-item-label>
        </q-item-section>

        <q-item-section side>
          <q-icon name="info" />
        </q-item-section>
      </q-item>

      <!-- ... -->
    </q-list>
  </div>
</template>
```

For demoing purposes in the example below, we're using the `active` prop instead of QItem's router props (`to`, `exact`). UMD doesn't have Vue Router so you wouldn't be able to play with it in Codepen/jsFiddle.

Example "Menu":

```vue
<template>
  <div style="max-width: 250px">
    <q-list bordered padding class="rounded-borders text-primary">
      <q-item
        v-ripple
        :active="link === 'inbox'"
        @click="link = 'inbox'"
        active-class="my-menu-link"
      >
        <q-item-section avatar>
          <q-icon name="inbox" />
        </q-item-section>

        <q-item-section>Inbox</q-item-section>
      </q-item>

      <!-- ... -->
    </q-list>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const link = ref('inbox')
</script>

<style lang="sass">
.my-menu-link
  color: white
  background: #F2C037
</style>
```

> [!TIP]
> For more complex menus, consider also using [QExpansionItem](expansion-item.md).

### Connecting to Vue Router

You can use QItems together with Vue Router through `<router-link>` properties bound to it. These allow for listening to the current app route and also triggering a route when clicked/tapped.

```html
<q-item to="/inbox" exact>
  <q-item-section avatar>
    <q-icon name="inbox" />
  </q-item-section>

  <q-item-section> Inbox </q-item-section>
</q-item>
```

You can also delay, cancel or redirect navigation, as seen below. For a more in-depth description of the `@click` event being used below, please refer to QItem API card at the top of the page.

Example "Links with delayed, cancelled or redirected navigation":

```vue
<template>
  <div class="q-gutter-sm">
    <q-item to="/" @click="onDelayedClick" dense>
      <q-item-section>Delayed navigation</q-item-section>
    </q-item>

    <q-item to="/" @click="onCancelledClick" dense>
      <q-item-section>Cancelled navigation</q-item-section>
    </q-item>

    <q-item to="/" @click="onRedirectedClick" dense>
      <q-item-section>Redirected navigation</q-item-section>
    </q-item>
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

QList exposes itself with the [WAI-ARIA `list` role](https://www.w3.org/TR/wai-aria-1.2/#list) by default (implicitly so when rendered as `ul`/`ol` through the `tag` prop) and the `role` prop overrides that. Each QItem derives its default role from the QList wrapping it:

| QItem | inside default QList | inside QList with `role="menu"`/`"menubar"` | outside QList / other QList `role` |
| --- | --- | --- | --- |
| with `clickable`, a `@click` listener or a link | `button` / native link | `menuitem` | `button` / native link |
| non-interactive | `listitem` | none | none |

This keeps the produced markup valid: ARIA's `list` may only own `listitem` children (which in turn require a list parent, so a standalone QItem claims no role), while `menu`/`menubar` may only own `menuitem`-type entries — declaring the role once on the QList is enough, as in the "Basic" example of [QMenu's Accessibility section](menu.md#accessibility). The `role` prop on QItem overrides the derived role for a single item (e.g. `menuitemcheckbox`/`menuitemradio` for toggle entries — managing `aria-checked` is then up to you).

> [!WARNING]
> A list made up of only interactive items has no valid claim to the `list` role — such a container owns no `listitem` children. Declare what it actually is: `role="menu"` if it pops up as a list of commands, or `role="none"` to keep the items (announced as buttons/links) without list semantics.
