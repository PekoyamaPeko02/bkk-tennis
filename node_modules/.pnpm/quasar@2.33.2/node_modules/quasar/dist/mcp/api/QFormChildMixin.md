## QFormChildMixin API

### Methods

- `validate(): boolean | Promise<boolean>`
  Needs to be overwritten when getting extended/mixed in
  Returns: `boolean | Promise<boolean>`
    Promise is always fulfilled and receives the outcome (true -> validation was a success, false -> invalid models detected)
    Examples: `validate().then(outcome => { ... })`
- `resetValidation(): void`
  Needs to be overwritten when getting extended/mixed in

