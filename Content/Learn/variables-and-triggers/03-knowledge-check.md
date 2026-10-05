---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on variables and triggers.
questions:
  - prompt: 'With `$pink=255,120,170` in `[Variables]`, what does osu! read for ` C,0,0,1000,$pink`?'
    choices:
      - '` C,0,0,1000,255,120,170`'
      - A colour command with the colour named "pink"
      - An error, because variables can't hold several numbers
    answer: 0
    explanation: Variables are plain text replacements, so a variable can hold any piece of a line.

  - prompt: Where do storyboard variables work?
    choices:
      - In .osb files only
      - In .osu files only
      - Anywhere in the beatmap folder
    answer: 0
    explanation: The [Variables] section is only supported in .osb files.

  - prompt: Why is `$count=4` a risky variable?
    choices:
      - Variable names can't contain the letter t
      - When the editor saves, it may replace every 4 in the file with $count
      - Numbers can't be stored in variables
    answer: 1
    explanation: Saving replaces every occurrence of a variable's value with its name. Short values like 4 appear all over a storyboard.

  - prompt: Inside a trigger, what are command times measured from?
    choices:
      - The start of the song
      - The trigger's start time
      - The moment the trigger fires
    answer: 2
    explanation: Each time the trigger fires, its commands play from 0 relative to that moment.

  - prompt: Which trigger fires when the player switches from the Pass state to the Fail state?
    choices:
      - Passing
      - Failing
      - HitSoundFinish
    answer: 1
    explanation: Failing fires on the change into the Fail state, and Passing on the change back.

  - prompt: Why can't you test triggers in this site's previews?
    choices:
      - Triggers only work on the Overlay layer
      - The previews don't run gameplay, so nothing can fire a trigger
      - Triggers are deprecated
    answer: 1
    explanation: Triggers depend on hitsounds and the player's state, so test them by playing the beatmap in osu!.
---

Six questions on variables and triggers.
