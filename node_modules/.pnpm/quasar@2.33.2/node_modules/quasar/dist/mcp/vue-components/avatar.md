---
title: Avatar
---
The QAvatar component creates a scalable, color-able element that can have text, icon or image within its shape. By default it is circular, but it can also be square or have a border-radius applied to give rounded corners to the square shape.

It is often used with other components in their slots.

## QAvatar API

Not inlined here: call the `get_api` tool with `name: "QAvatar"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!NOTE]
> The `size` property will determine the height and the width of the Avatar. The `font-size` property will set the size of the font used within the Avatar, which will have an effect on the size of letters and icons.

Example "Basic":

```vue
<template>
  <div class="q-gutter-sm">
    <q-avatar color="red" text-color="white" icon="directions" />
    <q-avatar color="primary" text-color="white">J</q-avatar>
    <q-avatar
      size="100px"
      font-size="52px"
      color="teal"
      text-color="white"
      icon="directions"
    />
    <q-avatar size="24px" color="orange">J</q-avatar>
    <q-avatar>
      <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar.png" />
    </q-avatar>
  </div>
</template>
```

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-avatar
      v-for="size in ['xs', 'sm', 'md', 'lg', 'xl']"
      :key="size"
      :size="size"
      color="primary"
      text-color="white"
      icon="directions"
    />
  </div>
</template>
```

Example "Square":

```vue
<template>
  <div class="q-gutter-sm">
    <q-avatar square color="red" text-color="white" icon="directions" />
    <q-avatar square color="primary" text-color="white">J</q-avatar>
    <q-avatar
      square
      size="100px"
      font-size="82px"
      color="teal"
      text-color="white"
      icon="directions"
    />
    <q-avatar square size="24px" color="orange">J</q-avatar>
    <q-avatar square>
      <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar.png" />
    </q-avatar>
  </div>
</template>
```

Example "Rounded":

```vue
<template>
  <div class="q-gutter-sm">
    <q-avatar rounded color="red" text-color="white" icon="directions" />
    <q-avatar rounded color="primary" text-color="white">J</q-avatar>
    <q-avatar
      rounded
      size="100px"
      font-size="82px"
      color="teal"
      text-color="white"
      icon="directions"
    />
    <q-avatar rounded size="24px" color="orange">J</q-avatar>
    <q-avatar rounded>
      <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar.png" />
    </q-avatar>
  </div>
</template>
```

Example "With other components":

```vue
<template>
  <div class="q-gutter-y-md">
    <div class="q-gutter-sm">
      <q-chip>
        <q-avatar color="red" text-color="white">50</q-avatar>
        Emails
      </q-chip>
      <q-chip>
        <q-avatar>
          <img
            alt="User avatar"
            src="https://cdn.quasar.dev/img/boy-avatar.png"
          />
        </q-avatar>
        John
      </q-chip>
    </div>

    <div class="q-gutter-x-sm">
      <q-btn round color="white">
        <q-avatar size="28px">
          <img alt="Quasar" src="https://cdn.quasar.dev/logo-v2/svg/logo.svg" />
        </q-avatar>
      </q-btn>
      <!-- ... -->
      <q-btn round color="white">
        <q-avatar size="40px">
          <img alt="Quasar" src="https://cdn.quasar.dev/logo-v2/svg/logo.svg" />
        </q-avatar>
      </q-btn>
    </div>

    <q-item clickable v-ripple>
      <q-item-section side>
        <q-avatar rounded size="48px">
          <img alt="User avatar" src="https://cdn.quasar.dev/img/avatar.png" />
          <q-badge floating color="teal">new</q-badge>
        </q-avatar>
      </q-item-section>
      <q-item-section>
        <q-item-label>Mary</q-item-label>
        <q-item-label caption>2 new messages</q-item-label>
      </q-item-section>
      <q-item-section side> 3 min ago </q-item-section>
    </q-item>

    <q-banner rounded class="bg-primary text-white">
      <template #avatar>
        <q-avatar icon="signal_wifi_off" color="white" text-color="primary" />
      </template>

      You have lost connection to the internet. This app is offline.

      <template #action>
        <q-btn flat color="white" label="Turn ON Wifi" />
      </template>
    </q-banner>
  </div>
</template>
```

Example "Overlapping avatars":

```vue
<template>
  <div class="q-gutter-sm" style="height: 80px">
    <q-avatar
      v-for="n in 5"
      :key="n"
      size="40px"
      class="overlapping"
      :style="`left: ${n * 25}px`"
    >
      <img
        alt="User avatar"
        :src="`https://cdn.quasar.dev/img/avatar${n + 1}.jpg`"
      />
    </q-avatar>
  </div>
</template>

<style lang="sass" scoped>
.overlapping
  border: 2px solid white
  position: absolute
</style>
```

## Accessibility *(v2.25+)*

QAvatar is a purely presentational container and claims no ARIA of its own (an icon set through the `icon` prop is marked as decorative). When the avatar conveys identity rather than decoration, the content you place inside must carry that meaning itself — give an `<img>` in the default slot its own `alt`, or add an `aria-label` where letter initials alone would not be understood.
