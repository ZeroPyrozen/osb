---
title: Knowledge check
type: quiz
minutes: 3
summary: Five questions on animations.
questions:
  - prompt: Your frames are `sb/fire0.png` to `sb/fire11.png`. How do you declare them?
    choices:
      - '`"sb/fire0.png"` with a frame count of 11'
      - '`"sb/fire.png"` with a frame count of 12'
      - '`"sb/fire*.png"` with a frame count of 12'
    answer: 1
    explanation: The name has no number, and frames count from 0, so 0 to 11 is 12 frames.

  - prompt: An animation has a frame delay of 50. How many frames does it show per second?
    choices:
      - "5"
      - "20"
      - "50"
    answer: 1
    explanation: 1000 ms divided by 50 ms per frame is 20 frames per second.

  - prompt: What does `LoopOnce` do?
    choices:
      - Plays the frames once, then hides the object
      - Plays the frames once, then keeps showing the last frame
      - Plays the frames twice
    answer: 1
    explanation: LoopOnce stops on the last frame. The object stays visible for as long as its commands keep it alive.

  - prompt: A 4-frame LoopForever animation with a frame delay of 100 starts at 0 ms. Which frame shows at 650 ms?
    choices:
      - Frame 2, from the file ending in 2
      - Frame 3
      - Frame 6
    answer: 0
    explanation: 650 ÷ 100 = 6.5, so it's the 7th frame shown (index 6). With 4 frames that wraps around to index 2.

  - prompt: Which is usually the better choice for a simple spinning star?
    choices:
      - An Animation with 60 drawn frames of the star rotating
      - A Sprite with a Rotate command
      - A video
    answer: 1
    explanation: Simple motion is lighter and smoother with commands. Save animations for changes that commands can't do.
---

Five questions on Animation objects.
