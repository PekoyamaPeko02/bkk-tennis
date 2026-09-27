---
title: Tree
---
Quasar Tree represents a highly configurable component that displays hierarchical data, such as a table of contents in a tree structure.

## QTree API

Not inlined here: call the `get_api` tool with `name: "QTree"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Defining the nodes

The `nodes` prop is an array of plain objects, one per node, nested through their children. QTree reads a fixed set of keys on each; the names of the key, label and children properties are yours to pick through the `node-key`, `label-key` and `children-key` props (`label` and `children` by default).

```js
nodes: [
  // array of Objects
  // node Object definition
  {
    // unique id, under the property named by "node-key" (required)
    id: 'fruits',

    // text of the node, under the property named by "label-key"
    label: 'Fruits',

    // (optional) icon, image or avatar shown before the label
    icon: 'restaurant_menu',
    iconColor: 'primary', // one of the Quasar Color Palette names
    // img: 'mountains.png', // from the /public folder
    // avatar: 'boy-avatar.png', // from the /public folder

    // (optional) the node cannot be selected, ticked, expanded or clicked
    disabled: false,

    // (optional) can the node be expanded? (default: true)
    expandable: true,

    // (optional) can the node be selected? (default: true)
    selectable: true,

    // (optional) called on click, receives the node
    handler: node => {},

    // (optional) with a tick strategy: show a checkbox, and can it be ticked?
    noTick: false,
    tickable: true,
    // (optional) tick strategy for this node only: 'leaf', 'leaf-filtered', 'strict', 'none'
    tickStrategy: 'leaf',

    // (optional) scoped slot names for this node's header and body,
    // without the 'header-' / 'body-' prefix
    header: 'story', // renders through the 'header-story' slot
    body: 'story', // renders through the 'body-story' slot

    // (optional) the sub-nodes, same shape, under the property named by "children-key"
    children: [
      { id: 'apple', label: 'Apple' },
      { id: 'pear', label: 'Pear', disabled: true }
    ]
  },

  {
    id: 'lazy',
    label: 'Loaded on first expand',
    // (optional) load the children on first expand through the "lazy-load" event;
    // do not set 'children' on a lazy node
    lazy: true
  }
]
```

| Node Property | Type | Behavior when not present | Description |
| --- | --- | --- | --- |
| <nodeKey> | String, Number | An error is generated | Node's key. The key is picked from the key specified in `nodeKey` property. |
| label | String | The item has no label | Node's label. When `labelKey` prop is set the label is picked from that key. |
| icon | String | The default icon is used | Node's icon. |
| iconColor | String | The inherited color is used | Node's icon color. One from Quasar Color Palette. |
| img | String | No image is displayed | Node's image. Use /public folder. Example: 'mountains.png' |
| avatar | String | No avatar is displayed | Node's avatar. Use /public folder. Example: 'boy-avatar.png' |
| children | Array | This node has no sub-nodes | Array of nodes as children. |
| disabled | Boolean | The node is enabled | Is node disabled? |
| expandable | Boolean | The node is expandable | Is node expandable? |
| selectable | Boolean | The node is selectable | Is node selectable? |
| handler | Function | No extra function is called | Custom function that should be called on click on node. Receives `node` as parameter. |
| tickable | Boolean | The node is tickable according to tick strategy | When using a tick strategy, each node shows a checkbox. Should a node's checkbox be disabled? |
| noTick | Boolean | Node displays a checkbox | When using a tick strategy, should node display a checkbox? |
| tickStrategy | String | Tick strategy 'none' is used | Override global tick strategy for this node only. One of 'leaf', 'leaf-filtered', 'strict', 'none'. |
| lazy | Boolean | Children are not lazy loaded | Should children be lazy loaded? In this case also don't specify 'children' prop. |
| header | String | Slot 'default-header' is used | Node header scoped slot name, without the required 'header-' prefix. Example: 'story' refers to 'header-story' scoped slot. |
| body | String | Slot 'default-body' is used | Node body scoped slot name, without the required 'body-' prefix. Example: 'story' refers to 'body-story' scoped slot. |

## Usage

### Basic

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree :nodes="simple" node-key="label" />
  </div>
</template>

<script setup>
const simple = [
  // ...
]
</script>
```

### Accessibility *(v2.25+)*

QTree follows the [WAI-ARIA tree pattern](https://www.w3.org/WAI/ARIA/apg/patterns/treeview/): the component exposes `role="tree"` and each node header is a `role="treeitem"` carrying `aria-expanded` on parents, `aria-selected` on selectable nodes, `aria-checked` on ticking ones (including the `mixed` state of partially ticked parents) and `aria-disabled` when disabled, while the nested child groups convey the hierarchy. In `virtual-scroll` mode the rows are rendered flat instead, so each one compensates with `aria-level`, `aria-setsize` and `aria-posinset`. Do give the tree an accessible name by setting `aria-label` (or `aria-labelledby`) on the component itself.

The tick checkboxes are pointer affordances only — the keyboard path is <kbd>Space</kbd> on the node header (see below), with the state announced through `aria-checked`. The "no nodes" and "no results" messages use localized strings from the [Quasar Language Pack](../options/quasar-language-packs.md).

Every node the user can see takes part in the roving Tab stop, as the tree pattern requires of a `role="treeitem"`. That includes the nodes nothing happens on (a leaf of a tree with no selection and no ticking) and the disabled ones: a disabled node stays reachable and announces itself through `aria-disabled`, but nothing acts on it — no selection, no expansion, no lazy loading and not even its own `handler`.

#### Keyboard navigation

When a tree node has focus:

- <kbd>Arrow Up</kbd> and <kbd>Arrow Down</kbd> move focus through the visible nodes.
- <kbd>Arrow Right</kbd> expands a collapsed parent or moves focus to its first visible child.
- <kbd>Arrow Left</kbd> collapses an expanded parent or moves focus to its parent.
- <kbd>Home</kbd> and <kbd>End</kbd> move focus to the first and last visible nodes.
- <kbd>Enter</kbd> performs the node's default action; <kbd>Space</kbd> toggles its expansion — or its checkbox, on tickable nodes (when using a `tick-strategy`). Both do nothing on a disabled node.

### No connector lines

Example "No connectors":

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree
      :nodes="simple"
      node-key="label"
      no-connectors
      v-model:expanded="expanded"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const expanded = ref([
  'Satisfied customers (with avatar)',
  'Good food (with icon)'
])

const simple = [
  // ...
]
</script>
```

### Dense

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree
      :nodes="simple"
      dense
      node-key="label"
      v-model:expanded="expanded"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const expanded = ref([
  'Satisfied customers (with avatar)',
  'Good food (with icon)'
])

const simple = [
  // ...
]
</script>
```

### Force dark mode

```vue
<template>
  <div class="bg-grey-9 text-white">
    <q-tree :nodes="simple" node-key="label" v-model:expanded="expanded" dark />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const expanded = ref([
  'Satisfied customers (with avatar)',
  'Good food (with icon)'
])

const simple = [
  // ...
]
</script>
```

### Perf considerations *(v2.25+)*

Starting with Quasar v2.25, QTree only pays for what is on screen: a collapsed node's children are not rendered until the node gets expanded for the first time (afterwards they are kept in the DOM — hidden — so that collapsing/expanding can still animate), and a state change (expanding, ticking, selecting, filtering, keyboard navigation) re-renders only the affected nodes. Rendering cost thus scales with the number of *visible* nodes, not with the total tree size — most trees need no tuning at all. If your code queried the DOM for the children of never-expanded nodes, it needs to expand those nodes first.

When a lot of nodes are visible at the same time, the sheer amount of DOM becomes the bottleneck. There are two remedies, in increasing order of effect:

1. The `no-transition` Boolean prop turns off the expand/collapse animation, which also allows QTree to drop collapsed subtrees from the DOM instead of keeping them alive for animating (on older Quasar versions it is the only way to avoid rendering collapsed content altogether). Recommended when using relatively large data.

```html
<q-tree no-transition ...
```

2. The `virtual-scroll` Boolean prop (see the Virtual scroll section below) keeps only the rows around the scrolling viewport in the DOM. This is the mode for really big trees: mounting, expanding all nodes and filtering stay at a constant cost no matter how much of the tree is expanded.

### Virtual scroll *(v2.25+)*

The `virtual-scroll` Boolean prop renders the visible nodes as a flat virtualized list: only the rows around the scrolling viewport (plus a configurable buffer — see the `virtual-scroll-*` props) exist in the DOM, so rendering cost stays constant regardless of how many nodes are expanded. The example below runs a fully expanded tree of 4,680 nodes; even the largest trees mount, expand-all and filter in milliseconds in this mode.

Things to be aware of in this mode:

- The tree itself becomes the scrolling container, so give it a height (through CSS) — or point `virtual-scroll-target` to a scrolling ancestor instead.
- Expanding and collapsing are instant: there is no slide transition, so the `duration` and `no-transition` props and the `@after-show`/`@after-hide` events do not apply.
- The `scrollTo` method scrolls any visible node's row into view; keyboard navigation does this automatically.

Example "Virtual scroll":

```vue
<template>
  <q-input v-model="filter" filled dense label="Filter" class="q-mb-md" />

  <q-tree
    :nodes="nodes"
    node-key="id"
    tick-strategy="leaf"
    default-expand-all
    virtual-scroll
    :filter="filter"
    style="height: 300px"
  />
</template>

<script setup>
import { ref } from 'vue'

const filter = ref('')

function buildNode(label, level) {
  const node = { id: label, label: `Node ${label}` }

  if (level < 4) {
    node.children = []
    for (let i = 1; i <= 8; i++) {
      node.children.push(buildNode(`${label}.${i}`, level + 1))
    }
  }

  return node
}

// 4,680 nodes
const nodes = []
for (let i = 1; i <= 8; i++) {
  nodes.push(buildNode(`${i}`, 1))
}
</script>
```

### Integrated example

Example "With QSplitter and QTabPanels":

```vue
<template>
  <q-splitter v-model="splitterModel" style="height: 400px">
    <template #before>
      <div class="q-pa-md">
        <q-tree
          :nodes="simple"
          node-key="label"
          selected-color="primary"
          v-model:selected="selected"
          default-expand-all
        />
      </div>
    </template>

    <template #after>
      <q-tab-panels
        v-model="selected"
        animated
        transition-prev="jump-up"
        transition-next="jump-up"
      >
        <q-tab-panel name="Relax Hotel">
          <div class="text-h4 q-mb-md">Welcome</div>
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
        </q-tab-panel>

        <q-tab-panel name="Food">
          <div class="text-h4 q-mb-md">Food</div>
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
        </q-tab-panel>

        <q-tab-panel name="Room service">
          <div class="text-h4 q-mb-md">Room service</div>
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
        </q-tab-panel>

        <q-tab-panel name="Room view">
          <div class="text-h4 q-mb-md">Room view</div>
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
          <p
            >Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis
            praesentium cumque magnam odio iure quidem, quod illum numquam
            possimus obcaecati commodi minima assumenda consectetur culpa fuga
            nulla ullam. In, libero.</p
          >
        </q-tab-panel>
      </q-tab-panels>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue'

const splitterModel = ref(50)
const selected = ref('Food')
const simple = [
  // ...
]
</script>
```

More info: [QSplitter](splitter.md), [QTabPanels](tab-panels.md).

### Customize content

Notice (in the example below) the default header and body slot customization.

Example "Default header and body slots":

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree :nodes="customize" node-key="label" default-expand-all>
      <template #default-header="prop">
        <div class="row items-center">
          <q-icon
            :name="prop.node.icon || 'share'"
            color="orange"
            class="q-mr-sm"
          />
          <div class="text-weight-bold text-primary">{{ prop.node.label }}</div>
        </div>
      </template>

      <template #default-body="prop">
        <div v-if="prop.node.story">
          <span class="text-weight-bold">This node has a story</span>:
          {{ prop.node.story }}
        </div>
        <span v-else class="text-weight-light text-black"
          >This is some default content.</span
        >
      </template>
    </q-tree>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const customize = ref([
  {
    label: 'Satisfied customers',
    header: 'root',
    children: [
      {
        label: 'Good food',
        icon: 'restaurant_menu',
        header: 'generic',
        children: [
          {
            label: 'Quality ingredients',
            header: 'generic',
            body: 'story',
            story: 'Lorem ipsum dolor sit amet.'
          },
          {
            label: 'Good recipe',
            body: 'story',
            story:
              'A Congressman works with his equally conniving wife to exact revenge on the people who betrayed him.'
          }
        ]
      },
      {
        label: 'Good service',
        header: 'generic',
        body: 'toggle',
        caption:
          'Why are we as consumers so captivated by stories of great customer service? Perhaps it is because...',
        enabled: false,
        children: [
          { label: 'Prompt attention' },
          { label: 'Professional waiter' }
        ]
      },
      {
        label: 'Pleasant surroundings',
        children: [
          { label: 'Happy atmosphere' },
          { label: 'Good table presentation', header: 'generic' },
          { label: 'Pleasing decor' }
        ]
      }
    ]
  }
])
</script>
```

Notice (in the example below) the custom header and body slots.

Example "Customizing nodes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree :nodes="customize" node-key="label" default-expand-all>
      <template #header-root="prop">
        <div class="row items-center">
          <img
            alt="Quasar logo"
            src="https://cdn.quasar.dev/logo-v2/svg/logo.svg"
            class="q-mr-sm"
            style="width: 50px; height: 50px"
          />
          <div>
            {{ prop.node.label }}
            <q-badge color="orange" class="q-ml-sm">New!</q-badge>
          </div>
        </div>
      </template>

      <template #header-generic="prop">
        <div class="row items-center">
          <q-icon
            :name="prop.node.icon || 'star'"
            color="orange"
            size="28px"
            class="q-mr-sm"
          />
          <div class="text-weight-bold text-primary">{{ prop.node.label }}</div>
        </div>
      </template>

      <template #body-story="prop">
        <span class="text-weight-thin">The story is:</span>
        {{ prop.node.story }}
      </template>

      <template #body-toggle="prop">
        <p class="text-caption">{{ prop.node.caption }}</p>
        <q-toggle
          v-model="prop.node.enabled"
          label="I agree to the terms and conditions"
        />
      </template>
    </q-tree>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const customize = ref([
  // ...
])
</script>
```

> [!NOTE]
> Clicking or pressing `ENTER` on the custom header selects the tree item (and the custom header is blurred). Pressing `SPACE` toggles its expansion.
>
> If you don't want this to happen just wrap the content of the custom header in a `<div @click.stop @keydown.stop>` (or add the listeners to the respective component/element that is emitting them).

### Accordion, filtering and selectable

In the example below, sibling nodes get contracted when one gets expanded.

Example "Accordion mode":

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree
      :nodes="simple"
      accordion
      node-key="label"
      v-model:expanded="expanded"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const expanded = ref([
  'Satisfied customers (with avatar)',
  'Good food (with icon)'
])

const simple = [
  // ...
]
</script>
```

Example "Filtering nodes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-input ref="filterRef" filled v-model="filter" label="Filter">
      <template #append>
        <q-icon
          v-if="filter !== ''"
          name="clear"
          class="cursor-pointer"
          @click="resetFilter"
        />
      </template>
    </q-input>

    <q-tree
      :nodes="simple"
      node-key="label"
      :filter="filter"
      default-expand-all
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const filter = ref('')
const filterRef = useTemplateRef('filterRef')

const simple = [
  // ...
]

function resetFilter() {
  filter.value = ''
  filterRef.value.focus()
}
</script>
```

Example "Selectable nodes":

```vue
<template>
  <div class="q-gutter-sm">
    <div>
      <div class="q-gutter-sm">
        <q-btn
          size="sm"
          color="primary"
          @click="selectGoodService"
          label="Select 'Good service'"
        />
        <q-btn
          v-if="selected"
          size="sm"
          color="red"
          @click="unselectNode"
          label="Unselect node"
        />
      </div>
    </div>
    <q-tree
      :nodes="props"
      default-expand-all
      v-model:selected="selected"
      node-key="label"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const selected = ref(null)

function selectGoodService() {
  if (selected.value !== 'Good service') {
    selected.value = 'Good service'
  }
}

function unselectNode() {
  selected.value = null
}

const props = [
  // ...
]
</script>
```

### Lazy loading

Example "Lazy loading nodes":

```vue
<template>
  <div class="q-gutter-sm">
    <q-tree
      :nodes="lazy"
      default-expand-all
      node-key="label"
      @lazy-load="onLazyLoad"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const nodes = [
  {
    label: 'Node 1',
    children: [
      { label: 'Node 1.1', lazy: true },
      { label: 'Node 1.2', lazy: true }
    ]
  },
  {
    label: 'Node 2',
    lazy: true
  },
  {
    label: 'Lazy load empty',
    lazy: true
  },
  {
    label: 'Node is not expandable',
    expandable: false,
    children: [{ label: 'Some node' }]
  }
]

const lazy = ref(nodes)

function onLazyLoad({ node, key, done, fail }) {
  // call fail() if any error occurs

  setTimeout(() => {
    // simulate loading and setting an empty node
    if (key.includes('Lazy load empty')) {
      done([])
      return
    }

    const label = node.label
    done([
      { label: `${label}.1` },
      { label: `${label}.2`, lazy: true },
      {
        label: `${label}.3`,
        children: [
          { label: `${label}.3.1`, lazy: true },
          { label: `${label}.3.2`, lazy: true }
        ]
      }
    ])
  }, 1000)
}
</script>
```

### Selection vs ticking, expansion

- Selection (through QTree `selected` prop) refers to the currently selected node. By default only its label color changes (see the `selected-color` prop); the node header also gets the `q-tree__node--selected` CSS class, so you can style it further (a background, for example) with your own CSS.
- Ticking (through QTree `ticked` prop) refers to the checkbox associated with each node.
- Expansion (through QTree `expanded` prop) refers to the nodes that are expanded.

All properties above require to be dynamically bound using `v-model:<prop_name>` directive in order for them to work correctly (example: `v-model:expanded`).

Example "Syncing node properties":

```vue
<template>
  <div class="row q-col-gutter-sm">
    <q-tree
      class="col-12 col-sm-6"
      :nodes="simple"
      node-key="label"
      tick-strategy="leaf"
      v-model:selected="selected"
      v-model:ticked="ticked"
      v-model:expanded="expanded"
    />
    <div class="col-12 col-sm-6 q-gutter-sm">
      <div class="text-h6">Selected</div>
      <div>{{ selected }}</div>

      <q-separator spaced />

      <div class="text-h6">Ticked</div>
      <div>
        <div v-for="tick in ticked" :key="`ticked-${tick}`">
          {{ tick }}
        </div>
      </div>

      <q-separator spaced />

      <div class="text-h6">Expanded</div>
      <div>
        <div v-for="expand in expanded" :key="`expanded-${expand}`">
          {{ expand }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const selected = ref('Pleasant surroundings')
const ticked = ref(['Quality ingredients', 'Good table presentation'])

const expanded = ref([
  'Satisfied customers (with avatar)',
  'Good food (with icon)'
])

const simple = [
  // ...
]
</script>
```

### Tick strategy

There are three ticking strategy: 'leaf', 'leaf-filtered', 'strict' with an additional (and default) 'none' which disables ticking.

| Strategy | Description |
| --- | --- |
| leaf | Ticked nodes are only the leaves. Ticking a node influences the parent's ticked state too (parent becomes partially ticked or ticked), as well as its children (all tickable children become ticked). |
| leaf-filtered | Same concept as `leaf`, only that this strategy applies only to filtered nodes (the nodes that remain visible after filtering). |
| strict | Ticked nodes are independent of parent or children tick state. |

You can apply a global tick strategy for a QTree and locally change the ticking strategy for a certain node by specifying the `tickStrategy` in the `nodes` model.

```vue
<template>
  <div class="row q-col-gutter-sm">
    <q-tree
      class="col-12 col-sm-6"
      :nodes="simple"
      v-model:ticked="ticked"
      v-model:expanded="expanded"
      node-key="label"
      :tick-strategy="tickStrategy"
      default-expand-all
    />
    <div class="col-12 col-sm-6">
      <q-option-group v-model="tickStrategy" :options="tickStrategies" />

      <div class="text-h6 q-mt-md">Ticked</div>
      <div>
        <div v-for="tick in ticked" :key="`ticked-${tick}`">
          {{ tick }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const ticked = ref(['Pleasant surroundings'])
const expanded = ref(['Good service (disabled node)'])
const tickStrategy = ref('strict')
const tickStrategies = [
  { value: 'none', label: 'None' },
  { value: 'strict', label: 'Strict' },
  { value: 'leaf', label: 'Leaf' },
  { value: 'leaf-filtered', label: 'Leaf Filtered' }
]

const simple = [
  // ...
]
</script>
```

### Partially ticked nodes *(v2.25+)*

With the `leaf` and `leaf-filtered` strategies, a parent whose tickable children are only partly ticked is neither ticked nor unticked, so it does not show up in the `ticked` model. Use the `getIndeterminateNodes()` method to get such nodes (in the order of the `nodes` model), or `isIndeterminate(key)` to check one of them.

Both always report nothing for the `strict` strategy, where a node's tick state is independent of its children.

When you need all three states at once, `getTickState(key)` returns them in the tri-state form that a QCheckbox takes as its model: `true` when ticked, `null` when partially ticked and `false` when unticked. It is the very value that the node's own tickbox gets, so it can be bound to a QCheckbox of yours as is.

The header and body slots also get an `indeterminate` boolean in their scope, next to the `ticked` one. It is read-only — a node becomes partially ticked through its children, so tick those instead.

Example "Partially ticked nodes":

```vue
<template>
  <div class="row q-col-gutter-sm">
    <q-tree
      ref="treeRef"
      class="col-12 col-sm-6"
      :nodes="simple"
      v-model:ticked="ticked"
      node-key="label"
      tick-strategy="leaf"
      default-expand-all
    >
      <template #default-header="prop">
        {{ prop.node.label }}
        <q-badge
          v-if="prop.indeterminate"
          class="q-ml-sm"
          color="orange"
          label="partial"
        />
      </template>
    </q-tree>

    <div class="col-12 col-sm-6">
      <div class="text-h6">Ticked</div>
      <div v-for="label in ticked" :key="`ticked-${label}`">
        {{ label }}
      </div>

      <div class="text-h6 q-mt-md">Partially ticked</div>
      <div v-for="node in indeterminate" :key="`indeterminate-${node.label}`">
        {{ node.label }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, useTemplateRef, watch } from 'vue'

const treeRef = useTemplateRef('treeRef')
const ticked = ref(['Good recipe'])
const indeterminate = ref([])

function refreshIndeterminate() {
  indeterminate.value = treeRef.value.getIndeterminateNodes()
}

onMounted(refreshIndeterminate)

// 'post' so that the QTree has already taken the new "ticked" value in
watch(ticked, refreshIndeterminate, { flush: 'post' })

const simple = [
  // ...
]
</script>
```

### Custom filter method

You can customize the filtering method by specifying the `filter-method` prop. The method below filters by input if it also has '(*)':

Example "Custom filter":

```vue
<template>
  <div class="q-gutter-sm">
    <q-input
      ref="filterRef"
      filled
      v-model="filter"
      label="Search - only filters labels that have also '(*)'"
    >
      <template #append>
        <q-icon
          v-if="filter !== ''"
          name="clear"
          class="cursor-pointer"
          @click="resetFilter"
        />
      </template>
    </q-input>

    <q-tree
      :nodes="simple"
      node-key="label"
      :filter="filter"
      :filter-method="myFilterMethod"
      v-model:expanded="expanded"
      default-expand-all
    />
  </div>
</template>

<script setup>
import { ref, useTemplateRef } from 'vue'

const filter = ref('de')
const filterRef = useTemplateRef('filterRef')
const expanded = ref(['Good service (disabled node) (*)'])
const simple = [
  // ...
]

function myFilterMethod(node, filterStr) {
  const filt = filterStr.toLowerCase()
  return (
    node.label &&
    node.label.toLowerCase().includes(filt) &&
    node.label.toLowerCase().includes('(*)')
  )
}

function resetFilter() {
  filter.value = ''
  filterRef.value.focus()
}
</script>
```
