---
title: QMenu
related:
  - title: v-close-popup directive
    path: ../vue-directives/close-popup.md
  - title: Quasar Components Transitions
    path: ../options/transitions.md
  - title: Popup Proxy
    path: popup-proxy.md
---
The QMenu component is a convenient way to show menus. Goes very well with [QList](list-and-list-items.md) as dropdown content, but it's by no means limited to it. When the content is indeed a list of actions, declare it through `role="menu"` on the QList — see [Accessibility](#accessibility).

## QMenu API

Not inlined here: call the `get_api` tool with `name: "QMenu"` for its definition, or add `part` (`props`, `computedProps`, `methods`, `events`, `slots`) for one of them.

## Usage

The idea with QMenu is to place it inside your DOM element / component that you want to be the trigger as direct child. Don’t worry about QMenu content inheriting CSS from the container as the QMenu will be injected as a direct child of `<body>` through a Quasar Portal.

> [!TIP]
> Don't forget to use the directive `v-close-popup` in your clickable menu items if you want the menu to close automatically.
> Alternatively, you can use the QMenu's property `auto-close` or handle closing the menu yourself through its v-model.

### Basic

```vue
<template>
  <div class="q-gutter-md">
    <q-btn color="primary" label="Basic Menu" aria-haspopup="menu">
      <q-menu>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="secondary" label="Auto-Close Menu" aria-haspopup="menu">
      <q-menu auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>

    <div
      class="inline bg-amber rounded-borders cursor-pointer"
      style="max-width: 300px"
      role="button"
      tabindex="0"
      aria-haspopup="menu"
    >
      <div class="fit flex flex-center text-center non-selectable q-pa-md">
        I am groot!<br />(Click me! Using touch-position)
      </div>

      <q-menu touch-position>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section>Branches</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section>Leaves</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section>Roots</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </div>
  </div>
</template>
```

Example "Idea for content":

```vue
<template>
  <q-btn color="purple" label="Account Settings">
    <q-menu>
      <div class="row no-wrap q-pa-md">
        <div class="column">
          <div class="text-h6 q-mb-md">Settings</div>
          <q-toggle v-model="mobileData" label="Use Mobile Data" />
          <q-toggle v-model="bluetooth" label="Bluetooth" />
        </div>

        <q-separator vertical inset class="q-mx-lg" />

        <div class="column items-center">
          <q-avatar size="72px">
            <img
              alt="User avatar"
              src="https://cdn.quasar.dev/img/avatar4.jpg"
            />
          </q-avatar>

          <div class="text-subtitle1 q-mt-md q-mb-xs">John Doe</div>

          <q-btn
            color="primary"
            label="Logout"
            push
            size="sm"
            v-close-popup
          />
        </div>
      </div>
    </q-menu>
  </q-btn>
</template>

<script setup>
import { ref } from 'vue'

const mobileData = ref(true)
const bluetooth = ref(false)
</script>
```

Example "Toggle through v-model":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      color="primary"
      @click="showing = true"
      label="Show"
      :disable="showing"
    />
    <q-btn
      color="primary"
      @click="showing = false"
      label="Hide"
      :disable="!showing"
    />
  </div>

  <div
    style="width: 200px; height: 70px"
    class="bg-purple text-white rounded-borders row flex-center q-mt-md cursor-pointer"
    role="button"
    tabindex="0"
    aria-haspopup="menu"
  >
    Click me

    <q-menu v-model="showing">
      <q-list role="menu" style="min-width: 100px">
        <q-item clickable v-close-popup>
          <q-item-section no-wrap>New tab</q-item-section>
        </q-item>
        <!-- ... -->
      </q-list>
    </q-menu>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const showing = ref(false)
</script>
```

> [!IMPORTANT]
> If you want to conditionally activate or de-activate a QMenu, please use `v-if` on it instead of `v-show`.

### Hover *(v2.26+)*

With the `hover` prop the menu also opens when the pointer hovers its target and closes once the pointer has left both the target and the menu. The `hover-hide-delay` prop controls the grace period in which the pointer can travel between the two (or return) before the menu closes, while `hover-delay` postpones the opening.

Click/tap and keyboard interactions keep toggling the menu as usual, so touch devices (which have no hover) simply fall back to them; this also means that clicking the target (or activating it with <kbd>Enter</kbd>) while the menu is hover-shown closes it. The one exception is a click that lands while the menu is still animating into view: it is ignored, so a single move-and-click gesture on the target cannot close the menu that the very same gesture just opened. A hover-opened menu does not move keyboard focus onto itself. Submenus work too: hovering from a menu into a submenu opened from it keeps the whole chain open.

> [!NOTE]
> The `hover` and `context-menu` props are mutually exclusive; when both are set, `context-menu` takes precedence and `hover` has no effect. Also, hover-triggered opens ignore `touch-position`, which keeps applying to click/tap opens only.

Example "Hover":

```vue
<template>
  <div class="q-gutter-md row items-center">
    <q-btn color="primary" label="Hover me" aria-haspopup="menu">
      <q-menu hover auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>New incognito tab</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable>
            <q-item-section>Settings</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="secondary" label="With submenus" aria-haspopup="menu">
      <q-menu hover>
        <q-list role="menu" dense style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section>Open...</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section>New</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable aria-haspopup="menu">
            <q-item-section>Preferences</q-item-section>
            <q-item-section side>
              <q-icon name="keyboard_arrow_right" />
            </q-item-section>

            <q-menu hover anchor="top end" self="top start">
              <q-list role="menu" dense>
                <q-item
                  v-for="n in 3"
                  :key="n"
                  clickable
                  aria-haspopup="menu"
                >
                  <q-item-section no-wrap>Submenu Label</q-item-section>
                  <q-item-section side>
                    <q-icon name="keyboard_arrow_right" />
                  </q-item-section>
                  <q-menu hover auto-close anchor="top end" self="top start">
                    <q-list role="menu" dense>
                      <q-item v-for="n in 3" :key="n" clickable>
                        <q-item-section no-wrap
                          >3rd level Label</q-item-section
                        >
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-item>
              </q-list>
            </q-menu>
          </q-item>
          <q-separator />
          <q-item clickable v-close-popup>
            <q-item-section>Quit</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="accent" label="With delays" aria-haspopup="menu">
      <q-menu hover :hover-delay="300" :hover-hide-delay="600" auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>Shows up after 300ms</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>Lingers for 600ms</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>
  </div>
</template>
```

### Submenus

Example "Menus in menus":

```vue
<template>
  <div class="q-gutter-md row items-center">
    <q-btn color="primary" label="Click me" aria-haspopup="menu">
      <q-menu>
        <q-list role="menu" dense style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section>Open...</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section>New</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable aria-haspopup="menu">
            <q-item-section>Preferences</q-item-section>
            <q-item-section side>
              <q-icon name="keyboard_arrow_right" />
            </q-item-section>

            <q-menu anchor="top end" self="top start">
              <q-list role="menu">
                <q-item
                  v-for="n in 3"
                  :key="n"
                  dense
                  clickable
                  aria-haspopup="menu"
                >
                  <q-item-section no-wrap>Submenu Label</q-item-section>
                  <q-item-section side>
                    <q-icon name="keyboard_arrow_right" />
                  </q-item-section>
                  <q-menu auto-close anchor="top end" self="top start">
                    <q-list role="menu">
                      <q-item v-for="n in 3" :key="n" dense clickable>
                        <q-item-section no-wrap
                          >3rd level Label</q-item-section
                        >
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-item>
              </q-list>
            </q-menu>
          </q-item>
          <q-separator />
          <q-item clickable v-close-popup>
            <q-item-section>Quit</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-bar
      style="min-width: 250px"
      class="bg-teal text-white rounded-borders"
    >
      <div
        class="cursor-pointer non-selectable"
        role="button"
        tabindex="0"
        aria-haspopup="menu"
      >
        File
        <q-menu>
          <q-list role="menu" dense style="min-width: 100px">
            <q-item clickable v-close-popup>
              <q-item-section>Open...</q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section>New</q-item-section>
            </q-item>
            <q-separator />
            <q-item clickable aria-haspopup="menu">
              <q-item-section>Preferences</q-item-section>
              <q-item-section side>
                <q-icon name="keyboard_arrow_right" />
              </q-item-section>

              <q-menu anchor="top end" self="top start">
                <q-list role="menu" dense>
                  <q-item
                    v-for="n in 3"
                    :key="n"
                    clickable
                    aria-haspopup="menu"
                  >
                    <q-item-section no-wrap>Submenu Label</q-item-section>
                    <q-item-section side>
                      <q-icon name="keyboard_arrow_right" />
                    </q-item-section>
                    <q-menu auto-close anchor="top end" self="top start">
                      <q-list role="menu" dense>
                        <q-item v-for="n in 3" :key="n" clickable>
                          <q-item-section no-wrap
                            >3rd level Label</q-item-section
                          >
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-item>
                </q-list>
              </q-menu>
            </q-item>
            <q-separator />
            <q-item clickable v-close-popup>
              <q-item-section>Quit</q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </div>
      <div
        class="cursor-pointer non-selectable"
        role="button"
        tabindex="0"
        aria-haspopup="menu"
        >Edit
        <q-menu>
          <q-list role="menu" dense style="min-width: 100px">
            <q-item clickable v-close-popup>
              <q-item-section>Cut</q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section>Copy</q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section>Paste</q-item-section>
            </q-item>
            <q-separator />
            <q-item clickable v-close-popup>
              <q-item-section no-wrap>Select All</q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </div>
      <q-space />
      <q-btn dense flat icon="minimize" />
      <q-btn dense flat icon="crop_square" />
      <q-btn dense flat icon="close" />
    </q-bar>
  </div>
</template>
```

### Sizing and styling

Example "Sizing":

```vue
<template>
  <div class="q-gutter-md">
    <q-btn
      color="accent"
      label="Fit Menu"
      style="width: 280px"
      aria-haspopup="menu"
    >
      <q-menu fit auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="brown" label="Max Height Menu" aria-haspopup="menu">
      <q-menu max-height="130px" auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="indigo" label="Max Width Menu" aria-haspopup="menu">
      <q-menu max-width="80px" auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section>
              <q-item-label lines="1">New tab</q-item-label>
            </q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>
  </div>
</template>
```

Example "Style":

```vue
<template>
  <div class="q-gutter-md row">
    <q-btn color="primary" label="Content Class Menu" aria-haspopup="menu">
      <q-menu class="bg-purple text-white" auto-close>
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Content Style Menu" aria-haspopup="menu">
      <q-menu
        :style="{ backgroundColor: '#9C27B0', color: 'white' }"
        auto-close
      >
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>New tab</q-item-section>
          </q-item>
          <!-- ... -->
        </q-list>
      </q-menu>
    </q-btn>
  </div>
</template>
```

### Context menu

You can also set QMenu to act as a context menu. A right click on the parent target triggers it, and so does a long press on touch-capable devices. A context menu opens on those interactions only, so the `hover` prop has no effect on it.

```vue
<template>
  <q-img src="https://cdn.quasar.dev/img/parallax1.jpg" style="height: 100px">
    <q-menu touch-position context-menu>
      <q-list role="menu" dense style="min-width: 100px">
        <q-item clickable v-close-popup>
          <q-item-section>Open...</q-item-section>
        </q-item>
        <q-item clickable v-close-popup>
          <q-item-section>New</q-item-section>
        </q-item>
        <q-separator />
        <q-item clickable aria-haspopup="menu">
          <q-item-section>Preferences</q-item-section>
          <q-item-section side>
            <q-icon name="keyboard_arrow_right" />
          </q-item-section>

          <q-menu anchor="top end" self="top start">
            <q-list role="menu">
              <q-item
                v-for="n in 3"
                :key="n"
                dense
                clickable
                aria-haspopup="menu"
              >
                <q-item-section no-wrap>Submenu Label</q-item-section>
                <q-item-section side>
                  <q-icon name="keyboard_arrow_right" />
                </q-item-section>
                <q-menu auto-close anchor="top end" self="top start">
                  <q-list role="menu">
                    <q-item v-for="n in 3" :key="n" dense clickable>
                      <q-item-section no-wrap>3rd level Label</q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-item>
            </q-list>
          </q-menu>
        </q-item>
        <q-separator />
        <q-item clickable v-close-popup>
          <q-item-section>Quit</q-item-section>
        </q-item>
      </q-list>
    </q-menu>
  </q-img>
</template>
```

### Persistent

If you want the QMenu to not close if app route changes or if hitting ESCAPE key or if clicking/tapping outside of the menu, then use `persistent` prop:

```vue
<template>
  <q-btn color="primary" label="Persistent Menu" aria-haspopup="menu">
    <q-menu persistent auto-close>
      <q-list role="menu" style="min-width: 100px">
        <q-item clickable>
          <q-item-section no-wrap>New tab</q-item-section>
        </q-item>
        <!-- ... -->
      </q-list>
    </q-menu>
  </q-btn>
</template>
```

### Transitions

In the example below there's a few transitions showcased. For a full list of transitions available, go to [Transitions](../options/transitions.md).

Example "Transition examples":

```vue
<template>
  <div class="q-gutter-md row">
    <q-btn color="primary" label="Flip Menu" aria-haspopup="menu">
      <q-menu transition-show="flip-right" transition-hide="flip-left">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>Having fun</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>Crazy for transitions</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable>
            <q-item-section no-wrap>Mind blown</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Scale Menu" aria-haspopup="menu">
      <q-menu transition-show="scale" transition-hide="scale">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>Having fun</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>Crazy for transitions</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable>
            <q-item-section no-wrap>Mind blown</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Jump Menu" aria-haspopup="menu">
      <q-menu transition-show="jump-down" transition-hide="jump-up">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>Having fun</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>Crazy for transitions</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable>
            <q-item-section no-wrap>Mind blown</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Rotate Menu" aria-haspopup="menu">
      <q-menu transition-show="rotate" transition-hide="rotate">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable>
            <q-item-section no-wrap>Having fun</q-item-section>
          </q-item>
          <q-item clickable>
            <q-item-section no-wrap>Crazy for transitions</q-item-section>
          </q-item>
          <q-separator />
          <q-item clickable>
            <q-item-section no-wrap>Mind blown</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
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
        v-bind="triggerAttrs('#target-img-1')"
      >
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px"
          >#target-img-1</div
        >
      </q-img>
      <q-img
        src="https://cdn.quasar.dev/img/parallax2.jpg"
        id="target-img-2"
        style="height: 100px"
        v-bind="triggerAttrs('#target-img-2')"
      >
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px"
          >#target-img-2</div
        >
      </q-img>
      <q-img
        src="https://cdn.quasar.dev/img/blueish.jpg"
        style="height: 100px"
        v-bind="triggerAttrs(true)"
      >
        <div class="absolute-bottom-right" style="border-top-left-radius: 5px"
          >Original parent</div
        >
        <q-menu touch-position :target="targetEl">
          <q-list role="menu">
            <q-item v-for="n in 5" :key="n" v-close-popup clickable>
              <q-item-section>Label</q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </q-img>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const targetEl = ref('#target-img-1')

function triggerAttrs(val) {
  return targetEl.value === val
    ? { role: 'button', tabindex: 0, 'aria-haspopup': 'menu' }
    : {}
}
</script>
```

### Positioning

Example "Position examples":

```vue
<template>
  <div class="q-gutter-md row">
    <q-btn color="primary" label="Anchor/Self Menu" aria-haspopup="menu">
      <q-menu anchor="top right" self="top left">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>anchor="top right"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>self="top left"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Anchor/Self Menu" aria-haspopup="menu">
      <q-menu anchor="center middle" self="center middle">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>anchor="center middle"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>self="center middle"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Anchor/Self Menu" aria-haspopup="menu">
      <q-menu anchor="bottom right" self="bottom left">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>anchor="bottom right"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>self="bottom left"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Offset Menu" aria-haspopup="menu">
      <q-menu :offset="[0, 20]">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[0, 20]"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[0, 20]"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Offset Menu" aria-haspopup="menu">
      <q-menu :offset="[50, 10]">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[50, 10]"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[50, 10]"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>

    <q-btn color="primary" label="Offset Menu" aria-haspopup="menu">
      <q-menu :offset="[-50, 10]">
        <q-list role="menu" style="min-width: 100px">
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[-50, 10]"</q-item-section>
          </q-item>
          <q-item clickable v-close-popup>
            <q-item-section no-wrap>:offset="[-50, 10]"</q-item-section>
          </q-item>
        </q-list>
      </q-menu>
    </q-btn>
  </div>
</template>
```

The position of QMenu can be customized. It keeps account of the `anchor` and `self` optional props.
The final position of QMenu popup is calculated so that it will be displayed on the available screen real estate: when the requested placement does not fit, the popup switches to the opposite side of the anchor on that axis if it offers more room, otherwise it stays on the requested side (so a trigger with equal room above and below keeps your `anchor`/`self`) and gets capped to the room it has.

For horizontal positioning you can use `start` and `end` when you want to automatically take into account if on RTL or non-RTL. `start` and `end` mean "left" for non-RTL and "right" for RTL.

> Visit the [live documentation](https://quasar.dev/vue-components/menu) for the interactive menu positioning demo.

> [!NOTE]
> The `offset` prop does not translate the popup by a number of pixels. It expands the **anchor element's bounding box** outward: `offset[0]` moves that box's `left` edge to the left and its `right` edge to the right, while `offset[1]` moves `top` up and `bottom` down. The popup's `self` point is then aligned to the `anchor` point of the expanded box, and only after that is the result clamped to the available screen real estate.
>
> Two consequences are worth knowing, because both make an `offset` look like it is being ignored on one axis:
>
> - **A `middle` or `center` anchor point does not move with the offset.** Expanding both edges by the same amount leaves the midpoint exactly where it was, so `offset[0]` is a no-op for `anchor="... middle"` and `offset[1]` is a no-op for `anchor="center ..."`, no matter which value you pass. QMenu's default `anchor` (`bottom start`) responds on both axes, but a `middle`/`center` one that you set yourself will not.
> - **A clamped popup does not move with the offset either.** Since the offset pushes the box outward, anchoring to a full-width or screen-edge element (or passing a very large value) can send the popup past a viewport edge, where it gets clamped back and the final position no longer depends on the offset value. Attach QMenu to an inline or `inline-block` trigger and point `anchor`/`self` into free space, so the offset has room to take effect.
>
> The `cover` prop ignores `offset` altogether, since the popup is meant to sit right on top of the anchor element.

## Accessibility *(v2.25+)*

### Semantics

QMenu renders as a plain positioned container, deliberately claiming no ARIA role of its own: it can host any kind of content, while the [WAI-ARIA `menu` role](https://www.w3.org/TR/wai-aria-1.2/#menu) permits nothing but menu items as children — a form, a date picker or a list would all become invalid markup under it.

So when the popup content really is a menu — a list of commands — declare it as such by setting `role="menu"` on the wrapping [QList](list-and-list-items.md). The contained QItems then adapt automatically: actionable ones (clickable or link items, including disabled ones) expose themselves as `menuitem`, anything else (section headers etc.) stays neutral, and QSeparator already announces itself as a separator. An individual item can override its derived role — for instance with `role="menuitemcheckbox"` or `role="menuitemradio"` for toggle entries, in which case managing `aria-checked` is up to you. The "Basic" example above shows the declaration.

If you attach a role to the QMenu container itself instead (it forwards any `role` you pass), make sure its entire content satisfies that role's requirements.

### Anchor semantics

The anchor is your own markup, yet it is the control that opens the popup, so QMenu maintains the trigger's ARIA on it from the outside: `aria-expanded` follows the open state, and a `role` declared on the QMenu container itself is mirrored onto the anchor as `aria-haspopup` (that attribute names the popup's role, so only `menu`, `listbox`, `tree`, `grid` and `dialog` can be mirrored). A `<q-btn>` wrapping a QMenu is therefore announced as a collapsed or expanded trigger with no work on your part.

This requires an anchor that ARIA allows the state on — a `<button>`, a link with an `href`, or any element declaring a widget role such as `role="button"`. A plain `<div>` computes to the generic role, where `aria-expanded` is invalid, so QMenu leaves it untouched: give such an anchor a proper role (and a keyboard path to activate it) before using it as a trigger. Two more cases are deliberately left alone — a `context-menu` popup, which opens on right click or long tap rather than on activation, and any `aria-expanded`/`aria-haspopup` you set on the anchor yourself, which QMenu never overwrites.

Do keep in mind that when the role lives on the [QList](list-and-list-items.md) inside the popup — the shape recommended above — QMenu cannot see it, so add `aria-haspopup="menu"` on the anchor yourself:

```html
<q-btn label="Actions" aria-haspopup="menu">
  <q-menu>
    <q-list role="menu">
      <!-- ... -->
    </q-list>
  </q-menu>
</q-btn>
```

### Keyboard navigation

Since the menu renders next to the end of the page, letting <kbd>Tab</kbd> walk past its last focusable element (or <kbd>Shift</kbd> + <kbd>Tab</kbd> before its first one) would drop keyboard focus out of the page. Following the [WAI-ARIA APG](https://www.w3.org/WAI/ARIA/apg/patterns/menu-button/), the menu instead closes and focus continues from its anchor, just like <kbd>Escape</kbd> closes it while returning focus to the anchor. Tabbing between multiple focusable elements *inside* the menu works as usual, and a `persistent` menu opts out of this dismissal too.

Note that focusable menu items are plain Tab stops — QMenu does not (yet) provide the Arrow-key navigation that the [APG menu pattern](https://www.w3.org/WAI/ARIA/apg/patterns/menu-button/) describes for `role="menu"` content.
