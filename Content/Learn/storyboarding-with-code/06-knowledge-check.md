---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on generating storyboards with code.
questions:
  - prompt: What does a storyboard generator produce?
    choices:
      - A special file that only storybrew can play
      - An ordinary .osb file that osu! reads like any other
      - A video
    answer: 1
    explanation: osu! neither knows nor cares how a .osb file was made.

  - prompt: In script mode, what does `dot.scale(1000, 2)` write?
    choices:
      - '` S,0,1000,2`'
      - '` S,0,1000,,2`, an instant change to scale 2 at 1000 ms'
      - A scale from 1000 to 2
    answer: 1
    explanation: The (time, value) form makes an instant command, written with the empty end time shorthand.

  - prompt: How do you give a script-mode command an easing?
    choices:
      - Put the easing's name first, like `dot.move('OutBack', 0, 500, …)`
      - Call `dot.easing(30)` before the command
      - Script mode can't use easings
    answer: 0
    explanation: An optional easing name comes before the times, just like the easing number does in a command line.

  - prompt: Why are the random numbers in script mode seeded?
    choices:
      - To make them more random
      - So the script draws the same storyboard every time it runs
      - Seeding makes scripts run faster
    answer: 1
    explanation: Without a seed, every edit would reshuffle every random particle.

  - prompt: Where does a point at angle `a` on a circle of radius `r` around (320, 240) sit?
    choices:
      - (320 + r × cos(a), 240 + r × sin(a))
      - (320 + r × a, 240 + r × a)
      - (320 × cos(a), 240 × sin(a))
    answer: 0
    explanation: Cosine gives the horizontal part and sine the vertical part. Because y grows downwards, growing angles go clockwise.

  - prompt: In storybrew, what does `[Configurable]` do?
    choices:
      - Makes a field show up as a setting in storybrew's interface
      - Exports the field to the .osb file
      - Marks the field as a variable for the [Variables] section
    answer: 0
    explanation: Configurable fields let you tweak an effect without editing its code.
---

Six questions on storyboarding with code.
