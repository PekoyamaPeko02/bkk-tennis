---
title: Ajax Bar
related:
  - title: Loading Plugin
    path: ../quasar-plugins/loading.md
  - title: LoadingBar
    path: ../quasar-plugins/loading-bar.md
  - title: Fetching Data
    path: 'https://quasar.dev/quasar-cli-vite/fetching-data'
---
In most mobile apps and even some desktop apps, you will most likely have some API communication to a server via an [Ajax call](https://en.wikipedia.org/wiki/Ajax_(programming)). Since these calls can take more than a second or two, it is good UX to offer the user feedback, when such an API call is being made. Which is where QAjaxBar comes into helping you out.

QAjaxBar is a component which displays a loading bar (like Youtube) whenever an Ajax call (regardless of Ajax library used) is in progress. It can be manually triggered as well.

> [!TIP]
> If you'd like **a simpler and more convenient way** to offer an Ajax Bar to your users, have a look at the [Loading Bar Plugin](../quasar-plugins/loading-bar.md), which is actually **the recommended way**.

## QAjaxBar API

Not inlined here: call the `get_api` tool with `name: "QAjaxBar"` for its definition, or add `part` (`props`, `methods`, `events`) for one of them.

## Usage

The QAjaxBar component captures Ajax calls automatically (unless told not to).

The example below triggers events manually for demonstrating purposes only. This one is set to appear at bottom (multiple positions available!) of the page, with a 10px size (default is different) and uses a custom color.

### Basic

```vue
<template>
  <q-ajax-bar
    ref="barRef"
    position="bottom"
    color="accent"
    size="10px"
    skip-hijack
  />

  <q-btn color="primary" label="Trigger" @click="trigger" />
</template>

<script setup>
import { useTemplateRef } from 'vue'

const barRef = useTemplateRef('barRef')

// we manually trigger it (this is not needed if we
// don't skip Ajax calls hijacking)
function trigger() {
  barRef.value.start()

  setTimeout(
    () => {
      barRef.value?.stop()
    },
    Math.random() * 3000 + 1000
  )
}
</script>
```

Please check out the API section for all properties that you can use.

### Ajax filter

Should you want QAjaxBar to trigger only for some URLs (and not for all, like in the default behavior), then you can use the `hijackFilter` property:

```html
<template>
  <q-ajax-bar :hijack-filter="myFilterFn" />
</template>

<script setup>
  function myFilterFn(url) {
    // example (only https://my-service.com/* should trigger)
    return /^https:\/\/my-service\.com/.test(url)
  }
</script>
```

## Tips

- If multiple events are captured by Ajax Bar simultaneously, `@start` and `@stop` will still be triggered only once: when bar starts showing up and when it becomes hidden.
- Each Ajax call makes a `start()` call when it is triggered. When it ends, it calls `stop()`. So yes, if you also manually trigger QAjaxBar you must call `start()` each time a new event is starting and `stop()` each time an event finished. QAjaxBar knows to handle multiple events simultaneously.
- The automatic capture is designed to function exclusively with libraries utilizing [XMLHttpRequest (XHR)](https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest). Consequently, if you opt for the native browser [Fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API), it won't initiate the loading bar automatically.

## Accessibility *(v2.25+)*

While running, the bar exposes `role="progressbar"` with the usual value attributes, and it is hidden from assistive technology (`aria-hidden`) when idle. Its progress values are synthetic — incremented on a timer, not measured — so treat it as decorative feedback: give it an `aria-label` (e.g. "Page loading"), and announce long-running operations that actually matter through a live region of your own, since the bar's start and completion are not announced.
