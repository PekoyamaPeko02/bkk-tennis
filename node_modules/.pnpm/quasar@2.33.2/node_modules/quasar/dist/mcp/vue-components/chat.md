---
title: Chat Message
---
Quasar supplies a chat component called QChatMessage which is really a chat entry that renders the data given by the props.

> [!TIP]
> To mix messages with avatar and without avatar in the same thread, use a placeholder avatar image.

## QChatMessage API

Not inlined here: call the `get_api` tool with `name: "QChatMessage"` for its definition, or add `part` (`props`, `slots`) for one of them.

## Usage

### The basics

> [!NOTE]
> Using the property `sent` is intended for the sender of the chat message. The other side is for received messages.

Example "Basic":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message :text="['hey, how are you?']" sent />
      <q-chat-message :text="['doing fine, how r you?']" />
    </div>
  </div>
</template>
```

Example "Name":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message name="me" :text="['hey, how are you?']" sent />
      <q-chat-message name="Jane" :text="['doing fine, how r you?']" />
    </div>
  </div>
</template>
```

Example "Avatar":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar1.jpg"
        :text="['hey, how are you?']"
        sent
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar2.jpg"
        :text="['doing fine, how r you?']"
      />
    </div>
  </div>
</template>
```

Example "Stamp":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar4.jpg"
        :text="['hey, how are you?']"
        sent
        stamp="7 minutes ago"
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar3.jpg"
        :text="[`doing fine, how r you?`]"
        stamp="4 minutes ago"
      />
    </div>
  </div>
</template>
```

Example "Label":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message label="Sunday, 19th" />

      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar4.jpg"
        :text="['hey, how are you?']"
        sent
        stamp="7 minutes ago"
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar3.jpg"
        :text="['doing fine, how r you?']"
        stamp="4 minutes ago"
      />
    </div>
  </div>
</template>
```

### Customization

Example "Text and background color":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar1.jpg"
        :text="['hey, how are you?']"
        stamp="7 minutes ago"
        sent
        bg-color="amber-7"
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        :text="['doing fine, how r you?']"
        stamp="4 minutes ago"
        text-color="white"
        bg-color="primary"
      />
    </div>
  </div>
</template>
```

Example "Size":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar3.jpg"
        :text="['hey, how are you?']"
        stamp="7 minutes ago"
        sent
        bg-color="amber-7"
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        :text="[
          'doing fine, how r you?',
          'I just feel like typing a really, really, REALLY long message to annoy you...'
        ]"
        size="6"
        stamp="4 minutes ago"
        text-color="white"
        bg-color="primary"
      />
      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        :text="['Did it work?']"
        stamp="1 minutes ago"
        size="8"
        text-color="white"
        bg-color="primary"
      />
    </div>
  </div>
</template>
```

### Slots

Example "Default slot":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="me"
        avatar="https://cdn.quasar.dev/img/avatar3.jpg"
        stamp="7 minutes ago"
        sent
        text-color="white"
        bg-color="primary"
      >
        <div> Hey there! </div>

        <div>
          Have you seen Quasar?
          <img
            alt="Surprised Quasar emoji"
            src="https://cdn.quasar.dev/img/discord-omq.png"
            class="my-emoticon"
          />
        </div>
      </q-chat-message>

      <q-chat-message
        name="Jane"
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        bg-color="amber"
      >
        <q-spinner-dots size="2rem" />
      </q-chat-message>
    </div>
  </div>
</template>

<style lang="sass">
.my-emoticon
  vertical-align: middle
  height: 2em
  width: 2em
</style>
```

Example "Avatar/Stamp/Name slots":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        :text="['Have you seen Quasar?']"
        sent
        text-color="white"
        bg-color="primary"
      >
        <template #name>me</template>
        <template #stamp>7 minutes ago</template>
        <template #avatar>
          <img
            alt="User avatar"
            class="q-message-avatar q-message-avatar--sent"
            src="https://cdn.quasar.dev/img/avatar4.jpg"
          />
        </template>
      </q-chat-message>

      <q-chat-message bg-color="amber">
        <template #name>Mary</template>
        <template #avatar>
          <img
            alt="User avatar"
            class="q-message-avatar q-message-avatar--received"
            src="https://cdn.quasar.dev/img/avatar2.jpg"
          />
        </template>

        <div>
          Already building an app with it...
          <img
            alt="Quasar heart emoji"
            src="https://cdn.quasar.dev/img/discord-qeart.png"
            class="my-emoji"
          />
        </div>

        <q-spinner-dots size="2rem" />
      </q-chat-message>
    </div>
  </div>
</template>

<style lang="sass">
.my-emoji
  vertical-align: middle
  height: 2em
  width: 2em
</style>
```

### Sanitization

> [!CAUTION]
> Always sanitize values if you do not trust the origin (if the value comes from user input).

Example "Sanitized content":

```vue
<template>
  <div class="row justify-center">
    <div style="width: 100%; max-width: 400px">
      <q-chat-message
        name="<span class='text-positive'>Untrusted Source</span>"
        avatar="https://cdn.quasar.dev/img/avatar3.jpg"
        :text="['hey, how are <strong>you</strong>?']"
        stamp="7 minutes ago"
        sent
        bg-color="amber-7"
      />
      <q-chat-message
        name="<span class='text-negative'>Jane (trusted name but untrusted text)</span>"
        name-html
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        :text="[
          'doing fine, how r you?',
          'I just feel like typing a really, really, <strong>REALLY</strong> long message to annoy you...'
        ]"
        size="6"
        stamp="4 minutes ago"
        text-color="white"
        bg-color="primary"
      />
      <q-chat-message
        name="<span class='text-negative'>Jao (trusted)</span>"
        name-html
        avatar="https://cdn.quasar.dev/img/avatar5.jpg"
        :text="['<strong>Did it work?</strong>']"
        text-html
        stamp="1 minutes ago"
        size="8"
        text-color="white"
        bg-color="primary"
      />
    </div>
  </div>
</template>
```

## Accessibility *(v2.25+)*

The message text is plain readable content and the avatar image is hidden from assistive technology. Sent vs. received is conveyed only visually (alignment and color), so always provide the `name` prop — or equivalent text content — so that authorship gets announced. The `*-html` props render raw HTML: sanitizing it is entirely your responsibility (see "Sanitization" above).
