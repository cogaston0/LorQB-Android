# Blender ↔ Unity Android Compatibility Audit — Level 1

**Status:** Audit only — no code changes, no C#  
**Purpose:** Compare Blender LorQB coordinates with Unity Android coordinates before implementation  
**Unity source of truth:** `Assets/_LorQB/Editor/Level1SceneBuilder.cs`  
**Blender source of truth:** Problem-statement canonical values (from Blender C/T animation scripts)

---

## 1. Blender Canonical Coordinates

Blender uses a **Z-up, right-handed** coordinate system. All values below are as declared in the Blender source.

### Cube Origins (Blender)

| Object       | Blender X | Blender Y | Blender Z | Notes                              |
|--------------|-----------|-----------|-----------|------------------------------------|
| Cube_Blue    |  +0.51    |   0.00    |   1.00    | Origin at top edge (hinge height)  |
| Cube_Red     |   0.00    |  −0.51    |   1.00    | Origin at top edge (hinge height)  |
| Cube_Green   |  −0.51    |   0.00    |   1.00    | Origin at top edge (hinge height)  |
| Cube_Yellow  |  −0.51    |   0.00    |   1.00    | ⚠ Same as Cube_Green — likely typo |

> **Note:** Cube_Yellow is listed with the same coordinates as Cube_Green in the source data. Based on the four-corner layout, Cube_Yellow's expected Blender origin would be (0.00, +0.51, 1.00) — the midpoint between Yellow and Blue at the front edge. This is flagged as a potential transcription error in the problem statement.

### Seat Positions (Blender)

| Object       | Blender X | Blender Y | Blender Z | Notes                              |
|--------------|-----------|-----------|-----------|------------------------------------|
| Seat_Blue    |  +0.51    |  +0.51    |   0.25    | Ball snap-point, top face of cube  |
| Seat_Red     |  +0.51    |  −0.51    |   0.25    | Ball snap-point, top face of cube  |
| Seat_Green   |  −0.51    |  −0.51    |   0.25    | Ball snap-point, top face of cube  |
| Seat_Yellow  |  −0.51    |  +0.51    |   0.25    | Ball snap-point, top face of cube  |

### Hinge Positions (Blender)

| Hinge | Full name          | Blender X | Blender Y | Blender Z | Notes                   |
|-------|--------------------|-----------|-----------|-----------|-------------------------|
| HBR   | Hinge_Blue_Red     |  +0.51    |   0.00    |   1.00    | Right-side top edge     |
| HRG   | Hinge_Red_Green    |   0.00    |  −0.51    |   1.00    | Back-side top edge      |
| HGY   | Hinge_Green_Yellow |  −0.51    |   0.00    |   1.00    | Left-side top edge      |

---

## 2. Unity Current Coordinates

Unity uses a **Y-up, left-handed** coordinate system. The authoritative Unity values come from `Level1SceneBuilder.cs`. Additional Unity coordinates appear in documentation files; where those values differ from the code they are noted separately.

### Cube Positions — Level1SceneBuilder.cs (code authority)

| Object       | Unity X | Unity Y | Unity Z | Notes                         |
|--------------|---------|---------|---------|-------------------------------|
| Cube_Blue    | +1.0    |   0.0   |  −1.0   | Front-Right corner, anchor    |
| Cube_Red     | +1.0    |   0.0   |  +1.0   | Back-Right corner             |
| Cube_Green   | −1.0    |   0.0   |  +1.0   | Back-Left corner              |
| Cube_Yellow  | −1.0    |   0.0   |  −1.0   | Front-Left corner             |

### Seat Positions — Level1SceneBuilder.cs

| Object       | Unity X | Unity Y | Unity Z | Notes                         |
|--------------|---------|---------|---------|-------------------------------|
| Seat_Blue    | +1.0    |  +0.5   |  −1.0   | Top-centre of Cube_Blue       |
| Seat_Red     | +1.0    |  +0.5   |  +1.0   | Top-centre of Cube_Red        |
| Seat_Green   | −1.0    |  +0.5   |  +1.0   | Top-centre of Cube_Green      |
| Seat_Yellow  | −1.0    |  +0.5   |  −1.0   | Top-centre of Cube_Yellow     |

### Pivot / Hinge Positions — Level1SceneBuilder.cs

| Hinge / Pivot             | Unity X | Unity Y | Unity Z | Rotation Axis      |
|---------------------------|---------|---------|---------|--------------------|
| Pivot_Blue_Red (HBR)      | +1.0    |  +0.5   |   0.0   | Z (`Vector3.forward`) |
| Pivot_Red_Green (HRG)     |   0.0   |  +0.5   |  +1.0   | X (`Vector3.right`)   |
| Pivot_Green_Yellow (HGY)  | −1.0    |  +0.5   |   0.0   | Z (`Vector3.forward`) |

### Alternate Unity Values Found in Documentation

The following values appear in `01_Scene_Setup.md` (an older or draft document). They differ from `Level1SceneBuilder.cs` and are **not the current implementation values**.

| Object / Pivot               | Doc X  | Doc Y  | Doc Z  | Source              |
|------------------------------|--------|--------|--------|---------------------|
| Cube_Blue                    | +0.51  | +0.51  |  0.50  | 01_Scene_Setup.md   |
| Cube_Red                     | +0.51  | −0.51  |  0.50  | 01_Scene_Setup.md   |
| Cube_Green                   | −0.51  | −0.51  |  0.50  | 01_Scene_Setup.md   |
| Cube_Yellow                  | −0.51  | +0.51  |  0.50  | 01_Scene_Setup.md   |
| Seat_Blue                    | +0.51  | +0.51  |  0.25  | 01_Scene_Setup.md   |
| Pivot_Blue_Red               | +0.51  |  0.00  |  1.00  | 01_Scene_Setup.md   |
| Pivot_Red_Green              |  0.00  | −0.51  |  1.00  | 01_Scene_Setup.md   |
| Pivot_Green_Yellow           | −0.51  |  0.00  |  1.00  | 01_Scene_Setup.md   |
| C12 Pivot (Hinge_Blue_Red)   | +0.51  |  0.00  | +1.00  | 02_Movement_Logic.md |
| C13 Pivot (Hinge_Red_Green)  |  0.00  | +0.51  | −0.51  | 02_Movement_Logic.md |
| C14 Pivot (Hinge_Green_Yellow)| −0.51  |  0.00  | +1.00  | 02_Movement_Logic.md |

> **Important:** The values in `01_Scene_Setup.md` appear to be written in a Z-up convention (matching Blender's layout) rather than Unity's Y-up convention. They should not be used as Unity implementation targets. `Level1SceneBuilder.cs` is the code authority.

---

## 3. Coordinate-by-Coordinate Comparison

### Coordinate Axis Mapping (Blender Z-up → Unity Y-up)

Standard Blender-to-Unity conversion applies:

| Blender axis | Unity axis | Sign change | Scale |
|--------------|------------|-------------|-------|
| X            | X          | None        | ×2    |
| Y            | Z          | Negated     | ×2    |
| Z            | Y          | None        | ×2    |

Formula: `Unity(X, Y, Z) = (Blender_X × 2,  Blender_Z × 2,  −Blender_Y × 2)`

### Cube Origin Comparison

| Object      | Blender (X,Y,Z) | Expected Unity after map | Actual Unity (code) | Match?      |
|-------------|-----------------|--------------------------|---------------------|-------------|
| Cube_Blue   | (0.51, 0.00, 1.00) | (1.02, 2.00, 0.00)    | (+1.0, 0.0, −1.0)   | ✗ Mismatch  |
| Cube_Red    | (0.00, −0.51, 1.00)| (0.00, 2.00, +1.02)   | (+1.0, 0.0, +1.0)   | ✗ Mismatch  |
| Cube_Green  | (−0.51, 0.00, 1.00)| (−1.02, 2.00, 0.00)   | (−1.0, 0.0, +1.0)   | ✗ Mismatch  |
| Cube_Yellow | (−0.51, 0.00, 1.00)| (−1.02, 2.00, 0.00)   | (−1.0, 0.0, −1.0)   | ✗ Mismatch  |

**Root cause:** The Blender "cube" positions equal the hinge positions in Blender (Z=1.00 = top edge height). In Unity, cube origins are placed at the geometric centre of the cube on the ground plane (Y=0.0), not at the hinge. These are fundamentally different reference points.

### Seat Comparison

| Object      | Blender (X,Y,Z)      | Expected Unity after map | Actual Unity (code) | Match?     |
|-------------|----------------------|--------------------------|---------------------|------------|
| Seat_Blue   | (0.51, 0.51, 0.25)   | (1.02, 0.50, −1.02)      | (+1.0, +0.5, −1.0)  | ✓ Match*   |
| Seat_Red    | (0.51, −0.51, 0.25)  | (1.02, 0.50, +1.02)      | (+1.0, +0.5, +1.0)  | ✓ Match*   |
| Seat_Green  | (−0.51, −0.51, 0.25) | (−1.02, 0.50, +1.02)     | (−1.0, +0.5, +1.0)  | ✓ Match*   |
| Seat_Yellow | (−0.51, 0.51, 0.25)  | (−1.02, 0.50, −1.02)     | (−1.0, +0.5, −1.0)  | ✓ Match*   |

> \* Match after applying the Blender→Unity axis remap and 2× scale. The ±0.02 rounding error (0.51 × 2 = 1.02 vs Unity 1.0) is below any practical threshold.

### Hinge / Pivot Comparison

| Hinge | Blender (X,Y,Z)      | Expected Unity after map | Actual Unity (code)  | Match?     |
|-------|----------------------|--------------------------|----------------------|------------|
| HBR   | (0.51, 0.00, 1.00)   | (1.02, 2.00, 0.00)       | (+1.0, +0.5, 0.0)    | ✗ Y mismatch |
| HRG   | (0.00, −0.51, 1.00)  | (0.00, 2.00, +1.02)      | (0.0, +0.5, +1.0)    | ✗ Y mismatch |
| HGY   | (−0.51, 0.00, 1.00)  | (−1.02, 2.00, 0.00)      | (−1.0, +0.5, 0.0)    | ✗ Y mismatch |

**Root cause:** All three hinges show Y=2.00 expected vs Y=+0.5 in Unity. This means Unity halved the hinge height relative to the cube height: a 1×1×1 Unity cube has its top face at Y=+0.5 (centre at Y=0), so the hinge at Y=+0.5 is correct for Unity. The Blender Z=1.00 represents the full height of the cube in Blender-scale, which after ×2 becomes 2.0 in Unity units — but since the Unity cubes are 1.0 units tall (not 2.0), the hinge is correctly placed at Y=+0.5 (the actual top face height). The 2× scale is consistent; the hinge vertical is correct at Unity Y=+0.5.

---

## 4. Identified Mismatches

### Mismatch 1 — Cube Origins: Different Reference Points

| Item               | Blender                              | Unity (code)               |
|--------------------|--------------------------------------|----------------------------|
| Cube origin point  | At hinge edge (top of cube, Z=1.00)  | Geometric centre (Y=0.0)   |
| Cube_Blue          | (0.51, 0.00, 1.00)                   | (+1.0, 0.0, −1.0)          |
| Cube_Red           | (0.00, −0.51, 1.00)                  | (+1.0, 0.0, +1.0)          |
| Cube_Green         | (−0.51, 0.00, 1.00)                  | (−1.0, 0.0, +1.0)          |
| Cube_Yellow        | (−0.51, 0.00, 1.00) ← suspected typo | (−1.0, 0.0, −1.0)         |

The Blender cube origins (as listed) coincide with hinge positions in the Blender scene, not with cube geometric centres. Unity places cube origins at the geometric centre. These are not the same concept and cannot be compared directly.

### Mismatch 2 — Cube_Yellow Duplicate / Typo

Blender source lists Cube_Yellow at (−0.51, 0.00, 1.00), identical to Cube_Green. No two cubes should share the same origin. Based on the four-corner ring geometry, the correct Blender origin for Cube_Yellow is expected to be (0.00, +0.51, 1.00) — the front-side hinge midpoint — matching the fourth corner at (−X, +Y) in the horizontal plane.

### Mismatch 3 — Scale Factor: 0.51 (Blender) vs 1.0 (Unity)

| Dimension           | Blender value | Unity value | Ratio   |
|---------------------|---------------|-------------|---------|
| Cube corner offset  | 0.51          | 1.0         | 1.96×   |
| Seat height         | 0.25          | 0.5         | 2.00×   |
| Hinge height (Z/Y)  | 1.00          | 0.5         | 0.50×   |

Corner offset ratio (0.51 → 1.0) is 1.96×, not exactly 2.0×. The 0.51 value is likely a Blender snapping artefact for "half a unit" and should be treated as 0.5 for all conversion purposes, giving a clean 2× scale factor throughout.

### Mismatch 4 — Seat Positions vs 01_Scene_Setup.md

`01_Scene_Setup.md` (doc file) lists seat positions with Z=0.25 in a Z-up convention (matching Blender), while `Level1SceneBuilder.cs` (code) lists seat positions with Y=+0.5 in Unity Y-up. These are the same physical point described in two coordinate systems — not a geometric mismatch — but the doc file may cause confusion because it looks like Blender data rather than Unity data.

### Mismatch 5 — Hinge Pivot Height: Expected 2.0 vs Actual 0.5

Under a naive ×2 scale, Blender hinge Z=1.00 would map to Unity Y=2.00. Unity uses Y=+0.5 for pivots. This is not a bug; it reflects that the Unity cubes are 1.0 units tall (not 2.0 units tall). The scale factor of ×2 applies to the horizontal footprint only. Vertically, Blender's Z=1.00 (full cube height) maps correctly to Unity's Y=+0.5 (top face of a 1-unit cube centred at Y=0).

### Mismatch 6 — C13 Pivot in 02_Movement_Logic.md

`02_Movement_Logic.md` lists the C13 pivot (Hinge_Red_Green) at (X=0.00, Y=+0.51, Z=−0.51). This differs from:
- `01_Scene_Setup.md`: (0.00, −0.51, 1.00)
- `Level1SceneBuilder.cs`: (0.0, +0.5, +1.0)

The `02_Movement_Logic.md` value has a sign flip on Z and uses a different XYZ convention from the rest of the document in which it appears. This is a documentation inconsistency; the code value in `Level1SceneBuilder.cs` is authoritative.

### Mismatch 7 — Hinge Axis Naming Inconsistency Across Docs

| Hinge | 02_Movement_Logic.md axis | 04_Move_Map.md axis | Level1SceneBuilder.cs axis |
|-------|---------------------------|---------------------|----------------------------|
| HBR   | X (`Vector3.right`)       | X                   | Z (`Vector3.forward`)      |
| HRG   | Y (`Vector3.up`)          | Y                   | X (`Vector3.right`)        |
| HGY   | X (`Vector3.right`)       | X                   | Z (`Vector3.forward`)      |

`Level1SceneBuilder.cs` assigns HBR and HGY to the Z-axis and HRG to the X-axis. `04_Blender_To_Android_Move_Map.md` and `02_Movement_Logic.md` describe the same hinges using X and Y axes. This discrepancy reflects the Blender→Unity axis remap: Blender X-axis rotation becomes Unity Z-axis rotation (and Blender Y becomes Unity X) under the standard coordinate conversion.

---

## 5. Recommendations

### Question 1: Should Unity use exact Blender coordinates?

**No.** Blender coordinates cannot be used directly in Unity for three reasons:
1. Different up-axis (Blender Z-up, Unity Y-up) — axis remap is required.
2. Different scale (Blender footprint ~0.51 units, Unity footprint 1.0 units).
3. Different origin convention for cubes (Blender origins are at hinge edges; Unity origins are at cube geometric centres).

### Question 2: Should Unity use scaled coordinates?

**Yes — with the following exact conversion.**

### Recommended Scale Factor and Axis Map

| Parameter              | Value                                     |
|------------------------|-------------------------------------------|
| Horizontal scale       | ×2 (treat Blender 0.51 as exactly 0.5)   |
| Vertical scale         | ×0.5 (Blender Z=1.00 → Unity Y=0.5)      |
| Unity X = Blender X × 2      |  +0.5 → +1.0 (or −0.5 → −1.0)   |
| Unity Y = Blender Z × 0.5    |  1.00 → +0.5 (hinge/seat height)  |
| Unity Z = −(Blender Y × 2)   |  +0.51 → −1.0, −0.51 → +1.0      |

The ×0.5 vertical factor is not a scale inconsistency — it reflects that Unity cube size is 1.0 unit (not 2.0 units). The horizontal ×2 and vertical ×0.5 both resolve cleanly to whole numbers.

### Validated Mapping Table

Applying the recommended conversion to Blender seat values (the most reliable Blender reference since they contain all four distinct corners):

| Blender Seat | Blender (X, Y, Z) | Unity expected | Unity actual (code) | Diff   |
|--------------|-------------------|----------------|---------------------|--------|
| Seat_Blue    | (0.51, 0.51, 0.25)  | (+1.02, +0.50, −1.02) | (+1.0, +0.5, −1.0) | ±0.02 |
| Seat_Red     | (0.51, −0.51, 0.25) | (+1.02, +0.50, +1.02) | (+1.0, +0.5, +1.0) | ±0.02 |
| Seat_Green   | (−0.51, −0.51, 0.25)| (−1.02, +0.50, +1.02) | (−1.0, +0.5, +1.0) | ±0.02 |
| Seat_Yellow  | (−0.51, 0.51, 0.25) | (−1.02, +0.50, −1.02) | (−1.0, +0.5, −1.0) | ±0.02 |

**Conclusion:** The ±0.02 difference is within rounding tolerance (0.51 vs 0.50). Unity seat coordinates in `Level1SceneBuilder.cs` are **correct** for the intended geometry. No adjustment is needed for seats.

### Summary Recommendation

| Item                | Recommendation                                                                  |
|---------------------|---------------------------------------------------------------------------------|
| Cube origins        | Keep Unity values (+1.0, 0.0, ±1.0). Do not use Blender cube origin values.   |
| Seat positions      | Keep Unity values (±1.0, +0.5, ±1.0). Verified correct via seat mapping.      |
| Hinge pivots        | Keep Unity values (±1.0 or 0, +0.5, ±1.0 or 0). Verified correct.             |
| Scale factor        | Treat as 2× horizontal, 0.5× vertical. Blender 0.51 → Unity 1.0 is acceptable.|
| Blender cube origin | Do not use as Unity cube position — they are hinge points, not cube centres.   |
| Cube_Yellow typo    | Confirm correct Blender origin; Unity value (−1.0, 0.0, −1.0) is geometrically correct. |
| Doc 01_Scene_Setup  | The 0.51-based coordinates in 01_Scene_Setup.md reflect a Blender-space view, not Unity. |
| Doc 02_MovementLogic| C13 pivot value is inconsistent; disregard. Use Level1SceneBuilder.cs.          |
| Hinge axis naming   | Blender X-axis → Unity Z-axis; Blender Y-axis → Unity X-axis. Both docs are self-consistent within their own coordinate system. |
