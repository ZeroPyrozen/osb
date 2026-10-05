---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on easing.
questions:
  - prompt: In ` M,7,0,1000,0,240,320,240`, what is `7`?
    choices:
      - The easing, Cubic Out
      - The layer
      - The number of times the move repeats
    answer: 0
    explanation: The easing always comes right after the command type.

  - prompt: Does choosing a different easing change when a command ends?
    choices:
      - Yes, strong easings take longer
      - No. Only the pace between the start and end changes
      - Only for Bounce and Elastic
    answer: 1
    explanation: The start and end times and values stay the same. Easing only shapes the journey in between.

  - prompt: An object slides onto the screen and stops in the middle. Which kind of easing usually feels best?
    choices:
      - An In easing, which starts slowly and speeds up
      - An Out easing, which starts fast and slows down
      - Linear
    answer: 1
    explanation: Things that arrive should slow down as they settle, which is what Out easings do.

  - prompt: Which family travels past its end value and then settles back?
    choices:
      - Sine
      - Expo
      - Back
    answer: 2
    explanation: Back overshoots and returns. Elastic overshoots too, with a springy wobble, and Bounce rebounds off the end value.

  - prompt: Which commands can be eased?
    choices:
      - Only Move, MX and MY
      - Any command with values, such as Fade, Move, Scale, Rotate and Colour
      - Only Fade
    answer: 1
    explanation: Every command that goes from a start value to an end value can use an easing.

  - prompt: Why is Back Out (30) a risky choice for a fade from 0 to 1?
    choices:
      - It overshoots past 1, and opacity above 1 can flicker
      - It makes the fade take longer
      - Fade commands can't use easings above 20
    answer: 0
    explanation: Overshooting easings work well on movement, scale and rotation, but opacity can't go beyond 1.
---

Six questions on easing.
