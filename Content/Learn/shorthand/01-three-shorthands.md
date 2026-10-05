---
title: Three shorthands
type: lesson
minutes: 7
summary: Leave out repeated values, leave out the end time for instant changes, and chain steps into one line.
---

Storyboards get long quickly. A busy one can have tens of thousands of lines, so osu! accepts three
**shorthands** that make commands shorter without changing what they do. You've been using the first
one since your very first sprite.

## 1. The value doesn't change

If the start and end values are the same, write the value once:

```
 F,0,0,3000,1          is the same as   F,0,0,3000,1,1
 M,0,0,1000,320,240    is the same as   M,0,0,1000,320,240,320,240
 C,0,0,1000,255,0,0    is the same as   C,0,0,1000,255,0,0,255,0,0
```

## 2. It happens in an instant

If a command starts and ends at the same moment, leave the end time **empty**, keeping the commas on
both sides of it:

```
 F,0,1000,,0.5         is the same as   F,0,1000,1000,0.5
```

An instant command sets the value at that moment, and by the rule you already know, the value then
holds until another command changes it. A chain of instant commands makes **hard cuts**: no fades, no
travel, just a switch. This square snaps to a new colour every half second:

```osb
Sprite,Foreground,Centre,"sb/square.png",320,240
 F,0,0,3000,1
 C,0,0,,255,90,90
 C,0,500,,255,220,90
 C,0,1000,,90,200,255
 C,0,1500,,255,90,90
 C,0,2000,,255,220,90
 C,0,2500,,90,200,255
```

Shorthands 1 and 2 often appear together, as in every line of that example.

::: note
Instant commands still count towards an object's life. An instant command at 5000 ms keeps the object
around until 5000 ms, even if nothing else happens to it.
:::

## 3. A sequence of steps

When the same command runs several times in a row, each step as long as the first, you can list all
the values on one line. Every extra value adds another step that starts where the previous one ended:

```
 F,0,1000,1500,0,1,0,1,0
```

is the same as

```
 F,0,1000,1500,0,1
 F,0,1500,2000,1,0
 F,0,2000,2500,0,1
 F,0,2500,3000,1,0
```

For Move and Vector scale each value is an x and y pair, and for Colour each value is three numbers.

::: warning
**osu!(lazer) doesn't support sequences yet.** It reads only the first two values and ignores the rest,
so only the first step plays there. If your storyboard should look the same for every player, write the
steps as separate lines, or let a script generate them for you (you'll do that in the Advanced path).
The playground marks lines that use a sequence.
:::

## Shorter isn't always better

The shorthands don't change how a storyboard plays. They only save characters, and that adds up to a
smaller file that loads a little faster. But a script you can't read is a script you can't fix. Use
shorthands 1 and 2 freely: they're everywhere, and every storyboarder reads them at a glance.

## Further reading

- [Storyboard scripting shorthand](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Shorthand) on the osu! wiki
