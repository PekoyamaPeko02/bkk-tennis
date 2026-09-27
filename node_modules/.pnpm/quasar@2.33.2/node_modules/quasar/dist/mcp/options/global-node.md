---
title: Global Nodes
---
Some Quasar components and plugins render outside of your app's mount point: they create a node, append it to `<body>` (a menu opened inside a dialog goes into that dialog instead) and render into it. These are the **global nodes**.

- QDialog, QMenu, QTooltip, QPopupProxy and anything else built on them (QSelect, QBtnDropdown, ...) each get a `#q-portal--dialog--N`, `#q-portal--menu--N` or `#q-portal--tooltip--N` node while they are shown
- the [Dialog](../quasar-plugins/dialog.md) plugin gets a `#q-portal--dialog--N` node for each dialog it opens
- the [Notify](../quasar-plugins/notify.md) plugin renders into `#q-notify`
- the [Loading](../quasar-plugins/loading.md) plugin renders into `#q-loading`
- the [LoadingBar](../quasar-plugins/loading-bar.md) plugin renders into `#q-loading-bar`

The portal nodes are created when the component is shown for the first time and removed when the component is unmounted; the plugin nodes stay for the lifetime of the app.

## Custom CSS class

Since the global nodes sit outside your app's root element, CSS scoped to that element does not reach them. You can have Quasar apply a class of your own to every global node it creates:

Example "Quasar CLI":

```js
// quasar.config file

framework: {
  config: {
    globalNodes: {
      class: 'my-app' // one or more classes, space separated
    }
  }
}
```

Example "Vite Plugin / UMD":

```js
// main.js file

app.use(Quasar, {
  config: {
    globalNodes: {
      class: 'my-app' // one or more classes, space separated
    }
  }
})
```

Then target the nodes from your CSS:

```css
.my-app .q-dialog__inner {
  padding: 24px;
}
```

> [!NOTE]
> The class is applied to the node itself (the element with the `q-portal--*`, `q-notify`, `q-loading` or `q-loading-bar` id), not to the component rendered inside of it. Use it as an ancestor selector, as in the example above.
