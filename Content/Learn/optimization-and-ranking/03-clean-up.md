---
title: "Exercise: clean-up crew"
type: exercise
minutes: 8
summary: Optimise a wasteful storyboard without changing a single visible frame.
exercise:
  duration: 3000
  starter: |
    // This storyboard looks right, but it's wasteful. Clean it up
    // without changing how it looks:
    // - nothing should stay active after it has faded out,
    // - don't use MX and MY together when one command will do,
    // - remove commands that don't change anything.
    Sprite,Background,Centre,"bg.jpg",320,240
     F,0,0,500,0,1
     F,0,500,2500,1
     F,0,2500,3000,1,0
     S,0,0,,1
    Sprite,Foreground,Centre,"sb/star.png",320,240
     MX,0,0,2000,100,540
     MY,0,0,2000,240,240
     F,0,0,500,0,1
     F,0,1500,2000,1,0
     R,0,2000,10000,0,0
  solution: |
    Sprite,Background,Centre,"bg.jpg",320,240
     F,0,0,500,0,1
     F,0,2500,3000,1,0
    Sprite,Foreground,Centre,"sb/star.png",320,240
     M,0,0,2000,100,240,540,240
     F,0,0,500,0,1
     F,0,1500,2000,1,0
  checks:
    - kind: matches-solution
      label: It looks exactly the same as before
      message: Step through it with the time slider and compare with the original. Something now looks or moves differently.
    - kind: lifetime
      endsBy: 3000
      every: true
      label: Nothing stays active after 3000 ms
    - kind: count
      of: commands
      command: MY
      max: 0
      label: No separate MY command
    - kind: count
      of: command-lines
      max: 5
      label: At most 5 command lines (the original has 10)
  hints:
    - The star fades out at 2000 ms, but one of its commands keeps it active until 10000 ms without changing anything you can see.
    - The MY command keeps y at 240, which the Sprite line already sets. Either drop it, or merge both into one Move.
    - Look for commands that repeat a value that's already there, like a Fade that holds the opacity at 1 or a Scale of 1.
---

This storyboard **looks** right, but it wastes work. Clean it up **without changing a single visible
frame**:

- **Nothing stays active** after it has faded out.
- **No MX and MY pair** where one command (or none) does the job.
- **No commands that change nothing.**

The first check compares your version with the original frame by frame, so you can experiment freely:
if you break something, it'll tell you.
