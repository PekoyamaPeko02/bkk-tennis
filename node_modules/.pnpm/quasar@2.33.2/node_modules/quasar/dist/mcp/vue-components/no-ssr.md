---
title: No SSR
related:
  - title: What is SSR
    path: 'https://quasar.dev/quasar-cli-vite/developing-ssr/introduction'
  - title: useHydration composable
    path: ../vue-composables/use-hydration.md
---
The QNoSsr component makes sense only if you are creating a SSR website/app.

It avoids rendering its content on the server and leaves that for client only. Useful when you got code that is not isomorphic and can only run on the client side, in a browser.

Alternatively, you can also use it to render content only on server-side and it automatically removes it if it ends up running on a client browser.

## QNoSsr API

Not inlined here: call the `get_api` tool with `name: "QNoSsr"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

### Basic

```html
<q-no-ssr>
  <div>This won't be rendered on server</div>
</q-no-ssr>
```

### Multiple client nodes

```html
<q-no-ssr>
  <div>This won't be rendered on server.</div>
  <div>This won't either.</div>
</q-no-ssr>
```

### Multiple client nodes with tag prop

```html
<q-no-ssr tag="blockquote">
  <div>This won't be rendered on server.</div>
  <div>This won't either.</div>
</q-no-ssr>
```

### Placeholder property

```html
<q-no-ssr placeholder="Rendered on server">
  <div>This won't be rendered on server</div>
</q-no-ssr>
```

### Placeholder slot

```html
<q-no-ssr>
  <div>This won't be rendered on server</div>
  <template #placeholder>
    <div>Rendered on server</div>
  </template>
</q-no-ssr>
```

### Multiple content in placeholder slot

```html
<q-no-ssr>
  <div>This won't be rendered on server</div>
  <template #placeholder>
    <div>Rendered on server (1/2)</div>
    <div>Rendered on server (2/2)</div>
  </template>
</q-no-ssr>
```

### Only placeholder slot

```html
<q-no-ssr>
  <template #placeholder>
    <div>Rendered on server</div>
  </template>
</q-no-ssr>
```

## Accessibility *(v2.25+)*

QNoSsr is a passthrough wrapper rendering only your own content (or placeholder), so it has no accessibility surface of its own.
