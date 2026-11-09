---
title: JavaScript for storyboards
type: lesson
minutes: 9
summary: Variables, arithmetic, loops, functions and arrays, the parts of JavaScript the rest of the path uses.
---

From the next module on, the Advanced path writes storyboards with code, in JavaScript. You don't need to
be a programmer for that. A handful of ideas covers nearly everything, and this lesson goes through them
one at a time. Every example runs in the playground's **script mode**: press **Run in playground**, then
change the numbers and see what happens.

## Values and `log`

Script mode runs your code and shows what you pass to `log` under the editor, after the number of
sprites it made. It shows the last three, which makes it a handy calculator:

```js
log(2 + 3);
log(10 * 4 - 2);
log((10 + 2) / 4);
```

This prints 5, 38 and 3. JavaScript uses `*` to multiply and `/` to divide, and works out brackets first.
Each line ends with a semicolon, and JavaScript ignores anything after `//`, so you can leave yourself
notes.

## Variables

A **variable** gives a value a name, so you can use it again and change it in one place:

```js
const bpm = 120;
const beat = 60000 / bpm;
log('One beat lasts', beat, 'ms');

let time = 1000;
time = time + beat;
log('The next beat is at', time);
```

Text goes in quotes, like `'ms'`. `const` makes a name whose value never changes, and `let` makes one you
can change later, like `time` here. Use `const` unless you need to change the value. Names can't contain
spaces, so storyboarders write `startTime`, and capitals matter: `startTime` and `starttime` are two
different names.

## A sprite from code

Script mode can also build a storyboard. The next module covers everything it can do; for now you only
need three calls:

```js
const layer = sb.layer('Foreground');
const x = 200;
const star = layer.sprite('sb/star.png', 'Centre', x, 240);
star.fade(0, 1000, 0, 1);
star.fade(2000, 2500, 1, 0);
```

`sb.layer` picks a layer, `layer.sprite` adds a sprite at a position, and `fade` gives it an F command
with the same four values as in a `.osb` file: start, end, from and to. The script writes this:

```osb
Sprite,Foreground,Centre,"sb/star.png",200,240
 F,0,0,1000,0,1
 F,0,2000,2500,1,0
```

## Loops

A `for` loop runs the same code several times. This one makes five stars in a row:

```js
const layer = sb.layer('Foreground');

for (let i = 0; i < 5; i++) {
    const star = layer.sprite('sb/star.png', 'Centre', 120 + i * 100, 240);
    const start = i * 200;
    star.fade(start, start + 400, 0, 1);
    star.fade(2500, 3000, 1, 0);
}
```

Read the first line as "start `i` at 0, keep going while `i` is less than 5, and add 1 to `i` after each
round" (`i++` means add 1). The code between the braces runs five times, with `i` set to 0, 1, 2, 3 and
4. Using `i` in the arithmetic gives each star its own place and its own start time: the stars sit at
x = 120, 220, 320, 420 and 520, and each one fades in 200 ms after the one before.

::: tip
Counting from 0 feels odd at first, but it keeps the arithmetic simple: the first star is at
`120 + 0 * 100`, which is just 120.
:::

## Functions

A **function** is a calculation with a name. You give it values, it works something out, and `return`
hands back the result:

```js
function beatTime(beat) {
    return 1200 + beat * 500;
}

log(beatTime(0));
log(beatTime(4));
```

This prints 1200 and 3200, the times of beats 0 and 4 in a song at 120 BPM whose first beat is at
1200 ms. `beat` is a **parameter**: a name for whatever value you pass in when you call `beatTime(4)`. A
function can take several parameters, separated by commas. Keeping a calculation in one function means
that when the offset changes, you fix one line instead of twenty.

## Arrays

An **array** is a list of values in square brackets. Each value has an **index**, counting from 0, and
`length` says how many values there are:

```js
const images = ['sb/star.png', 'sb/heart.png', 'sb/note.png'];
log(images[0]);
log(images[2]);
log(images.length);
```

This prints sb/star.png, sb/note.png and 3.

Script mode also uses arrays of two numbers as points, `[x, y]`. A function can return one, and `[0]`
and `[1]` read its parts:

```js
function middle(a, b) {
    return [(a[0] + b[0]) / 2, (a[1] + b[1]) / 2];
}

const point = middle([100, 200], [300, 400]);
log(point);
log(point[0], point[1]);
```

Loops and arrays work well together. With `i < images.length`, this loop visits every image in the
list, however long it is:

```js
const layer = sb.layer('Foreground');
const images = ['sb/star.png', 'sb/heart.png', 'sb/note.png'];

for (let i = 0; i < images.length; i++) {
    const sprite = layer.sprite(images[i], 'Centre', 200 + i * 120, 240);
    sprite.fade(i * 300, i * 300 + 300, 0, 1);
    sprite.fade(2500, 3000, 1, 0);
}
```

## When something goes wrong

If the code has a mistake, the playground says so under the editor, with the line number when it can,
and keeps showing the last version that worked. The usual causes are:

- a missing bracket, brace or quote
- a misspelt name: capitals count, so `Math.Cos` isn't `Math.cos`
- giving a `const` a new value

When a value isn't what you expect, `log` it and look.

## Further reading

- [JavaScript Guide](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide) on MDN, whose first
  chapters cover this lesson in more depth
