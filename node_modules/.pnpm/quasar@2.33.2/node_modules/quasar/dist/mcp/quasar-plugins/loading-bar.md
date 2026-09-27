---
title: LoadingBar
related:
  - title: Ajax Bar
    path: ../vue-components/ajax-bar.md
  - title: Linear Progress
    path: ../vue-components/linear-progress.md
  - title: Skeleton
    path: ../vue-components/skeleton.md
---
The Quasar LoadingBar plugin offers an easy way to set up your app with a [QAjaxBar](../vue-components/ajax-bar.md) in case you don't want to handle a QAjaxBar component yourself.

For a demo, please visit the QAjaxBar documentation page.

## LoadingBar API

Not inlined here: call the `get_api` tool with `name: "LoadingBar"` for its definition, or add `part` (`props`, `methods`, `injection`, `quasarConfOptions`) for one of them.

## Installation

Add to `quasar.config.js`:

```js
framework: {
    plugins: [
      'LoadingBar'
    ],
    config: {
      loadingBar: { /* look at QuasarConfOptions from the API card */ }
    }
}
```

LoadingBar options are same as when configuring a [QAjaxBar](../vue-components/ajax-bar.md).

> [!NOTE]
> When using the UMD version of Quasar, all components, directives and plugins are installed by default. This includes LoadingBar. Should you wish to disable it, specify `loadingBar: { skipHijack: true }` (which turns off listening to Ajax traffic).

## Usage

Inside Vue components:

```js
import { useQuasar } from 'quasar'

setup () {
  const $q = useQuasar()

  $q.loadingBar.start()
  $q.loadingBar.stop()
  $q.loadingBar.increment(value)
}
```

Outside of Vue components:

```js
import { LoadingBar } from 'quasar'

LoadingBar.start()
LoadingBar.stop()
LoadingBar.increment(value)
```

### Setting Up Defaults

Should you wish to set up some defaults, rather than specifying them each time, you can do so by using quasar.config file > framework > config > loadingBar: {...} or by calling `LoadingBar.setDefaults({...})` or `$q.loadingBar.setDefaults({...})`. Supports all [QAjaxBar](../vue-components/ajax-bar.md) properties.

Inside Vue components:

```js
import { useQuasar } from 'quasar'

setup () {
  const $q = useQuasar()

  $q.loadingBar.setDefaults({
    color: 'purple',
    size: '15px',
    position: 'bottom'
  })
}
```

Outside of Vue components (includes boot files):

```js
import { LoadingBar } from 'quasar'

LoadingBar.setDefaults({
  color: 'purple',
  size: '15px',
  position: 'bottom'
})
```

### Using an Ajax filter

Should you want to trigger LoadingBar only for some URLs, then you can use the `setDefaults()` method (described above) to configure the `hijackFilter` property:

```js
import { LoadingBar } from 'quasar'

LoadingBar.setDefaults({
  // return a Boolean which has the meaning of
  // "does this URL should trigger LoadingBar?"
  hijackFilter(url) {
    // example (only https://my-service.com/* should trigger)
    return /^https:\/\/my-service\.com/.test(url)
  }
})
```
