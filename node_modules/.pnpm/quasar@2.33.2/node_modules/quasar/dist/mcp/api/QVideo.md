## QVideo API

### Props

- `ratio` (string | number, optional)
  Aspect ratio for the content; If value is a String, then avoid using a computational statement (like '16/9') and instead specify the String value of the result directly (eg. '1.7777')
  Examples: `1`, `'1.7778'`, `:ratio="4/3"`, `:ratio="16/9"`
- `src` (string, required)
  The source url to display in an iframe
  Examples: `'https://www.youtube.com/embed/k3_tw44QsZQ'`
- `title` (string, optional)
  (Accessibility) Set the native 'title' attribute value of the inner iframe being used
  Examples: `'My Daily Marathon'`
- `fetchpriority` (string, optional), default `'auto'`
  Provides a hint of the relative priority to use when fetching the iframe document
  Accepts: `'high'`, `'low'`, `'auto'`
- `loading` (string, optional), default `'eager'`
  Indicates how the browser should load the iframe
  Accepts: `'eager'`, `'lazy'`
- `referrerpolicy` (string, optional), default `'strict-origin-when-cross-origin'`
  Indicates which referrer to send when fetching the frame's resource
  Accepts: `'no-referrer'`, `'no-referrer-when-downgrade'`, `'origin'`, `'origin-when-cross-origin'`, `'same-origin'`, `'strict-origin'`, `'strict-origin-when-cross-origin'`, `'unsafe-url'`

