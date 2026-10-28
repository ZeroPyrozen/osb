---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on sprites, origins and layers.
questions:
  - prompt: Which line declares a sprite correctly?
    choices:
      - '`Sprite,"sb/star.png",320,240,Foreground,Centre`'
      - '`Sprite,Foreground,Centre,"sb/star.png",320,240`'
      - '`Sprite,320,240,Centre,Foreground,"sb/star.png"`'
    answer: 1
    explanation: The order is `Sprite,layer,origin,"file",x,y`.

  - prompt: A sprite is declared, but has no commands under it. What does the player see?
    choices:
      - The sprite at its position for the whole song
      - The sprite for one second
      - Nothing. It never appears.
    answer: 2
    explanation: Objects only exist while their commands are running. Without commands, a sprite never appears.

  - prompt: A 100 × 100 image is placed at (0, 0) with the `TopLeft` origin. Where does it end up?
    choices:
      - It fills the area from (0, 0) to (100, 100)
      - Its centre is on the top-left corner of the screen, so only a quarter is visible
      - It's centred on the screen
    answer: 0
    explanation: With TopLeft, (x, y) is where the image's top-left corner goes.

  - prompt: Around which point does a sprite rotate?
    choices:
      - Always its centre
      - The centre of the screen
      - Its origin
    answer: 2
    explanation: The origin is the pivot for both rotation and scaling.

  - prompt: Two sprites on the Foreground layer overlap. Which one is drawn in front?
    choices:
      - The one declared later in the file
      - The one declared earlier in the file
      - The bigger one
    answer: 0
    explanation: On the same layer, objects are drawn in declaration order, so later ones cover earlier ones.

  - prompt: Which layer is drawn on top of the hit circles?
    choices:
      - Foreground
      - Overlay
      - Pass
    answer: 1
    explanation: Overlay is the only storyboard layer above the hit circles, so use it carefully.

  - prompt: What's true about the Pass and Fail layers?
    choices:
      - Only one is visible at a time, depending on how the player is doing
      - Both are always visible
      - The Fail layer only shows in the editor
    answer: 0
    explanation: osu! shows either the Pass layer or the Fail layer, never both, based on the player's state.
---

Seven questions about declaring sprites, origins and layers.
