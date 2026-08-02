# Level 1 — Test Checklist

**Version:** 1.0
**Level:** Level 1 — LorQB / Cubolita
**Platform:** Android
**Tester:** ___________________
**Date:** ___________________
**Build version:** ___________________
**Device tested on:** ___________________

---

## Instructions

- Mark each item: ✅ Pass | ❌ Fail | ⚠️ Partial | N/A Not Applicable
- Note the defect ID for any failures (see `AGENTS/06_Testing.md` for defect format)
- All Critical and High items must pass before the build is approved

---

## Section 1 — Scene Load & Rendering

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-R-01 | Scene loads without crash or error | Critical | | |
| TC-R-02 | All 4 cubes appear on screen | Critical | | |
| TC-R-03 | Cubes are visible and correctly colored (Blue, Red, Green, Yellow) | High | | |
| TC-R-04 | Cubes are semi-transparent (interior visible) | High | | |
| TC-R-05 | Ball is visible inside Blue cube at start | Critical | | |
| TC-R-06 | Ball is correctly sized (fits inside cube, visible through walls) | High | | |
| TC-R-07 | Camera shows all 4 cubes in frame simultaneously | High | | |
| TC-R-08 | Background color is dark (not white/default) | Medium | | |
| TC-R-09 | No Z-fighting or visual artifacts on cube faces | Medium | | |
| TC-R-10 | Frame rate is ≥ 30 FPS (use device profiler) | High | | |

---

## Section 2 — Player Selector

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-PS-01 | Player Selector panel appears immediately on scene load | Critical | | |
| TC-PS-02 | Game is paused / timer is not running before player confirms | Critical | | |
| TC-PS-03 | Player can select from dropdown (Player 1–4, Guest) | High | | |
| TC-PS-04 | Tapping custom name field opens Android keyboard | High | | |
| TC-PS-05 | Custom name is accepted and displayed correctly | High | | |
| TC-PS-06 | Tapping [START] hides Player Selector panel | Critical | | |
| TC-PS-07 | Timer starts immediately after [START] is tapped | Critical | | |

---

## Section 3 — Shuffle Display

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-SD-01 | Shuffle order display is visible in HUD | Critical | | |
| TC-SD-02 | 4 colored dots shown in correct shuffle order colors | High | | |
| TC-SD-03 | First dot (Blue) is highlighted/active at start | High | | |
| TC-SD-04 | After ball transfers to cube N, dot N is marked complete | High | | |
| TC-SD-05 | Next dot in sequence becomes highlighted after each transfer | High | | |
| TC-SD-06 | All 4 dots are shown in correct positions with arrows between | Medium | | |
| TC-SD-07 | Shuffle order is different between game sessions (random) | Medium | | |
| TC-SD-08 | Blue is always the first dot (ball always starts in Blue) | Critical | | |

---

## Section 4 — Timer

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-T-01 | Timer displays "00:00" before game starts | High | | |
| TC-T-02 | Timer starts counting up after player confirms | Critical | | |
| TC-T-03 | Timer format is MM:SS | High | | |
| TC-T-04 | Timer counts accurately (10 seconds real time = 00:10 displayed) | High | | |
| TC-T-05 | Timer pauses when app goes to background | High | | |
| TC-T-06 | Timer resumes correctly when app returns to foreground | High | | |
| TC-T-07 | Timer stops when Level Complete triggers | Critical | | |
| TC-T-08 | Timer value shown on Level Complete screen matches stop time | High | | |

---

## Section 5 — Touch Controls

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-TC-01 | Tapping a cube selects it (outline/highlight appears) | High | | |
| TC-TC-02 | Tapping elsewhere deselects the cube | High | | |
| TC-TC-03 | Tapping same cube again deselects it | Medium | | |
| TC-TC-04 | Swiping up on a cube rotates it +90° | Critical | | |
| TC-TC-05 | Swiping down on a cube rotates it −90° | Critical | | |
| TC-TC-06 | Short tap is not mistaken for a swipe | High | | |
| TC-TC-07 | Slow swipe (> 400ms) is not recognized as rotation trigger | High | | |
| TC-TC-08 | Swipe distance < 30px is not recognized | High | | |
| TC-TC-09 | Long press (≥ 600ms) locks the cube (lock icon appears) | High | | |
| TC-TC-10 | Swiping a locked cube does not rotate it | High | | |
| TC-TC-11 | Second long press unlocks the cube | High | | |
| TC-TC-12 | Touch on UI elements does not trigger cube selection | High | | |
| TC-TC-13 | Touch is blocked during rotation animation | Critical | | |
| TC-TC-14 | Touch is blocked during ball transfer animation | Critical | | |
| TC-TC-15 | Blue cube cannot be rotated (it is the anchor) | Critical | | |

---

## Section 6 — Rotation Arrow Buttons

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-AB-01 | Arrow buttons are hidden when no cube is selected | High | | |
| TC-AB-02 | Arrow buttons appear when a cube is selected | High | | |
| TC-AB-03 | Selected cube name shown between arrow buttons | Medium | | |
| TC-AB-04 | Tapping [◀] rotates selected cube −90° | Critical | | |
| TC-AB-05 | Tapping [▶] rotates selected cube +90° | Critical | | |
| TC-AB-06 | Buttons are grayed out during rotation animation | High | | |
| TC-AB-07 | Buttons are grayed out during ball transfer | High | | |
| TC-AB-08 | Buttons are grayed out when cube is locked | High | | |

---

## Section 7 — Cube Rotation

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-CR-01 | Cube rotates smoothly (no jitter, no instant snap) | High | | |
| TC-CR-02 | Cube rotation is exactly 90° per input | Critical | | |
| TC-CR-03 | Cube snaps cleanly to 0°, 90°, 180°, 270° | High | | |
| TC-CR-04 | Only one cube rotates at a time | Critical | | |
| TC-CR-05 | Cube rotation is around the top hinge (not center) | Critical | | |
| TC-CR-06 | Adjacent cube does not move when one cube is rotated | Critical | | |
| TC-CR-07 | Hinge position remains correct after multiple rotations | High | | |
| TC-CR-08 | No physics artifacts during rotation (no bouncing/sliding) | Critical | | |

---

## Section 8 — Hole Alignment & Ball Transfer

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-HA-01 | Ball does NOT transfer when holes are misaligned | Critical | | |
| TC-HA-02 | Ball transfers automatically when holes align (0° match) | Critical | | |
| TC-HA-03 | Ball transfers when both cubes are at matching 180° rotation | High | | |
| TC-HA-04 | Ball transfer animation is smooth (no teleport) | High | | |
| TC-HA-05 | Ball moves along X axis (through the holes, not through walls) | Critical | | |
| TC-HA-06 | Ball is inside the destination cube after transfer completes | Critical | | |
| TC-HA-07 | Ball cannot transfer backwards (must follow shuffle order) | Critical | | |
| TC-HA-08 | Ball transfer does not trigger if next cube in order is not adjacent | High | | |
| TC-HA-09 | Input is blocked for the full duration of ball transfer | Critical | | |
| TC-HA-10 | Ball transfer completes even if player attempts input during it | High | | |

---

## Section 9 — Text Mode

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-TM-01 | Text mode toggle button is visible in header | High | | |
| TC-TM-02 | Tapping toggle turns text mode ON | High | | |
| TC-TM-03 | Cube name labels appear in shuffle display when text mode ON | High | | |
| TC-TM-04 | Tapping toggle again turns text mode OFF | High | | |
| TC-TM-05 | Cube name labels disappear when text mode OFF | High | | |
| TC-TM-06 | Text mode can be toggled during active gameplay | Medium | | |
| TC-TM-07 | Text is readable on 5" phone screen | High | | |

---

## Section 10 — Level Complete

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-LC-01 | Level Complete overlay appears after ball reaches final cube | Critical | | |
| TC-LC-02 | Level Complete overlay shows "Level Complete" message | High | | |
| TC-LC-03 | Player name is shown correctly on Level Complete screen | High | | |
| TC-LC-04 | Final time (MM:SS) is shown correctly | High | | |
| TC-LC-05 | [PLAY AGAIN] button reloads Level 1 correctly | High | | |
| TC-LC-06 | [MAIN MENU] button navigates to Main Menu scene | Medium | | |
| TC-LC-07 | Timer does not continue running after Level Complete | Critical | | |
| TC-LC-08 | New shuffle order is generated on Play Again | High | | |

---

## Section 11 — Performance & Stability

| ID | Test | Priority | Result | Notes |
|----|------|----------|--------|-------|
| TC-PF-01 | App does not crash during 5 minutes of normal play | Critical | | |
| TC-PF-02 | Frame rate stays ≥ 30 FPS during cube rotation | High | | |
| TC-PF-03 | Frame rate stays ≥ 30 FPS during ball transfer | High | | |
| TC-PF-04 | APK size is < 50 MB | Medium | | |
| TC-PF-05 | No memory leak after 3+ full level completions | High | | |
| TC-PF-06 | App resumes correctly after phone call interruption | High | | |
| TC-PF-07 | App resumes correctly after notification interruption | Medium | | |
| TC-PF-08 | Screen does not stay on permanently (battery concern) | Low | | |

---

## Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Tester | | | |
| Project Manager | | | |
| Developer | | | |

---

## Defect Summary

| Defect ID | Title | Severity | Status |
|-----------|-------|----------|--------|
| | | | |

> Fill in during test execution. Attach defect reports to this document.
