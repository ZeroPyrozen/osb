---
title: "Exercise: a clock face"
type: exercise
minutes: 10
summary: Write a function that finds points on a circle, then use it to draw the twelve marks of a clock.
outcomes: [variables-and-arithmetic, for-loops, functions, arrays, points-on-a-circle]
exercise:
  mode: script
  duration: 2000
  starter: |
    // Draw the 12 hour marks of a clock face, on a circle of radius 160
    // around (320, 240). Mark 0 is at the top, where 12 is, and the others
    // go round clockwise: mark i is at the angle i * 2π / 12, minus a
    // quarter turn (Math.PI / 2). Mark i fades in from i * 100 ms to
    // i * 100 + 200 ms, and every mark stays visible until 2000 ms.
    const layer = sb.layer('Foreground');
    const radius = 160;

    // Finish this function: it should return the [x, y] point on the
    // circle at this angle.
    function pointOnCircle(angle) {
        return [320, 240];
    }

    const point = pointOnCircle(0);
    const mark = layer.sprite('sb/dot.png', 'Centre', point[0], point[1]);
    mark.fade(0, 200, 0, 1);
    mark.fade(200, 2000, 1, 1);
  solution: |
    const layer = sb.layer('Foreground');
    const radius = 160;

    function pointOnCircle(angle) {
        return [320 + radius * Math.cos(angle), 240 + radius * Math.sin(angle)];
    }

    for (let i = 0; i < 12; i++) {
        const angle = i * 2 * Math.PI / 12 - Math.PI / 2;
        const point = pointOnCircle(angle);
        const start = i * 100;
        const mark = layer.sprite('sb/dot.png', 'Centre', point[0], point[1]);
        mark.fade(start, start + 200, 0, 1);
        mark.fade(start + 200, 2000, 1, 1);
    }
  checks:
    - kind: count
      of: sprites
      equals: 12
      label: There are 12 marks
    - kind: object
      index: 0
      x: 320
      y: 80
      tolerance: 1
      label: Mark 0 is at the top, at (320, 80)
    - kind: object
      index: 1
      x: 400
      y: 101.44
      tolerance: 1
      label: Mark 1 is at one o'clock, near (400, 101)
    - kind: object
      index: 3
      x: 480
      y: 240
      tolerance: 1
      label: Mark 3 is on the right, at (480, 240)
    - kind: object
      index: 6
      x: 320
      y: 400
      tolerance: 1
      label: Mark 6 is at the bottom, at (320, 400)
    - kind: object
      index: 9
      x: 160
      y: 240
      tolerance: 1
      label: Mark 9 is on the left, at (160, 240)
    - kind: value
      object: { index: 11 }
      time: 1100
      property: opacity
      equals: 0
      label: The last mark starts fading in at 1100 ms
    - kind: value
      object: { index: 11 }
      time: 1300
      property: opacity
      equals: 1
      label: and has faded in by 1300 ms
    - kind: visible
      time: 2000
      every: true
      label: Every mark is visible at 2000 ms
  hints:
    - "Start with the function. The point is the centre plus radius × cos(angle) across and radius × sin(angle) down, returned together as one array."
    - "return [320 + radius * Math.cos(angle), 240 + radius * Math.sin(angle)]; To check it, log(pointOnCircle(0)) should print [480,240]."
    - "Then put the mark in a loop, for (let i = 0; i < 12; i++) { … }, and work out each mark's angle and start time from i: i * 2 * Math.PI / 12 - Math.PI / 2, and i * 100."
---

Draw the **12 hour marks** of a clock face:

- the marks sit on a circle of radius **160** around **(320, 240)**,
- mark *i* (counting from 0) is at the angle *i* × 2π ÷ 12, **minus a quarter turn**, so mark 0 is at the
  **top**, where 12 is, and the others go round clockwise,
- mark *i* **fades in** from *i* × 100 ms to *i* × 100 + 200 ms,
- every mark stays **visible until 2000 ms**.

Finish the `pointOnCircle` function first, then use it in a loop. Without the quarter turn, mark 0 would
sit on the right, where three o'clock is.
