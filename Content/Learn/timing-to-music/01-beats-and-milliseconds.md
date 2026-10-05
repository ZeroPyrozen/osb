---
title: Beats and milliseconds
type: lesson
minutes: 7
summary: BPM, beat length, the offset, beat divisions and bars, and where to find them in a beatmap.
---

The most satisfying storyboards move **with** the music: a flash on the drop, a pulse on every beat,
lyrics that appear as they're sung. To do that, you need to know where the beats are in milliseconds.

## Beat length

Music is measured in **BPM**, beats per minute. A minute is 60,000 ms, so:

```
beat length (ms) = 60000 ÷ BPM
```

| BPM | One beat |
| :-: | :-: |
| 100 | 600 ms |
| 120 | 500 ms |
| 150 | 400 ms |
| 180 | 333.33 ms |
| 200 | 300 ms |

## The offset

The beats don't start at 0 ms. Songs often begin with silence or a pickup. The **offset** is the time
of the first beat, and every other beat follows at a fixed distance:

```
beat n is at   offset + n × beat length
```

With an offset of 1200 ms at 120 BPM, the beats fall at 1200, 1700, 2200, 2700, and so on.

## Finding them in a beatmap

The beatmap already knows the timing, because the mapper set it up for the hit objects. In the `.osu`
file, the `[TimingPoints]` section lists it. For an uninherited (red) timing point, the first number is
its time and the second is the beat length:

```
[TimingPoints]
1200,500,4,2,0,60,1,0
```

That's a first beat at 1200 ms and 500 ms per beat, so 120 BPM. The third number, 4, is the number
of beats in a bar. A song that changes tempo has several uninherited points, and each one starts a
new grid of beats from its own time.

::: tip
The beatmap editor is the easiest way to find exact times. In the Design tab it shows the current time
in milliseconds, and Ctrl+C copies it, ready to paste into your script.
:::

## Divisions and bars

Beats divide into smaller steps, just like the snap divisor in the editor. Half a beat at 120 BPM is
250 ms and a quarter is 125 ms. Fast effects often run on half or quarter beats.

Beats also group into **bars** (also called measures), usually of 4. Big moments in a song, like a new
section or a drop, almost always land on the **first beat of a bar**, the downbeat. Those are the best
places for big changes in a storyboard.

Storyboard times are normally whole milliseconds, so round the result: at 180 BPM, the beats fall at
roughly 0, 333, 667, 1000 and so on. Rounding each beat from the exact formula keeps the error from
adding up.

## Pulsing on the beat

Put it together with a loop: one pass per beat. At 120 BPM each pass lasts 500 ms. This ring pulses
on every beat, and an Out easing makes it snap out and then settle:

```osb
Sprite,Foreground,Centre,"sb/ring.png",320,240
 L,0,8
  S,7,0,500,1.4,1
  F,7,0,500,1,0.4
```

## Further reading

- [Timing](https://osu.ppy.sh/wiki/en/Client/Beatmap_editor/Timing) in the osu! wiki's beatmap editor guide
