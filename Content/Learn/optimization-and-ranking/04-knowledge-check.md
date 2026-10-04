---
title: Knowledge check
type: quiz
minutes: 5
summary: Seven questions on performance and the ranking criteria. The last knowledge check!
questions:
  - prompt: A sprite has faded to opacity 0 but has another command much later. Does it still cost performance in between?
    choices:
      - No, invisible sprites are free
      - Yes. Active sprites are processed whether they're visible or not
      - Only on the Overlay layer
    answer: 1
    explanation: End sprites when they're done, and use a new sprite if the image comes back much later.

  - prompt: What is the largest area a storyboard image may have in a ranked beatmap?
    choices:
      - 1920 × 1080 pixels
      - 17,000,000 pixels
      - There's no limit
    answer: 1
    explanation: That's roughly 4100 × 4100. Larger images take too long to load.

  - prompt: A storyboard flashes the whole screen five times a second during the chorus. What do the ranking criteria require?
    choices:
      - Nothing, as long as it's on beat
      - An epilepsy warning
      - Moving the flashes to the Overlay layer
    answer: 1
    explanation: Repetitive strobes need an epilepsy warning. Flashing at 3 Hz or slower is unlikely to cause concern.

  - prompt: Which of these is a ranking rule rather than a guideline?
    choices:
      - The difficulty must load without parsing errors
      - Leave a one-pixel transparent border around rotating images
      - Leave at least 16 ms between commands of the same type
    answer: 0
    explanation: Parsing errors are never allowed. The other two are good advice that can be broken in exceptional cases.

  - prompt: Two Move commands on the same sprite overlap in time. What do the guidelines say?
    choices:
      - That's fine, the later one wins
      - Adjust them so they no longer overlap
      - Convert them into a trigger
    answer: 1
    explanation: Overlapping commands of the same type count as conflicting commands.

  - prompt: Which image format is usually best for a large, fully opaque background?
    choices:
      - JPG
      - PNG
      - It makes no difference
    answer: 0
    explanation: JPG is much smaller for photos and large opaque images. Keep PNG for images that need transparency.

  - prompt: Which tool does the ranking criteria recommend for checking a beatmap against many of its rules?
    choices:
      - Mapset Verifier
      - storybrew
      - A spreadsheet
    answer: 0
    explanation: Mapset Verifier automates many checks, but it doesn't replace reading the criteria yourself.
---

The final knowledge check: seven questions on performance and ranking.
