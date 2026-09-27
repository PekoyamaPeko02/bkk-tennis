---
title: Parallax
related:
  - title: Video
    path: video.md
---
Parallax scrolling is a technique in computer graphics and web design, where background images move by the camera slower than foreground images, creating an illusion of depth in a 2D scene and adding to the immersion.

QParallax takes care of a lot of quirks, including image/video size which can actually be smaller than the window width/height.

## QParallax API

Not inlined here: call the `get_api` tool with `name: "QParallax"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!NOTE]
> **Scrolling container**
>
> QParallax reacts to scrolling in any container on the page. The auto detected scrolling container (see [here](scroll-observer.md#determining-scrolling-container) about how Quasar determines it) only defines the box the scroll percentage is computed against; use the `scroll-target` prop when the detection picks the wrong one.

### Image background

```vue
<template>
  <div class="q-gutter-md">
    <div class="row justify-between">
      <q-parallax src="https://cdn.quasar.dev/img/parallax2.jpg">
        <h1 class="text-white">Basic</h1>
      </q-parallax>
    </div>
  </div>
</template>
```

### Video background

> [!WARNING]
> On some iOS platforms there may be problems regarding the autoplay feature of the native `<video>` tag. [Reference](https://webkit.org/blog/6784/new-video-policies-for-ios/). QParallax and Quasar are not interfering in any way with the client browser's ability/restrictions on the `<video>` tag.

> [!IMPORTANT]
> When using the `video` tag inside QParallax, you **must** provide the `width` and `height` attributes in order for QParallax to work properly because of the intrinsic resizing capabilities of this type of media. Also, be aware that the actual video width and height are not available until the video's metadata has been loaded.

Example "Custom height with video background":

```vue
<template>
  <q-parallax :height="150">
    <template #media>
      <video
        width="720"
        height="440"
        poster="https://cdn.quasar.dev/img/polina.jpg"
        autoplay
        loop
        muted
      >
        <source
          type="video/webm"
          src="https://cdn.quasar.dev/img/polina.webm"
        />
        <source
          type="video/mp4"
          src="https://cdn.quasar.dev/img/polina.mp4"
        />
      </video>
    </template>

    <h3 class="text-white">Video</h3>
  </q-parallax>
</template>
```

### Custom speed

```vue
<template>
  <q-parallax :height="200" :speed="0.5">
    <template #media>
      <img
        alt="Landscape photo"
        src="https://cdn.quasar.dev/img/parallax1.jpg"
      />
    </template>

    <h1 class="text-white">Docks</h1>
  </q-parallax>
</template>
```

### Using slot

For perf reasons, use the `content` slot only if you need the scope that it provides. Otherwise, use the `default` slot.

Example "Using the slot":

```vue
<template>
  <q-parallax>
    <template #media>
      <img
        alt="Landscape photo"
        src="https://cdn.quasar.dev/img/parallax2.jpg"
      />
    </template>

    <template #content="scope">
      <div
        class="absolute column items-center"
        :style="{
          opacity: 0.45 + (1 - scope.percentScrolled) * 0.55,
          top: scope.percentScrolled * 60 + '%',
          left: 0,
          right: 0
        }"
      >
        <img
          alt="Quasar logo"
          src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg"
          style="width: 150px; height: 150px"
        />
        <div class="text-h3 text-white text-center">Quasar Framework</div>
        <div class="text-h6 text-grey-3 text-center"> v{{ $q.version }} </div>
      </div>
    </template>
  </q-parallax>
</template>
```

### Refreshing *(v2.32+)*

QParallax settles at mount which container it scrolls along, how large the media is and where it stands. Changes it cannot observe on its own are not picked up: an ancestor changing its `overflow`, a media swapped without a `load` event or a `scroll-target` that appears later. Call the `refresh()` method on the component (through a template ref) after such a change.

## Accessibility *(v2.25+)*

The default media image has no `alt` and there is no prop to set one — when the imagery is meaningful, use the `media` slot and supply your own attributes. The scroll-driven motion does not respect `prefers-reduced-motion`, so consider offering a reduced-motion alternative yourself. Also verify that text overlaid on the moving image keeps sufficient contrast throughout the scroll range.
