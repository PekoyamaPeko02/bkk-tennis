---
title: Typography
related:
  - title: CSS Visibility
    path: visibility.md
  - title: CSS Positioning Classes
    path: positioning.md
  - title: CSS Spacing Classes
    path: spacing.md
---
We'll handle the typography supplied by Quasar in the sections below.

## Headings

| Class | HTML equivalent | Sample |
| --- | --- | --- |
| `text-h1` | `h1` | Headline 1 |
| `text-h2` | `h2` | Headline 2 |
| `text-h3` | `h3` | Headline 3 |
| `text-h4` | `h4` | Headline 4 |
| `text-h5` | `h5` | Headline 5 |
| `text-h6` | `h6` | Headline 6 |
| `text-subtitle1` |  | Subtitle 1 |
| `text-subtitle2` |  | Subtitle 2 |
| `text-body1` |  | Body 1 |
| `text-body2` |  | Body 2 |
| `text-caption` |  | Caption text |
| `text-overline` |  | Overline |

## Font Weights

- `text-weight-thin`
- `text-weight-light`
- `text-weight-regular`
- `text-weight-medium`
- `text-weight-bold`
- `text-weight-bolder`

## CSS Helper Classes

| Class Name | Description |
| --- | --- |
| `text-right` | Align text to the right |
| `text-left` | Align text to the left |
| `text-center` | Align text to the center |
| `text-justify` | Text will be justified |
| `text-bold` | Text will be in bold |
| `text-italic` | Text will be in italic |
| `text-no-wrap` | Non wrappable text (applies `white-space: nowrap`) |
| `text-strike` | Applies `text-decoration: line-through` |
| `text-uppercase` | Transform text to uppercase |
| `text-lowercase` | Transform text to lowercase |
| `text-capitalize` | Capitalize first letter of the text |

## Default Font

The default webfont embedded is [Roboto](https://fonts.google.com/specimen/Roboto). **But it is not required**. You can use whatever font(s) you like.

Roboto ships as a variable font, so any `font-weight` from 100 to 900 works. It covers the Latin, Latin Extended, Cyrillic, Greek and Vietnamese scripts, split into subsets that the browser downloads only when a page uses them.

This is where Roboto font comes embedded by default, if you are looking to remove it:

Example "/quasar.config file":

```js
extras: ['roboto-font']
```

## Add custom fonts

It is also possible to include other fonts to use them in the app. The following is one way to do it:

1. Copy your new webfont `[customfont].woff2` (or whatever extension it has; `woff2` is supported by every browser and is the smallest) in a directory of your choice, for example: `./src/css/fonts/[customfont.woff2]`
2. Declare your font in `./src/css/app.{css|sass|scss|styl}` (or in any place you see fit, but correctly update the relative path to the webfont file):

```css
@font-face {
  font-family: customfont;
  src: url(./fonts/customfont.woff2);
}

// declare a class which applies it
.my-font {
  font-family: 'customfont';
}
```

3. Then use that class where you need it.
