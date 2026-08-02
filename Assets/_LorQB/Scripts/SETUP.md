# LorQB V2 — Unity Prototype Setup

## What's in this folder

| File | Purpose |
|---|---|
| `LorQBTypes.cs` | Enums, all geometry constants, chapter table |
| `LorQBSceneBuilder.cs` | Builds the full 3D scene programmatically on Play |
| `LorQBGameManager.cs` | State machine, chapter execution, win logic |
| `LorQBCubeNode.cs` | Per-cube glow, color, state visuals |
| `LorQBBallController.cs` | Ball arc movement + color blending |
| `LorQBInputHandler.cs` | Android touch + Editor mouse raycast |

---

## Drop-in steps (takes ~5 minutes)

1. **Copy all `.cs` files** into `Assets/LorQB/Scripts/` in your Unity project.

2. **Create an empty scene** (or a clean scene — the builder replaces any manual setup).

3. **Create one empty GameObject**, name it `LorQBSceneBuilder`.

4. **Attach `LorQBSceneBuilder.cs`** to that GameObject.

5. **Press Play.** The scene builds itself: 4 glass cubes, 3 hinges, ball, lights, camera.

6. **Test interaction:**
   - In Editor: left-click the **Red** cube → ball should arc from Blue to Red.
   - Continue: click Green → Yellow.
   - On Android: tap the next cube in sequence.

---

## ROT_SIGN verification (Rule 5 — do this before calling it done)

Each chapter has a `RotSign` value in `LorQBTypes.cs` → `LorQBConfig.Chapters[]`.
These are **placeholders** and must be verified empirically:

```
C12 (Blue→Red):   RotSign =  1f  ← verify
C13 (Red→Green):  RotSign =  1f  ← verify
C14 (Green→Yel):  RotSign = -1f  ← verify (C14 is the known active bug chapter)
C15 (Yel→Blue):   RotSign = -1f  ← verify
```

**How to verify:** Press Play → trigger C12 → watch the hinge cylinder rotate.
- If it rotates away from Red (wrong direction): flip the sign in `LorQBTypes.cs`.
- Repeat for each chapter independently. Never infer one from another.

---

## Android build

1. In **Build Settings**: switch platform to Android, add the scene.
2. In **Player Settings**: set orientation to Landscape (or Portrait — your call).
3. The touch input in `LorQBInputHandler.cs` uses `Input.GetTouch(0)` — works with Unity's default touch system. No extra packages needed.
4. Build & Run.

---

## What this prototype does NOT do yet

- T-series (diagonal) transfers — only C-series ring (Blue→Red→Green→Yellow→Blue)
- Physics-based ball rolling (ball arcs programmatically, not via Rigidbody)
- Canvas/UI (uses legacy OnGUI — replace with a proper Canvas for production)
- Obstacle cubes, timer, level system
- Blender-verified hinge rotation angles (90° is used; verify empirically)

---

## File dependencies

```
LorQBTypes.cs
   ↑ referenced by all others

LorQBSceneBuilder.cs
   → creates and wires: LorQBGameManager, LorQBCubeNode, LorQBBallController, LorQBInputHandler

LorQBGameManager.cs
   → drives: LorQBCubeNode (SetState), LorQBBallController (ArcToSeat), hinge GameObjects

LorQBInputHandler.cs
   → calls: LorQBGameManager.OnCubeTapped()
```

No additional packages, no TextMeshPro dependency, no Addressables. Works with Unity 6 built-in renderer and URP (material creation uses Standard shader fallback).
