## Platform API

### Props

- `userAgent` (string, optional)
  Client browser User Agent
  Examples:
    - `'mozilla/5.0 (macintosh; intel mac os x 10_14_5) applewebkit/537.36 (khtml, like gecko) chrome/75.0.3770.100 safari/537.36'`
- `is` (object, optional)
  Client browser details (property names depend on browser)
  Examples:
    - `{ chrome: true, version: '71.0.3578.98', versionNumber: 71, mac: true, desktop: true, webkit: true, name: 'chrome', platform: 'mac' }`
  Object shape:
    - `name` (string, optional)
      Browser name
      Examples: `'chrome'`
    - `platform` (string, optional)
      Platform name
      Examples: `'mac'`
    - `version` (string, optional)
      Detailed browser version
      Examples: `'71.0.3578.98'`
    - `versionNumber` (number, optional)
      Major browser version as a number
    - `mobile` (boolean, optional)
      Whether the platform is mobile
    - `desktop` (boolean, optional)
      Whether the platform is desktop
    - `cordova` (boolean, optional)
      Whether the platform is Cordova
    - `capacitor` (boolean, optional)
      Whether the platform is Capacitor
    - `nativeMobile` (boolean, optional)
      Whether the platform is a native mobile wrapper
    - `nativeMobileWrapper` (string, optional)
      Type of the native mobile wrapper
      Accepts: `'cordova'`, `'capacitor'`
    - `electron` (boolean, optional)
      Whether the platform is Electron
    - `bex` (boolean, optional)
      Whether the platform is BEX(Browser Extension)
    - `linux` (boolean, optional)
      Whether the operating system is Linux
    - `mac` (boolean, optional)
      Whether the operating system is Mac OS
    - `win` (boolean, optional)
      Whether the operating system is Windows
    - `cros` (boolean, optional)
      Whether the operating system is Chrome OS
    - `chrome` (boolean, optional)
      Whether the browser is Google Chrome
    - `firefox` (boolean, optional)
      Whether the browser is Firefox
    - `opera` (boolean, optional)
      Whether the browser is Opera
    - `safari` (boolean, optional)
      Whether the browser is Safari
    - `vivaldi` (boolean, optional)
      Whether the browser is Vivaldi
    - `edge` (boolean, optional)
      Whether the browser is Microsoft Edge
    - `webkit` (boolean, optional)
      Whether the browser is a Webkit or Webkit-based one
    - `android` (boolean, optional)
      Whether the operating system is Android
    - `ios` (boolean, optional)
      Whether the operating system is iOS
    - `ipad` (boolean, optional)
      Whether the device is an iPad
    - `iphone` (boolean, optional)
      Whether the device is an iPhone
    - `ipod` (boolean, optional)
      Whether the device is an iPod
- `has` (object, optional)
  Client browser detectable properties
  Examples:
    - `{ touch: false, webStorage: true }`
  Object shape:
    - `touch` (boolean, optional)
      Client browser runs on device with touch support
    - `webStorage` (boolean, optional)
      Client browser has Web Storage support
- `within` (object, optional)
  Client browser environment
  Examples: `{ iframe: false }`
  Object shape:
    - `iframe` (boolean, optional)
      Does the app run under an iframe?

### Methods

- `parseSSR(ssrContext: object): object`
  For SSR/SSG usage only, and only on the global import (not on $q.platform)
  Params:
    - `ssrContext` (object, required)
      SSR Context Object
  Returns: `object`
    Platform object (like $q.platform) for SSR/SSG usage purposes

### Vue Injection

Accessible via `$q.platform` (e.g., `this.$q.platform` in Options API or `useQuasar().platform` in Composition API).

