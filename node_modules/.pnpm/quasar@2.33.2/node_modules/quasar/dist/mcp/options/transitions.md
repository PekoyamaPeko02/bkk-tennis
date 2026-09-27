---
title: Quasar Components Transitions
---
There are a few Quasar components that implement transitions through `transition-show` / `transition-hide` or `transition-prev` / `transition-next` or simply `transition` props:

- `transition-show` / `transition-hide`
  - [QBtnDropdown](../vue-components/button-dropdown.md)
  - [QInnerLoading](../vue-components/inner-loading.md)
  - [QTooltip](../vue-components/tooltip.md)
  - [QMenu](../vue-components/menu.md)
  - [QDialog](../vue-components/dialog.md)
  - [QSelect](../vue-components/select.md) (through QMenu and QDialog)
  - [QPopupProxy](../vue-components/popup-proxy.md) (through QMenu and QDialog)
- `transition-prev` / `transition-next`
  - [QCarousel](../vue-components/carousel.md)
  - [QTabPanels](../vue-components/tab-panels.md)
  - [QStepper](../vue-components/stepper.md)
- `transition`
  - [QIntersection](../vue-components/intersection.md)

We're going to showcase these transitions here.

The transitions, by the name the props take (the CSS classes are `q-transition--<name>`):

- `slide-right`
- `slide-left`
- `slide-up`
- `slide-down`
- `fade`
- `scale`
- `rotate`
- `flip-right`
- `flip-left`
- `flip-up`
- `flip-down`
- `jump-right`
- `jump-left`
- `jump-up`
- `jump-down`

Use the names indicated in the captions above for the transition props. Example:

```html
<q-menu transition-show="jump-down" transition-hide="jump-up" />
```
