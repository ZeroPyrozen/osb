---
title: Why write code?
type: lesson
minutes: 5
summary: The limits of writing storyboards by hand, what a generator does, and the tools storyboarders use.
---

Everything so far, you've written by hand, and that's how every storyboarder starts. But look at a
storyboard from the showcase and you'll find tens of thousands of commands: snow made of hundreds of
flakes, lyrics appearing letter by letter, spectrums dancing to the music. Nobody types those.

## The problem with hand-written scripts

- **It doesn't scale.** A hundred particles, each with its own position, timing and speed, means hundreds
  of lines of careful arithmetic.
- **Changes are painful.** Want every particle to fall a little faster? That's every line, edited again.
- **Variety is hard.** Natural-looking effects need randomness, and inventing hundreds of "random"
  numbers by hand is slow and never looks quite random.

## Generators

The solution is to write a **program that writes the storyboard**. The program uses ordinary
programming tools (loops, maths, functions, random numbers) and its output is a normal `.osb` file.
osu! neither knows nor cares how the file was made.

With a generator:

- a loop places 100 sprites as easily as 1,
- a formula puts them on a circle, a wave or a spiral,
- a random number generator gives every particle its own personality,
- and changing a single number regenerates the whole effect.

## The tools

- **storybrew**, by Damnae, is the tool most storyboarders use. It's a free storyboard editor where
  you write effects in C#, see the result immediately with the music, and export to `.osb`. It can also
  read the beatmap and the audio, so effects can follow hit objects or the song's spectrum.
- Many storyboarders write their **own scripts** in Python, JavaScript or any other language that can
  write a text file.
- This site has a **script mode** in the playground and in exercises. You write JavaScript using an API
  modelled on storybrew's, so everything you learn here carries over.

The next lesson introduces script mode. You'll use it for the rest of the Advanced path.

## Further reading

- [Storyboard scripting](https://osu.ppy.sh/wiki/en/Storyboard/Scripting) on the osu! wiki
- [storybrew on GitHub](https://github.com/Damnae/storybrew)
