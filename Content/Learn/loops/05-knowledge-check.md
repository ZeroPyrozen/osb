---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on loops.
questions:
  - prompt: In ` L,2000,4`, what does `4` mean?
    choices:
      - The commands play 4 times in total
      - The commands play once, then repeat 4 more times
      - Each pass lasts 4 seconds
    answer: 0
    explanation: The loop count is the total number of passes.

  - prompt: Inside a loop, what are command times measured from?
    choices:
      - The start of the song
      - The start of each pass
      - The end of the previous loop
    answer: 1
    explanation: Times inside a loop are relative, so the same commands can play again in every pass.

  - prompt: 'A loop contains ` F,0,0,300,0,1` and ` F,0,300,600,1,0`. How long is each pass?'
    choices:
      - 300 ms
      - 600 ms
      - 900 ms
    answer: 1
    explanation: A pass runs from the earliest start (0) to the latest end (600) of the commands inside.

  - prompt: 'A loop contains only ` F,0,0,200,1,0`, but you wanted 300 ms of darkness after each flash. What''s wrong?'
    choices:
      - Nothing. osu! adds the pause automatically
      - Each pass is only 200 ms long, so there's no pause. A command must cover the rest of the pass
      - The loop count is too low
    answer: 1
    explanation: Pauses only exist if a command covers them, for example ` F,0,200,500,0`.

  - prompt: Can a loop contain another loop?
    choices:
      - Yes, as deep as you like
      - Only one level deep
      - No. Loops and triggers can't be nested
    answer: 2
    explanation: osu! doesn't support nesting. Use separate loops one after another instead.

  - prompt: Why do the ranking criteria recommend loops for repeating motion?
    choices:
      - They make the storyboard play faster
      - They keep the file much smaller, since the commands are written once
      - Loops are required for every storyboard
    answer: 1
    explanation: Writing repetitions out by hand bloats the file, and loops avoid that.
---

Six questions on loops.
