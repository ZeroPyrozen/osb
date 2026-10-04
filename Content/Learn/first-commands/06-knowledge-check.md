---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on commands, time, Fade and Move.
questions:
  - prompt: In ` F,0,1000,2000,0,1`, what is `2000`?
    choices:
      - The opacity at the end
      - The end time, in milliseconds
      - How long the fade lasts, in milliseconds
    answer: 1
    explanation: The order is type, easing, start time, end time, then the values. This fade runs from 1000 ms to 2000 ms.

  - prompt: A sprite's only command is ` F,0,1000,2000,0,1`. What is its opacity at 1500 ms?
    choices:
      - "0"
      - "0.5"
      - "1"
    answer: 1
    explanation: With linear easing (0), halfway through the fade the opacity is halfway between 0 and 1.

  - prompt: The same sprite, at 2500 ms?
    choices:
      - It's fully visible, because the end value holds
      - It's invisible, but still exists
      - It isn't drawn at all, because its last command ended at 2000 ms
    answer: 2
    explanation: An object only exists from its earliest command to its latest one. The end value would hold if a later command kept the sprite alive.

  - prompt: 'A sprite has ` M,0,0,4000,0,240,640,240` and ` F,0,2000,3000,1,0`. What is its opacity at 3500 ms?'
    choices:
      - 0, and it still exists because the move runs until 4000 ms
      - 1, because the fade has finished
      - It isn't drawn, because the fade ended at 3000 ms
    answer: 0
    explanation: The Move keeps the sprite alive until 4000 ms, and the fade's end value of 0 holds after 3000 ms.

  - prompt: 'A sprite is declared at (320, 240) and its first command is ` M,0,1000,2000,100,100,500,100`. Where is it at 500 ms, if another command keeps it alive?'
    choices:
      - At (320, 240), from the Sprite line
      - At (100, 100), the move's start position
      - At (500, 100), the move's end position
    answer: 1
    explanation: Before a property's first command, the object already has that command's start value. The Sprite line's position is only used when nothing moves the object.

  - prompt: 'You want a sprite to go from (100, 100) to (500, 300) between 0 and 1000 ms. Which is the best way?'
    choices:
      - '` M,0,0,1000,100,100,500,300`'
      - '` MX,0,0,1000,100,500` and ` MY,0,0,1000,100,300`'
      - '` M,0,0,500,100,100,300,200` and ` M,0,500,1000,300,200,500,300`'
    answer: 0
    explanation: All three give the same motion, but one Move command is the shortest. The ranking criteria recommend it over two axis-specific commands.

  - prompt: Two Fade commands on the same sprite overlap in time. What happens during the overlap?
    choices:
      - The two opacities are added together
      - The command that started later takes over
      - The game reports an error
    answer: 1
    explanation: The later command wins. That's rarely what you want, so keep commands of the same type back to back instead.
---

Seven questions on commands and the rules of storyboard time.
