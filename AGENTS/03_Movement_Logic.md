# Agent 03 — Movement Logic

## Role
Implement all cube rotation, hinge constraint logic, and ball transfer mechanics using **scripted transform-based movement only**. No physics engine. No bones or drivers.

---

## Core Constraints

- **No Rigidbody, no physics joints, no Collider-based movement**
- **No bones, armatures, or Blender drivers**
- All rotations are **scripted** using `Transform.rotation` / `Quaternion.RotateTowards` or eased lerp
- Ball position is **calculated mathematically**, not simulated
- Hinge rotation is **single-axis only** (Z axis, top hinge)

---

## System Overview

```
CubeRotationController
    ├── Rotates a cube around its top hinge (Z axis)
    ├── Accepts target angle from Touch/UI input
    ├── Eases rotation using Lerp or SmoothDamp
    └── Fires OnRotationComplete event when done

HoleAlignmentDetector
    ├── Reads current rotation of each cube pair
    ├── Calculates angular offset between adjacent hole axes
    ├── Returns bool: IsAligned (within threshold ± N degrees)
    └── Fires OnHolesAligned event

BallTransferController
    ├── Listens to OnHolesAligned event
    ├── Moves ball from current cube to next cube in shuffle order
    ├── Animates ball along local X axis through the hole
    └── Updates CurrentCube reference
```

---

## Script Specifications

### 3.1 CubeRotationController.cs

**Namespace:** `LorQB.Movement`

**Fields:**
```
[SerializeField] Transform hingeTransform     // the hinge empty GO
[SerializeField] float rotationSpeed = 90f    // degrees per second
[SerializeField] float[] allowedAngles        // snap angles (e.g. 0, 90, 180, 270)
```

**Public Methods:**
```
void RotateTo(float targetAngle)
    // Initiates smooth rotation to target angle around Z axis
    // Does NOT use physics

void SnapToNearest()
    // Snaps cube to nearest allowed angle immediately

bool IsRotating { get; }
    // Returns true while a rotation is in progress
```

**Events:**
```
event Action OnRotationComplete
    // Fired when rotation reaches target angle (within 0.5° threshold)
```

**Implementation Notes:**
- Use `Mathf.MoveTowardsAngle` or `Quaternion.RotateTowards` — never physics
- Rotation is about the **hinge's local Z axis**
- Only one cube may rotate at a time (enforced by GameManager lock)
- While ball is in transit, all rotation is **locked**

---

### 3.2 HoleAlignmentDetector.cs

**Namespace:** `LorQB.Movement`

**Fields:**
```
[SerializeField] Transform cubeA              // left cube
[SerializeField] Transform cubeB              // right cube (child of hinge)
[SerializeField] float alignmentThreshold = 5f // degrees
```

**Public Methods:**
```
bool AreHolesAligned()
    // Returns true if hole axes of cubeA and cubeB are within threshold

float GetAngularOffset()
    // Returns angular difference between the two hole axes in world space
```

**Events:**
```
event Action OnHolesAligned
    // Fired once per alignment event (debounced — not spammed each frame)

event Action OnHolesMisaligned
    // Fired when holes go out of alignment
```

**Implementation Notes:**
- Holes are defined by a **child empty GameObject** on each cube face named `HoleAxis`
- Alignment is checked each frame while a rotation is active
- Use `Vector3.Angle` between the two `HoleAxis.forward` vectors

---

### 3.3 BallTransferController.cs

**Namespace:** `LorQB.Movement`

**Fields:**
```
[SerializeField] Transform ballTransform
[SerializeField] float transferSpeed = 2f     // units per second
[SerializeField] ShuffleOrder shuffleOrder    // reference to UI/shuffle data
```

**State:**
```
int currentCubeIndex = 0                      // index into shuffle order
bool isTransferring = false
```

**Public Methods:**
```
void TriggerTransfer()
    // Called by HoleAlignmentDetector.OnHolesAligned
    // Moves ball from current cube center to next cube center
    // Uses Lerp over time — no physics

bool CanTransfer()
    // Returns true if holes are aligned AND ball is not already in transit
    // AND next cube in shuffle order is the correct adjacent cube
```

**Events:**
```
event Action<int> OnBallTransferred
    // Fires with new cube index after each successful transfer

event Action OnLevelComplete
    // Fires when ball reaches the final cube in the shuffle order
```

**Implementation Notes:**
- Ball movement path: straight line along world X axis through hole openings
- Ball position is interpolated between `cubeA.center` and `cubeB.center`
- The ball is **never** inside two cubes simultaneously
- Transfer is **atomic**: once started it cannot be interrupted

---

## Hinge Logic Rules

1. Each hinge connects two adjacent cubes at their **shared top edge**.
2. The **left cube** (anchor) does not move — only the **right cube** (child) rotates.
3. Rotation is about the **hinge's Z axis** only.
4. Allowed rotation states per cube:
   - `0°` — holes facing left/right (aligned for transfer)
   - `90°` — holes facing up/down
   - `180°` — holes facing left/right (inverted — still alignable if both 180°)
   - `270°` — holes facing up/down
5. Transfer is only possible when both adjacent cubes share the same effective hole direction.

---

## Rotation Sequencing

```
Player initiates rotation (touch or tap)
    │
    ▼
GameManager checks: isBallTransferring? → if yes, BLOCK rotation
    │
    ▼
CubeRotationController.RotateTo(targetAngle)
    │
    ▼
Each frame: HoleAlignmentDetector.AreHolesAligned()
    │
    ├── YES → HoleAlignmentDetector fires OnHolesAligned
    │              └── BallTransferController.TriggerTransfer()
    │                      └── Ball animates to next cube
    │                              └── OnBallTransferred fires
    │
    └── NO → continue rotation
            └── OnRotationComplete fires when target reached
```

---

## Deliverables (Phase 3)

- [ ] `CubeRotationController.cs` written and attached to each Cube prefab
- [ ] `HoleAlignmentDetector.cs` written and attached to each Hinge GO
- [ ] `BallTransferController.cs` written and attached to GameManager
- [ ] Single-cube rotation verified in Editor (no physics)
- [ ] Hole alignment detection triggers correctly at 0° and 180°
- [ ] Ball transfers between two adjacent cubes when holes align
- [ ] Ball does NOT transfer when holes are misaligned
- [ ] All 4-cube chain transfers tested in sequence
