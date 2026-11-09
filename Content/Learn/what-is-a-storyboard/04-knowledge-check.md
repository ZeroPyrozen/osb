---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on storyboards, how they're made, files, coordinates and time.
questions:
  - prompt: Where can a beatmap's storyboard be stored?
    choices:
      - Only in the beatmap's background image
      - In a separate `.osb` file, or in the `[Events]` section of a `.osu` file
      - Only in the `[Events]` section of the `.osu` file
      - Inside the audio file
    answer: 1
    outcome: find-storyboard-files
    explanation: A `.osb` file is shared by every difficulty; the `[Events]` section of a `.osu` file belongs to that difficulty alone. You can use both.

  - prompt: You put your storyboard in the beatmapset's `.osb` file. Which difficulties show it?
    choices:
      - Every difficulty of the beatmapset
      - Only the hardest one
      - Only the one that was open in the editor when you saved
    answer: 0
    outcome: find-storyboard-files
    explanation: The `.osb` file belongs to the whole beatmapset, so every difficulty shows it.

  - prompt: You want 500 stars that each twinkle at a different moment through a two-minute chorus. Which way of making a storyboard fits this job best?
    choices:
      - Scripting by hand, typing each star's commands into the `.osb` file
      - A generator, a program that writes each star's commands for you
      - The Design tab, placing and animating each star with the mouse
    answer: 1
    outcome: compare-ways
    explanation: Hundreds of stars with their own timing need thousands of commands, which is the job generators are for. The Design tab suits quick, simple ideas, and hand scripting suits effects where you want exact control over every value.

  - prompt: Which position is the centre of the 640 × 480 storyboard canvas?
    choices:
      - "(0, 0)"
      - "(427, 240)"
      - "(320, 240)"
      - "(640, 480)"
    answer: 2
    outcome: locate-on-canvas
    explanation: "(0, 0) is the top-left corner, so the centre is half of 640 and half of 480: (320, 240)."

  - prompt: With Widescreen support turned on, which x positions are visible on a 16:9 screen?
    choices:
      - 0 to 640
      - 0 to 854
      - −320 to 960
      - −107 to 747
    answer: 3
    outcome: locate-on-canvas
    explanation: The widescreen area is 854 osu! pixels wide and centred on the 640 canvas, adding 107 pixels on each side.

  - prompt: A command starts at time `2500`. When does it start?
    choices:
      - 2.5 seconds after the beatmap starts loading
      - On the 2500th frame
      - 2.5 seconds after the start of the song's audio file
      - On beat 2500
    answer: 2
    outcome: read-times
    explanation: Storyboard times are milliseconds from the start of the audio file. Negative times play before the music starts.

  - prompt: Which of these can a storyboard do that a background video can't?
    choices:
      - Show different scenes depending on whether the player is passing or failing
      - Play sound
      - Be longer than the song
    answer: 0
    outcome: explain-storyboards
    explanation: Videos play the same way every time. Storyboards can react to the player's state and to hitsounds.
---

Check what you've learned about storyboards and the stage they're drawn on.
