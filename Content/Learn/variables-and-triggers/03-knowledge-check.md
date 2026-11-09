---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on variables and triggers.
questions:
  - prompt: 'With `$pink=255,120,170` in `[Variables]`, what does osu! read for ` C,0,0,1000,$pink`?'
    choices:
      - A colour command with the colour named "pink"
      - '` C,0,0,1000,255,120,170`'
      - An error, because variables can't hold several numbers
    answer: 1
    outcome: use-variables
    explanation: Variables are plain text replacements, so a variable can hold any piece of a line.

  - prompt: Where do storyboard variables work?
    choices:
      - In .osb files only
      - In .osu files only
      - Anywhere in the beatmap folder
    answer: 0
    outcome: use-variables
    explanation: The [Variables] section is only supported in .osb files.

  - prompt: Why is `$count=4` a risky variable?
    choices:
      - Variable names can't contain the letter t
      - When the editor saves, it may replace every 4 in the file with $count
      - Numbers can't be stored in variables
    answer: 1
    outcome: variable-traps
    explanation: Saving replaces every occurrence of a variable's value with its name. Short values like 4 appear all over a storyboard.

  - prompt: Inside a trigger, what are command times measured from?
    choices:
      - The start of the song
      - The trigger's start time
      - The moment the trigger fires
    answer: 2
    outcome: write-triggers
    explanation: Each time the trigger fires, its commands play from 0 relative to that moment.

  - prompt: Which trigger fires when the player switches from the Pass state to the Fail state?
    choices:
      - Failing
      - Passing
      - HitSoundFinish
    answer: 0
    outcome: write-triggers
    explanation: Failing fires on the change into the Fail state, and Passing on the change back.

  - prompt: A glow has ` T,HitSoundClap,10000,40000` with ` F,0,0,300,0.8,0` inside. Two claps play 100 ms apart. What happens?
    choices:
      - The flash starts over at the second clap, so the glow is back at 0.8 and fades out over the next 300 ms
      - Two flashes play on top of each other, so the glow gets brighter
      - The second clap is ignored, because the first flash is still playing
      - The glow flashes once, because a trigger only fires the first time
    answer: 0
    outcome: trigger-restarts
    explanation: A trigger that fires while its commands are still playing stops them and starts again from the beginning. Triggers on the same sprite with the same group number stop each other in the same way.

  - prompt: Why can't you test triggers in this site's previews?
    choices:
      - Triggers only work on the Overlay layer
      - Triggers are deprecated
      - The previews don't run gameplay, so nothing can fire a trigger
    answer: 2
    outcome: test-triggers
    explanation: Triggers depend on hitsounds and the player's state, so test them by playing the beatmap in osu!.
---

Seven questions on variables and triggers.
