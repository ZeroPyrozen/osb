---
title: "Exercise: drop and bounce"
type: exercise
minutes: 5
summary: Let a ball fall onto the floor and bounce with Bounce Out.
exercise:
  duration: 2000
  starter: |
    // The floor: a stretched pixel at y = 400.
    Sprite,Background,TopCentre,"sb/pixel.png",320,400
     V,0,0,2000,854,2
     C,0,0,2000,144,169,163
    // Drop the ball! From 0 to 1500 ms it falls from y = -60 to the
    // floor at y = 400, bouncing as it lands. Then it rests there
    // until 2000 ms.
    Sprite,Foreground,BottomCentre,"sb/circle.png",320,400
     S,0,0,2000,0.5
     MY,0,0,1500,-60,400
  solution: |
    Sprite,Background,TopCentre,"sb/pixel.png",320,400
     V,0,0,2000,854,2
     C,0,0,2000,144,169,163
    Sprite,Foreground,BottomCentre,"sb/circle.png",320,400
     S,0,0,2000,0.5
     MY,33,0,1500,-60,400
  checks:
    - kind: value
      object: { path: sb/circle.png }
      time: 0
      property: y
      equals: -60
      tolerance: 1
      label: The ball starts above the screen, at y = -60
    - kind: command
      object: { path: sb/circle.png }
      easing: 33
      start: 0
      end: 1500
      label: Its fall uses Bounce Out (33), from 0 to 1500 ms
    - kind: value
      object: { path: sb/circle.png }
      time: 1500
      property: y
      equals: 400
      tolerance: 1
      label: It comes to rest on the floor at 1500 ms
    - kind: value
      object: { path: sb/circle.png }
      time: 2000
      property: y
      equals: 400
      tolerance: 1
      label: It's still on the floor at 2000 ms
    - kind: visible
      object: { path: sb/circle.png }
      time: 2000
      label: It's still visible at 2000 ms
  hints:
    - Bounce Out is easing 33. The easing is the number right after the command type.
---

The ball falls at a steady speed and stops dead on the floor, which looks like it's made of lead. Give
its fall the **Bounce Out** easing so it bounces when it lands, without changing the times or
positions.

The ball uses the `BottomCentre` origin, so its y position is where its bottom edge touches. That keeps
it sitting neatly on the floor at y = 400.
