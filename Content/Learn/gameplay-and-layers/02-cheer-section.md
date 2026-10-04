---
title: "Exercise: cheer section"
type: exercise
minutes: 5
summary: Give passing and failing players their own version of the same scene.
exercise:
  duration: 3000
  starter: |
    // Hifumi cheers on players who are passing. Add a second Hifumi on
    // the Fail layer, at the same position, scale 0.6 and tinted grey
    // (120,120,120), visible for the same time.
    // Untick "Pass state" under the preview to see the Fail layer.
    Sprite,Pass,Centre,"sb/hifumi.png",320,260
     F,0,0,3000,1
     S,0,0,3000,0.8
  solution: |
    Sprite,Pass,Centre,"sb/hifumi.png",320,260
     F,0,0,3000,1
     S,0,0,3000,0.8
    Sprite,Fail,Centre,"sb/hifumi.png",320,260
     F,0,0,3000,1
     S,0,0,3000,0.6
     C,0,0,3000,120,120,120
  checks:
    - kind: object
      path: sb/hifumi.png
      layer: Pass
      label: Hifumi is still on the Pass layer
    - kind: object
      path: sb/hifumi.png
      layer: Fail
      x: 320
      y: 260
      label: A second Hifumi is on the Fail layer, at the same position
    - kind: value
      object: { layer: Fail }
      time: 1500
      property: scale
      equals: 0.6
      label: The Fail version is smaller (scale 0.6)
    - kind: value
      object: { layer: Fail }
      time: 1500
      property: color
      equals: [120, 120, 120]
      tolerance: 2
      label: and tinted grey (120, 120, 120)
    - kind: visible
      object: { layer: Fail }
      time: 0
      label: The Fail version is visible at 0 ms
    - kind: visible
      object: { layer: Fail }
      time: 3000
      label: and still visible at 3000 ms
  hints:
    - Copy the whole Pass sprite, both the Sprite line and its commands, then change the layer to Fail.
    - Change the scale to 0.6, and add a Colour command with 120,120,120 that lasts from 0 to 3000 ms.
---

Passing players see Hifumi cheering them on. Failing players should see her too, just **smaller and
greyed out**.

Add a second Hifumi on the **Fail** layer:

- at the **same position**,
- at **scale 0.6**,
- tinted **grey (120, 120, 120)**,
- visible from **0 to 3000 ms**.

Untick **Pass state** under the preview to see what failing players see.
