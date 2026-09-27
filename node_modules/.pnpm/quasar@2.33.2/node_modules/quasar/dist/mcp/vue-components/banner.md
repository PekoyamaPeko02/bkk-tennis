---
title: Banner
---
The QBanner component creates a banner element to display a prominent message and related optional actions.

According to the Material Design spec, the banner should be "displayed at the top of the screen, below a top app bar" - but of course you can put one anywhere that makes sense, even in a QDialog.

## QBanner API

Not inlined here: call the `get_api` tool with `name: "QBanner"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

Example "Basic":

```vue
<template>
  <div class="q-gutter-sm">
    <q-banner class="bg-primary text-white">
      Unfortunately, the credit card did not go through, please try again.
      <template #action>
        <q-btn flat color="white" label="Dismiss" />
        <q-btn flat color="white" label="Update Credit Card" />
      </template>
    </q-banner>

    <q-banner :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'">
      <template #avatar>
        <q-icon name="signal_wifi_off" color="primary" />
      </template>
      You have lost connection to the internet. This app is offline.
      <template #action>
        <q-btn flat color="primary" label="Turn on Wifi" />
      </template>
    </q-banner>

    <q-banner inline-actions class="text-white bg-red">
      You have lost connection to the internet. This app is offline.
      <template #action>
        <q-btn flat color="white" label="Turn ON Wifi" />
      </template>
    </q-banner>
  </div>
</template>
```

Example "Rounded border":

```vue
<template>
  <div class="q-gutter-sm">
    <q-banner rounded class="bg-purple-8 text-white">
      We can't find your saved recipes until you sign in.

      <template #action>
        <q-btn flat color="white" label="Continue as a Guest" />
        <q-btn flat color="white" label="Sign in" />
      </template>
    </q-banner>
  </div>
</template>
```

Example "With an image":

```vue
<template>
  <div class="q-gutter-sm">
    <q-banner rounded :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'">
      <template #avatar>
        <img
          alt="Mountains"
          src="https://cdn.quasar.dev/img/mountains.jpg"
          style="width: 100px; height: 64px"
        />
      </template>

      Could not retrieve travel data.
      <template #action>
        <q-btn flat label="Retry" />
      </template>
    </q-banner>
  </div>
</template>
```

Example "Inline actions":

```vue
<template>
  <div class="q-gutter-sm">
    <q-banner inline-actions rounded class="bg-orange text-white">
      You have lost connection to the internet. This app is offline.

      <template #action>
        <q-btn flat label="Turn ON Wifi" />
        <q-btn flat label="Dismiss" />
      </template>
    </q-banner>
  </div>
</template>
```

Example "Dense":

```vue
<template>
  <div class="q-gutter-sm">
    <q-banner dense class="bg-primary text-white">
      Unfortunately, the credit card did not go through, please try again.
      <template #action>
        <q-btn flat color="white" label="Dismiss" />
        <q-btn flat color="white" label="Update Credit Card" />
      </template>
    </q-banner>

    <q-banner dense :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-2'">
      <template #avatar>
        <q-icon name="signal_wifi_off" color="primary" />
      </template>
      You have lost connection to the internet. This app is offline.
      <template #action>
        <q-btn flat color="primary" label="Turn on Wifi" />
      </template>
    </q-banner>

    <q-banner dense inline-actions class="text-white bg-red">
      You have lost connection to the internet. This app is offline.
      <template #action>
        <q-btn flat color="white" label="Turn ON Wifi" />
      </template>
    </q-banner>
  </div>
</template>
```

## Accessibility *(v2.25+)*

QBanner renders with `role="alert"`, an assertive live region: a banner inserted dynamically (say, an error appearing after a failed request) is announced immediately, while one rendered together with the page is not announced at all. For a persistent, informational banner that should not interrupt, override the role by passing an attribute — `role="status"` for polite announcements, or a landmark role such as `region` (with an `aria-label`) for purely static content.
