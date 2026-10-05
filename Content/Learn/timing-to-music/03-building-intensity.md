---
title: Following the song's energy
type: lesson
minutes: 6
summary: Song structure, build-ups, drops and kiai, and how to make a storyboard rise and fall with the music.
---

Being on the beat is the start. Great storyboards also follow the song's **shape**. They stay calm when
the music is calm, build up when it builds, and hit hardest when it does.

## Listen for the sections

Most songs are made of recognisable sections: an intro, verses, a build-up, a chorus or drop, a bridge
and an outro. Before writing a single line, listen through and note the times where sections change.
Those timestamps become the skeleton of your storyboard.

The beatmap helps here too. Mappers usually put **kiai time** on the chorus, so its kiai sections are a
good guide to where the song is most intense. A storyboard can't read kiai directly, but you can use the
same times.

## Turn the energy up and down

A storyboard has plenty of dials to turn as a song gets more intense:

- **Rhythm:** pulse every bar, then every beat, then every half beat.
- **Brightness and colour:** dark and muted for verses, bright and saturated for the chorus.
- **Motion:** slow drifting at first, faster and bigger movements later.
- **Density:** a few elements early on, more particles and layers at the peak.

Holding back is as important as going big. A chorus only feels huge if the verse before it was calmer.

## A build-up in miniature

This glow pulses every two beats, then speeds up to every beat, before one big flash on the "drop" at
3000 ms (the song is 120 BPM). It uses two loops one after another on the same sprite, plus additive
blending for the light:

```osb
Sprite,Background,Centre,"bg.jpg",320,240
 F,0,0,4000,1
Sprite,Foreground,Centre,"sb/glow.png",320,240
 P,0,0,,A
 S,0,0,4000,6
 L,0,2
  F,7,0,1000,0.6,0
 L,2000,2
  F,7,0,500,0.6,0
 F,0,3000,4000,1,0
```

::: warning
**Flashing can be dangerous.** Repeated strobes, pulsing images and rapid changes in brightness can
trigger seizures in photosensitive players. The ranking criteria require an **epilepsy warning** for
difficulties with effects like these. Flashing at 3 times a second or slower is unlikely to cause
concern. When in doubt, add the warning.
:::

## Sync lyrics and accents

Beats aren't the only thing worth matching. Lyrics, drum hits, vocal chops and sound effects all make
great sync points. Find their exact times in the editor, and remember that the most memorable effects
are often a single, perfectly timed moment rather than constant motion.
