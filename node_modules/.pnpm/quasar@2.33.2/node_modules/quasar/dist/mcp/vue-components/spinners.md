---
title: Spinners
related:
  - title: Linear Progress
    path: linear-progress.md
  - title: Circular Progress
    path: circular-progress.md
  - title: Skeleton
    path: skeleton.md
  - title: Inner Loading
    path: inner-loading.md
  - title: Loading Plugin
    path: ../quasar-plugins/loading.md
  - title: LoadingBar
    path: ../quasar-plugins/loading-bar.md
---
A Spinner is used to show the user a timely process is currently taking place. It is an important UX feature, which gives the user the feeling the system is continuing to work for longer term activities, like grabbing data from the server or some heavy calculations.

## QSpinner API

Not inlined here: call the `get_api` tool with `name: "QSpinner"` for its `props` definition.

> [!NOTE]
> The API below applies to all spinners, except for QSpinner. Making an example with QSpinnerCube.

## QSpinnerCube API

Not inlined here: call the `get_api` tool with `name: "QSpinnerCube"` for its `props` definition.

## Usage

Example "QSpinner":

```vue
<template>
  <div class="q-gutter-md row">
    <q-spinner color="primary" size="3em" />

    <q-spinner color="primary" size="3em" :thickness="2" />

    <q-spinner color="primary" size="3em" :thickness="10" />
  </div>
</template>
```

In the example below, hover over the spinners to see their names.

Example "Other spinners":

```vue
<template>
  <div class="q-gutter-xs">
    <div class="q-gutter-md row justify-center">
      <div>
        <q-spinner-audio color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerAudio</q-tooltip>
      </div>
      <div>
        <q-spinner-ball color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerBall</q-tooltip>
      </div>
      <div>
        <q-spinner-bars color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerBars</q-tooltip>
      </div>
      <div>
        <q-spinner-box color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerBox</q-tooltip>
      </div>
      <div>
        <q-spinner-clock color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerClock</q-tooltip>
      </div>
      <div>
        <q-spinner-comment color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerComment</q-tooltip>
      </div>
      <div>
        <q-spinner-cube color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerCube</q-tooltip>
      </div>
      <div>
        <q-spinner-dots color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerDots</q-tooltip>
      </div>
      <div>
        <q-spinner-facebook color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerFacebook</q-tooltip>
      </div>
      <div>
        <q-spinner-gears color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerGears</q-tooltip>
      </div>
      <div>
        <q-spinner-grid color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerGrid</q-tooltip>
      </div>
      <div>
        <q-spinner-hearts color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerHearts</q-tooltip>
      </div>
      <div>
        <q-spinner-hourglass color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerHourglass</q-tooltip>
      </div>
      <div>
        <q-spinner-infinity color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerInfinity</q-tooltip>
      </div>
      <div>
        <q-spinner-ios color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerIos</q-tooltip>
      </div>
      <div>
        <q-spinner-orbit color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerOrbit</q-tooltip>
      </div>
      <div>
        <q-spinner-oval color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerOval</q-tooltip>
      </div>
      <div>
        <q-spinner-pie color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerPie</q-tooltip>
      </div>
      <div>
        <q-spinner-puff color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerPuff</q-tooltip>
      </div>
      <div>
        <q-spinner-radio color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerRadio</q-tooltip>
      </div>
      <div>
        <q-spinner-rings color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerRings</q-tooltip>
      </div>
      <div>
        <q-spinner-tail color="primary" size="2em" />
        <q-tooltip :offset="[0, 8]">QSpinnerTail</q-tooltip>
      </div>
    </div>
  </div>
</template>
```

Example "Coloring":

```vue
<template>
  <div class="q-gutter-xs">
    <div class="q-gutter-md row justify-center" style="font-size: 2em">
      <q-spinner-audio color="secondary" />
      <q-spinner-ball color="red" />
      <q-spinner-bars color="purple" />
      <q-spinner-box color="deep-orange" />
      <q-spinner-clock color="brown" />
      <q-spinner-comment color="deep-purple" />
      <q-spinner-cube color="indigo" />
      <q-spinner-dots color="blue" />
      <q-spinner-facebook color="light-blue" />
      <q-spinner-gears color="cyan" />
      <q-spinner-grid color="teal" />
      <q-spinner-hearts color="green" />
      <q-spinner-hourglass color="light-green" />
      <q-spinner-infinity color="lime" />
      <q-spinner-ios color="yellow" />
      <q-spinner-orbit color="blue" />
      <q-spinner-oval color="amber" />
      <q-spinner-pie color="orange" />
      <q-spinner-puff color="deep-orange" />
      <q-spinner-radio color="brown" />
      <q-spinner-rings color="grey" />
      <q-spinner-tail color="blue-grey" />
    </div>
  </div>
</template>
```

Please note that by default, QSpinner and all other spinners inherit the font-size of the parent and applies it as its size.

Example "Size":

```vue
<template>
  <div class="q-gutter-xs">
    <div class="q-gutter-md row items-center">
      <q-spinner-comment color="secondary" size="2em" />

      <q-spinner-radio color="red" size="3em" />

      <q-spinner-hourglass color="purple" size="4em" />

      <q-spinner-cube color="orange" size="5.5em" />
    </div>
  </div>
</template>
```

Example "Standard sizes":

```vue
<template>
  <div class="q-gutter-md row items-center">
    <q-spinner-cube
      v-for="size in ['xs', 'sm', 'md', 'lg', 'xl']"
      :key="size"
      :size="size"
      color="primary"
    />
  </div>
</template>
```

## Accessibility *(v2.25+)*

Spinners render a bare animated SVG with no ARIA at all, so the loading state is invisible to screen readers. Pair a spinner with a live region that says so — an element with `role="status"` containing visually-hidden "Loading…" text — or mark the spinner itself `aria-hidden="true"` when a text alternative already exists elsewhere.
