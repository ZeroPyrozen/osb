---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on easing.
questions:
  - prompt: In ` M,7,0,1000,0,240,320,240`, what is `7`?
    choices:
      - The layer
      - The easing, Cubic Out
      - The number of times the move repeats
    answer: 1
    explanation: The easing always comes right after the command type.

  - prompt: Does choosing a different easing change when a command ends?
    choices:
      - No. Only the pace between the start and end changes
      - Yes, strong easings take longer
      - Only for Bounce and Elastic
    answer: 0
    explanation: The start and end times and values stay the same. Easing only shapes the journey in between.

  - prompt: An object slides onto the screen and stops in the middle. Which kind of easing usually feels best?
    choices:
      - An In easing, which starts slowly and speeds up
      - Linear
      - An Out easing, which starts fast and slows down
    answer: 2
    explanation: Things that arrive should slow down as they settle, which is what Out easings do.

  - prompt: Which family travels past its end value and then settles back?
    choices:
      - Back
      - Sine
      - Expo
    answer: 0
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
      - It makes the fade take longer
      - Fade commands can't use easings above 20
      - It overshoots past 1, and opacity above 1 wraps around to nearly invisible
    answer: 2
    explanation: Back Out goes past 1 near the end. In osu!stable, opacity above 1 wraps around (1.1 shows as about 0.1), so the sprite blinks out just before it settles. Overshooting easings suit movement, scale and rotation.
---

Six questions on easing.
