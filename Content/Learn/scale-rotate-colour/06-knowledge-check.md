---
title: Knowledge check
type: quiz
minutes: 4
summary: Eight questions on Scale, Vector scale, Rotate, sharp images, Colour and parameters.
reviewed: 2026-11-09
questions:
  - prompt: A 100 × 100 image has ` S,0,0,1000,1,2`. How big is it at 1000 ms?
    choices:
      - 200 × 200
      - 102 × 102
      - 200 × 100
    answer: 0
    outcome: scale-objects
    explanation: Scale multiplies the image's own size, in both directions.

  - prompt: A 100 × 100 image has ` V,0,0,1000,1,1,2,0.5`. How big is it at 1000 ms?
    choices:
      - 50 wide and 200 tall
      - 100 × 100, because V only works with S
      - 200 wide and 50 tall
    answer: 2
    outcome: scale-objects
    explanation: Vector scale's values are x then y, so the end scale is 2 across and 0.5 down.

  - prompt: Which rotation turns a sprite a quarter turn clockwise?
    choices:
      - '` R,0,0,1000,0,90`'
      - '` R,0,0,1000,0,1.571`'
      - '` R,0,0,1000,0,-1.571`'
    answer: 1
    outcome: rotate-objects
    explanation: Rotation is in radians, and a quarter turn is π ÷ 2 ≈ 1.571. Negative values turn anti-clockwise.

  - prompt: A bar uses the `BottomCentre` origin and rotates. Where is its hinge?
    choices:
      - The middle of its bottom edge
      - Its centre
      - The centre of the screen
    answer: 0
    outcome: rotate-objects
    explanation: Objects rotate and scale around their origin.

  - prompt: A logo will spin slowly, shown 240 osu! pixels tall. Most players have 1080p screens. Which image should you prepare?
    choices:
      - 240 × 240 pixels with a one-pixel transparent border, shown at scale 1
      - About 540 × 540 pixels cropped right to the logo's edges, shown at scale 0.444
      - 9000 × 9000 pixels, so it stays sharp on any screen
      - About 540 × 540 pixels with a one-pixel transparent border, shown at scale 0.444
    answer: 3
    outcome: prepare-images
    explanation: A 1080p screen draws the canvas 2.25 times larger, so a logo shown 240 tall needs about 540 pixels to stay sharp, scaled down by 480 ÷ 1080 ≈ 0.444. The transparent border keeps its edges smooth as it turns. 9000 × 9000 is 81,000,000 pixels, which loads slowly and breaks the 17,000,000 limit.

  - prompt: Why are so many storyboard images drawn in plain white?
    choices:
      - White images load faster
      - osu! can only tint white pixels
      - The Colour command multiplies colours, so white takes on any colour exactly
    answer: 2
    outcome: colour-tinting
    explanation: Colour multiplies each pixel's colour. White becomes exactly the chosen colour, and black stays black.

  - prompt: A sprite lives from 0 to 3000 ms and has ` P,0,1000,2000,H`. Is it flipped at 2500 ms?
    choices:
      - No. Parameters only apply while their command is running
      - Yes. The flip stays on, like other commands' end values
      - Only in the editor
    answer: 0
    outcome: use-parameters
    explanation: Parameters switch off when their command ends. To keep one for the object's whole life, give it the same start and end time.

  - prompt: What does additive blending (` P,0,1000,,A`) do?
    choices:
      - Adds a second copy of the sprite
      - Adds the sprite's colours to what's behind it, so it brightens and black parts vanish
      - Makes the sprite draw on top of every other layer
    answer: 1
    outcome: use-parameters
    explanation: Additive sprites can only brighten what's behind them, which makes them ideal for glows and light effects.
---

Eight questions on transforming sprites.
