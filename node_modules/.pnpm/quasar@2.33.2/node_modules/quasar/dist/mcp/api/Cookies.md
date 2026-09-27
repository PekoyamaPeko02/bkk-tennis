## Cookies API

### Methods

- `get(name: string): string`
  Get cookie
  Params:
    - `name` (string, required)
      Cookie name
      Examples: `'userId'`
  Returns: `string`
    Cookie value; Returns null if cookie not found
    Examples: `'john12'`
- `getAll(): object`
  Get all cookies
  Returns: `object`
    Object with cookie names (as keys) and their values
    Examples:
      - `{ userId: 'john12', XFrame: 'x534' }`
- `set(name: string, value: string, options?: object): void`
  Set cookie
  Params:
    - `name` (string, required)
      Cookie name
      Examples: `'userId'`
    - `value` (string, required)
      Cookie value
      Examples: `'john12'`
    - `options` (object, optional)
      Cookie options
      Object shape:
        - `expires` (number | string | Date, optional)
          Cookie expires detail; If specified as Number, then the unit is days; If specified as String, it can either be raw stringified date or in Xd Xh Xm Xs format (see examples)
          Examples:
            - `30`
            - `'Wed, 13 Jan 2021 22:23:01 GMT'`
            - `'1d'`
            - `'15m'`
            - `'13d'`
            - `'1d 15m'`
            - `'1d 3h 5m 3s'`
        - `path` (string, optional)
          Cookie path
          Examples: `'/accounts'`
        - `domain` (string, optional)
          Cookie domain
          Examples: `'.foo.com'`
        - `sameSite` (string, optional)
          SameSite cookie option
          Accepts: `'Lax'`, `'Strict'`, `'None'`
        - `httpOnly` (boolean, optional)
          Is cookie Http Only?
        - `secure` (boolean, optional)
          Is cookie secure? (https only)
        - `other` (string, optional)
          Raw string for other cookie options; To be used as a last resort for possible newer props that are currently not yet implemented in Quasar
          Examples: `'SomeNewCookieProp'`
- `has(name: string): boolean`
  Check if cookie exists
  Params:
    - `name` (string, required)
      Cookie name
      Examples: `'userId'`
  Returns: `boolean`
    Does cookie exists or not?
- `remove(name: string, options?: object): void`
  Remove a cookie
  Params:
    - `name` (string, required)
      Cookie name
      Examples: `'userId'`
    - `options` (object, optional)
      Cookie options
      Object shape:
        - `path` (string, optional)
          Cookie path
          Examples: `'/accounts'`
        - `domain` (string, optional)
          Cookie domain
          Examples: `'.foo.com'`
- `parseSSR(ssrContext: object): object`
  For SSR/SSG usage only, and only on the global import (not on $q.cookies)
  Params:
    - `ssrContext` (object, required)
      SSR Context Object
  Returns: `object`
    Cookie object (like $q.cookies) for SSR/SSG usage purposes

### Vue Injection

Accessible via `$q.cookies` (e.g., `this.$q.cookies` in Options API or `useQuasar().cookies` in Composition API).

