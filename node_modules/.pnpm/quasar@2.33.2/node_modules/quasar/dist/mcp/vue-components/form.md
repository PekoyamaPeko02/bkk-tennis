---
title: Form
related:
  - title: Input
    path: input.md
  - title: Select
    path: select.md
  - title: Field
    path: field.md
  - title: useFormChild composable
    path: ../vue-composables/use-form-child.md
---
The QForm component renders a `<form>` DOM element and allows you to easily validate child form components (like [QInput](input.md#internal-validation), [QSelect](select.md) or your [QField](field.md) wrapped components) that have the **internal validation** (NOT the external one) through `rules` associated with them.

## QForm API

Not inlined here: call the `get_api` tool with `name: "QForm"` for its definition, or add `part` (`props`, `methods`, `events`, `slots`) for one of them.

## Usage

> [!IMPORTANT]
> Please be aware of the following:
>
> - QForm hooks into QInput, QSelect or QField wrapped components
> - QInput, QSelect or QField wrapped components must use the internal validation (NOT the external one).
> - The `validate()` method runs the components' internal validation (their `rules`) only. Native HTML constraints (like `type="email"` or a `required` attribute on the underlying native input) are enforced by the browser on a native form submission, but `validate()` does not consult them, so express such constraints as rules too (e.g. `:rules="['email']"`).
> - If you want to take advantage of the `reset` functionality, then be sure to also capture the `@reset` event on QForm and make its handler reset all of the wrapped components models.

Example "Basic":

```vue
<template>
  <div style="max-width: 400px">
    <q-form @submit="onSubmit" @reset="onReset" class="q-gutter-md">
      <q-input
        filled
        v-model="name"
        label="Your name *"
        hint="Name and surname"
        lazy-rules
        :rules="[val => (val && val.length > 0) || 'Please type something']"
      />

      <q-input
        filled
        type="number"
        v-model.number="age"
        label="Your age *"
        lazy-rules
        :rules="[
          val => (val !== null && val !== '') || 'Please type your age',
          val => (val > 0 && val < 100) || 'Please type a real age'
        ]"
      />

      <q-toggle v-model="accept" label="I accept the license and terms" />

      <div>
        <q-btn label="Submit" type="submit" color="primary" />
        <q-btn
          label="Reset"
          type="reset"
          color="primary"
          flat
          class="q-ml-sm"
        />
      </div>
    </q-form>
  </div>
</template>

<script setup>
import { useQuasar } from 'quasar'
import { ref } from 'vue'

const $q = useQuasar()

const name = ref(null)
const age = ref(null)
const accept = ref(false)

function onSubmit() {
  if (accept.value !== true) {
    $q.notify({
      color: 'red-5',
      textColor: 'white',
      icon: 'warning',
      message: 'You need to accept the license and terms first'
    })
  } else {
    $q.notify({
      color: 'green-4',
      textColor: 'white',
      icon: 'cloud_done',
      message: 'Submitted'
    })
  }
}

function onReset() {
  name.value = null
  age.value = null
  accept.value = false
}
</script>
```

In order for the user to be able to activate the `@submit` or `@reset` events on the form, create a QBtn with `type` set to `submit` or `reset`:

```html
<div>
  <q-btn label="Submit" type="submit" color="primary" />
  <q-btn label="Reset" type="reset" color="primary" flat class="q-ml-sm" />
</div>
```

Alternatively, you can give the QForm a Vue ref name and call the `validate` and `resetValidation` functions directly:

```js
// <q-form ref="myFormRef">

setup () {
  const myFormRef = useTemplateRef('myFormRef')

  function validate () {
    myFormRef.value.validate().then(success => {
      if (success) {
        // yay, models are correct
      }
      else {
        // oh no, user has filled in
        // at least one invalid value
      }
    })
  }

  // to reset validations:
  function reset () {
    myFormRef.value.resetValidation()
  }

  return {
    // ...
  }
}
```

## Turning off Autocompletion

If you want to turn off the way that some browsers use autocorrection or spellchecking of all of the input elements of your form, you can also add these pure HTML attributes to the QForm component:

```html
autocorrect="off" autocapitalize="off" autocomplete="off" spellcheck="false"
```

## Submitting to a URL (native form submit)

If you are using the native `action` and `method` attributes on a QForm, please remember to use the `name` prop on each Quasar form component, so that the sent formData to actually contain what the user has filled in.

```html
<q-form action="https://some-url.com" method="post">
  <q-input name="firstname" ...>
  <!-- ... -->
</q-form>
```

- Control the way the form is submitted by setting `action`, `method`, `enctype` and `target` attributes of QForm
- If a listener on `@submit` IS NOT present on the QForm then the form will be submitted if the validation is successful
- If a listener on `@submit` IS present on the QForm then the listener will be called if the validation is successful. In order to do a native submit in this case:

```html
<q-form action="https://some-url.com" method="post" @submit.prevent="onSubmit">
  <q-input name="firstname" ...>
  <!-- ... -->
</q-form>
```

```js
methods: {
  onSubmit (evt) {
    console.log('@submit - do something here', evt)
    evt.target.submit()
  }
}
```

## Child communication

By default, all the Quasar form components communicate with the parent QForm instance. If, for some reason, you are creating your own form component (**that doesn't wrap a Quasar form component**), then you can make QForm aware of it by using:

```js
import { useFormChild } from 'quasar'

setup () {
  // function validate () { ... }

  useFormChild({
    validate, // Function; Can be async;
              // Should return a Boolean (or a Promise resolving to a Boolean)
    resetValidation,    // Optional function which resets validation
    requiresQForm: true // should it error out if no parent QForm is found?
  })
}
```

## Accessibility *(v2.25+)*

QForm renders a native `<form>` element, so the browser's built-in form semantics (including implicit submission with <kbd>Enter</kbd>) apply as-is. When validation fails, QForm moves keyboard focus to the first invalid field (opt out with the `no-error-focus` prop), and screen readers pick up that field's error through its own `role="alert"` message — see [QField's Accessibility section](field.md#accessibility). The `autofocus` prop focuses the first `[autofocus]` element (falling back to the first tabbable one) when the form is mounted.

There is no aggregate error summary: a screen reader user hears the alert of the field that receives focus, not how many fields failed overall. For long forms, consider rendering a live region of your own (e.g. "3 fields need attention") when validation fails.
