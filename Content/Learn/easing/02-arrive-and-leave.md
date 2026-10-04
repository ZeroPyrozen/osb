---
title: "Exercise: arrive and leave"
type: exercise
minutes: 5
summary: Pick the right direction of easing for an entrance and an exit.
exercise:
  duration: 3000
  starter: |
    // The note glides in from the left and settles in the centre
    // (0 to 1000 ms), waits, then flies off to the right
    // (2000 to 3000 ms). Change the easings so the entrance
    // slows down as it arrives and the exit speeds up as it leaves.
    Sprite,Foreground,Centre,"sb/note.png",-140,240
     MX,0,0,1000,-140,320
     MX,0,2000,3000,320,780
  solution: |
    Sprite,Foreground,Centre,"sb/note.png",-140,240
     MX,7,0,1000,-140,320
     MX,6,2000,3000,320,780
  checks:
    - kind: value
      time: 0
      property: x
      max: -131
      label: It starts completely off the left edge
    - kind: value
      time: 500
      property: x
      min: 150
      label: Halfway through the entrance it has already covered most of the way (it starts fast)
    - kind: value
      time: 1000
      property: x
      equals: 320
      tolerance: 1
      label: It arrives in the centre at 1000 ms
    - kind: value
      time: 2000
      property: x
      equals: 320
      tolerance: 1
      label: It waits in the centre until 2000 ms
    - kind: value
      time: 2500
      property: x
      max: 500
      label: Halfway through the exit it has covered less than half the way (it starts slowly)
    - kind: value
      time: 3000
      property: x
      min: 771
      label: At 3000 ms it's completely off the right edge
  hints:
    - Arriving calls for an Out easing, which starts fast and slows down. Leaving calls for an In easing, which starts slowly and speeds up.
    - "The original pair works: 1 is Easing Out and 2 is Easing In. Cubic (7 and 6) or Quint (13 and 12) feel stronger."
---

The note already moves at the right times, but with linear easing it slides in and out like it's on
a conveyor belt. Change the two easings so that:

- the **entrance** (0 to 1000 ms) **slows down** as the note arrives in the centre, and
- the **exit** (2000 to 3000 ms) **speeds up** as it leaves.

Any easing of the right direction will do, so try a few and keep the one that feels best. This preview
is widescreen, so the edges are at x = −107 and x = 747.
