---
title: The stage
type: lesson
minutes: 6
summary: Storyboard coordinates, widescreen, the play area and time.
---

Before you place anything, it helps to know the stage: how big it is, where (0, 0) is, and how time
is counted.

## A 640 × 480 canvas

Storyboard positions are measured in **osu! pixels** on a canvas that's 640 wide and 480 tall,
whatever the player's real screen size is. osu! scales everything up to fit.

- **(0, 0)** is the **top-left** corner.
- **x** grows to the **right**, **y** grows **downwards**.
- The centre is **(320, 240)** and the bottom-right corner is **(640, 480)**.

Positions outside the canvas are allowed. A sprite at x = −100 is just off the left edge, which is how
you make things slide in from off-screen.

```osb
// A dot in each corner of the 4:3 area and one in the centre.
// Turn on Guides in the preview and hover it to read coordinates.
Sprite,Foreground,Centre,"sb/dot.png",0,0
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/dot.png",640,0
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/dot.png",0,480
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/dot.png",640,480
 F,0,0,3000,1
Sprite,Foreground,Centre,"sb/dot.png",320,240
 F,0,0,3000,1
```

## Widescreen

Most players have 16:9 screens, which show more than 640 pixels of width. With the beatmap's
**Widescreen support** setting turned on, the visible area is 854 pixels wide: **x from −107 to 747**.
The extra 107 pixels on each side are only seen on widescreen displays.

- Keep anything important inside 0 to 640, so 4:3 players still see it.
- Backgrounds should cover the full 854 width (for example placed at x = −107 with the TopLeft anchor),
  or widescreen players will see black bars.
- If you design for 4:3 only, turn Widescreen support off. The ranking criteria ask for this setting to
  match the storyboard, and to be the same in every storyboarded difficulty.

## The play area

Hit circles live in a smaller rectangle, roughly **x 60 to 570** and **y 55 to 440**. That's where the
player's eyes are, so busy effects there compete with the gameplay. Many storyboarders keep the busiest
motion around the edges and calm things down where the circles are.

## Time

Storyboard time is in **milliseconds** (1000 ms = 1 second), counted from the **start of the song's
audio file**. So `2500` means two and a half seconds into the song.

- Times can be **negative**: anything before 0 plays as an intro before the music starts, and osu!
  shows a Skip button for it.
- Time doesn't depend on the beatmap's BPM or timing. If you retime the map later, the storyboard won't
  follow, so time the map well before you storyboard it.
- After the last hit object, osu! waits until the last storyboard command has finished before showing
  the results screen, so long outros keep players waiting.

::: tip
The preview players on this site have a **Guides** switch that draws the play area and the 4:3 frame,
and they show the coordinates under your cursor. Use them whenever you're unsure where something is.
:::

## Further reading

- [General rules for storyboarding](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/General_Rules) on the osu! wiki
