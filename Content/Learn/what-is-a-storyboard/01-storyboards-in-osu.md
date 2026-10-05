---
title: Storyboards in osu!
type: lesson
minutes: 4
summary: What a storyboard is and where osu! looks for one.
---

A **storyboard** is the animated scene that plays behind a beatmap while you play it. It's made of
ordinary images, called **sprites**, that the game moves, fades, scales, spins and recolours in time
with the music. A storyboard can be a gentle slideshow of the song's artwork, lyrics that appear word by
word, or a full music video built from thousands of moving pieces.

Storyboards were inspired by the animated scenes of *Osu! Tatakae! Ouendan* on the Nintendo DS, where
the story on the top screen changed depending on how well you played. osu! kept that idea: a storyboard
can show one thing to a player who's doing well and something else to one who's struggling.

## Why not just use a video?

Beatmaps can have a background video too, so why build a storyboard? A few reasons:

- **It reacts to the player.** A storyboard can show different scenes for passing and failing, and
  respond to hitsounds. A video plays the same way every time.
- **It stays sharp.** osu! draws a storyboard live, at whatever resolution the player uses.
- **It's usually smaller.** The same few images can be reused for minutes of animation, which often
  makes a storyboard a much smaller download than a video of the same length.

## Where storyboards live

osu! reads storyboard instructions from two places in a beatmap's folder:

| Where | Who sees it |
| :-- | :-- |
| A separate **`.osb` file** | Every difficulty of the beatmapset |
| The **`[Events]` section** of a difficulty's `.osu` file | Only that difficulty |

You can use both at once. A common pattern is to put the main storyboard in the `.osb` file and add a
few extra touches for one specific difficulty in its `.osu` file.

Both are plain text. Here's a tiny but complete storyboard: one sprite that fades in, drifts to the
right and fades out. Press **Preview** to watch it.

```osb
[Events]
Sprite,Foreground,Centre,"sb/text/hello.png",320,240
 F,0,0,500,0,1
 MX,0,0,3000,300,340
 F,0,2500,3000,1,0
```

By the end of the first module you'll be able to read every part of that, and by the end of the path
you'll be writing your own.

::: note
Players can turn storyboards off or dim them in their settings. Make sure your beatmap is still
perfectly playable without its storyboard.
:::

## Further reading

- [Storyboard](https://osu.ppy.sh/wiki/en/Storyboard) on the osu! wiki
- The [showcase](/showcase) on this site, for storyboards made by the community
