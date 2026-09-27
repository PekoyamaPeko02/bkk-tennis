---
title: Popup Edit
related:
  - title: Input
    path: input.md
  - title: QMenu
    path: menu.md
---
The QPopupEdit component can be used to edit a value “in place”, like for example a cell in QTable. By default, a cell is displayed as a String, then if you are using QPopupEdit and a user clicks/taps on the table cell, a popup will open where the user will be able to edit the value using a textfield.

This component injects a [QMenu](menu.md) into its parent DOM element and enables the behavior described above, so **it can be used anywhere**, not only in QTable. QMenu's props are passed through via this component (except for `model-value`, which is QPopupEdit's own edited model), along with QMenu's `escape-key` event.

## QPopupEdit API

Not inlined here: call the `get_api` tool with `name: "QPopupEdit"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!WARNING]
> If used on a QTable, QPopupEdit won't work with cell scoped slots.

### Standalone

Example "Click on text":

```vue
<template>
  <div class="cursor-pointer">
    {{ label }}
    <q-popup-edit v-model="label" auto-save #default="scope">
      <q-input
        v-model="scope.value"
        dense
        autofocus
        counter
        @keyup.enter="scope.set"
      />
    </q-popup-edit>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const label = ref('Click me')
</script>
```

### With QTable

Click on the cells to see the popup editor. The column "Name" demonstrates the `title` prop. The column "Calories" displays a numeric value usage. The column "Fat" also demonstrates the `disable` prop. If you look at the source code, you'll see the cell for "fat" is using QPopupEdit, yet when clicking on the cell, the popup doesn't show.

Example "Edit first columns":

```vue
<template>
  <q-table
    :rows="rows"
    :columns="columns"
    title="QDataTable with QPopupEdit"
    :rows-per-page-options="[]"
    row-key="name"
  >
    <template #body="props">
      <q-tr :props="props">
        <q-td key="desc" :props="props">
          {{ props.row.name }}
          <q-popup-edit
            v-model="props.row.name"
            title="Edit the Name"
            auto-save
            #default="scope"
          >
            <q-input
              v-model="scope.value"
              dense
              autofocus
              counter
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="calories" :props="props">
          {{ props.row.calories }}
          <q-popup-edit
            v-model.number="props.row.calories"
            auto-save
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="fat" :props="props">
          {{ props.row.fat }}
          <q-popup-edit
            disable
            v-model="props.row.fat"
            auto-save
            #default="scope"
          >
            <div class="text-italic text-primary q-mb-xs">
              My Custom Title
            </div>

            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="carbs" :props="props">
          {{ props.row.carbs }}
        </q-td>
        <q-td key="protein" :props="props">
          {{ props.row.protein }}
        </q-td>
        <q-td key="sodium" :props="props">
          {{ props.row.sodium }}
        </q-td>
        <q-td key="calcium" :props="props">
          {{ props.row.calcium }}
          <q-popup-edit v-model="props.row.calcium" #default="scope">
            <div class="text-italic text-primary"> My Custom Title </div>
            <q-input
              v-model="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="iron" :props="props">
          {{ props.row.iron }}
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { ref } from 'vue'

const columns = [
  // ...
]

const rows = ref([
  // ...
])
</script>
```

### Customizing

Example "Customizing QPopupEdit":

```vue
<template>
  <div class="q-gutter-md">
    <div class="cursor-pointer" style="width: 100px">
      {{ label }}
      <q-popup-edit
        v-model="label"
        class="bg-accent text-white"
        #default="scope"
      >
        <q-input
          dark
          color="white"
          v-model="scope.value"
          dense
          autofocus
          counter
          @keyup.enter="scope.set"
        >
          <template #append>
            <q-icon name="edit" />
          </template>
        </q-input>
      </q-popup-edit>
    </div>

    <div class="cursor-pointer" style="width: 100px">
      {{ label2 }}
      <q-popup-edit
        v-model="label2"
        :cover="false"
        :offset="[0, 10]"
        #default="scope"
      >
        <q-input
          color="accent"
          v-model="scope.value"
          dense
          autofocus
          counter
          @keyup.enter="scope.set"
        >
          <template #prepend>
            <q-icon name="record_voice_over" color="accent" />
          </template>
        </q-input>
      </q-popup-edit>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const label = ref('Click me')
const label2 = ref('Also click me')
</script>
```

### Persistent and with buttons

You can also add two buttons with the `buttons` prop, "Cancel" and "Set" (the default labels). These buttons help to control the user's input. Along with the `buttons` prop, you also have the `persistent` prop, which denies the user from closing the popup with the escape key or clicking/ tapping outside of the popup. Lastly, you can control the labels of the two buttons with the `label-set` and `label-cancel` props, as seen in the "Protein" column. Notice "Save" is replacing "Set" and "Close" is replacing "Cancel".

> The `persistent` prop is demonstrated in the "carbs" column.

Example "Persistent edit, and with buttons":

```vue
<template>
  <q-table
    :rows="rows"
    :columns="columns"
    title="QDataTable with QPopupEdit"
    :rows-per-page-options="[]"
    row-key="name"
  >
    <template #body="props">
      <q-tr :props="props">
        <q-td key="desc" :props="props">
          {{ props.row.name }}
          <q-popup-edit v-model="props.row.name" buttons #default="scope">
            <q-input
              v-model="scope.value"
              dense
              autofocus
              counter
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="calories" :props="props">
          {{ props.row.calories }}
          <q-popup-edit
            v-model.number="props.row.calories"
            buttons
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="fat" :props="props">
          <div class="text-pre-wrap">{{ props.row.fat }}</div>
          <q-popup-edit
            v-model.number="props.row.fat"
            buttons
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="carbs" :props="props">
          {{ props.row.carbs }}
          <q-popup-edit
            v-model.number="props.row.carbs"
            buttons
            persistent
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="protein" :props="props">
          {{ props.row.protein }}
          <q-popup-edit
            v-model.number="props.row.protein"
            buttons
            label-set="Save"
            label-cancel="Close"
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="sodium" :props="props">
          {{ props.row.sodium }}
          <q-popup-edit
            v-model.number="props.row.sodium"
            buttons
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="calcium" :props="props">
          {{ props.row.calcium }}
          <q-popup-edit v-model="props.row.calcium" buttons #default="scope">
            <q-input
              v-model="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="iron" :props="props">
          {{ props.row.iron }}
          <q-popup-edit v-model="props.row.iron" buttons #default="scope">
            <q-input
              v-model="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { ref } from 'vue'

const columns = [
  // ...
]

const rows = ref([
  // ...
])
</script>
```

### The default slot

The default slot's parameters are:

```js
{
  initialValue, value, validate, set, cancel, updatePosition
}
```

> [!WARNING]
> Do not destructure the slot's parameters as it will generate linting errors when using the `value` prop directly with `v-model`.

Example "Default slot parameters":

```vue
<template>
  <div class="cursor-pointer">
    {{ nickname }}
    <q-popup-edit
      v-model="nickname"
      :validate="val => val.length > 5"
      #default="scope"
    >
      <q-input
        autofocus
        dense
        v-model="scope.value"
        :model-value="scope.value"
        hint="Your nickname"
        :rules="[val => scope.validate(val) || 'More than 5 chars required']"
      >
        <template #after>
          <q-btn
            flat
            dense
            color="negative"
            icon="cancel"
            @click.stop.prevent="scope.cancel"
          />

          <q-btn
            flat
            dense
            color="positive"
            icon="check_circle"
            @click.stop.prevent="scope.set"
            :disable="
              !scope.validate(scope.value) ||
              scope.initialValue === scope.value
            "
          />
        </template>
      </q-input>
    </q-popup-edit>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const nickname = ref('Click me')
</script>
```

### Textarea / QEditor

Since QPopupEdit wraps QInput, you can basically use any type of QInput. For instance, you can also use a text area as shown below in the "Comments" column.

> [!IMPORTANT]
> When using a multi-line control (textarea, QEditor) for input, you'll need to also use `@keyup.enter.stop` on the component in order to stop the enter key from closing the popup. You'll also need to add buttons for controlling the popup too.

Example "QInput textarea":

```vue
<template>
  <q-table
    :rows="rows"
    :columns="columns"
    title="QDataTable with QPopupEdit"
    :rows-per-page-options="[]"
    row-key="name"
    wrap-cells
  >
    <template #body="props">
      <q-tr :props="props">
        <q-td key="desc" :props="props">
          {{ props.row.name }}
        </q-td>

        <q-td key="comment" :props="props">
          <div>{{ props.row.comment }}</div>
          <q-popup-edit buttons v-model="props.row.comment" #default="scope">
            <q-input
              type="textarea"
              v-model="scope.value"
              autofocus
              counter
              @keyup.enter.stop
            />
          </q-popup-edit>
        </q-td>

        <q-td key="calories" :props="props">
          {{ props.row.calories }}
        </q-td>

        <q-td key="fat" :props="props">
          <div>{{ props.row.fat }}</div>
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { ref } from 'vue'

const columns = [
  {
    name: 'desc',
    style: 'min-width: 160px; width: 160px',
    align: 'left',
    label: 'Dessert',
    field: 'name'
  },
  {
    name: 'comment',
    style: 'min-width: 200px; width: 200px',
    align: 'left',
    label: 'Comment (editable)',
    field: 'comment'
  },
  { name: 'calories', align: 'center', label: 'Calories', field: 'calories' },
  { name: 'fat', label: 'Fat (g)', field: 'fat' }
]

const rows = ref([
  // ...
])
</script>
```

Example "QEditor":

```vue
<template>
  <q-table
    :rows="rows"
    :columns="columns"
    title="QDataTable with QPopupEdit"
    :rows-per-page-options="[]"
    row-key="name"
    wrap-cells
  >
    <template #body="props">
      <q-tr :props="props">
        <q-td key="desc" :props="props">
          {{ props.row.name }}
          <q-popup-edit v-model="props.row.name" #default="scope">
            <q-input
              v-model="scope.value"
              dense
              autofocus
              counter
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>

        <q-td key="comment" :props="props">
          <div v-html="props.row.comment"></div>
          <q-popup-edit buttons v-model="props.row.comment" #default="scope">
            <q-editor
              v-model="scope.value"
              min-height="5rem"
              autofocus
              @keyup.enter.stop
            />
          </q-popup-edit>
        </q-td>

        <q-td key="calories" :props="props">
          {{ props.row.calories }}
          <q-popup-edit v-model.number="props.row.calories" #default="scope">
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>

        <q-td key="fat" :props="props">
          <div class="text-pre-wrap">{{ props.row.fat }}</div>
          <q-popup-edit v-model.number="props.row.fat" #default="scope">
            <q-input
              type="number"
              v-model.number="scope.value"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { ref } from 'vue'

const columns = [
  {
    name: 'desc',
    style: 'min-width: 160px; width: 160px',
    align: 'left',
    label: 'Dessert',
    field: 'name'
  },
  {
    name: 'comment',
    style: 'min-width: 200px; width: 200px',
    align: 'left',
    label: 'Comment (editable)',
    field: 'comment'
  },
  { name: 'calories', align: 'center', label: 'Calories', field: 'calories' },
  { name: 'fat', label: 'Fat (g)', field: 'fat' }
]

const rows = ref([
  // ...
])
</script>
```

### Validation

QPopupEdit also allows for simple validation of the input. To use it, you give it a callback function in the form of an arrow function and it should return a Boolean. `(value) => Boolean`. This is **demonstrated in the "Calories" column** below.

> [!NOTE]
> **Tip 1**
>
> Notice we are using the `hide` event to also revalidate the input. If we don't, QInput's error prop will 'hang' in an invalid state.

> [!TIP]
> **Tip 2**
>
> With this example, we are using QInput's external error handling. We could also use QInput's validation prop and emit the value to QPopupEdit's validation prop. The same concept can be implemented, when using [Regle](https://reglejs.dev/) external validation library too. In other words, the value given to QPopupEdit's validate function can come from anywhere.

Example "Edit with validation":

```vue
<template>
  <q-table
    :rows="rows"
    :columns="columns"
    title="QDataTable with QPopupEdit"
    :rows-per-page-options="[]"
    row-key="name"
  >
    <template #body="props">
      <q-tr :props="props">
        <q-td key="desc" :props="props">
          {{ props.row.name }}
        </q-td>
        <q-td key="calories" :props="props">
          {{ props.row.calories }}
          <q-popup-edit
            v-model.number="props.row.calories"
            buttons
            label-set="Save"
            label-cancel="Close"
            :validate="caloriesRangeValidation"
            @hide="caloriesRangeValidation"
            #default="scope"
          >
            <q-input
              type="number"
              v-model.number="scope.value"
              hint="Enter a number between 4 and 7"
              :error="errorCalories"
              :error-message="errorMessageCalories"
              dense
              autofocus
              @keyup.enter="scope.set"
            />
          </q-popup-edit>
        </q-td>
        <q-td key="fat" :props="props">
          <div class="text-pre-wrap">{{ props.row.fat }}</div>
        </q-td>
        <q-td key="carbs" :props="props">
          {{ props.row.carbs }}
        </q-td>
        <q-td key="protein" :props="props">
          {{ props.row.protein }}
        </q-td>
      </q-tr>
    </template>
  </q-table>
</template>

<script setup>
import { ref } from 'vue'

const columns = [
  { name: 'desc', align: 'left', label: 'Dessert', field: 'name' },
  {
    name: 'calories',
    align: 'center',
    label: 'Calories (editable)',
    field: 'calories'
  },
  { name: 'fat', label: 'Fat', field: 'fat' },
  { name: 'carbs', label: 'Carbs', field: 'carbs' },
  { name: 'protein', label: 'Protein', field: 'protein' }
]

const rows = ref([
  // ...
])

const errorCalories = ref(false)
const errorMessageCalories = ref('')

function caloriesRangeValidation(val) {
  if (val < 4 || val > 7) {
    errorCalories.value = true
    errorMessageCalories.value = 'The value must be between 4 and 7!'
    return false
  }
  errorCalories.value = false
  errorMessageCalories.value = ''
  return true
}
</script>
```

## Accessibility *(v2.25+)*

QPopupEdit is built on [QMenu](menu.md), so the popup itself claims no ARIA role — see [QMenu's Accessibility section](menu.md#accessibility) for the underlying keyboard and focus behavior. <kbd>Escape</kbd> cancels the edit and returns focus to the element the popup covers, and closing the popup any other way without going through validation never silently commits: depending on the `auto-save` prop, either a value that passes validation is saved or `cancel` is emitted. When using the `buttons` prop, "Set" and "Cancel" render as real buttons whose default labels come localized from the [Quasar Language Pack](../options/quasar-language-packs.md) (override them with `label-set` / `label-cancel`).

A few responsibilities remain yours: put `autofocus` on your input so keyboard focus lands in the editor as soon as the popup opens; wire <kbd>Enter</kbd>-to-save yourself with `@keyup.enter="scope.set"` (as the examples above do); and note that the `title` prop renders purely visual text — it is not wired up as the popup's accessible name.
