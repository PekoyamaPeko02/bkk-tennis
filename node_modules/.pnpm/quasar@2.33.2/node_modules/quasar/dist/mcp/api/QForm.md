## QForm API

### Props

- `autofocus` (boolean, optional)
  Focus first focusable element on initial component render
- `no-error-focus` (boolean, optional)
  Do not try to focus on first component that has a validation error when submitting form
- `no-reset-focus` (boolean, optional)
  Do not try to focus on first component when resetting form
- `greedy` (boolean, optional)
  Validate all fields in form (by default it stops after finding the first invalid field with synchronous validation)

### Methods

- `focus(): void`
  Focus on first focusable element/component in the form
- `validate(shouldFocus?: boolean): Promise<boolean>`
  Triggers a validation on all applicable inner Quasar components
  Params:
    - `shouldFocus` (boolean, optional)
      Tell if it should focus or not on component with error on submitting form; Overrides 'no-focus-error' prop if specified
  Returns: `Promise<boolean>`
    Promise is always fulfilled and receives the outcome (true -> validation was a success, false -> invalid models detected)
    Examples: `validate().then(outcome => { ... })`
- `resetValidation(): void`
  Resets the validation on all applicable inner Quasar components
- `submit(evt?: Event): void`
  Manually trigger form validation and submit
  Params:
    - `evt` (Event, optional)
      JS event object
- `reset(evt?: Event): void`
  Manually trigger form reset
  Params:
    - `evt` (Event, optional)
      JS event object
- `getValidationComponents(): any[]`
  Get an array of children Vue component instances that support Quasar validation API (derived from QField, or using useFormChild()/QFormChildMixin)
  Returns: `any[]`
    Quasar validation API-compatible Vue component instances

### Events

- `@submit`
  Emitted when all validations have passed when tethered to a submit button
  Params:
    - `evt` (Event | SubmitEvent, optional)
      Form submission event object
- `@reset`
  Emitted when all validations have been reset when tethered to a reset button; It is recommended to manually reset the wrapped components models in this handler
- `@validation-success`
  Emitted after a validation was triggered and all inner Quasar components models are valid
- `@validation-error`
  Emitted after a validation was triggered and at least one of the inner Quasar components models are NOT valid
  Params:
    - `ref` (ComponentInstance, optional)
      Vue reference to the first component that triggered the validation error

### Slots

- `#default`
  Default slot in the devland unslotted content of the component

