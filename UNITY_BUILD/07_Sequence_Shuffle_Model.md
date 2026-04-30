# 07 — Sequence Shuffle Model (Level 1)

## Overview

Level 1 uses a four-color sequence system that determines the required order in which the player must transfer balls across the three pivot groups. At the start of each round a single permutation is chosen from the 24 possible orderings of Blue, Red, Green, and Yellow. The player must then move balls in that exact order to complete the round.

---

## 1. Four-Color Sequence

| Property | Value |
|---|---|
| Colors used | Blue (B), Red (R), Green (G), Yellow (Y) |
| Sequence length | 4 colors, each appearing exactly once |
| Total possible orderings | 24 permutations (4!) |

Every sequence is a strict permutation — no color is repeated, and all four colors are present.

---

## 2. Round Start

1. **Generate** — The system selects one of the 24 permutations at random to form `sequence[0..3]`.
2. **Display** — The chosen order is shown in the **Shuffle / Order Bar** so the player can see the required sequence before acting.
3. **Select** — The player picks a ball whose color matches `sequence[0]`.
4. **Place** — The player places that ball into the first required cube to begin the sequence.

---

## 3. Progress Tracking

| Variable | Initial value | Behavior |
|---|---|---|
| `sequenceIndex` | `0` | Advances by 1 after each valid transfer |

- The **first** placement must match `sequence[0]`.
- After a valid transfer is confirmed, `sequenceIndex` increments to the next position.
- The round is **complete** when `sequenceIndex` advances past the final color (i.e., after the 4th successful transfer, `sequenceIndex` reaches `4`).

---

## 4. Validation

A transfer is valid only when all of the following conditions are satisfied simultaneously:

| Condition | Requirement |
|---|---|
| Current cube color | Must equal `sequence[sequenceIndex]` |
| Destination cube color | Must equal `sequence[sequenceIndex + 1]` |
| Move Map rule | The move from current cube to destination cube must be an allowed move per the Move Map |
| Release state | Ball must have been physically released by the player |
| Alignment | Ball and destination cube must be within the valid alignment threshold |

If any condition fails the ball remains in (or returns to) the current cube and no index advance occurs.

---

## 5. All 24 Permutations

| # | Col 1 | Col 2 | Col 3 | Col 4 |
|---|---|---|---|---|
| 1 | B | R | G | Y |
| 2 | B | R | Y | G |
| 3 | B | G | R | Y |
| 4 | B | G | Y | R |
| 5 | B | Y | R | G |
| 6 | B | Y | G | R |
| 7 | R | B | G | Y |
| 8 | R | B | Y | G |
| 9 | R | G | B | Y |
| 10 | R | G | Y | B |
| 11 | R | Y | B | G |
| 12 | R | Y | G | B |
| 13 | G | B | R | Y |
| 14 | G | B | Y | R |
| 15 | G | R | B | Y |
| 16 | G | R | Y | B |
| 17 | G | Y | B | R |
| 18 | G | Y | R | B |
| 19 | Y | B | R | G |
| 20 | Y | B | G | R |
| 21 | Y | R | B | G |
| 22 | Y | R | G | B |
| 23 | Y | G | B | R |
| 24 | Y | G | R | B |

---

## 6. Player Feedback

| Event | Feedback |
|---|---|
| Correct transfer | Order marker in the Shuffle / Order Bar advances to the next color position |
| Incorrect release (wrong color or invalid move) | Ball stays in / snaps back to the current cube; no visual advance |
| Round in progress | Timer continues running regardless of correct or incorrect attempts |

No additional penalty is applied for failed attempts beyond the continued timer pressure.

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, animator states) are deferred to later build documents.
