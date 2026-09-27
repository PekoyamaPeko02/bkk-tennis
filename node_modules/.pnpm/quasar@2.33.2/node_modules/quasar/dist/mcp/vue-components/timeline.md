---
title: Timeline
---
The QTimeline component displays a list of events in chronological order. It is typically a graphic design showing a long bar labelled with dates alongside itself and usually events. Timelines can use any time scale, depending on the subject and data.

QTimeline has 3 layouts:

- `dense` (default) is showing headings, titles, subtitles and content on the **timeline-specified side** of the time line (default on right)
- `comfortable` is showing headings, titles and content on the **timeline-specified side** of the time line (default on right) and the subtitles on the other side
- `loose` is showing headings on center, titles and content on the **entry-specified side** of the time line (default on right) and the subtitles on the other side

## QTimeline API

Not inlined here: call the `get_api` tool with `name: "QTimeline"` for its definition, or add `part` (`props`, `slots`) for one of them.

## QTimelineEntry API

Not inlined here: call the `get_api` tool with `name: "QTimelineEntry"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

### Basic

```vue
<template>
  <q-timeline color="secondary">
    <q-timeline-entry heading> Timeline heading </q-timeline-entry>

    <q-timeline-entry title="Event Title" subtitle="February 22, 1986">
      <div>
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
        eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
        minim veniam, quis nostrud exercitation ullamco laboris nisi ut
        aliquip ex ea commodo consequat. Duis aute irure dolor in
        reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
        pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
        culpa qui officia deserunt mollit anim id est laborum.
      </div>
    </q-timeline-entry>

    <!-- ... -->
  </q-timeline>
</template>
```

### Using props only

Below is the same example, but using QTimelineEntry properties only instead of the default slot:

Example "Props only":

```vue
<template>
  <q-timeline color="secondary">
    <q-timeline-entry heading body="Timeline heading" />

    <q-timeline-entry
      title="Event Title"
      subtitle="February 22, 1986"
      avatar="https://cdn.quasar.dev/img/avatar3.jpg"
      :body="body"
    />

    <!-- ... -->
  </q-timeline>
</template>

<script setup>
const body =
  'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.'
</script>
```

### Using slots only

Below is again the same example, but using only QTimelineEntry slots:

Example "Slots only":

```vue
<template>
  <q-timeline color="secondary">
    <q-timeline-entry heading> Timeline heading </q-timeline-entry>

    <q-timeline-entry>
      <template #title> Event Title </template>
      <template #subtitle> February 22, 1986 </template>

      <div>
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
        eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
        minim veniam, quis nostrud exercitation ullamco laboris nisi ut
        aliquip ex ea commodo consequat. Duis aute irure dolor in
        reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
        pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
        culpa qui officia deserunt mollit anim id est laborum.
      </div>
    </q-timeline-entry>

    <!-- ... -->
  </q-timeline>
</template>
```

### Dark design

Example "Force dark mode":

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-timeline dark color="secondary">
      <q-timeline-entry heading>Timeline heading</q-timeline-entry>

      <q-timeline-entry
        title="Event Title"
        subtitle="February 22, 1986"
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
      >
        <div>
          Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
          eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
          minim veniam, quis nostrud exercitation ullamco laboris nisi ut
          aliquip ex ea commodo consequat. Duis aute irure dolor in
          reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
          pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
          culpa qui officia deserunt mollit anim id est laborum.
        </div>
      </q-timeline-entry>

      <!-- ... -->
    </q-timeline>
  </div>
</template>
```

### Layouts and side selection

> [!NOTE]
> QTimelineEntry only takes into account its `side` prop if QTimeline has the `loose` layout.

```vue
<template>
  <div class="row q-gutter-md q-mb-lg">
    <q-option-group
      type="radio"
      dense
      v-model="layout"
      :options="[
        { label: 'Dense layout', value: 'dense' },
        { label: 'Comfortable layout', value: 'comfortable' },
        { label: 'Loose layout', value: 'loose' }
      ]"
    />
    <q-option-group
      type="radio"
      dense
      v-model="side"
      :disable="layout === 'loose'"
      :options="[
        { label: 'Content on right', value: 'right' },
        { label: 'Content on left', value: 'left' }
      ]"
    />
  </div>

  <q-timeline :layout="layout" :side="side" color="secondary">
    <q-timeline-entry heading>Timeline heading</q-timeline-entry>

    <q-timeline-entry
      title="Event Title"
      subtitle="February 22, 1986"
      side="left"
    >
      <div>
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
        eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
        minim veniam, quis nostrud exercitation ullamco laboris nisi ut
        aliquip ex ea commodo consequat. Duis aute irure dolor in
        reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
        pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
        culpa qui officia deserunt mollit anim id est laborum.
      </div>
    </q-timeline-entry>

    <!-- ... -->
  </q-timeline>
</template>

<script setup>
import { ref } from 'vue'

const layout = ref('dense')
const side = ref('right')
</script>
```

### Responsive

> [!NOTE]
> The examples below uses `$q.screen` to detect changes in window size to see all 3 layouts in action.

Example "Responsive layout":

```vue
<template>
  <q-timeline :layout="layout" color="secondary">
    <q-timeline-entry heading>
      Timeline heading
      <br />
      ({{
        $q.screen.lt.sm ? 'Dense' : $q.screen.lt.md ? 'Comfortable' : 'Loose'
      }}
      layout)
    </q-timeline-entry>

    <q-timeline-entry
      title="Event Title"
      subtitle="February 22, 1986"
      side="left"
    >
      <div>
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do
        eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad
        minim veniam, quis nostrud exercitation ullamco laboris nisi ut
        aliquip ex ea commodo consequat. Duis aute irure dolor in
        reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla
        pariatur. Excepteur sint occaecat cupidatat non proident, sunt in
        culpa qui officia deserunt mollit anim id est laborum.
      </div>
    </q-timeline-entry>

    <!-- ... -->
  </q-timeline>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { computed } from 'vue'

const $q = useQuasar()
const layout = computed(() =>
  $q.screen.lt.sm ? 'dense' : $q.screen.lt.md ? 'comfortable' : 'loose'
)
</script>
```

## Accessibility *(v2.25+)*

QTimeline renders a native `<ul>` with each entry as a `<li>`, so entries read as a list. Be aware that entry titles render as `h6` elements regardless of where the timeline sits in your document's heading outline (only a `heading` entry lets you pick its level, through the `tag` prop — an `h3` by default), and that avatar images carry no `alt` attribute — supply meaningful structure and text through the slots and props accordingly.
