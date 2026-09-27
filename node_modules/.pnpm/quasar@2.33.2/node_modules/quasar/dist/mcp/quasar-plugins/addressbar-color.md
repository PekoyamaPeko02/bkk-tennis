---
title: AddressbarColor Plugin
---
Newer mobile browsers have the ability to specify a color for the addressbar, like in the image below.

> [!WARNING]
>
> - There isn't yet a Web standard for this so it won't work for all mobile browsers.
> - This applies when building a website only. For coloring top bar on a mobile app (built with Cordova mode), please refer to [cordova-plugin-statusbar](https://cordova.apache.org/docs/en/latest/reference/cordova-plugin-statusbar/).

![Mobile Addressbar Coloring](https://cdn.quasar.dev/img/mobile-address-bar-colors.jpg)

## AddressbarColor API

Not inlined here: call the `get_api` tool with `name: "AddressbarColor"` for its definition, or add `part` (`methods`, `injection`) for one of them.

## Installation

Add to `quasar.config.js`:

```js
framework: {
    plugins: [
      'AddressbarColor'
    ]
}
```

## Usage

We create boot file to initialize its usage: `quasar new boot addressbar-color [--format ts]`. A file is created (`/src/boot/addressbar-color.js`). We edit it:

Example "/src/boot/addressbar-color.js":

```js
import { defineBoot } from '#q-app'
import { AddressbarColor } from 'quasar'

export default defineBoot(() => {
  AddressbarColor.set('#a2e3fa')
})
```

We then have to tell quasar to use this boot file we just created. To do this we edit the boot section of the quasar config:

Example "/quasar.config file":

```js
return {
  boot: ['addressbar-color']
}
```

What this does is that it injects some `<meta>` tags into your `index.html` at runtime.

Because the meta tag doesn't get injected until run time you can dynamically change this color multiple times, based on the page the user is on (by calling the `set` method in the `created()` lifecycle hook on the respective pages):

Example "A .vue file representing a page":

```js
import { useQuasar } from 'quasar'

export default {
  setup() {
    // equivalent to calling this on creating the component
    const $q = useQuasar()
    $q.addressbarColor.set('#a2e3fa')
  }
}
```

> [!NOTE]
> Calling `set()` with no parameters will use the primary color.
