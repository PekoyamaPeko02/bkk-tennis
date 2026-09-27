## AddressbarColor API

### Methods

- `set(hexColor: string): void`
  Sets addressbar color (for browsers that support it)
  Params:
    - `hexColor` (string, required)
      Color in hex format
      Examples: `'#ff0000'`

### Vue Injection

Accessible via `$q.addressbarColor` (e.g., `this.$q.addressbarColor` in Options API or `useQuasar().addressbarColor` in Composition API).

