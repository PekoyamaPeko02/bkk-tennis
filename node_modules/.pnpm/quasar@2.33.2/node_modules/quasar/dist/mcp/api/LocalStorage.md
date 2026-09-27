## LocalStorage API

### Methods

- `hasItem(key: string): boolean`
  Check if storage item exists
  Params:
    - `key` (string, required)
      Entry key
      Examples: `'userId'`
  Returns: `boolean`
    Does the item exists or not?
- `getLength(): number`
  Get storage number of entries
  Returns: `number`
    Number of entries
- `getItem(key: string): number | boolean | Date | RegExp | Function | object | any[] | string`
  Get a storage item value
  Params:
    - `key` (string, required)
      Entry key
      Examples: `'userId'`
  Returns: `number | boolean | Date | RegExp | Function | object | any[] | string`
    Storage item value
    Examples: `'john12'`, `702`
- `getIndex(index: number): number | boolean | Date | RegExp | Function | object | any[] | string`
  Get the storage item value at specific index
  Params:
    - `index` (number, required)
      Entry index
  Returns: `number | boolean | Date | RegExp | Function | object | any[] | string`
    Storage item index
- `getKey(index: number): string`
  Get the storage key at specific index
  Params:
    - `index` (number, required)
      Entry index
  Returns: `string`
    Storage key
    Examples: `'userId'`
- `getAll(): object`
  Retrieve all items in storage
  Returns: `object`
    Object syntax: item name as Object key and its value
    Examples:
      - `{ userId: 'rstoenescu', timesLoggedIn: 14 }`
- `getAllKeys(): any[]`
  Retrieve all keys in storage
  Returns: `any[]`
    Storage keys (Array of Strings)
    Examples:
      - `['userId', 'password']`
- `setItem(key: string, value: number | boolean | Date | RegExp | Function | object | any[] | string): void`
  Set item in storage
  Params:
    - `key` (string, required)
      Entry key
      Examples: `'userId'`
    - `value` (number | boolean | Date | RegExp | Function | object | any[] | string, required)
      Entry value
      Function signature: `(...params: any) => any`
      Examples: `'john12'`
      Params:
        - `...params` (any, optional)
      Returns: `any`
- `removeItem(key: string): void`
  Remove a storage item
  Params:
    - `key` (string, required)
      Storage key
      Examples: `'userId'`
- `clear(): void`
  Remove everything from the storage
- `isEmpty(): boolean`
  Determine if storage has any items
  Returns: `boolean`
    Tells if storage is empty or not

### Vue Injection

Accessible via `$q.localStorage` (e.g., `this.$q.localStorage` in Options API or `useQuasar().localStorage` in Composition API).

