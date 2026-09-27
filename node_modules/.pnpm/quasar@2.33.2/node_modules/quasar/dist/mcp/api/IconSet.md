## IconSet API

### Props

- `props` (object, required)
  Contents (icons) of the Quasar icon set
  Object shape:
    - `name` (string, required)
      Name of the Quasar icon set
      Examples: `'material-icons'`, `'eva-icons'`
    - `type` (object, optional)
      Generic types
      Object shape:
        - `positive` (string, optional)
          Icon name following Quasar convention
          Examples: `'check_circle'`
        - `negative` (string, optional)
          Icon name following Quasar convention
          Examples: `'warning'`
        - `info` (string, optional)
          Icon name following Quasar convention
          Examples: `'info'`
        - `warning` (string, optional)
          Icon name following Quasar convention
          Examples: `'priority_high'`
    - `arrow` (object, optional)
      Arrow types
      Object shape:
        - `up` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_upward'`
        - `right` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_forward'`
        - `down` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_downward'`
        - `left` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_back'`
        - `dropdown` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_drop_down'`
    - `chevron` (object, optional)
      Chevron types
      Object shape:
        - `left` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_left'`
        - `right` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_right'`
    - `colorPicker` (object, optional)
      Used by QColorPicker
      Object shape:
        - `spectrum` (string, optional)
          Icon name following Quasar convention
          Examples: `'gradient'`
        - `tune` (string, optional)
          Icon name following Quasar convention
          Examples: `'tune'`
        - `palette` (string, optional)
          Icon name following Quasar convention
          Examples: `'style'`
    - `pullToRefresh` (object, optional)
      Used by QPullToRefresh
      Object shape:
        - `icon` (string, optional)
          Icon name following Quasar convention
          Examples: `'refresh'`
    - `carousel` (object, optional)
      Used by QCarousel
      Object shape:
        - `left` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_left'`
        - `right` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_right'`
        - `up` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_up'`
        - `down` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_down'`
        - `navigationIcon` (string, optional)
          Icon name following Quasar convention
          Examples: `'lens'`
    - `chip` (object, optional)
      Used by QChip
      Object shape:
        - `remove` (string, optional)
          Icon name following Quasar convention
          Examples: `'cancel'`
        - `selected` (string, optional)
          Icon name following Quasar convention
          Examples: `'check'`
    - `datetime` (object, optional)
      Used by QDate/QTime
      Object shape:
        - `arrowLeft` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_left'`
        - `arrowRight` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_right'`
        - `now` (string, optional)
          Icon name following Quasar convention
          Examples: `'access_time'`
        - `today` (string, optional)
          Icon name following Quasar convention
          Examples: `'today'`
    - `editor` (object, optional)
      Used by QEditor
      Object shape:
        - `bold` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_bold'`
        - `italic` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_italic'`
        - `strikethrough` (string, optional)
          Icon name following Quasar convention
          Examples: `'strikethrough_s'`
        - `underline` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_underlined'`
        - `unorderedList` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_list_bulleted'`
        - `orderedList` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_list_numbered'`
        - `subscript` (string, optional)
          Icon name following Quasar convention
          Examples: `'vertical_align_bottom'`
        - `superscript` (string, optional)
          Icon name following Quasar convention
          Examples: `'vertical_align_top'`
        - `hyperlink` (string, optional)
          Icon name following Quasar convention
          Examples: `'link'`
        - `toggleFullscreen` (string, optional)
          Icon name following Quasar convention
          Examples: `'fullscreen'`
        - `quote` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_quote'`
        - `left` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_align_left'`
        - `center` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_align_center'`
        - `right` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_align_right'`
        - `justify` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_align_justify'`
        - `print` (string, optional)
          Icon name following Quasar convention
          Examples: `'print'`
        - `outdent` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_indent_decrease'`
        - `indent` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_indent_increase'`
        - `removeFormat` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_clear'`
        - `formatting` (string, optional)
          Icon name following Quasar convention
          Examples: `'text_format'`
        - `fontSize` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_size'`
        - `align` (string, optional)
          Icon name following Quasar convention
          Examples: `'format_align_left'`
        - `hr` (string, optional)
          Icon name following Quasar convention
          Examples: `'remove'`
        - `undo` (string, optional)
          Icon name following Quasar convention
          Examples: `'undo'`
        - `redo` (string, optional)
          Icon name following Quasar convention
          Examples: `'redo'`
        - `heading` (string, optional)
          Used only when headingX is missing; Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading1` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading2` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading3` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading4` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading5` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `heading6` (string, optional)
          (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `code` (string, optional)
          Icon name following Quasar convention
          Examples: `'code'`
        - `size` (string, optional)
          Used only when sizeX is missing; Icon name following Quasar convention
          Examples: `'format_size'`
        - `size1` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size2` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size3` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size4` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size5` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size6` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `size7` (string, optional)
          (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
          Examples: `'format_size'`
        - `font` (string, optional)
          Icon name following Quasar convention
          Examples: `'font_download'`
        - `viewSource` (string, optional)
          Icon name following Quasar convention
          Examples: `'code'`
    - `expansionItem` (object, optional)
      Used by QExpansionItem
      Object shape:
        - `icon` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_down'`
        - `denseIcon` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_drop_down'`
    - `fab` (object, optional)
      Used by QFab
      Object shape:
        - `icon` (string, optional)
          Icon name following Quasar convention
          Examples: `'add'`
        - `activeIcon` (string, optional)
          Icon name following Quasar convention
          Examples: `'close'`
    - `field` (object, optional)
      Used by QField/QInput/QSelect/...
      Object shape:
        - `clear` (string, optional)
          Icon name following Quasar convention
          Examples: `'cancel'`
        - `error` (string, optional)
          Icon name following Quasar convention
          Examples: `'error'`
    - `pagination` (object, optional)
      Used by QPagination
      Object shape:
        - `first` (string, optional)
          Icon name following Quasar convention
          Examples: `'first_page'`
        - `prev` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_left'`
        - `next` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_right'`
        - `last` (string, optional)
          Icon name following Quasar convention
          Examples: `'last_page'`
    - `rating` (object, optional)
      Used by QRating
      Object shape:
        - `icon` (string, optional)
          Icon name following Quasar convention
          Examples: `'grade'`
    - `stepper` (object, optional)
      Used by QStepper
      Object shape:
        - `done` (string, optional)
          Icon name following Quasar convention
          Examples: `'check'`
        - `active` (string, optional)
          Icon name following Quasar convention
          Examples: `'edit'`
        - `error` (string, optional)
          Icon name following Quasar convention
          Examples: `'warning'`
    - `tabs` (object, optional)
      Used by QTabs
      Object shape:
        - `left` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_left'`
        - `right` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_right'`
        - `up` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_up'`
        - `down` (string, optional)
          Icon name following Quasar convention
          Examples: `'keyboard_arrow_down'`
    - `table` (object, optional)
      Used by QTable
      Object shape:
        - `arrowUp` (string, optional)
          Icon name following Quasar convention
          Examples: `'arrow_upward'`
        - `warning` (string, optional)
          Icon name following Quasar convention
          Examples: `'warning'`
        - `firstPage` (string, optional)
          Icon name following Quasar convention
          Examples: `'first_page'`
        - `prevPage` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_left'`
        - `nextPage` (string, optional)
          Icon name following Quasar convention
          Examples: `'chevron_right'`
        - `lastPage` (string, optional)
          Icon name following Quasar convention
          Examples: `'last_page'`
    - `tree` (object, optional)
      Used by QTree
      Object shape:
        - `icon` (string, optional)
          Icon name following Quasar convention
          Examples: `'play_arrow'`
    - `uploader` (object, optional)
      Used by QUploader
      Object shape:
        - `done` (string, optional)
          Icon name following Quasar convention
          Examples: `'done'`
        - `clear` (string, optional)
          Icon name following Quasar convention
          Examples: `'clear'`
        - `add` (string, optional)
          Icon name following Quasar convention
          Examples: `'add_box'`
        - `upload` (string, optional)
          Icon name following Quasar convention
          Examples: `'cloud_upload'`
        - `removeQueue` (string, optional)
          Icon name following Quasar convention
          Examples: `'clear_all'`
        - `removeUploaded` (string, optional)
          Icon name following Quasar convention
          Examples: `'done_all'`
- `iconMapFn` (Function, optional)
  Function to map icon names to other icon names; It is designed to be used internally by Quasar only; Only assign a function to it, but do not call it yourself
  Function signature: `(iconName: string) => object`
  Examples: `iconName => (myIcons[ iconName ] !== void 0? { icon: myIcons[ iconName ] } : void 0)`
  Params:
    - `iconName` (string, required)
      Icon name to test
  Returns: `object`
    The icon  following Quasar convention
    Object shape:
      - `icon` (string, optional)
        The mapped icon string, which will be handled by Quasar as if the original icon name was this value; ; Either use only 'icon' or 'cls' + 'content'
        Examples: `'favorite'`
      - `cls` (string, optional)
        CSS classes to apply to the created DOM element; Either use 'cls' + 'content' or only 'icon'
        Examples: `'material-icons'`
      - `content` (string, optional)
        Optional, in case you are using a ligature font and you need it as content of the created DOM element; Either use 'cls' + 'content' or only 'icon'
        Examples: `'favorite'`

### Methods

- `set(iconSet: object, ssrContent?: object): void`
  Set another Quasar Icon Set
  Params:
    - `iconSet` (object, required)
      Usually you will import such an object directly from quasar (eg: import qIconSet from 'quasar/icon-set/<icon-set-name>')
      Object shape:
        - `name` (string, required)
          Name of the Quasar icon set
          Examples: `'material-icons'`, `'eva-icons'`
        - `type` (object, required)
          Generic types
          Object shape:
            - `positive` (string, required)
              Icon name following Quasar convention
              Examples: `'check_circle'`
            - `negative` (string, required)
              Icon name following Quasar convention
              Examples: `'warning'`
            - `info` (string, required)
              Icon name following Quasar convention
              Examples: `'info'`
            - `warning` (string, required)
              Icon name following Quasar convention
              Examples: `'priority_high'`
        - `arrow` (object, required)
          Arrow types
          Object shape:
            - `up` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_upward'`
            - `right` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_forward'`
            - `down` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_downward'`
            - `left` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_back'`
            - `dropdown` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_drop_down'`
        - `chevron` (object, required)
          Chevron types
          Object shape:
            - `left` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_left'`
            - `right` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_right'`
        - `colorPicker` (object, optional)
          Used by QColorPicker
          Object shape:
            - `spectrum` (string, required)
              Icon name following Quasar convention
              Examples: `'gradient'`
            - `tune` (string, required)
              Icon name following Quasar convention
              Examples: `'tune'`
            - `palette` (string, required)
              Icon name following Quasar convention
              Examples: `'style'`
        - `pullToRefresh` (object, required)
          Used by QPullToRefresh
          Object shape:
            - `icon` (string, required)
              Icon name following Quasar convention
              Examples: `'refresh'`
        - `carousel` (object, required)
          Used by QCarousel
          Object shape:
            - `left` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_left'`
            - `right` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_right'`
            - `up` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_up'`
            - `down` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_down'`
            - `navigationIcon` (string, required)
              Icon name following Quasar convention
              Examples: `'lens'`
        - `chip` (object, required)
          Used by QChip
          Object shape:
            - `remove` (string, required)
              Icon name following Quasar convention
              Examples: `'cancel'`
            - `selected` (string, required)
              Icon name following Quasar convention
              Examples: `'check'`
        - `datetime` (object, required)
          Used by QDate/QTime
          Object shape:
            - `arrowLeft` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_left'`
            - `arrowRight` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_right'`
            - `now` (string, required)
              Icon name following Quasar convention
              Examples: `'access_time'`
            - `today` (string, required)
              Icon name following Quasar convention
              Examples: `'today'`
        - `editor` (object, required)
          Used by QEditor
          Object shape:
            - `bold` (string, required)
              Icon name following Quasar convention
              Examples: `'format_bold'`
            - `italic` (string, required)
              Icon name following Quasar convention
              Examples: `'format_italic'`
            - `strikethrough` (string, required)
              Icon name following Quasar convention
              Examples: `'strikethrough_s'`
            - `underline` (string, required)
              Icon name following Quasar convention
              Examples: `'format_underlined'`
            - `unorderedList` (string, required)
              Icon name following Quasar convention
              Examples: `'format_list_bulleted'`
            - `orderedList` (string, required)
              Icon name following Quasar convention
              Examples: `'format_list_numbered'`
            - `subscript` (string, required)
              Icon name following Quasar convention
              Examples: `'vertical_align_bottom'`
            - `superscript` (string, required)
              Icon name following Quasar convention
              Examples: `'vertical_align_top'`
            - `hyperlink` (string, required)
              Icon name following Quasar convention
              Examples: `'link'`
            - `toggleFullscreen` (string, required)
              Icon name following Quasar convention
              Examples: `'fullscreen'`
            - `quote` (string, required)
              Icon name following Quasar convention
              Examples: `'format_quote'`
            - `left` (string, required)
              Icon name following Quasar convention
              Examples: `'format_align_left'`
            - `center` (string, required)
              Icon name following Quasar convention
              Examples: `'format_align_center'`
            - `right` (string, required)
              Icon name following Quasar convention
              Examples: `'format_align_right'`
            - `justify` (string, required)
              Icon name following Quasar convention
              Examples: `'format_align_justify'`
            - `print` (string, required)
              Icon name following Quasar convention
              Examples: `'print'`
            - `outdent` (string, required)
              Icon name following Quasar convention
              Examples: `'format_indent_decrease'`
            - `indent` (string, required)
              Icon name following Quasar convention
              Examples: `'format_indent_increase'`
            - `removeFormat` (string, required)
              Icon name following Quasar convention
              Examples: `'format_clear'`
            - `formatting` (string, required)
              Icon name following Quasar convention
              Examples: `'text_format'`
            - `fontSize` (string, required)
              Icon name following Quasar convention
              Examples: `'format_size'`
            - `align` (string, required)
              Icon name following Quasar convention
              Examples: `'format_align_left'`
            - `hr` (string, required)
              Icon name following Quasar convention
              Examples: `'remove'`
            - `undo` (string, required)
              Icon name following Quasar convention
              Examples: `'undo'`
            - `redo` (string, required)
              Icon name following Quasar convention
              Examples: `'redo'`
            - `heading` (string, required)
              Used only when headingX is missing; Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading1` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading2` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading3` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading4` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading5` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `heading6` (string, optional)
              (Can be omitted; defaults to 'heading' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `code` (string, required)
              Icon name following Quasar convention
              Examples: `'code'`
            - `size` (string, required)
              Used only when sizeX is missing; Icon name following Quasar convention
              Examples: `'format_size'`
            - `size1` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size2` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size3` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size4` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size5` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size6` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `size7` (string, optional)
              (Can be omitted; defaults to 'size' instead) Icon name following Quasar convention
              Examples: `'format_size'`
            - `font` (string, required)
              Icon name following Quasar convention
              Examples: `'font_download'`
            - `viewSource` (string, required)
              Icon name following Quasar convention
              Examples: `'code'`
        - `expansionItem` (object, required)
          Used by QExpansionItem
          Object shape:
            - `icon` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_down'`
            - `denseIcon` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_drop_down'`
        - `fab` (object, required)
          Used by QFab
          Object shape:
            - `icon` (string, required)
              Icon name following Quasar convention
              Examples: `'add'`
            - `activeIcon` (string, required)
              Icon name following Quasar convention
              Examples: `'close'`
        - `field` (object, required)
          Used by QField/QInput/QSelect/...
          Object shape:
            - `clear` (string, required)
              Icon name following Quasar convention
              Examples: `'cancel'`
            - `error` (string, required)
              Icon name following Quasar convention
              Examples: `'error'`
        - `pagination` (object, required)
          Used by QPagination
          Object shape:
            - `first` (string, required)
              Icon name following Quasar convention
              Examples: `'first_page'`
            - `prev` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_left'`
            - `next` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_right'`
            - `last` (string, required)
              Icon name following Quasar convention
              Examples: `'last_page'`
        - `rating` (object, required)
          Used by QRating
          Object shape:
            - `icon` (string, required)
              Icon name following Quasar convention
              Examples: `'grade'`
        - `stepper` (object, required)
          Used by QStepper
          Object shape:
            - `done` (string, required)
              Icon name following Quasar convention
              Examples: `'check'`
            - `active` (string, required)
              Icon name following Quasar convention
              Examples: `'edit'`
            - `error` (string, required)
              Icon name following Quasar convention
              Examples: `'warning'`
        - `tabs` (object, required)
          Used by QTabs
          Object shape:
            - `left` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_left'`
            - `right` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_right'`
            - `up` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_up'`
            - `down` (string, required)
              Icon name following Quasar convention
              Examples: `'keyboard_arrow_down'`
        - `table` (object, required)
          Used by QTable
          Object shape:
            - `arrowUp` (string, required)
              Icon name following Quasar convention
              Examples: `'arrow_upward'`
            - `warning` (string, required)
              Icon name following Quasar convention
              Examples: `'warning'`
            - `firstPage` (string, required)
              Icon name following Quasar convention
              Examples: `'first_page'`
            - `prevPage` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_left'`
            - `nextPage` (string, required)
              Icon name following Quasar convention
              Examples: `'chevron_right'`
            - `lastPage` (string, required)
              Icon name following Quasar convention
              Examples: `'last_page'`
        - `tree` (object, required)
          Used by QTree
          Object shape:
            - `icon` (string, required)
              Icon name following Quasar convention
              Examples: `'play_arrow'`
        - `uploader` (object, required)
          Used by QUploader
          Object shape:
            - `done` (string, required)
              Icon name following Quasar convention
              Examples: `'done'`
            - `clear` (string, required)
              Icon name following Quasar convention
              Examples: `'clear'`
            - `add` (string, required)
              Icon name following Quasar convention
              Examples: `'add_box'`
            - `upload` (string, required)
              Icon name following Quasar convention
              Examples: `'cloud_upload'`
            - `removeQueue` (string, required)
              Icon name following Quasar convention
              Examples: `'clear_all'`
            - `removeUploaded` (string, required)
              Icon name following Quasar convention
              Examples: `'done_all'`
    - `ssrContent` (object, optional)
      Required for SSR/SSG only

### Vue Injection

Accessible via `$q.iconSet` (e.g., `this.$q.iconSet` in Options API or `useQuasar().iconSet` in Composition API).

