---
title: Body classes
---
Quasar attaches some very useful helper CSS classes to document.body which you can take advantage of:

| Name | Description |
| --- | --- |
| `body--dark` | When in [dark mode](dark-mode.md) |
| `body--light` | When not in [dark mode](dark-mode.md) |
| `desktop` | When client is on a desktop |
| `mobile` | When client is on a mobile device |
| `touch` | When client has touch support |
| `no-touch` | When client does NOT have touch support |
| `platform-android` | When client is on an Android device |
| `platform-ios` | When client is on an iOS device |
| `native-mobile` | When client is on a Quasar CLI Cordova or Capacitor app |
| `electron` | When client is on a Quasar CLI Electron app |
| `bex` | When app runs from a browser extension |
| `within-iframe` | When app runs from an iframe |
| `screen--*` | If [enabled (only)](../options/screen-plugin.md#how-to-enable-body-classes), tells current window breakpoint (`screen--xs`, `screen--sm`, ..., `screen--xl`) |
