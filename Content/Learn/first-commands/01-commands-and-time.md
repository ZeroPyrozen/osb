---
title: Commands and time
type: lesson
minutes: 7
summary: How a command line is built, and the three rules for what an object looks like before, during and after its commands.
---

A Sprite line says *what* to draw and *where*. Everything else (when it appears, how it moves, how
it fades) comes from **commands**, the indented lines underneath it.

## Anatomy of a command

```
 F,0,1000,2000,0,1
```

| Part | Here | Meaning |
| :-- | :-- | :-- |
| type | `F` | Which command. `F` is Fade. The space in front attaches it to the object above. |
| easing | `0` | How the change speeds up or slows down. `0` is a steady, linear change. Easing gets its own module later. |
| start time | `1000` | When the change begins, in milliseconds. |
| end time | `2000` | When the change is finished. |
| values | `0,1` | The value at the start time, then the value at the end time. |

Read it as: *between 1 and 2 seconds, fade from opacity 0 to opacity 1*. Every command follows the
same pattern of type, easing, start, end and values. Only the values differ: one number for Fade, an
x and a y for Move, three numbers for a colour, and so on.

## Three rules about time

Commands only describe the moments they cover. These three rules decide everything else:

1. **An object exists from its earliest command to its latest one.** Outside that span it isn't
   drawn at all, whatever its values would be.
2. **After a command ends, its end value stays** until another command of the same type changes it.
3. **Before a property's first command, the object already has that command's start value.**

Here's all three at once. The Move command keeps the circle alive from 0 to 3000 ms (rule 1). It's
invisible until 1000 ms, because the fade's start value of 0 already applies before the fade begins
(rule 3). It then fades in, and stays fully visible after 2000 ms because the end value of 1 holds
(rule 2).

```osb
Sprite,Foreground,Centre,"sb/circle.png",320,240
 M,0,0,3000,160,240,480,240
 F,0,1000,2000,0,1
```

::: tip
Drag the time slider under the preview, or click the preview and use the arrow keys, to step through
it slowly. Watching a storyboard frame by frame is the quickest way to understand it.
:::

## Fade

Fade (`F`) sets the **opacity**: `0` is invisible and `1` is fully visible, with anything in between
allowed. An object with no Fade commands at all is simply fully visible for its whole life.

When a value doesn't change, you can write it just once. ` F,0,0,3000,1` means "opacity 1 from 0 to
3000 ms", exactly like ` F,0,0,3000,1,1`. That's the line you've seen under every sprite so far: the
simplest way to keep something on screen.

To show something, then hide it, you need two fades. Nothing has to happen in between: the first
fade's end value holds until the second one starts.

```osb
Sprite,Foreground,Centre,"sb/star.png",320,240
 F,0,0,500,0,1
 F,0,2500,3000,1,0
```

## Commands work together

An object can have as many commands as you like, and commands of **different types** can run at the
same time. A sprite can fade in while it moves, for example. That's normal and expected.

Commands of the **same type** shouldn't overlap, though. If two fades on one sprite cover the same
moment, the one that started later takes over, which is rarely what you meant. The ranking criteria
also ask for overlapping commands like these to be fixed. Put same-type commands back to back
instead, so one ends where the next begins.

## Further reading

- [Storyboard scripting commands](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Commands) on the osu! wiki
