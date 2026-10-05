---
title: Scale
type: lesson
minutes: 6
summary: S and V, the pivot point, turning a single pixel into bars, and making images look sharp.
---

Scale (`S`) changes an object's size, as a multiple of the image's own size. `1` is the original size,
`2` is twice as big, `0.5` is half, and `0` shrinks it to nothing.

```
 S,easing,start,end,startScale,endScale
```

This circle grows from nothing to one and a half times its size while it fades in:

```osb
Sprite,Foreground,Centre,"sb/circle.png",320,240
 S,0,0,1000,0,1.5
 F,0,0,1000,0,1
 F,0,1000,3000,1
```

## Stretching with Vector scale

Vector scale (`V`) scales the width and the height separately, so it takes an x and a y for each value:

```
 V,easing,start,end,startX,startY,endX,endY
```

A favourite trick is to take a **1 × 1 white pixel** and stretch it into whatever rectangle you need.
The playground's `sb/pixel.png` is exactly that. Here it becomes a bar 40 pixels wide that grows
upwards and shrinks back:

```osb
Sprite,Foreground,BottomCentre,"sb/pixel.png",320,400
 V,0,0,1500,40,0,40,250
 V,0,1500,3000,40,250,40,0
```

If an object has both `S` and `V`, they multiply. A scale of 2 and a vector scale of (1, 0.5) make the
object twice as wide and exactly as tall as the image.

## Scale grows from the origin

Just like rotation, scaling happens around the object's **origin**. That's why the bar above uses
`BottomCentre`: its bottom edge stays on the floor while it grows. Compare a `Centre` square, which grows
in every direction, with a `TopLeft` one, which grows right and down from its corner:

```osb
Sprite,Foreground,Centre,"sb/square.png",200,240
 S,0,0,2000,0.2,1.5
Sprite,Foreground,TopLeft,"sb/square.png",380,170
 S,0,0,2000,0.2,1.5
```

## Keeping images sharp

The storyboard canvas is only 480 pixels tall, but players' screens are much bigger. On a 1080p screen
osu! draws everything about 2.25 times larger, so an image shown at scale 1 gets blurry when it's
enlarged that much.

The usual fix is to make images **larger than you need and scale them down**. For example, a background
made at 1920 × 1080 fills the height of the screen at a scale of 480 ÷ 1080 ≈ 0.444. Don't go
overboard, though. Huge images take longer to load, and the ranking criteria limit each storyboard
image to 17,000,000 pixels in total.

## Further reading

- [Scale commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
