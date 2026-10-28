---
title: Knowledge check
type: quiz
minutes: 3
summary: Five questions on shorthand.
questions:
  - prompt: Which line means the same as ` S,0,1000,2000,1.5,1.5`?
    choices:
      - '` S,0,1000,,1.5`'
      - '` S,0,1000,2000,1.5`'
      - '` S,0,1000,2000`'
    answer: 1
    explanation: When the start and end values are the same, you write the value once. Leaving out the end time would make it an instant command instead.

  - prompt: What does ` C,0,1000,,255,0,0` do?
    choices:
      - Turns the object red at 1000 ms, and it stays red until another Colour command
      - Fades to red over one second
      - Nothing, because the end time is missing
    answer: 0
    explanation: An empty end time makes the command instant, and its value holds afterwards.

  - prompt: How does osu!stable read ` M,0,0,500,0,0,100,0,100,100`?
    choices:
      - As one move from (0, 0) to (100, 100)
      - As an error
      - As two moves, 0 to 500 ms from (0, 0) to (100, 0), then 500 to 1000 ms from (100, 0) to (100, 100)
    answer: 2
    explanation: Extra values make a sequence. Each step lasts as long as the first, so the second step runs from 500 to 1000 ms.

  - prompt: What does osu!(lazer) currently do with a sequence like ` F,0,0,500,0,1,0`?
    choices:
      - Plays only the first step and ignores the remaining values
      - Plays every step, like osu!stable
      - Refuses to load the storyboard
    answer: 0
    explanation: Lazer reads only the first start and end values, so write steps on separate lines if they matter.

  - prompt: Does using shorthand change how a storyboard plays?
    choices:
      - Yes, shorthand commands always run faster
      - No. It only makes the file shorter
      - Yes, shorthand turns off easing
    answer: 1
    explanation: Shorthand is just a shorter way of writing the same commands.
---

Five questions on writing commands the short way.
