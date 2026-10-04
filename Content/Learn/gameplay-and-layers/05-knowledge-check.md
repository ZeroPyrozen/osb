---
title: Knowledge check
type: quiz
minutes: 4
summary: Seven questions on storyboards and gameplay.
questions:
  - prompt: Which state is the player in before the first hit object?
    choices:
      - Always Pass
      - Always Fail
      - It depends on their last play
    answer: 0
    explanation: Nobody has played anything yet, so osu! treats everyone as passing. That makes Pass and Fail layers pointless during an intro.

  - prompt: In osu! (the standard mode), when is a player in the Pass state during play?
    choices:
      - Whenever their HP bar is above half
      - In the first combo, and after finishing a combo with nothing but 300s
      - Only during kiai time
    answer: 1
    explanation: The state updates at the end of each combo. A single 100 or worse in the combo switches to Fail.

  - prompt: During a break, what decides the state?
    choices:
      - Whether the HP bar ended the last section above half
      - The player's accuracy over the whole map
      - Nothing. Breaks always show the Pass layer
    answer: 0
    explanation: Above half shows an O and the Pass layer. Below half shows an X and the Fail layer.

  - prompt: The Pass ending lasts until 95 seconds, but the Fail ending runs until 105 seconds. What do passing players see?
    choices:
      - The results screen at 95 seconds
      - An empty storyboard for 10 seconds, because osu! waits for the last event on either layer
      - The Fail ending
    answer: 1
    explanation: The results screen waits for everything to finish, including the layer the player can't see. Make both endings the same length.

  - prompt: Your storyboard uses the beatmap's background file as a sprite. What happens?
    choices:
      - The background is drawn twice
      - The static background is hidden once the beatmap loads, so your sprite takes over
      - osu! reports an error
    answer: 1
    explanation: That's the standard way to animate a beatmap's background.

  - prompt: Which layer is drawn above the hit objects?
    choices:
      - Foreground
      - Overlay, which is still below the HP bar and cursor
      - Pass
    answer: 1
    explanation: Overlay sits between the hit objects and the game's interface, so use it sparingly.

  - prompt: A Sample is on layer 1. When does it play?
    choices:
      - Always
      - Only if the player is in the Fail state at that moment
      - Only if the player is passing at that moment
    answer: 1
    explanation: Layer 1 is the Fail layer. Layer 2 (Pass) plays only for passing players, and 0 and 3 always play.
---

Seven questions about reacting to the player.
