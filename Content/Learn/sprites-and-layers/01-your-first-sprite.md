---
title: Your first sprite
type: lesson
minutes: 5
summary: The Sprite line, file paths, and why a sprite needs a command.
---

Everything you see in a storyboard is an **object**. The most common one is a **sprite**: a still
image. You declare a sprite with a single line:

```
Sprite,layer,origin,"file",x,y
```

| Part | Example | Meaning |
| :-- | :-- | :-- |
| `Sprite` | `Sprite` | This line declares a still image. |
| layer | `Foreground` | Which layer it's drawn on. You'll meet the five layers later in this module. |
| origin | `Centre` | Which point of the image is placed at (x, y). |
| file | `"sb/star.png"` | The image, relative to the beatmap's folder. |
| x, y | `320,240` | Where the origin point goes, in storyboard coordinates. |

So this line puts the centre of `star.png`, from the beatmap's `sb` folder, at the centre of the screen,
on the Foreground layer:

```osb
Sprite,Foreground,Centre,"sb/star.png",320,240
 F,0,0,3000,1
```

## File paths

- Paths are **relative to the beatmap's folder**: write `"sb/star.png"`, never `"C:\..."`.
- Subfolders are fine, with forward or back slashes.
- The quotes are optional unless the path contains spaces, but writing them every time is a good habit.
- Use **PNG** for anything that needs transparency, and **JPG** for photos and backgrounds, where it
  makes much smaller files.

::: tip
The playground on this site has a small library of images (`sb/dot.png`, `sb/star.png`, `bg.jpg` and
more) so you can try things without making your own. You can also add your own images there.
:::

## A sprite needs a command

Notice the second line in the example, ` F,0,0,3000,1`. Delete it and press **Preview** again: nothing
shows up at all.

That's one of the most important rules of storyboarding: **an object only exists while its commands are
running**. The Sprite line says *what* and *where*; the commands say *when*. A sprite with no commands
never appears.

The command here is a **Fade** (`F`) that keeps the opacity at 1 (fully visible) from 0 to 3000 ms.
It's the simplest way to say "show this, unchanged, for three seconds". You'll learn commands properly
in the next module.

::: note
Command lines are indented with a space or an underscore (`_`). That's how osu! knows a command belongs
to the object above it.
:::

## Declaration order

The order of objects in the file doesn't decide *when* they appear (only their commands do), but it does
decide what's drawn **in front**: on the same layer, later objects are drawn over earlier ones. Most
storyboarders still keep objects roughly in the order they appear, which makes long scripts easier to
read.

## Further reading

- [Storyboard objects](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Objects) on the osu! wiki
