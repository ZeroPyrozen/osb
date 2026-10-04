---
title: How storyboards are made
type: lesson
minutes: 5
summary: The editor, scripting by hand, and generating storyboards with code.
---

There are three ways to make a storyboard. Most storyboarders end up using more than one.

## 1. The Design tab of the beatmap editor

osu!'s beatmap editor has a **Design** tab where you can drop images onto the screen and animate them
with the mouse. It's the quickest way to try something simple, like fading in the background or
flashing an image on a beat.

It's also handy while you script: the Design tab shows the current time in milliseconds and the
cursor's position in storyboard coordinates, and pressing <kbd>Ctrl</kbd> + <kbd>C</kbd> copies the
current time so you can paste it into your script.

## 2. Scripting by hand

The storyboard is just text, so you can open the `.osb` file in any text editor (Notepad++,
Visual Studio Code, or even Notepad) and write the instructions yourself. This gives you exact control
over every value, and it's what this course teaches first, because everything else builds on it.

A script is a list of **objects**, each followed by indented **commands**:

```osb
Sprite,Foreground,Centre,"sb/star.png",320,240
 F,0,1000,2000,0,1
 R,0,1000,3000,0,6.283
```

- The first line declares a sprite: which layer it's on, which point of the image is its anchor,
  the image file, and where it starts.
- Each indented line is a command. `F` fades it in between 1000 and 2000 milliseconds;
  `R` rotates it one full turn between 1000 and 3000.

You'll learn every part of these lines in the next module.

::: warning
Don't edit a storyboard script while the same beatmap is open in osu!'s editor. When the editor saves,
it can overwrite your changes.
:::

## 3. Generating storyboards with code

Big effects need a lot of commands. A snowstorm with 300 flakes, each falling on its own path, is
thousands of lines you wouldn't want to type. So storyboarders write programs that write the script
for them.

The most popular tool is [storybrew](https://github.com/Damnae/storybrew), where you write effects in
C# and preview them live. Others use Python or JavaScript. The Advanced path of this course teaches
the ideas behind generating storyboards, using the playground's own script mode.

## What you need for this course

Nothing to install: every exercise runs in your browser. When you're ready to try things in osu!
itself, you'll need a beatmap folder to work in and a text editor.

## Further reading

- [Storyboard scripting](https://osu.ppy.sh/wiki/en/Storyboard/Scripting) on the osu! wiki
- [The Design tab](https://osu.ppy.sh/wiki/en/Client/Beatmap_editor/Design) on the osu! wiki
