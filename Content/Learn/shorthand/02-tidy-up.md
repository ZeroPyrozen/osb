---
title: "Exercise: tidy up"
type: exercise
minutes: 7
summary: Rewrite a long-winded script so it looks exactly the same in half the lines.
exercise:
  duration: 3000
  starter: |
    // This storyboard works, but it's long-winded. Rewrite it so it
    // looks exactly the same, using at most 5 command lines.
    Sprite,Foreground,Centre,"sb/circle.png",320,240
     S,0,0,0,1,1
     F,0,0,500,0,1
     F,0,500,2500,1,1
     F,0,2500,3000,1,0
     C,0,0,1000,255,90,90,255,90,90
     C,0,1000,1000,90,200,255,90,200,255
     M,0,0,1500,200,240,440,240
     M,0,1500,3000,440,240,440,240
  solution: |
    Sprite,Foreground,Centre,"sb/circle.png",320,240
     F,0,0,500,0,1
     F,0,2500,3000,1,0
     C,0,0,,255,90,90
     C,0,1000,,90,200,255
     M,0,0,1500,200,240,440,240
  checks:
    - kind: matches-solution
      label: It looks exactly the same as before
      message: Compare frame by frame with the original. Something now appears, moves or changes colour differently.
    - kind: count
      of: command-lines
      max: 5
      label: It uses at most 5 command lines (the original has 8)
  hints:
    - Start with lines that change nothing. A scale of 1 is the default, and a command whose start and end values are equal often just repeats what the previous command left behind.
    - A command with the same start and end time can leave its end time empty, and a value that doesn't change only needs writing once.
    - "Careful with the last Move: it holds the circle at 440, which it would do anyway, but does anything else keep the circle alive until 3000 ms?"
---

The circle fades in, turns from red to blue, slides to the right and fades out. The script is
correct, but it's twice as long as it needs to be.

Rewrite it so that it **looks exactly the same** using **at most 5 command lines**. The check compares
your version with the original frame by frame, so everything has to happen at the same moments.

Some lines can be shortened with shorthand, and some aren't needed at all. Avoid the sequence shorthand,
since osu!(lazer) can't read it.
