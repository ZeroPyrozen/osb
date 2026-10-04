---
title: Triggers
type: lesson
minutes: 8
summary: Commands that play when something happens in gameplay, like a clap or the player starting to fail.
---

Every command so far has played at a fixed time. A **trigger** plays its commands when something
**happens during gameplay** instead: a hitsound plays, or the player switches between passing and
failing. It's the most interactive part of storyboarding.

## Writing a trigger

A trigger looks a lot like a loop. A `T` line, then the commands to play, indented one level deeper:

```osb-static
 T,triggerName,startTime,endTime
  F,0,0,300,1,0
```

- `triggerName` says what to listen for (see the table below).
- `startTime` and `endTime` are the window in which the trigger listens. Outside it, nothing happens.
- The commands inside use **relative times**, counted from the moment the trigger fires.

There's also an optional fifth value, a **group number**, which comes up below.

## What triggers can listen for

| Trigger | Fires when |
| :-- | :-- |
| `HitSound` | any hitsound plays |
| `HitSoundClap`, `HitSoundWhistle`, `HitSoundFinish` | a hitsound with that addition plays |
| `HitSoundSoft`, `HitSoundNormal`, `HitSoundDrum` | a hitsound with that sample set plays |
| `Passing` | the player switches from the Fail state to the Pass state |
| `Failing` | the player switches from the Pass state to the Fail state |

The hitsound triggers can be combined further. After `HitSound` you can name a sample set, then an
additions sample set, then an addition, then a custom sample number, and every part is optional. The osu!
wiki has the full list of combinations.

## Example: flash on every clap

Whenever a clap plays between 10 and 40 seconds, this glow flashes for 300 ms. Mappers put claps on
the snare, so the storyboard ends up flashing in time with the drums:

```osb-static
Sprite,Foreground,Centre,"sb/glow.png",320,240
 T,HitSoundClap,10000,40000
  P,0,0,300,A
  S,0,0,300,4
  F,0,0,300,0.8,0
```

## Example: reacting to failing

A red tint washes over the screen at the moment the player starts failing:

```osb-static
Sprite,Foreground,TopLeft,"sb/pixel.png",-107,0
 T,Failing,20000,60000
  V,0,0,500,854,480
  C,0,0,500,200,30,30
  F,0,0,500,0.5,0
```

## The rules

- **A trigger that fires again starts over.** If the condition happens while the trigger's commands are
  still playing, the running commands stop and start again from the beginning.
- **Group numbers link triggers.** Triggers on the same sprite with the same group number stop each other:
  when one starts, the others in its group stop.
- **Keep triggered sprites trigger-only.** A trigger doesn't play until the sprite's other commands have
  finished, so it's best to give a triggered sprite nothing but triggers.
- **No nesting.** Triggers can't contain loops or other triggers.

## Testing triggers

The previews on this site don't run gameplay, so they can't fire triggers. That's why the examples on
this page have no Preview button. Test triggers in osu! by playing the beatmap.

::: warning
The ranking criteria have two things to say about triggers. A trigger that can never fire (say, a clap
trigger on a difficulty with no claps) is an obsolete command and should be removed. And sprites that
triggers activate stay active until the end of the difficulty, so fade them out once you're done with
them.
:::

## Further reading

- [Compound commands: triggers](https://osu.ppy.sh/wiki/en/Storyboard/Scripting/Compound_Commands) on the osu! wiki
