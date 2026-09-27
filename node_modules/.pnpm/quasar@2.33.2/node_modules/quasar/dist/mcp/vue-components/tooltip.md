---
title: Tooltip
related:
  - title: QMenu
    path: menu.md
---
The QTooltip component is to be used when you want to offer the user more information about a certain area in your App. When hovering the mouse over the target element (or touching and holding on touch-capable devices), the tooltip will appear. A stylus behaves like a mouse while hovering and like touch while pressed to the screen.

## QTooltip API

Not inlined here: call the `get_api` tool with `name: "QTooltip"` for its definition, or add `part` (`props`, `computedProps`, `methods`, `events`, `slots`) for one of them.

## Usage

The idea with QTooltip is to place it inside your DOM element / component that you want to be the trigger as direct child. Don’t worry about QTooltip content inheriting CSS from the container as the QTooltip will be injected as a direct child of `<body>` through a Quasar Portal.

Example "Basic":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn label="Hover me" color="primary">
      <q-tooltip> Some text as content of Tooltip </q-tooltip>
    </q-btn>

    <div
      class="inline bg-amber rounded-borders cursor-pointer"
      style="max-width: 300px"
    >
      <div class="fit flex flex-center text-center non-selectable q-pa-md">
        I am groot!<br />(Hover me!)
      </div>

      <q-tooltip> I am groot! </q-tooltip>
    </div>
  </div>
</template>
```

Example "Toggle through v-model":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="primary" @click="showing = true" label="Show" />
    <q-btn color="primary" @click="showing = false" label="Hide" />
  </div>

  <div
    style="width: 200px; height: 70px"
    class="bg-purple text-white rounded-borders row flex-center q-mt-md"
  >
    Hover here or click buttons
    <q-tooltip v-model="showing">Tooltip text</q-tooltip>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const showing = ref(false)
</script>
```

> [!IMPORTANT]
> If you want to conditionally activate or de-activate a QTooltip, please use `v-if` on it instead of `v-show`.

### Customize

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="primary">
      Hover
      <q-tooltip class="bg-indigo" :offset="[10, 10]"> Here I am! </q-tooltip>
    </q-btn>

    <q-btn color="primary">
      Over
      <q-tooltip class="bg-red" :offset="[10, 10]"> Here I am! </q-tooltip>
    </q-btn>

    <q-btn color="primary">
      These
      <q-tooltip class="bg-purple text-body2" :offset="[10, 10]">
        Here I am!
      </q-tooltip>
    </q-btn>

    <q-btn color="primary">
      Buttons
      <q-tooltip class="bg-amber text-black shadow-4" :offset="[10, 10]">
        Here I am!
      </q-tooltip>
    </q-btn>
  </div>
</template>
```

Example "Custom delay (1 second)":

```vue
<template>
  <div
    style="width: 200px; height: 70px"
    class="bg-secondary text-white rounded-borders non-selectable row flex-center"
  >
    One second delay
    <q-tooltip :delay="1000" :offset="[0, 10]">Quasar Rulz!</q-tooltip>
  </div>
</template>
```

Example "With offset":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="indigo">
      Hover
      <q-tooltip anchor="top middle" self="bottom middle" :offset="[10, 10]">
        <strong>Tooltip</strong> on <em>top</em> (<q-icon
          name="keyboard_arrow_up"
        />)
      </q-tooltip>
    </q-btn>

    <q-btn color="red">
      Over
      <q-tooltip anchor="center right" self="center left" :offset="[10, 10]">
        <strong>Tooltip</strong> on <em>right</em> (<q-icon
          name="keyboard_arrow_right"
        />)
      </q-tooltip>
    </q-btn>

    <q-btn color="purple">
      These
      <q-tooltip anchor="center left" self="center right" :offset="[10, 10]">
        <strong>Tooltip</strong> on <em>left</em> (<q-icon
          name="keyboard_arrow_left"
        />)
      </q-tooltip>
    </q-btn>

    <q-btn color="amber">
      Buttons
      <q-tooltip anchor="bottom middle" self="top middle" :offset="[10, 10]">
        <strong>Tooltip</strong> on <em>bottom</em> (<q-icon
          name="keyboard_arrow_down"
        />)
      </q-tooltip>
    </q-btn>
  </div>
</template>
```

### Transitions

In the example below there's a few transitions showcased. For a full list of transitions available, go to [Transitions](../options/transitions.md).

Example "Custom transition":

```vue
<template>
  <div class="q-gutter-md row">
    <q-btn color="primary" label="Flip">
      <q-tooltip transition-show="flip-right" transition-hide="flip-left">
        Here I am!
      </q-tooltip>
    </q-btn>

    <q-btn color="primary" label="Scale">
      <q-tooltip transition-show="scale" transition-hide="scale">
        Here I am!
      </q-tooltip>
    </q-btn>

    <q-btn color="primary" label="Rotate">
      <q-tooltip transition-show="rotate" transition-hide="rotate">
        Here I am!
      </q-tooltip>
    </q-btn>
  </div>
</template>
```

### Reusable

The example below shows how to create a re-usable menu that can be shared with different targets.

Example "Using target":

```vue
<template>
  <div class="q-gutter-md">
    <div class="row justify-center">
      <div class="row items-center q-gutter-x-sm">
        <q-radio
          v-model="targetEl"
          :val="false"
          label="false (no target whatsoever)"
        />
        <q-radio
          v-model="targetEl"
          :val="true"
          label="true (original parent)"
        />
        <q-radio v-model="targetEl" val="#target-img-1" label="#target-img-1" />
        <q-radio v-model="targetEl" val="#target-img-2" label="#target-img-2" />
        <q-radio v-model="targetEl" val="#bogus" label="#bogus" />
      </div>
    </div>
    <div class="row justify-center">
      <q-img
        src="https://cdn.quasar.dev/img/material.png"
        id="target-img-1"
        style="height: 100px"
      >
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px">
          #target-img-1
        </div>
      </q-img>
      <q-img
        src="https://cdn.quasar.dev/img/parallax2.jpg"
        id="target-img-2"
        style="height: 100px"
      >
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px">
          #target-img-2
        </div>
      </q-img>
      <q-img src="https://cdn.quasar.dev/img/blueish.jpg" style="height: 100px">
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px">
          Original parent
        </div>
        <q-tooltip
          :target="targetEl"
          anchor="center middle"
          self="center middle"
          class="bg-black"
          >Quasar Rulz!</q-tooltip
        >
      </q-img>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const targetEl = ref('#target-img-1')
</script>
```

### Positioning

The position of QTooltip can be customized. It keeps account of the `anchor` and `self` optional props.
The final position of QTooltip popup is calculated so that it will be displayed on the available screen real estate: when the requested placement does not fit, the popup switches to the opposite side of the anchor on that axis if it offers more room, otherwise it stays on the requested side (so a trigger with equal room above and below keeps your `anchor`/`self`) and gets capped to the room it has.

For horizontal positioning you can use `start` and `end` when you want to automatically take into account if on RTL or non-RTL. `start` and `end` mean "left" for non-RTL and "right" for RTL.

> Visit the [live documentation](https://quasar.dev/vue-components/tooltip) for the interactive tooltip positioning demo.

> [!NOTE]
> The `offset` prop does not translate the popup by a number of pixels. It expands the **anchor element's bounding box** outward: `offset[0]` moves that box's `left` edge to the left and its `right` edge to the right, while `offset[1]` moves `top` up and `bottom` down. The popup's `self` point is then aligned to the `anchor` point of the expanded box, and only after that is the result clamped to the available screen real estate.
>
> Two consequences are worth knowing, because both make an `offset` look like it is being ignored on one axis:
>
> - **A `middle` or `center` anchor point does not move with the offset.** Expanding both edges by the same amount leaves the midpoint exactly where it was, so `offset[0]` is a no-op for `anchor="... middle"` and `offset[1]` is a no-op for `anchor="center ..."`, no matter which value you pass. QTooltip's default `anchor` is `bottom middle`, so its horizontal offset only starts having an effect once you pick a `left`, `right`, `start` or `end` anchor.
> - **A clamped popup does not move with the offset either.** Since the offset pushes the box outward, anchoring to a full-width or screen-edge element (or passing a very large value) can send the popup past a viewport edge, where it gets clamped back and the final position no longer depends on the offset value. Attach QTooltip to an inline or `inline-block` trigger and point `anchor`/`self` into free space, so the offset has room to take effect.
>
> In short, to place the popup at a fixed pixel distance from one side of the anchor element, name that side in `anchor` instead of relying on `middle`/`center`.

#### Following the pointer *(v2.30+)*

On a large trigger, a popup placed against the anchor element's box ends up far from what the user is actually pointing at. The `cursor-position` prop places QTooltip at the pointer instead: `anchor` is then ignored, since the pointer itself is the anchor point, while `self` and `offset` keep working.

The `offset` reads differently in this mode, and more simply: with no anchor box to expand, it is the distance between the pointer and the tooltip, applied in the direction the tooltip grows away from it. A `self` naming a `top`/`left` edge grows down and right, so it clears the pointer that way; `bottom`/`right` grows up and left and clears it the other way; and a `middle`/`center` axis, which straddles the pointer and has no direction of its own, takes the positive one, where a mouse cursor's body sits below and right of the spot it points at. Unlike the anchor-box mode above, every `self` value therefore takes both offset values.

The tooltip waits for the pointer to settle before showing, so that a pointer sweeping across the trigger does not open it at a coordinate it has already left. Small movements are tolerated, and a `delay` longer than the settle window still wins. Once shown, the position is frozen: it stays where it opened and follows the trigger through scrolling, rather than trailing the pointer around.

Two shows report no pointer position at all and keep the regular `anchor`-relative placement: a keyboard focus, and a `v-model` toggle. A touch or stylus press opens at the contact point without waiting, since a press is already deliberate.

Example "Positioned at the cursor":

```vue
<template>
  <div
    class="cursor-pointer bg-indigo text-white rounded-borders flex flex-center"
    style="height: 200px"
  >
    Hover anywhere over this area, then hold still for a moment

    <q-tooltip cursor-position> Opened where the pointer settled </q-tooltip>
  </div>
</template>
```

## Accessibility *(v2.25+)*

QTooltip implements the [WAI-ARIA tooltip pattern](https://www.w3.org/WAI/ARIA/apg/patterns/tooltip/): the popup renders with `role="tooltip"` and, while it is shown, its `id` is added to the anchor element's `aria-describedby` (any values you set there yourself are preserved and restored on hide), so screen readers announce the tooltip content as the anchor's description. The tooltip shows on keyboard focus (when the anchor matches `:focus-visible`) just as it does on hover, and <kbd>Escape</kbd> hides it without moving focus, as [WCAG 1.4.13](https://www.w3.org/WAI/WCAG21/Understanding/content-on-hover-or-focus.html) requires.

Two things to keep in mind. A tooltip is a *description*, not a name — an icon-only button still needs its own `aria-label`, with the tooltip merely supplementing it. And the tooltip itself is transparent to the mouse (it cannot be hovered, and it hides when the pointer leaves the anchor), so keep its content short and non-interactive.
