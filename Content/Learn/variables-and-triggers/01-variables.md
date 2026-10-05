---
title: Variables
type: lesson
minutes: 5
summary: Name a value once in a [Variables] section, reuse it everywhere, and avoid the two classic traps.
---

When the same long value turns up again and again (a file path, a colour, a position), you can give
it a **name**. Variables live in a `[Variables]` section above the events in an `.osb` file:

```
[Variables]
$mint=97,188,166
$star="sb/star.png"
```

Each line is a name that starts with `$`, an equals sign, and the value. Wherever the name appears
in `[Events]`, osu! swaps in the value **before** it reads the line. It's a plain text replacement,
so a variable can stand for one number, several numbers, a file path, or any other piece of a line:

```osb
[Variables]
$mint=97,188,166
$star="sb/star.png"

[Events]
Sprite,Foreground,Centre,$star,220,240
 F,0,0,3000,1
 C,0,0,3000,$mint
Sprite,Foreground,Centre,$star,420,240
 F,0,0,3000,1
 C,0,0,3000,$mint
```

Change `$mint` once and both stars change colour.

## The rules

- Variables only work in **`.osb`** files, not in a difficulty's `.osu` file.
- They're **constants**. A variable has one value for the whole storyboard and can't change during
  the song.

## Two traps

**Saving in the editor.** When osu!'s beatmap editor saves a storyboard that uses variables, it goes
looking for each variable's *value* and replaces every match with the variable's name, including matches
you never meant. With `$loops=12`, every `12` in the file turns into `$loops`, even inside a colour
like `12,12,12`. Only give variables long, distinctive values.

**Names inside names.** Because it's plain text replacement, `$bg` also matches the start of `$bg2`,
which ends up as `$bg`'s value followed by a `2`. Give variables names where neither is the start of
another, like `$bgDay` and `$bgNight`.

## Do you need them?

Variables help most in storyboards written by hand. If a program generates your storyboard (the next
module), the program's own variables do this job, with none of the traps.

## Further reading

- [Storyboard scripting variables](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Variables) on the osu! wiki
