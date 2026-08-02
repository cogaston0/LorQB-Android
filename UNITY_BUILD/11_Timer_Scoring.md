# 11 — Timer and Scoring (Level 1)

## Overview

This document defines the timer and scoring rules for a Level 1 round. The timer is the sole mechanism by which a round ends without a valid completion. Scoring is derived entirely from completion time. No code is included; this is a planning document only.

---

## 1. Timer Start

| Rule | Detail |
|------|--------|
| Trigger | Timer starts when the player's first ball placement is **accepted** — i.e., the Game State Manager transitions into `ACTIVE_PLAY` for the first time in the round |
| No early start — ball selection | Timer does **not** start during `BALL_SELECTION`; the player choosing which ball to pick has no time pressure |
| No early start — shuffle display | Timer does **not** start while the sequence shuffle is being displayed (the window between `IDLE` and `BALL_SELECTION`) |
| No early start — ball placement | Timer does **not** start while the ball is being dragged toward the first cube (`BALL_PLACEMENT`); the clock begins only once placement is confirmed and the state advances |

---

## 2. Timer Behavior

| Rule | Detail |
|------|--------|
| Direction | Timer counts **down** from a fixed starting value toward zero |
| Active during play | Timer runs continuously during `ACTIVE_PLAY`, `VALIDATION`, `TRANSFER`, and `POST_TRANSFER` |
| Continues after invalid releases | An `INVALID` result from the validation pipeline returns the state to `ACTIVE_PLAY`; the timer is not paused or reset — it continues from where it was |
| Stops on round complete | When the Game State Manager enters `ROUND_COMPLETE`, the timer is frozen immediately; the final elapsed time is recorded for scoring |
| Expires to TIME_UP | When the countdown reaches zero, the Game State Manager immediately enters `TIME_UP` regardless of the current state; this transition takes priority over all other pending transitions |

---

## 3. Scoring

| Rule | Detail |
|------|--------|
| Basis | Score is determined by **completion time only** — the elapsed time between timer start and `ROUND_COMPLETE` |
| Faster is better | A lower completion time yields a higher score; the ranking is purely inverse of elapsed time |
| Invalid releases have no score penalty | An invalid release returns the state to `ACTIVE_PLAY` and does not advance the sequence; it incurs no explicit score deduction in the current version |
| Optional future penalty | A time penalty for each invalid release is reserved for future consideration; it is **not active** in Level 1 |
| No score before round complete | A score value is only recorded when `ROUND_COMPLETE` is reached; a round that ends in `TIME_UP` yields no score |
| Time-up result | If the round ends in `TIME_UP` the result is recorded as a failed attempt; no score is assigned for that round |

---

## 4. Multiplayer (Future)

| Rule | Detail |
|------|--------|
| Player count | Up to 4 players are supported in a future multiplayer mode |
| Independent timers | Each player runs their own countdown timer; one player's `TIME_UP` does not end the round for other players |
| Independent scores | Each player's completion time is tracked separately |
| Winner determination | The winner is the player who reaches `ROUND_COMPLETE` with the **lowest completion time** |
| Scope | Multiplayer logic is deferred; Level 1 currently targets a single player only |

---

## 5. State Integration

| State | Timer action |
|-------|-------------|
| `IDLE` | Timer is not running; not initialized |
| `BALL_SELECTION` | Timer is not running |
| `BALL_PLACEMENT` | Timer is not running |
| `ACTIVE_PLAY` (first entry) | **Timer starts** on the transition from `BALL_PLACEMENT` into `ACTIVE_PLAY` |
| `ACTIVE_PLAY` (re-entry from VALIDATION) | Timer continues — no pause or reset |
| `VALIDATION` | Timer continues — pipeline is synchronous and brief |
| `TRANSFER` | Timer continues — snap is one frame |
| `POST_TRANSFER` | Timer continues — index update is immediate |
| `ROUND_COMPLETE` | **Timer stops**; final time is captured |
| `TIME_UP` | Timer has already reached zero; this state is the direct consequence of expiry |

### BALL_PLACEMENT → ACTIVE_PLAY

The moment the Game State Manager confirms the ball is seated and transitions into `ACTIVE_PLAY` for the first time in the round, the timer begins counting down. This is the only valid start point.

### ROUND_COMPLETE

On entry to `ROUND_COMPLETE`, the timer is frozen. The value at that instant is the player's completion time and is used directly for scoring.

### TIME_UP

When the countdown reaches zero, the Game State Manager enters `TIME_UP`. All input is disabled. No further ball transfer or sequence advancement is allowed. Any in-progress `VALIDATION` or `TRANSFER` that has not yet completed is abandoned.

---

## 6. Constraints

| Constraint | Detail |
|------------|--------|
| No timer before first placement | The timer must not start during `IDLE`, `BALL_SELECTION`, or `BALL_PLACEMENT`; the only valid start is the `BALL_PLACEMENT` → `ACTIVE_PLAY` transition |
| No score until round complete | A score value is only written when `ROUND_COMPLETE` is reached; any other terminal state (including `TIME_UP`) produces no score |
| No automatic movement | The timer expiring does not cause any automatic cube rotation or ball snap; `TIME_UP` only freezes state and disables input |
| Timer is not paused | There is no pause mechanic for the timer in Level 1; once started, the timer runs until `ROUND_COMPLETE` or zero |
| Single timer per round | There is exactly one timer per round; it is reset at the start of each new round (when `IDLE` is entered) |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, coroutines, UI countdown display) are deferred to later build documents.
