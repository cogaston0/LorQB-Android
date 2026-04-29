# Agent 06 — Testing

## Role
Define and execute the Quality Assurance (QA) strategy for LorQB / Cubolita Level 1. This agent does not write gameplay scripts but is responsible for writing Unity Test Runner tests, executing manual test checklists, and reporting defects.

---

## Testing Strategy

### Testing Layers

| Layer | Tool | When |
|-------|------|------|
| Unit Tests | Unity Test Runner (Edit Mode) | After each script is written |
| Integration Tests | Unity Test Runner (Play Mode) | After Phase 3 & 4 complete |
| Manual Functional Tests | Test checklist (`TESTING/Level_1_Test_Checklist.md`) | After each phase |
| Device Tests | Physical Android device or emulator | Phase 4+ |
| Performance Tests | Unity Profiler | Before release candidate |

---

## Edit Mode Unit Tests

### Test Suite: MovementLogic

**File:** `Assets/Tests/EditMode/MovementLogicTests.cs`

```
TC-U-01: CubeRotationController initializes with angle 0°
TC-U-02: RotateTo(90) sets target angle to 90°
TC-U-03: IsRotating returns true while rotation is in progress
TC-U-04: IsRotating returns false after OnRotationComplete fires
TC-U-05: HoleAlignmentDetector returns true when both cubes at 0°
TC-U-06: HoleAlignmentDetector returns true when both cubes at 180°
TC-U-07: HoleAlignmentDetector returns false when cubes differ by 90°
TC-U-08: HoleAlignmentDetector fires OnHolesAligned exactly once per alignment
TC-U-09: BallTransferController.CanTransfer() returns false when ball is in transit
TC-U-10: BallTransferController advances currentCubeIndex on successful transfer
```

### Test Suite: ShuffleOrder

**File:** `Assets/Tests/EditMode/ShuffleOrderTests.cs`

```
TC-U-11: GenerateOrder returns array of length 4
TC-U-12: GenerateOrder first element is always Blue
TC-U-13: GenerateOrder contains each cube color exactly once
TC-U-14: GameTimerController.FormattedTime returns "00:00" at start
TC-U-15: GameTimerController.FormattedTime returns "01:00" after 60 seconds
```

### Test Suite: InputSystem

**File:** `Assets/Tests/EditMode/InputSystemTests.cs`

```
TC-U-16: TouchInputManager.IsInputBlocked returns false in IDLE state
TC-U-17: TouchInputManager.BlockInput() sets IsInputBlocked to true
TC-U-18: TouchInputManager.UnblockInput() sets IsInputBlocked to false
TC-U-19: Swipe gesture with distance < minSwipeDistance is not recognized
TC-U-20: Swipe gesture with duration > maxSwipeDuration is not recognized
```

---

## Play Mode Integration Tests

**File:** `Assets/Tests/PlayMode/Level1IntegrationTests.cs`

```
TC-P-01: Scene loads without errors or exceptions
TC-P-02: All 4 cubes are present in scene after load
TC-P-03: Ball starts inside Blue cube
TC-P-04: Rotating Red cube 90° does NOT transfer ball (holes misaligned)
TC-P-05: Rotating Red cube to match Blue hole axis transfers ball Blue→Red
TC-P-06: Ball position is inside Red cube after transfer from Blue
TC-P-07: Ball cannot transfer backwards (must follow shuffle order)
TC-P-08: Timer starts at 0 and counts up after player confirms
TC-P-09: Shuffle display advances step after each ball transfer
TC-P-10: Level complete fires after ball reaches final cube in shuffle order
TC-P-11: Level complete screen shows correct elapsed time
TC-P-12: Input is blocked for the duration of rotation animation
TC-P-13: Input is blocked for the duration of ball transfer animation
TC-P-14: Long press locks cube (subsequent swipe does not rotate)
TC-P-15: Second long press unlocks cube (subsequent swipe rotates)
```

---

## Manual Device Tests

### Functional Tests — Performed on Android Device

See full checklist in `TESTING/Level_1_Test_Checklist.md`.

Key test areas:
1. Scene loads on target Android device
2. All cubes render correctly (transparent, correct colors)
3. Touch swipe correctly identifies cube and direction
4. Cube rotates smoothly (no jitter, no physics artifacts)
5. Hole alignment correctly detected
6. Ball transfers smoothly through hole
7. Timer visible and accurate
8. Shuffle display updates correctly
9. Player selector works with touch keyboard
10. Text mode toggle works
11. Level complete screen appears
12. Play Again reloads correctly

---

## Performance Benchmarks

| Metric | Target | Fail Threshold |
|--------|--------|----------------|
| Frame rate | ≥ 60 FPS | < 30 FPS |
| Scene load time | < 2 seconds | > 5 seconds |
| APK size | < 50 MB | > 100 MB |
| Memory usage | < 200 MB RAM | > 400 MB RAM |
| Battery drain | Normal | Excessive (device hot) |

---

## Defect Reporting Format

When a defect is found, report it using this format:

```
DEFECT-XXX
Title: [short description]
Phase: [phase number where found]
Severity: Critical / High / Medium / Low
Steps to reproduce:
  1. ...
  2. ...
Expected result: ...
Actual result: ...
Screenshot/video: [attach if applicable]
Assigned to: Agent [NN]
```

---

## Regression Policy

- All unit tests must pass before any Phase gate is opened
- All play mode tests for a phase must pass before moving to the next phase
- Any defect rated Critical or High blocks the phase gate
- Defects must be fixed in the same phase they are found (no carry-forward)

---

## Deliverables (Phase 6)

- [ ] All Edit Mode unit tests written and passing
- [ ] All Play Mode integration tests written and passing
- [ ] Manual test checklist completed and signed off
- [ ] No Critical or High defects open
- [ ] Performance benchmarks met on target Android device
- [ ] APK build tested and verified
