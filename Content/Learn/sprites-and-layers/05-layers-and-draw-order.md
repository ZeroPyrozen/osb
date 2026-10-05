---
title: Layers and draw order
type: lesson
minutes: 6
summary: The five layers, what's drawn in front, and the Pass and Fail states.
---

Every object belongs to one of five **layers**. From back to front:

| Layer | Number | Notes |
| :-- | :-: | :-- |
| `Background` | 0 | The back of the stage. Backgrounds and scenery go here. |
| `Fail` | 1 | Only shown while the player is in the **fail** state. |
| `Pass` | 2 | Only shown while the player is in the **pass** state. |
| `Foreground` | 3 | In front of the scenery, still behind the hit circles. |
| `Overlay` | 4 | Drawn **over the hit circles**. Use with care. |

All storyboard layers are drawn behind the game's interface (the HP bar, the cursor and so on).
Every layer except Overlay is also behind the hit circles.

::: warning
Anything on the Overlay layer covers the circles the player is trying to read. Keep it subtle, brief and
away from the play area.
:::

## Who's in front?

Two rules decide what covers what:

1. **Different layers** are drawn in the order above. A Foreground sprite is always in front of a
   Background sprite, whatever the order in the file.
2. **On the same layer**, objects are drawn in the order they're declared. The one further down the file
   is drawn on top.

Here the white circle is declared after the mint one, so it covers it. Swap the two blocks and preview
again to see the difference.

```osb
Sprite,Foreground,Centre,"sb/circle.png",290,240
 F,0,0,3000,1
 C,0,0,,97,188,166
Sprite,Foreground,Centre,"sb/circle.png",350,240
 F,0,0,3000,1
```

If a beatmap has a storyboard both in its `.osb` file and in a difficulty's `.osu` file, the `.osb`
objects are drawn after (so in front of) the `.osu` ones on the same layer.

## Pass and Fail

The Pass and Fail layers are never visible at the same time. osu! decides which one to show from how the
player is doing, so you can show a happy scene to someone playing well and a gloomier one to someone
missing notes. Before the first hit object, the player always counts as passing.

Untick **Pass state** in this preview to see what a failing player would see:

```osb
Sprite,Pass,Centre,"sb/heart.png",320,240
 F,0,0,3000,1
 C,0,0,,255,120,150
Sprite,Fail,Centre,"sb/note.png",320,240
 F,0,0,3000,1
 C,0,0,,120,130,140
```

The exact rules for when a player is passing or failing come later, in the module on storyboards and
gameplay.

## Further reading

- [General rules for storyboarding](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/General_Rules) on the osu! wiki
