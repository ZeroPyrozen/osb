---
title: Knowledge check
type: quiz
minutes: 4
summary: Five questions on particles and procedural effects.
questions:
  - prompt: Why should particles fade in and out instead of simply appearing and disappearing?
    choices:
      - Fading is required by the ranking criteria
      - Particles without fades don't render
      - Popping in and out looks harsh. Short fades make the effect soft and natural
    answer: 2
    explanation: Even 200 or 300 ms of fade at each end makes a big difference.

  - prompt: Every particle in your effect starts at 0 ms. What will it look like?
    choices:
      - A clump at the start that thins out as particles finish
      - Evenly busy throughout
      - Exactly the same as spreading the start times out
    answer: 0
    explanation: Spread start times across the effect, or loop each particle, to keep the screen evenly busy.

  - prompt: What does giving each sprite in a row a slightly later start (a phase offset) do to a bobbing motion?
    choices:
      - It makes every sprite move in sync
      - It turns the row into a travelling wave
      - Nothing visible
    answer: 1
    explanation: Each sprite reaches its peak a little after the previous one, so the peak appears to travel along the row.

  - prompt: Why do spectrum bars usually use the `BottomCentre` origin?
    choices:
      - So they grow upwards from a fixed floor
      - It's the only origin Vector scale works with
      - To make them additive
    answer: 0
    explanation: Scaling happens around the origin, so a bottom origin keeps the base of each bar in place.

  - prompt: Which choice keeps a particle effect cheapest to draw?
    choices:
      - Hundreds of 512 × 512 images scaled down to look small
      - As few small images as give the look you want, each ending its life when it fades out
      - Leaving faded-out particles alive until the end of the song
    answer: 1
    explanation: Fewer, smaller sprites with short lives cost the least.
---

Five questions on particles and procedural effects.
