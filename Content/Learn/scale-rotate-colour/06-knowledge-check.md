---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on Scale, Vector scale, Rotate, Colour and parameters.
questions:
  - prompt: A 100 × 100 image has ` S,0,0,1000,1,2`. How big is it at 1000 ms?
    choices:
      - 102 × 102
      - 200 × 200
      - 200 × 100
    answer: 1
    explanation: Scale multiplies the image's own size, in both directions.

  - prompt: A 100 × 100 image has ` V,0,0,1000,1,1,2,0.5`. How big is it at 1000 ms?
    choices:
      - 200 wide and 50 tall
      - 50 wide and 200 tall
      - 100 × 100, because V only works with S
    answer: 0
    explanation: Vector scale's values are x then y, so the end scale is 2 across and 0.5 down.

  - prompt: Which rotation turns a sprite a quarter turn clockwise?
    choices:
      - '` R,0,0,1000,0,90`'
      - '` R,0,0,1000,0,1.571`'
      - '` R,0,0,1000,0,-1.571`'
    answer: 1
    explanation: Rotation is in radians, and a quarter turn is π ÷ 2 ≈ 1.571. Negative values turn anti-clockwise.

  - prompt: A bar uses the `BottomCentre` origin and rotates. Where is its hinge?
    choices:
      - Its centre
      - The middle of its bottom edge
      - The centre of the screen
    answer: 1
    explanation: Objects rotate and scale around their origin.

  - prompt: Why are so many storyboard images drawn in plain white?
    choices:
      - White images load faster
      - The Colour command multiplies colours, so white takes on any colour exactly
      - osu! can only tint white pixels
    answer: 1
    explanation: Colour multiplies each pixel's colour. White becomes exactly the chosen colour, and black stays black.

  - prompt: A sprite lives from 0 to 3000 ms and has ` P,0,1000,2000,H`. Is it flipped at 2500 ms?
    choices:
      - Yes. The flip stays on, like other commands' end values
      - No. Parameters only apply while their command is running
      - Only in the editor
    answer: 1
    explanation: Parameters switch off when their command ends. To keep one for the object's whole life, give it the same start and end time.

  - prompt: What does additive blending (` P,0,1000,,A`) do?
    choices:
      - Adds the sprite's colours to what's behind it, so it brightens and black parts vanish
      - Adds a second copy of the sprite
      - Makes the sprite draw on top of every other layer
    answer: 0
    explanation: Additive sprites can only brighten what's behind them, which makes them ideal for glows and light effects.
---

Seven questions on transforming sprites.
