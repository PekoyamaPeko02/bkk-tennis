---
title: Tab Panels
related:
  - title: Tabs
    path: tabs.md
---
Tab panels are a way of displaying more information using less window real estate.

> [!NOTE]
> Works great along with [QTabs](tabs.md) but it is not required to be used with it.

## QTabPanels API

Not inlined here: call the `get_api` tool with `name: "QTabPanels"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## QTabPanel API

Not inlined here: call the `get_api` tool with `name: "QTabPanel"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

> [!TIP]
>
> - Works great along with [QTabs](tabs.md), a component which offers a nice way to select the active tab panel to display.
> - If the QTabpanel content also has images and you want to use swipe actions to navigate, you might want to add `draggable="false"` to them, otherwise the native browser behavior might interfere in a negative way.

> [!NOTE]
> Do not be mistaken by the "QTabPanels" component name. Panels do not require QTabs. They can be used as standalone too.

> [!IMPORTANT]
> **Keep Alive**
>
> - Please take notice of the Boolean `keep-alive` prop for QTabPanels, if you need this behavior. Do NOT use Vue's native `<keep-alive>` component over QTabPanel.
> - Should you need the `keep-alive-include` or `keep-alive-exclude` props then the QTabPanel `name`s must be valid Vue component names (no spaces allowed, don't start with a number etc).

### Basic

```vue
<template>
  <div class="q-gutter-y-md" style="max-width: 350px">
    <q-option-group
      v-model="panel"
      inline
      :options="[
        { label: 'Mails', value: 'mails' },
        { label: 'Alarms', value: 'alarms' },
        { label: 'Movies', value: 'movies' }
      ]"
    />

    <q-tab-panels v-model="panel" animated class="shadow-2 rounded-borders">
      <q-tab-panel name="mails">
        <div class="text-h6">Mails</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="alarms">
        <div class="text-h6">Alarms</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="movies">
        <div class="text-h6">Movies</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>
    </q-tab-panels>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const panel = ref('mails')
</script>
```

### Accessibility *(v2.25+)*

Each QTabPanel renders with the `tabpanel` ARIA role and is focusable (`tabindex="0"`), so keyboard and screen reader users can reach the panel content even when nothing inside it is focusable. Should your panel start with its own focusable content, you can remove the extra Tab stop with `tabindex="-1"`.

Since QTabPanels does not require a QTabs (and can be placed anywhere relative to one), the two components cannot wire the ARIA relationship between a tab and its panel for you. When pairing them, supply the attributes yourself:

```html
<q-tabs v-model="tab">
  <q-tab
    name="mails"
    id="mails-tab"
    aria-controls="mails-panel"
    label="Mails"
  />
</q-tabs>

<q-tab-panels v-model="tab">
  <q-tab-panel name="mails" id="mails-panel" aria-labelledby="mails-tab">
    ...
  </q-tab-panel>
</q-tab-panels>
```

### With QTabs

> [!NOTE]
> QTabPanels can be used as standalone too. They do not depend on the presence of a QTabs. Also, they can be placed anywhere within a page, not just near QTabs.

```vue
<template>
  <div class="q-gutter-y-md" style="max-width: 600px">
    <q-card>
      <q-tabs
        v-model="tab"
        dense
        class="text-grey"
        active-color="primary"
        indicator-color="primary"
        align="justify"
        narrow-indicator
      >
        <q-tab name="mails" label="Mails" />
        <q-tab name="alarms" label="Alarms" />
        <q-tab name="movies" label="Movies" />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="alarms">
          <div class="text-h6">Alarms</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="movies">
          <div class="text-h6">Movies</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>
      </q-tab-panels>
    </q-card>

    <q-card>
      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="alarms">
          <div class="text-h6">Alarms</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="movies">
          <div class="text-h6">Movies</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>
      </q-tab-panels>

      <q-separator />

      <q-tabs
        v-model="tab"
        dense
        :class="$q.dark.isActive ? 'bg-grey-9' : 'bg-grey-3'"
        align="justify"
        narrow-indicator
      >
        <q-tab name="mails" label="Mails" />
        <q-tab name="alarms" label="Alarms" />
        <q-tab name="movies" label="Movies" />
      </q-tabs>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('mails')
</script>
```

Example "A more complex example":

```vue
<template>
  <div style="max-width: 600px">
    <q-card>
      <q-tabs
        v-model="tab"
        dense
        class="text-grey"
        active-color="primary"
        indicator-color="primary"
        align="justify"
      >
        <q-tab name="mails" label="Mails" />
        <q-tab name="alarms" label="Alarms" />
        <q-tab name="movies" label="Movies" />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="mails" class="q-pa-none">
          <q-splitter v-model="splitterModel" style="height: 250px">
            <template #before>
              <q-tabs v-model="innerTab" vertical class="text-teal">
                <q-tab name="innerMails" icon="mail" label="Mails" />
                <q-tab name="innerAlarms" icon="alarm" label="Alarms" />
                <q-tab name="innerMovies" icon="movie" label="Movies" />
              </q-tabs>
            </template>

            <template #after>
              <q-tab-panels
                v-model="innerTab"
                animated
                transition-prev="slide-down"
                transition-next="slide-up"
              >
                <q-tab-panel name="innerMails">
                  <div class="text-h4 q-mb-md">Mails</div>
                  <p
                    >Lorem ipsum dolor sit, amet consectetur adipisicing elit.
                    Quis praesentium cumque magnam odio iure quidem, quod illum
                    numquam possimus obcaecati commodi minima assumenda
                    consectetur culpa fuga nulla ullam. In, libero.</p
                  >
                  <p
                    >Lorem ipsum dolor sit, amet consectetur adipisicing elit.
                    Quis praesentium cumque magnam odio iure quidem, quod illum
                    numquam possimus obcaecati commodi minima assumenda
                    consectetur culpa fuga nulla ullam. In, libero.</p
                  >
                </q-tab-panel>

                <!-- ... -->
              </q-tab-panels>
            </template>
          </q-splitter>
        </q-tab-panel>

        <q-tab-panel name="alarms">
          <div class="text-h6">Alarms</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="movies">
          <div class="text-h6">Movies</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('mails')
const innerTab = ref('innerMails')
const splitterModel = ref(20)
</script>
```

### Coloring

```vue
<template>
  <div class="q-gutter-y-md" style="max-width: 600px">
    <q-card>
      <q-tabs
        v-model="tab"
        dense
        class="bg-grey-2 text-grey-7"
        active-color="primary"
        indicator-color="purple"
        align="justify"
      >
        <q-tab name="mails" label="Mails" />
        <q-tab name="alarms" label="Alarms" />
        <q-tab name="movies" label="Movies" />
      </q-tabs>

      <q-tab-panels v-model="tab" animated class="bg-primary text-white">
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="alarms">
          <div class="text-h6">Alarms</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="movies">
          <div class="text-h6">Movies</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>
      </q-tab-panels>
    </q-card>

    <q-card>
      <q-tabs
        v-model="tab"
        class="bg-purple text-white"
        align="justify"
        narrow-indicator
      >
        <q-tab name="mails" label="Mails" />
        <q-tab name="alarms" label="Alarms" />
        <q-tab name="movies" label="Movies" />
      </q-tabs>

      <q-separator />

      <q-tab-panels
        v-model="tab"
        animated
        class="bg-orange-1 text-dark text-center"
      >
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="alarms" class="bg-grey-9 text-white">
          <div class="text-h6">Alarms</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <q-tab-panel name="movies" class="bg-lime-1 text-dark">
          <div class="text-h6">Movies</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('mails')
</script>
```

### With vertical QTabs and QSplitter

```vue
<template>
  <q-splitter v-model="splitterModel" style="height: 250px">
    <template #before>
      <q-tabs v-model="tab" vertical class="text-teal">
        <q-tab name="mails" icon="mail" label="Mails" />
        <q-tab name="alarms" icon="alarm" label="Alarms" />
        <q-tab name="movies" icon="movie" label="Movies" />
      </q-tabs>
    </template>

    <template #after>
      <q-tab-panels
        v-model="tab"
        animated
        swipeable
        vertical
        transition-prev="jump-up"
        transition-next="jump-up"
      >
        <q-tab-panel name="mails">
          <div class="text-h4 q-mb-md">Mails</div>
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
        </q-tab-panel>

        <!-- ... -->
      </q-tab-panels>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('mails')
const splitterModel = ref(20)
</script>
```

For a full list of transitions, please check out [Transitions](../options/transitions.md).

### Custom transitions

Example "Custom transition examples":

```vue
<template>
  <div style="max-width: 600px">
    <q-tabs v-model="tab" align="justify" narrow-indicator class="q-mb-lg">
      <q-tab class="text-purple" name="mails" label="Mails" />
      <q-tab class="text-orange" name="alarms" label="Alarms" />
      <q-tab class="text-teal" name="movies" label="Movies" />
    </q-tabs>

    <div class="q-gutter-y-sm">
      <q-tab-panels
        v-model="tab"
        animated
        transition-prev="scale"
        transition-next="scale"
        class="bg-purple text-white text-center"
      >
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <!-- ... -->
      </q-tab-panels>

      <q-tab-panels
        v-model="tab"
        animated
        transition-prev="fade"
        transition-next="fade"
        class="bg-orange text-white text-center"
      >
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <!-- ... -->
      </q-tab-panels>

      <q-tab-panels
        v-model="tab"
        animated
        transition-prev="jump-up"
        transition-next="jump-down"
        class="bg-teal text-white text-center"
      >
        <q-tab-panel name="mails">
          <div class="text-h6">Mails</div>
          Lorem ipsum dolor sit amet consectetur adipisicing elit.
        </q-tab-panel>

        <!-- ... -->
      </q-tab-panels>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const tab = ref('mails')
</script>
```

In the example below, use your mouse to swipe through the panels or, if on a touch capable device, swipe with your fingers.

### Swipeable and infinite

```vue
<template>
  <div style="max-width: 350px">
    <q-tab-panels
      v-model="panel"
      animated
      swipeable
      infinite
      class="bg-purple text-white shadow-2 rounded-borders"
    >
      <q-tab-panel name="mails">
        <div class="text-h6">Mails</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="alarms">
        <div class="text-h6">Alarms</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="movies">
        <div class="text-h6">Movies</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>
    </q-tab-panels>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const panel = ref('mails')
</script>
```

### Vertical swipeable and infinite

```vue
<template>
  <div style="max-width: 350px">
    <q-tab-panels
      v-model="panel"
      animated
      swipeable
      vertical
      infinite
      class="bg-purple text-white shadow-2 rounded-borders"
    >
      <q-tab-panel name="mails">
        <div class="text-h6">Mails</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="alarms">
        <div class="text-h6">Alarms</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>

      <q-tab-panel name="movies">
        <div class="text-h6">Movies</div>
        Lorem ipsum dolor sit amet consectetur adipisicing elit.
      </q-tab-panel>
    </q-tab-panels>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const panel = ref('mails')
</script>
```
