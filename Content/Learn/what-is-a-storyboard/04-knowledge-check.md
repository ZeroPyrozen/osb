---
title: Knowledge check
type: quiz
minutes: 4
summary: Six questions on storyboards, files, coordinates and time.
questions:
  - prompt: Where can a beatmap's storyboard be stored?
    choices:
      - Only in the beatmap's background image
      - In a separate `.osb` file, or in the `[Events]` section of a `.osu` file
      - Only in the `[Events]` section of the `.osu` file
      - Inside the audio file
    answer: 1
    explanation: A `.osb` file is shared by every difficulty; the `[Events]` section of a `.osu` file belongs to that difficulty alone. You can use both.

  - prompt: You put your storyboard in the beatmapset's `.osb` file. Which difficulties show it?
    choices:
      - Only the hardest one
      - Only the one that was open in the editor when you saved
      - Every difficulty of the beatmapset
    answer: 2
    explanation: The `.osb` file belongs to the whole beatmapset, so every difficulty shows it.

  - prompt: Which position is the centre of the 640 × 480 storyboard canvas?
    choices:
      - "(0, 0)"
      - "(320, 240)"
      - "(427, 240)"
      - "(640, 480)"
    answer: 1
    explanation: "(0, 0) is the top-left corner, so the centre is half of 640 and half of 480: (320, 240)."

  - prompt: With Widescreen support turned on, which x positions are visible on a 16:9 screen?
    choices:
      - 0 to 640
      - −107 to 747
      - 0 to 854
      - −320 to 960
    answer: 1
    explanation: The widescreen area is 854 osu! pixels wide and centred on the 640 canvas, adding 107 pixels on each side.

  - prompt: A command starts at time `2500`. When does it start?
    choices:
      - 2.5 seconds after the beatmap starts loading
      - 2.5 seconds after the start of the song's audio file
      - On the 2500th frame
      - On beat 2500
    answer: 1
    explanation: Storyboard times are milliseconds from the start of the audio file. Negative times play before the music starts.

  - prompt: Which of these can a storyboard do that a background video can't?
    choices:
      - Show different scenes depending on whether the player is passing or failing
      - Play sound
      - Be longer than the song
    answer: 0
    explanation: Videos play the same way every time. Storyboards can react to the player's state and to hitsounds.
---

Check what you've learned about storyboards and the stage they're drawn on.
