---
title: CSS Shadows (Elevation)
---
Simple yet effective way to add shadows to create a depth/elevation effect.
The shadows are in accordance to Material Design specifications (24 levels of depth).

## Usage

| CSS Class Name | Description |
| --- | --- |
| `no-shadow` | Remove any shadow |
| `inset-shadow` | Set an inset shadow on top |
| `inset-shadow-down` | Set an inset shadow on bottom |
| `shadow-1` | Set a depth of 1 |
| `shadow-2` | Set a depth of 2 |
| `shadow-N` | Where `N` is an integer from 1 to 24. |
| `shadow-transition` | Apply the default CSS transition effect on the shadow |

Example "Standard shadows":

```vue
<template>
  <div
    class="flex inline shadow-box flex-center"
    v-for="n in 24"
    :key="n"
    :class="`shadow-${n}`"
  >
    .shadow-{{ n }}
  </div>
</template>

<style lang="sass" scoped>
.shadow-box
  width: 90px
  height: 90px
  margin: 25px
  border-radius: 50%
  font-size: 12px
</style>
```

The shadows above point towards the bottom of the element. If you want them to point towards the top of the element, add `up` before the number:

| CSS Class Name | Description |
| --- | --- |
| `shadow-up-1` | Set a depth of 1 |
| `shadow-up-2` | Set a depth of 2 |
| `shadow-up-N` | Where `N` is an integer from 1 to 24. |

Example "Shadows pointing up":

```vue
<template>
  <div
    class="flex inline shadow-box flex-center"
    v-for="n in 24"
    :key="n"
    :class="`shadow-up-${n}`"
  >
    .shadow-up-{{ n }}
  </div>
</template>

<style lang="sass" scoped>
.shadow-box
  width: 90px
  height: 90px
  margin: 25px
  border-radius: 50%
  font-size: 12px
</style>
```

Example "Inset shadow":

```vue
<template>
  <div class="q-gutter-md">
    <div
      class="inset-shadow flex inline shadow-box flex-center doc-inset-shadow"
    >
      .inset-shadow
    </div>

    <div
      class="inset-shadow-down flex inline shadow-box flex-center doc-inset-shadow"
    >
      .inset-shadow-down
    </div>
  </div>
</template>

<style lang="sass" scoped>
.shadow-box
  width: 90px
  height: 90px
  margin: 25px
  border-radius: 50%
  font-size: 12px
.doc-inset-shadow
  width: 120px
  height: 120px
  border: 1px solid #eee
  padding: 4px
</style>
```

## Shadow color *(v2.31+)*

The shadow color is a CSS custom property declared on `:root`: `--q-shadow-color` (default `#000`) while in light mode and `--q-dark-shadow-color` (default `#fff`) while in [Dark Mode](dark-mode.md#shadows-in-dark-mode). Override them on `:root` or on `body` to recolor (or, with `transparent`, remove) every shadow. The shadow tints are derived from them at the `body` level, so an override placed deeper in the DOM has no effect.

```css
:root {
  --q-shadow-color: #1a237e;
}

body.body--dark {
  --q-dark-shadow-color: #000;
}
```
