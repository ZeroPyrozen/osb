---
title: Animation objects
type: lesson
minutes: 7
summary: Frame files, frame count and delay, LoopForever and LoopOnce, and when an animation is the right tool.
---

A Sprite shows one image. An **Animation** shows a series of images, one after another, like a flip
book. It's perfect for small looping effects: sparkles, flickering flames, blinking eyes, a waving
flag.

## Declaring an animation

```
Animation,layer,origin,"file",x,y,frameCount,frameDelay,loopType
```

The first six parts are exactly like a Sprite. The new ones are:

| Part | Example | Meaning |
| :-- | :-- | :-- |
| frameCount | `6` | How many images (frames) the animation has. |
| frameDelay | `80` | How long each frame shows, in milliseconds. |
| loopType | `LoopForever` | `LoopForever` starts again after the last frame. `LoopOnce` stops on the last frame and keeps showing it. |

If you leave the loop type out, the animation loops forever.

## Naming the frames

Each frame is its own image file, numbered from **0**, with the number just before the extension. In
the Animation line you write the name **without** a number. For a 6-frame animation declared as
`"sb/spark.png"`, osu! loads:

```
sb/spark0.png  sb/spark1.png  sb/spark2.png  sb/spark3.png  sb/spark4.png  sb/spark5.png
```

The playground has exactly this sparkle, so you can try it:

```osb
Animation,Foreground,Centre,"sb/spark.png",320,240,6,80,LoopForever
 S,0,0,3000,2
```

## Speed

The frame delay sets the speed. A delay of 100 ms shows 10 frames per second, and 50 ms shows 20.
Divide 1000 by the delay to get frames per second. One full run through the animation takes frame
count × frame delay: 6 × 80 = 480 ms for the sparkle.

Frames start counting from frame 0 when the object's life begins, at its first command. Here the same
sparkle plays once on the left and forever on the right, at a slower speed so you can see the
difference:

```osb
Animation,Foreground,Centre,"sb/spark.png",220,240,6,150,LoopOnce
 S,0,0,3000,2
Animation,Foreground,Centre,"sb/spark.png",420,240,6,150,LoopForever
 S,0,0,3000,2
```

## Commands work as usual

An Animation takes the same commands as a Sprite: fade it, move it, scale it, tint it, loop it. The
commands apply to whichever frame is showing.

## When to use one

Animations are great for **short, repeating** effects with a handful of frames. Every frame is a
separate image in memory, so a long, smooth animation with hundreds of frames gets heavy fast. For those,
a beatmap video is usually the better choice. And if the motion is simple (sliding, spinning, pulsing),
a single Sprite with commands is lighter and smoother than drawn frames.

::: tip
Make every frame the **same size**, and keep the subject in the same place in each image. Otherwise the
animation wobbles as it changes frames, because each frame is positioned using its own size and the
origin.
:::

## Further reading

- [Storyboard objects](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Objects) on the osu! wiki
