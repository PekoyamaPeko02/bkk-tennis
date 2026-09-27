---
title: Button
related:
  - title: Button Group
    path: button-group.md
  - title: Dropdown Button
    path: button-dropdown.md
  - title: Button Toggle
    path: button-toggle.md
---
Quasar has a component called QBtn which is a button with a few extra useful features. For instance, it comes in two shapes: rectangle (default) and round. It also has the material ripple effect baked in (which can be disabled).

The button component also comes with a spinner or loading effect. You would use this for times when app execution may cause a delay and you want to give the user some feedback about that delay. When used, the button will display a spinning animation as soon as the user clicks the button.

When not disabled or spinning, QBtn emits a `@click` event, as soon as it is clicked or tapped.

## QBtn API

Not inlined here: call the `get_api` tool with `name: "QBtn"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

### Standard

Example "Standard buttons":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="white" text-color="black" label="Standard" />
    <q-btn color="primary" label="Primary" />
    <q-btn color="secondary" label="Secondary" />
    <q-btn color="amber" glossy label="Amber" />
    <q-btn color="brown-5" label="Brown 5" />
    <q-btn color="deep-orange" glossy label="Deep Orange" />
    <q-btn color="purple" label="Purple" />
    <q-btn color="black" label="Black" />
  </div>
</template>
```

### Custom colors

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn style="background: #ff0080; color: white" label="Fuchsia" />
    <q-btn flat style="color: #ff0080" label="Fuchsia Flat" />
    <q-btn style="background: goldenrod; color: white" label="Goldenrod" />
    <q-btn outline style="color: goldenrod" label="Goldenrod" />
    <q-btn
      color="grey-4"
      text-color="purple"
      glossy
      unelevated
      icon="camera_enhance"
      label="Purple text"
    />
  </div>
</template>
```

### With icon

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="primary" icon="mail" label="On Left" />
    <q-btn color="secondary" icon-right="mail" label="On Right" />
    <q-btn
      color="red"
      icon="mail"
      icon-right="send"
      label="On Left and Right"
    />
    <br />
    <q-btn icon="phone" label="Stacked" stack glossy color="purple" />
  </div>
</template>
```

### Round

Example "Round buttons":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn round color="primary" icon="shopping_cart" />
    <q-btn round color="secondary" icon="navigation" />
    <q-btn round color="amber" glossy text-color="black" icon="layers_clear" />
    <q-btn round color="brown-5" icon="directions" />
    <q-btn round color="deep-orange" icon="edit_location" />
    <q-btn round color="purple" glossy icon="local_grocery_store" />
    <q-btn round color="black" icon="my_location" />
  </div>
</template>
```

### Square

Example "Square buttons":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn square color="primary" icon="shopping_cart" />
    <q-btn square color="secondary" icon="navigation" />
    <q-btn square color="amber" glossy text-color="black" icon="layers_clear" />
    <q-btn square color="brown-5" icon="directions" />
    <q-btn square color="deep-orange" icon="edit_location" />
    <q-btn square color="purple" glossy icon="local_grocery_store" />
    <q-btn square color="black" icon="my_location" />
  </div>
</template>
```

### Custom content

```vue
<template>
  <div class="q-gutter-md">
    <q-btn color="teal">
      <q-icon left size="3em" name="map" />
      <div>Label</div>
    </q-btn>

    <q-btn round>
      <q-avatar size="42px">
        <img alt="John Doe" src="https://cdn.quasar.dev/img/avatar2.jpg" />
      </q-avatar>
    </q-btn>

    <q-btn color="indigo" no-caps> Multiline<br />Button </q-btn>

    <q-btn color="deep-orange" push>
      <div class="row items-center no-wrap">
        <q-icon left name="map" />
        <div class="text-center"> Custom<br />Content </div>
      </div>
    </q-btn>
  </div>
</template>
```

Example "Truncate label":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="primary" style="width: 200px">
      <div class="ellipsis">
        This is some very long text that is expected to be truncated
      </div>
    </q-btn>
  </div>
</template>
```

### Design

Example "Button design":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn flat color="primary" label="Flat" />
    <q-btn flat rounded color="primary" label="Flat Rounded" />
    <q-btn flat round color="primary" icon="card_giftcard" />
    <br />
    <q-btn outline color="primary" label="Outline" />
    <q-btn outline rounded color="primary" label="Outline Rounded" />
    <q-btn outline round color="primary" icon="card_giftcard" />
    <br />
    <q-btn push color="primary" label="Push" />
    <q-btn push color="primary" round icon="card_giftcard" />
    <q-btn push color="white" text-color="primary" label="Push" />
    <q-btn push color="white" text-color="primary" round icon="card_giftcard" />
    <br />
    <q-btn unelevated color="primary" label="Unelevated" />
    <q-btn unelevated rounded color="primary" label="Unelevated Rounded" />
    <q-btn unelevated round color="primary" icon="card_giftcard" />
    <br />
    <q-btn no-caps color="primary" label="No caps" />
    <br />
    <q-btn class="glossy" color="teal" label="Glossy" />
    <q-btn class="glossy" rounded color="deep-orange" label="Glossy Rounded" />
    <q-btn class="glossy" round color="primary" icon="card_giftcard" />
    <q-btn class="glossy" round color="secondary" icon="local_florist" />
    <q-btn class="glossy" round color="deep-orange" icon="local_activity" />
  </div>
</template>
```

### Alignment

Example "Button alignment":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      align="left"
      class="btn-fixed-width"
      color="primary"
      label="Align to left"
    />
    <q-btn
      align="right"
      class="btn-fixed-width"
      color="secondary"
      label="Align to right"
    />
    <q-btn
      align="between"
      class="btn-fixed-width"
      color="accent"
      label="Align between"
      icon="flight_takeoff"
    />
    <q-btn
      align="around"
      class="btn-fixed-width"
      color="brown-5"
      label="Align around"
      icon="lightbulb_outline"
    />
  </div>
</template>

<style lang="sass" scoped>
.btn-fixed-width
  width: 200px
</style>
```

### Size

Example "Button size":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      v-for="size in sizes"
      :key="`btn_size_sq_${size}`"
      color="primary"
      :size="size"
      :label="`Size ${size}`"
    />
    <br />
    <q-btn
      v-for="size in sizes"
      :key="`btn_size_rd_${size}`"
      rounded
      color="primary"
      :size="size"
      :label="`Size ${size}`"
    />
    <br />
    <q-btn
      v-for="(size, i) in sizes"
      :key="`btn_size_round_${size}`"
      round
      color="primary"
      :size="size"
      :icon="icons[i]"
    />

    <br />

    <q-btn
      v-for="size in sizes"
      :key="`btn_size_dense_sq_${size}`"
      dense
      color="primary"
      :size="size"
      :label="`Size ${size}`"
    />
    <br />
    <q-btn
      v-for="size in sizes"
      :key="`btn_size_dense_rd_${size}`"
      rounded
      dense
      color="primary"
      :size="size"
      :label="`Size ${size}`"
    />
    <br />
    <q-btn
      v-for="(size, i) in sizes"
      :key="`btn_size_dense_round_${size}`"
      round
      dense
      color="primary"
      :size="size"
      :icon="icons[i]"
    />

    <br />

    <q-btn size="10px" color="black" label="Text height: 10px" />
    <q-btn size="22px" class="q-px-xl q-py-xs" color="purple" label="Custom" />
    <q-btn size="35px" round color="teal" icon="map" />
  </div>
</template>

<script setup>
const sizes = ['xs', 'sm', 'md', 'lg', 'xl']
const icons = [
  'navigation',
  'add_a_photo',
  'camera',
  'camera_front',
  'my_location'
]
</script>
```

### Padding

The default padding is "xs md". However, you can use `padding` prop to customize it:

Example "Button padding":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn padding="none" color="primary" icon="eco" />

    <q-btn padding="xs" color="primary" icon="eco" />

    <q-btn padding="lg" color="primary" icon="eco" />

    <q-btn padding="10px 5px" color="primary" icon="eco" />

    <q-btn padding="xs lg" color="primary" icon="eco" />

    <q-btn padding="xl" color="primary" round icon="eco" />

    <q-btn padding="xs lg" color="primary" round icon="eco" />

    <q-btn padding="lg xs" color="primary" round icon="eco" />
  </div>
</template>

<script setup>
const sizes = ['xs', 'sm', 'md', 'lg', 'xl']
const icons = [
  'navigation',
  'add_a_photo',
  'camera',
  'camera_front',
  'my_location'
]
</script>
```

### Progress related

Some button actions involve contacting a server, so an asynchronous response. It’s best that you inform the user about a background process taking place until the asynchronous response is ready. QBtn offers this possibility through the `loading` prop. This property will display a QSpinner (by default) instead of the icon and/or label of the button. Custom loading content can also be used (not only text or spinners).

Example "Indeterminate progress":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      :loading="loading[0]"
      color="secondary"
      @click="simulateProgress(0)"
      label="Button"
    />
    <q-btn :loading="loading[1]" color="red" @click="simulateProgress(1)">
      Button
      <template #loading> Loading... </template>
    </q-btn>
    <q-btn :loading="loading[2]" color="purple" @click="simulateProgress(2)">
      Button
      <template #loading>
        <q-spinner-radio />
      </template>
    </q-btn>
    <q-btn
      :loading="loading[3]"
      color="primary"
      @click="simulateProgress(3)"
      style="width: 150px"
    >
      Button
      <template #loading>
        <q-spinner-hourglass class="on-left" />
        Loading...
      </template>
    </q-btn>
    <br />
    <q-btn
      round
      :loading="loading[4]"
      color="brown"
      @click="simulateProgress(4)"
      icon="camera_front"
    >
      <template #loading>
        <q-spinner-facebook />
      </template>
    </q-btn>
    <q-btn
      round
      :loading="loading[5]"
      color="black"
      @click="simulateProgress(5)"
      icon="camera_rear"
    >
      <template #loading>
        <q-spinner-gears />
      </template>
    </q-btn>
    <br />

    <q-btn :loading="progress" color="primary" @click="progress = true">
      Controlled from outside
      <template #loading>
        <q-spinner-radio class="on-left" />
        Click "Stop" Button
      </template>
    </q-btn>
    <q-btn
      :disable="!progress"
      color="negative"
      @click="progress = false"
      label="Stop"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'

const loading = ref([false, false, false, false, false, false])

const progress = ref(false)

function simulateProgress(number) {
  // we set loading state
  loading.value[number] = true

  // simulate a delay
  setTimeout(() => {
    // we're done, we reset loading state
    loading.value[number] = false
  }, 3000)
}
</script>
```

Should you wish, you can also display a deterministic progress within the button by using the additional `percentage` property along with what you’ve already learned about buttons with progress:

Example "Deterministic progress":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      :loading="progress[0].loading"
      :percentage="progress[0].percentage"
      color="primary"
      @click="startComputing(0)"
      style="width: 150px"
    >
      Compute PI
      <template #loading>
        <q-spinner-gears class="on-left" />
        Computing...
      </template>
    </q-btn>

    <q-btn
      :loading="progress[1].loading"
      :percentage="progress[1].percentage"
      round
      color="secondary"
      @click="startComputing(1)"
      icon="cloud_upload"
    />

    <q-btn
      :loading="progress[2].loading"
      :percentage="progress[2].percentage"
      dark-percentage
      unelevated
      color="orange"
      text-color="grey-9"
      @click="startComputing(2)"
      icon="cloud_upload"
      style="width: 100px"
    />
  </div>
</template>

<script setup>
import { onBeforeUnmount, ref } from 'vue'

const progress = ref([
  { loading: false, percentage: 0 },
  { loading: false, percentage: 0 },
  { loading: false, percentage: 0 }
])

const intervals = [null, null, null]

function startComputing(id) {
  progress.value[id].loading = true
  progress.value[id].percentage = 0

  intervals[id] = setInterval(() => {
    progress.value[id].percentage += Math.floor(Math.random() * 8 + 10)
    if (progress.value[id].percentage >= 100) {
      clearInterval(intervals[id])
      progress.value[id].loading = false
    }
  }, 700)
}

onBeforeUnmount(() => {
  intervals.forEach(val => {
    clearInterval(val)
  })
})
</script>
```

### Custom ripple

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn :ripple="false" color="secondary" label="No ripple" no-caps />
    <q-btn
      :ripple="{ color: 'yellow' }"
      color="secondary"
      label="Yellow ripple"
      no-caps
    />
    <q-btn
      :ripple="{ center: true }"
      color="secondary"
      label="Center ripple"
      no-caps
    />
  </div>
</template>
```

### Connecting to Vue Router

> [!IMPORTANT]
> **UMD usage**
>
> - If you will be using `to` & `replace` props, make sure that you also inject Vue Router in your project (Quasar CLI projects have this out of the box). Otherwise use the alternative `href` prop.
> - Due to the above, some of the QBtn below won't work in Codepen/jsFiddle too.

> [!TIP]
> Prefer the Vue Router props over `href` when you can, because with `href` you will trigger a window navigation instead of an in-page Vue Router navigation.

Example "Links":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      to="/start/pick-quasar-flavour"
      label="To Docs index"
      outline
      color="purple"
    />
    <q-btn
      to="/start/pick-quasar-flavour"
      label="To Docs index in 2s"
      @click="linkClick"
      glossy
      color="purple"
    />

    <q-btn
      href="start/pick-quasar-flavour"
      label="With href"
      push
      color="purple"
    />
    <q-btn
      href="start/pick-quasar-flavour"
      target="_blank"
      label="With href - open in new window"
      color="purple"
    />
  </div>
</template>

<script setup>
function linkClick(e, go) {
  e.preventDefault() // we choose when we navigate

  console.log('triggering navigation in 2s')
  setTimeout(() => {
    console.log('navigating as promised 2s ago')
    go()
  }, 2000)
}
</script>
```

You can also delay, cancel or redirect navigation, as seen below. For a more in-depth description of the `@click` event being used below, please refer to QBtn API card at the top of the page.

Example "Links with delayed, cancelled or redirected navigation":

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn
      to="/"
      label="Delayed navigation"
      @click="onDelayedClick"
      outline
      color="purple"
      no-caps
    />
    <q-btn
      to="/"
      label="Cancelled navigation"
      @click="onCancelledClick"
      glossy
      color="purple"
      no-caps
    />
    <q-btn
      to="/"
      label="Redirected navigation"
      @click="onRedirectedClick"
      glossy
      color="purple"
      no-caps
    />
  </div>
</template>

<script setup>
function onDelayedClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate

  console.log('triggering navigation in 2s')
  setTimeout(() => {
    console.log('navigating as promised 2s ago')
    go()
  }, 2000)
}

function onCancelledClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate
  // then we never call go()
}

function onRedirectedClick(e, go) {
  e.preventDefault() // mandatory; we choose when we navigate

  // call this at your convenience
  go({
    to: '/start/pick-quasar-flavour' // we pick another route
    // replace: boolean; default is what the tab is configured with
    // returnRouterError: boolean
  })
    .then(_vueRouterResult => {
      /* ... */
    })
    .catch(_vueRouterError => {
      /* ...will not reach here unless returnRouterError === true */
    })
}
</script>
```

For more convoluted use-cases, you can also directly use the native Vue `<router-link>` component to wrap a QBtn. This also gives the opportunity to control the state according to app's current route:

Example "Scoped slot of RouterLink":

```vue
<template>
  <div class="column q-gutter-sm">
    <router-link :to="{ hash: '#Handling-links' }" custom #default="props">
      <q-btn v-bind="buttonProps(props)" />
    </router-link>

    <router-link
      :to="{ hash: '#Handling-links', query: { search: '1' } }"
      custom
      #default="props"
    >
      <q-btn v-bind="buttonProps(props)" />
    </router-link>

    <router-link
      :to="{ hash: '#Handling-links', query: { search: '1', test: '1' } }"
      custom
      #default="props"
    >
      <q-btn v-bind="buttonProps(props)" />
    </router-link>

    <router-link
      :to="{ hash: '#Handling-links', query: { search: '1', test: '2' } }"
      custom
      #default="props"
    >
      <q-btn v-bind="buttonProps(props)" />
    </router-link>

    <router-link
      :to="{ hash: '#Handling-links', query: { search: '1', test: '1' } }"
      custom
      #default="props"
    >
      <q-btn
        v-bind="buttonProps(props)"
        icon-right="timer_3"
        @click="linkClick"
      />
    </router-link>
  </div>
</template>

<script setup>
function linkClick(e, go) {
  e.preventDefault() // we choose when we navigate

  console.log('triggering navigation in 3s')
  setTimeout(() => {
    console.log('navigating as promised 3s ago')
    go()
  }, 3000)
}

function buttonProps({ href, route, isActive, isExactActive }) {
  const props = {
    color: 'black',
    noCaps: true,
    label: `To "${route.fullPath}"`,
    outline: true,
    to: href
  }

  if (isActive) {
    props.color = isExactActive ? 'primary' : 'amber-9'
  } else {
    props.color = 'black'
  }

  return props
}
</script>
```

### Other options

```vue
<template>
  <q-btn color="black" class="full-width" label="Full-width" />

  <q-btn color="primary" label="With Tooltip" class="q-mt-md">
    <q-tooltip class="bg-accent">I'm a tooltip</q-tooltip>
  </q-btn>
</template>
```

### Disable

```vue
<template>
  <div class="q-gutter-sm">
    <q-btn color="primary" disable label="Disabled" />
    <q-btn round color="primary" disable icon="card_giftcard" />
    <q-btn flat color="primary" disable label="Disabled" />
    <q-btn flat round color="primary" disable icon="card_giftcard" />
    <q-btn outline color="primary" disable label="Disabled" />
    <q-btn outline round color="primary" disable icon="card_giftcard" />
    <q-btn push color="primary" disable label="Disabled" />
    <q-btn push round color="primary" disable icon="card_giftcard" />
    <q-btn class="glossy" color="primary" disable label="Disabled" />
    <q-btn class="glossy" round color="primary" disable icon="card_giftcard" />
  </div>
</template>
```

### Controlling the button for form submission

When you have a button to submit a form's input to the server, like a "Save" button, more often than not you will also want to give the user the ability to submit the form with a press of the ENTER key. If you would also like to give the user feedback of the saving process being in progress, and to prevent the user repeatedly pressing the button, you would need the button to show a loading spinner and be disabled from click events. QBtn allows this behavior if configured so.

> [!IMPORTANT]
> When placing a QBtn with type "submit" in one of the "before", "after", "prepend", or "append" slots of a QField, QInput or QSelect, you should also add a `@click` listener on the QBtn in question. This listener should call the method that submits your form. All "click" events in such slots are not propagated to their parent elements.

Example "Form Submission":

```vue
<template>
  <form @submit.prevent="simulateSubmit" class="q-pa-md">
    <!-- a simple text field watching for the enter key release -->
    <q-input
      filled
      color="teal"
      hint="Type then hit Enter key above"
      v-model="test"
    />

    <!--
      A button with v-model set to submit.
      v-model scope variable must be a strict Boolean
    -->
    <div class="row justify-end">
      <q-btn
        type="submit"
        :loading="submitting"
        label="Save"
        class="q-mt-md"
        color="teal"
      >
        <template #loading>
          <q-spinner-facebook />
        </template>
      </q-btn>
    </div>
  </form>
</template>

<script setup>
import { ref } from 'vue'

const test = ref('')
const submitting = ref(false)

function simulateSubmit() {
  submitting.value = true

  // Simulating a delay here.
  // When we are done, we reset "submitting"
  // Boolean to false to restore the
  // initial state.
  setTimeout(() => {
    // delay simulated, we are done,
    // now restoring submit to its initial state
    submitting.value = false
  }, 3000)
}
</script>
```

## Accessibility *(v2.25+)*

QBtn renders as a native `<button>` element, or as an `<a>` when it acts as a link (`href`, `to` or `type="a"`); a link button that ends up without an `href` receives `role="button"` so it still announces as a button. On link buttons QBtn also synthesizes activation on <kbd>Space</kbd> — a native `<a>` only responds to <kbd>Enter</kbd> — so both keys work regardless of the rendered tag.

While in `disable` or `loading` state, the button is marked `aria-disabled="true"` and drops out of the tab order (a disabled non-link button also gets the native `disabled` attribute), and a loading button swallows all interaction until it settles. When loading with the `percentage` prop, the button exposes itself as a `progressbar` and reports its progress through `aria-valuenow`.

QBtn has no dedicated `aria-label` prop, but the attribute falls through to the rendered element — be sure to supply one on icon-only (e.g. `round`) buttons, which otherwise have no accessible name. A [QTooltip](tooltip.md#accessibility) inside the button does not cover this: it becomes the button's *description* (`aria-describedby`), never its name, so an icon-only button whose only text is a tooltip still announces as an unnamed button.
