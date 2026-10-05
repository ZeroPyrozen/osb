---
title: Passing and failing
type: lesson
minutes: 7
summary: Exactly when osu! shows the Pass layer or the Fail layer, and how to design for both.
---

A video plays the same way for everyone. A storyboard can **react to the player**. Back in the first
modules you met the Pass and Fail layers: osu! draws one or the other, never both, depending on how
the player is doing. Now let's look at the exact rules.

## The rules

**Before the first hit object**, the player is always in the **Pass** state. Nobody has played anything
yet, so the Fail layer never shows during an intro, and it's best not to use either layer there.

**During play**, it depends on the game mode:

- **osu!:** the state updates at the end of every combo (every colour change). The player is passing in
  the first combo, and afterwards if the combo they just finished was **all 300s**. Anything less, even
  a single 100, switches to Fail until a perfect combo brings them back.
- **osu!taiko:** Fail right after a missed note, Pass otherwise.
- **osu!catch:** the state from the previous break, starting with Pass.

**During a break**, it's Pass if the HP bar finished the last section **above half** (the break shows
an O), and Fail otherwise (an X).

**After the last hit object**, if the beatmap had breaks, it's Pass when at least half the breaks were
spent passing. Without breaks, the rule for breaks applies.

## Try both states

The preview players have a **Pass state** switch. Here a warm glow lights the scene for passing players,
and a cold, dark filter covers it for failing ones. Untick the switch to see the difference:

```osb
Sprite,Background,Centre,"bg.jpg",320,240
 F,0,0,4000,1
Sprite,Pass,Centre,"sb/glow.png",320,200
 P,0,0,,A
 S,0,0,4000,5
 C,0,0,4000,255,200,120
Sprite,Fail,TopLeft,"sb/pixel.png",-107,0
 V,0,0,4000,854,480
 C,0,0,4000,20,30,60
 F,0,0,4000,0.6
```

## Designing for both

- **Share the scenery.** Put everything both groups should see on Background, Foreground or Overlay,
  and use Pass and Fail only for the parts that differ.
- **Keep both versions meaningful.** A failing player should still get a good-looking storyboard, just a
  different mood: darker colours, rain instead of sun, a sad ending instead of a happy one.
- **End both at the same time.** osu! waits for the very last event on *either* layer before showing
  the results screen. If the Fail ending is 10 seconds longer than the Pass ending, passing players sit
  in front of an empty screen for 10 seconds.

The moment of switching from one state to the other can also be animated, with the Passing and Failing
**triggers**. They're coming up in the Advanced path.

## Further reading

- [Game state](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/General_Rules#game-state) on the osu! wiki
