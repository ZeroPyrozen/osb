---
title: Storyboard sounds
type: lesson
minutes: 4
summary: The Sample line, layers for sounds, volume, and what storyboard sounds shouldn't be used for.
---

Storyboards can play sounds too. A sound is declared with a **Sample** line, on its own like a Sprite,
with no commands underneath:

```
Sample,time,layer,"file",volume
```

| Part | Meaning |
| :-- | :-- |
| time | When the sound starts, in milliseconds. |
| layer | A number: 0 Background, 1 Fail, 2 Pass, 3 Foreground. |
| file | A WAV, MP3 or OGG file, relative to the beatmap folder, just like images. |
| volume | From 1 to 100. Leave it off for 100. |

Sounds don't overlap or cover each other like images do; they're all mixed together. The layer only
decides **whether** a sound plays. Sounds on layer 1 play only for failing players and sounds on layer 2
only for passing ones, so you can give each ending its own audio:

```osb
Sample,0,0,"sfx/intro-whoosh.ogg",70
Sample,95000,2,"sfx/cheering.ogg",100
Sample,95000,1,"sfx/rain.ogg",80
```

Because samples aren't commands, they can't go inside loops or triggers.

::: note
The previews on this site don't play storyboard sounds. Test them in osu! itself.
:::

## Use sounds with care

Players rely on **hitsounds** to feel the rhythm: they play exactly when the player hits a note, so they
tell players whether they were early or late. Storyboard sounds play at fixed times whatever the player
does, so the ranking criteria say:

- storyboard sounds **can't replace the hitsounds** of the notes players actually hit, and
- they shouldn't be used during gameplay in ways that are **easily confused with hitsounds**.

Sound effects for intros, outros, breaks and endings are where storyboard sounds shine.

## Further reading

- [Storyboard audio samples](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Audio) on the osu! wiki
