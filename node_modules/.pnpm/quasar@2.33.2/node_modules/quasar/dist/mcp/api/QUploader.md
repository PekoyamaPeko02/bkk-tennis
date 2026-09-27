## QUploader API

### Props

- `factory` (Function, optional)
  Function which should return an Object or a Promise resolving with an Object; For best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => object | Promise<any>`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `object | Promise<any>`
    Optional configuration for the upload process; You can override QUploader props in this Object (url, method, headers, formFields, fieldName, withCredentials, sendRaw); Props of these Object can also be Functions with the form of (file[s]) => value
- `url` (string | Function, optional)
  URL or path to the server which handles the upload. Takes String or factory function, which returns String. Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => string`
  Examples: `'https://example.com/path'`, `files => `https://example.com?count=${ files.length }``
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `string`
    URL or path to the server which handles the upload
- `method` (string | Function, optional), default `'POST'`
  HTTP method to use for upload; Takes String or factory function which returns a String; Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => string`
  Accepts: `'POST'`, `'PUT'`
  Examples: `'POST'`, `files => (files.length > 10 ? 'POST' : 'PUT')`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `string`
    HTTP method to use for upload
- `field-name` (string | Function, optional), default `file => file.name`
  Field name for each file upload; This goes into the following header: 'Content-Disposition: form-data; name="__HERE__"; filename="somefile.png"; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: File) => string`
  Examples: `'backgroundFile'`, `file => ('background' + file.name)`
  Params:
    - `files` (File, optional)
      The current file being processed
  Returns: `string`
    Field name for the current file upload
- `headers` (any[] | Function, optional)
  Array or a factory function which returns an array; Array consists of objects with header definitions; Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => any[]`
  Examples:
    - `[{ name: 'Content-Type', value: 'application/json' }, { name: 'Accept', value: 'application/json' }]`
    - `() => [ { name: 'X-Custom-Timestamp', value: Date.now() }]`
    - `files => [ { name: 'X-Custom-Count', value: files.length }]`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `any[]`
    An array consisting of objects with header definitions
  Object shape:
    - `name` (string, required)
      Header name
      Examples: `'Content-Type'`, `'Accept'`, `'Cache-Control'`
    - `value` (string, required)
      Header value
      Examples: `'application/json'`, `'no-cache'`
- `form-fields` (any[] | Function, optional)
  Array or a factory function which returns an array; Array consists of objects with additional fields definitions (used by Form to be uploaded); Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => any[]`
  Examples:
    - `[{ name: 'my-field', value: 'my-value' }]`
    - `() => [ { name: 'my-field', value: 'my-value' }]`
    - `files => [ { name: 'my-field', value: 'my-value' + files.length }]`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `any[]`
    An array consisting of objects with additional field definitions (used by FormData to be uploaded)
  Object shape:
    - `name` (string, required)
      Field name
      Examples: `'Some field'`
    - `value` (string, required)
      Field value
      Examples: `'some-value'`
- `with-credentials` (boolean | Function, optional)
  Sets withCredentials to true on the XHR that manages the upload; Takes boolean or factory function for Boolean; Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => boolean`
  Examples: `true`, `files => (files.length === 2)`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `boolean`
    If true, withCredentials will be set to true on the XHR that manages the upload
- `send-raw` (boolean | Function, optional)
  Send raw files without wrapping into a Form(); Takes boolean or factory function for Boolean; Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => boolean`
  Examples: `true`, `files => (files.length > 2)`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `boolean`
    If true, raw files will get sent without wrapping into a Form()
- `batch` (boolean | Function, optional)
  Upload files in batch (in one XHR request); Takes boolean or factory function for Boolean; Function is called right before upload; If using a function then for best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => boolean`
  Examples: `files => files.length > 10`
  Params:
    - `files` (any[], optional)
      Uploaded files
  Returns: `boolean`
    If true, files will be uploaded in a batch (in one XHR request)
- `multiple` (boolean, optional)
  Allow multiple file uploads
- `accept` (string, optional)
  Comma separated list of unique file type specifiers. Maps to 'accept' attribute of native input type=file element
  Examples:
    - `'.jpg, .pdf, image/*'`
    - `'image/jpeg, .pdf'`
- `capture` (string, optional)
  Optionally, specify that a new file should be captured, and which device should be used to capture that new media of a type defined by the 'accept' prop. Maps to 'capture' attribute of native input type=file element
  Accepts: `'user'`, `'environment'`
- `max-file-size` (number | string, optional)
  Maximum size of individual file in bytes
  Examples: `1024`, `'1048576'`
- `max-total-size` (number | string, optional)
  Maximum size of all files combined in bytes
- `max-files` (number | string, optional)
  Maximum number of files to contain
- `filter` (Function, optional)
  Custom filter for added files; Only files that pass this filter will be added to the queue and uploaded; For best performance, reference it from your scope and do not define it inline
  Function signature: `(files?: any[]) => any[]`
  Examples: `files => files.filter(file => file.size === 1024)`
  Params:
    - `files` (any[], optional)
      Candidate files to be added to queue
  Returns: `any[]`
    Filtered files to be added to queue
- `label` (string, optional)
  Label for the uploader
  Examples: `'Upload photo here'`
- `color` (string, optional)
  Color name for component from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `text-color` (string, optional)
  Overrides text color (if needed); Color name from the Quasar Color Palette
  Examples: `'primary'`, `'teal'`, `'teal-10'`
- `dark` (boolean, optional), default `null`
  Notify the component that the background is a dark color
- `square` (boolean, optional)
  Removes border-radius so borders are squared
- `flat` (boolean, optional)
  Applies a 'flat' design (no default shadow)
- `bordered` (boolean, optional)
  Applies a default border to the component
- `no-thumbnails` (boolean, optional)
  Don't display thumbnails for image files
- `auto-upload` (boolean, optional)
  Upload files immediately when added
- `hide-upload-btn` (boolean, optional)
  Don't show the upload button
- `thumbnail-fit` (string, optional), default `'cover'` *(added v2.17)*
  How the thumbnail image will fit into the container; Equivalent of the background-size prop
  Examples: `'cover'`, `'contain'`, `'auto'`, `'50%'`
- `disable` (boolean, optional)
  Put component in disabled mode
- `readonly` (boolean, optional)
  Put component in readonly mode

### Computed Props

- `files` (any[], optional)
  List of all files
- `queuedFiles` (any[], optional)
  List of files that are waiting to be uploaded
- `uploadedFiles` (any[], optional)
  List of files that have been uploaded
- `uploadedSize` (number, optional)
  Size of all uploaded files in bytes
- `uploadSizeLabel` (string, optional)
  Label for the size total of all files
  Examples: `'1.0MB'`
- `uploadProgressLabel` (string, optional)
  Label for the upload progress (in %)
  Examples: `'52.76%'`
- `canAddFiles` (boolean, optional)
  Whether new files can be added to the list
- `canUpload` (boolean, optional)
  Whether the files can be uploaded
- `isBusy` (boolean, optional)
  The component state is set as busy; User should not be able to interact with the component
- `isUploading` (boolean, optional)
  The component is uploading files

### Methods

- `pickFiles(evt: Event): void`
  Trigger the file picker dialog; The event must come from a user interaction event handler
  Params:
    - `evt` (Event, required)
      JS event object of the original user interaction handler
- `addFiles(files: any[] | FileList): void`
  Add files programmatically
  Params:
    - `files` (any[] | FileList, required)
      Array of files (instances of File) or FileList
- `upload(): void`
  Start uploading (same as clicking the upload button)
- `abort(): void`
  Abort upload of all files (same as clicking the abort button)
- `reset(): void`
  Resets uploader to default; Empties queue, aborts current uploads
- `removeUploadedFiles(): void`
  Removes already uploaded files from the list
- `removeQueuedFiles(): void`
  Remove files that are waiting for upload to start (same as clicking the left clear button)
- `removeFile(file: File): void`
  Remove specified file from the queue
  Params:
    - `file` (File, required)
      The file to remove
- `updateFileStatus(file: File, status: string, uploadedSize: number): void`
  Update the status of a file
  Params:
    - `file` (File, required)
      The file to update
    - `status` (string, required)
      Status of file
      Accepts: `'idle'`, `'failed'`, `'uploading'`, `'uploaded'`
    - `uploadedSize` (number, required)
      The number of uploaded bytes of the file; Is required explicitly only when status is NOT 'uploaded'
- `isAlive(): boolean`
  Is the component alive (activated but not unmounted); Useful to determine if you still need to compute anything going further
  Returns: `boolean`
    If true, the current component is still activated and mounted

### Events

- `@uploaded`
  Emitted when file or batch of files is uploaded
  Params:
    - `info` (object, optional)
      Object containing information about the event
      Object shape:
        - `files` (any[], required)
          Uploaded files
        - `xhr` (object, required)
          XMLHttpRequest that has been used to upload this batch of files
- `@failed`
  Emitted when file or batch of files has encountered error while uploading
  Params:
    - `info` (object, optional)
      Object containing information about the event
      Object shape:
        - `files` (any[], required)
          Files which encountered error
        - `xhr` (object, required)
          XMLHttpRequest that has been used to upload this batch of files
- `@uploading`
  Emitted when file or batch of files started uploading
  Params:
    - `info` (object, optional)
      Object containing information about the event
      Object shape:
        - `files` (any[], required)
          Files which are now uploading
        - `xhr` (object, required)
          XMLHttpRequest used for uploading
- `@factory-failed`
  Emitted when the factory function throws, returns an invalid value, or supplies a Promise which is rejected or aborted
  Params:
    - `err` (Error, optional)
      Error object which is the Promise rejection reason
    - `files` (any[], optional)
      Files which were to get uploaded
- `@rejected`
  Emitted after files are picked and some do not pass the validation props (accept, max-file-size, max-total-size, filter, etc)
  Params:
    - `rejectedEntries` (any[], optional)
      Array of { failedPropValidation: string, file: File } Objects for files that do not pass the validation
- `@added`
  Emitted when files are added into the list
  Params:
    - `files` (any[], optional)
      Array of files that were added
- `@removed`
  Emitted when files are removed from the list
  Params:
    - `files` (any[], optional)
      Array of files that were removed
- `@start`
  Started working
- `@finish`
  Finished working (regardless of success or fail)

### Scoped Slots

- `#header`
  Slot for custom header; Scope is the QUploader instance itself
  Scope:
    - `...self` (ComponentInstance, optional)
      QUploader instance
- `#list`
  Slot for custom list; Scope is the QUploader instance itself
  Scope:
    - `...self` (ComponentInstance, optional)
      QUploader instance

