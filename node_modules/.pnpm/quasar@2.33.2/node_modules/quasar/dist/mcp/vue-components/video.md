---
title: Video
---
Using the QVideo component makes embedding a video like YouTube easy. It also resizes to fit the container by default.

> [!TIP]
> You may also want to check our own HTML 5 video player component: [QMediaPlayer](https://github.com/quasarframework/quasar-ui-qmediaplayer), which is far more advanced than QVideo (which essentially is an iframe pointing to embedded YouTube videos).

## QVideo API

Not inlined here: call the `get_api` tool with `name: "QVideo"` for its `props` definition.

## Usage

### Basic

```vue
<template>
  <q-video src="https://www.youtube.com/embed/aqz-KE-bpKQ?rel=0" />
</template>
```

### With aspect ratio

```vue
<template>
  <q-video
    :ratio="16 / 9"
    src="https://www.youtube.com/embed/aqz-KE-bpKQ?rel=0"
  />
</template>
```

### Markup equivalent

Example "HTML markup":

```vue
<template>
  <div class="q-video">
    <iframe
      src="https://www.youtube.com/embed/aqz-KE-bpKQ?rel=0"
      frameborder="0"
      allowfullscreen
    />
  </div>
</template>
```

## Accessibility *(v2.25+)*

The iframe's accessible name comes from the `title` prop — always provide it, otherwise screen readers announce an anonymous frame with no way to tell what it embeds. Captions and player keyboard support are the embedded player's responsibility, not QVideo's.
