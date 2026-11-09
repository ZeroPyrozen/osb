---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on timing a storyboard to music.
reviewed: 2026-11-09
questions:
  - prompt: How long is one beat at 150 BPM?
    choices:
      - 400 ms
      - 150 ms
      - 600 ms
    answer: 0
    outcome: beat-times
    explanation: 60000 ÷ 150 = 400 ms.

  - prompt: A song is 120 BPM with its first beat at 1200 ms. When is beat number 4, counting the first beat as 0?
    choices:
      - 2000 ms
      - 3200 ms
      - 4800 ms
    answer: 1
    outcome: beat-times
    explanation: 1200 + 4 × 500 = 3200 ms.

  - prompt: How long is a quarter of a beat at 120 BPM?
    choices:
      - 250 ms
      - 30 ms
      - 125 ms
    answer: 2
    outcome: beat-times
    explanation: A beat is 500 ms, so a quarter is 125 ms.

  - prompt: 'An uninherited timing point reads `2500,333.333333333333,4,2,0,70,1,0`. What does it tell you?'
    choices:
      - The song is 333 BPM and starts at 2.5 seconds
      - Beats start at 2500 ms, 333.33 ms apart, which is 180 BPM
      - There are 2500 beats
    answer: 1
    outcome: read-timing-points
    explanation: The first number is the time and the second the beat length. 60000 ÷ 333.33 = 180 BPM.

  - prompt: Where do big changes in a song usually land?
    choices:
      - On the first beat of a bar
      - Halfway between two beats
      - At random
    answer: 0
    outcome: match-the-song
    explanation: New sections and drops almost always start on a downbeat, which makes it the best spot for big storyboard changes.

  - prompt: A song has a quiet verse from 0 to 20000 ms, a build-up from 20000 ms and a drop into a kiai chorus at 28000 ms. Which plan follows its energy?
    choices:
      - Bright colours and fast particles from 0 ms, so the storyboard is exciting from start to finish
      - One pulse on every beat from start to finish, since being on the beat is what matters most
      - The biggest flash at 20000 ms, where the build-up starts, then a steady glow through the chorus
      - Dim, slow drifting in the verse, faster pulses through the build-up, and the brightest effects from 28000 ms
    answer: 3
    outcome: match-the-song
    explanation: Follow the song's shape. Stay calm while it's calm, build as it builds, and go biggest at the drop into the kiai chorus. Holding back in the verse is what makes the chorus feel huge.

  - prompt: When do the ranking criteria require an epilepsy warning?
    choices:
      - Whenever a storyboard uses colour
      - Only for storyboards longer than five minutes
      - When the storyboard or video has repetitive strobes, pulsing images or rapid changes in brightness or colour
    answer: 2
    outcome: epilepsy-warning
    explanation: Strobing at 3 flashes per second or slower is unlikely to cause concern. Beyond that, or when in doubt, add the warning.
---

Seven questions on beats, offsets and song structure.
