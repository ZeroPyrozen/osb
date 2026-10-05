---
title: Backgrounds, overlays and settings
type: lesson
minutes: 6
summary: Taking over the beatmap's background, what players can change, the Overlay layer, and the beatmap settings that affect storyboards.
---

## The beatmap's background

Every beatmap has a background image, and osu! draws it behind everything else. Storyboards have a
neat way to take control of it: **if a storyboard uses the same image file as a sprite, osu! hides the
static background as soon as the beatmap loads.** Your sprite takes its place, and now it can fade,
zoom, pan or change colour like anything else.

That's why so many storyboards start with the background as their very first sprite:

```osb
Sprite,Background,Centre,"bg.jpg",320,240
 F,0,0,1000,0,1
 S,0,0,4000,1,1.1
 F,0,3000,4000,1,0
```

If other images completely cover the background for a while, fade it out during that time. A sprite
that's hidden behind others still costs performance to draw, and the ranking criteria suggest fading
out anything that's completely covered.

## Players change what they see

Two player settings change how your storyboard looks:

- **Background dim.** Players can dim the background and storyboard to concentrate on the game. Many play
  with some dim, so don't rely on faint, dark details to tell a story.
- **Storyboards can be turned off entirely.** The beatmap should still look fine with just its
  background.

## The Overlay layer

Overlay is the only layer drawn **above the hit objects**, though still below the score, the HP bar and
the cursor. Anything there competes directly with the gameplay, so keep it light: a quick flash on a big
moment, letterbox bars, or lyrics near the edges of the screen. Busy motion on the Overlay layer, right
on top of the play area (roughly x 60 to 570, y 55 to 440), makes a beatmap harder to read.

## Beatmap settings for storyboards

A few settings in each difficulty's `.osu` file (most of them are in the editor's Song Setup) affect the
storyboard:

| Setting | In the .osu file | What it does |
| :-- | :-- | :-- |
| Widescreen support | `WidescreenStoryboard: 1` | Shows the full 854-pixel width instead of 4:3. |
| Letterbox during breaks | `LetterboxInBreaks: 1` | Adds black bars at the top and bottom during breaks. |
| Epilepsy warning | `EpilepsyWarning: 1` | Shows a warning before the beatmap starts. |
| Use skin sprites | `UseSkinSprites: 1` | Lets the storyboard use images from the player's skin by their file names. |

The ranking criteria require Widescreen support to be the same in every storyboarded difficulty, and
Letterbox during breaks to be consistent between difficulties of the same mode that have breaks.

## Further reading

- [General rules for storyboarding](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/General_Rules) on the osu! wiki
- [Storyboard .osu file toggles](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/osu!_File_Toggles) on the osu! wiki
