# LorQB Unity Architecture — Visual Reference

## HIERARCHY STRUCTURE

### ❌ OLD (BROKEN) — NESTED HIERARCHY
```
Scene Root
└── LorQB_Runtime
	└── Cube_Blue
		└── Hinge_Blue_Red
			└── Cube_Red
				└── Hinge_Red_Green
					└── Cube_Green
						└── Hinge_Green_Yellow
							└── Cube_Yellow
```

**Problem:** Rotating Blue rotates entire chain → recursive inheritance corruption

---

### ✅ NEW (CORRECT) — FLAT HIERARCHY
```
Scene Root
├── LorQB_Runtime (management scripts)
└── LorQB_Root (scene objects)
	├── Cube_Blue
	├── Cube_Red
	├── Cube_Green
	├── Cube_Yellow
	├── Hinge_Blue_Red
	├── Hinge_Red_Green
	├── Hinge_Green_Yellow
	├── Ball
	├── Seat_Blue
	├── Seat_Red
	├── Seat_Green
	└── Seat_Yellow
```

**Benefit:** All objects independent → no recursive inheritance → stable rotations

---

## SPATIAL LAYOUT (Top View)

```
		 Y+
		  |
	Yellow|Blue
	  ●---+---●
		  |
   -------+-------  X+
		  |
	Green |Red
	  ●---+---●
		  |
```

**Positions (world coordinates):**
- Blue (top-right): (0.51, 0.5, 0.51)
- Red (bottom-right): (0.51, 0.5, -0.51)
- Green (bottom-left): (-0.51, 0.5, -0.51)
- Yellow (top-left): (-0.51, 0.5, 0.51)

**Hinges (world coordinates):**
- Hinge_Blue_Red: (0.51, 1.0, 0.0) — right edge, X-axis rotation
- Hinge_Red_Green: (0.0, 1.0, -0.51) — bottom edge, Y-axis rotation
- Hinge_Green_Yellow: (-0.51, 1.0, 0.0) — left edge, X-axis rotation
- [Implied Yellow-Blue connection at top edge]

---

## MOVEMENT PATTERN (Temporary Rotation Groups)

### C12: Blue → Red Transfer

**BEFORE:**
```
LorQB_Root
├── Cube_Blue ← independent
├── Hinge_Blue_Red ← independent
├── Cube_Red ← independent
└── ...
```

**DURING (Temporary Group):**
```
LorQB_Root
├── C12_RotGroup [pivot at Hinge_Blue_Red]
│   ├── Cube_Blue ← temporary child
│   ├── Hinge_Blue_Red ← temporary child
│   ├── Cube_Red ← temporary child
│   └── Ball ← temporary child
└── ... (other cubes remain independent)
```

Rotate `C12_RotGroup` by 90° → only Blue, HBR, Red, Ball affected

**AFTER (Restored):**
```
LorQB_Root
├── Cube_Blue ← back to sibling
├── Hinge_Blue_Red ← back to sibling
├── Cube_Red ← back to sibling (rotated)
├── Ball ← back to sibling (at Red seat)
└── ...

C12_RotGroup → DESTROYED
```

---

## ROTATION AXES

### Hinge_Blue_Red (right edge)
- Position: (0.51, 1.0, 0.0)
- Axis: Vector3.right (1, 0, 0)
- Rotation: X-axis
- Connects: Blue ↔ Red

### Hinge_Red_Green (bottom edge)
- Position: (0.0, 1.0, -0.51)
- Axis: Vector3.forward (0, 0, 1)
- Rotation: Y-axis (Z-axis in Unity)
- Connects: Red ↔ Green

### Hinge_Green_Yellow (left edge)
- Position: (-0.51, 1.0, 0.0)
- Axis: Vector3.right (1, 0, 0)
- Rotation: X-axis
- Connects: Green ↔ Yellow

### Yellow-Blue (top edge, C15)
- Position: (0.0, 1.0, 0.51) [implied]
- Axis: Vector3.forward (0, 0, 1)
- Rotation: Y-axis (Z-axis in Unity)
- Connects: Yellow ↔ Blue (closes loop)

---

## BALL TRANSFER SEQUENCE

### Initial State
```
Blue: ● (Ball here)
Red:  ○
Green: ○
Yellow: ○
```

### After C12 (Blue → Red)
```
Blue: ○
Red:  ● (Ball here)
Green: ○
Yellow: ○
```

### After C13 (Red → Green)
```
Blue: ○
Red:  ○
Green: ● (Ball here)
Yellow: ○
```

### After C14 (Green → Yellow)
```
Blue: ○
Red:  ○
Green: ○
Yellow: ● (Ball here)
```

### After C15 (Yellow → Blue) — Loop Complete
```
Blue: ● (Ball here)
Red:  ○
Green: ○
Yellow: ○
```

---

## ROTATION GROUP LIFECYCLE

### Phase 1: Creation
```csharp
GameObject rotGroup = LorQBRotationHelper.CreateC12RotationGroup();
```
- Creates new GameObject "C12_RotGroup"
- Positions at hinge location
- Temporarily parents Blue, HBR, Red to group
- Preserves world positions (SetParent with worldPositionStays=true)

### Phase 2: Animation
```csharp
rotGroup.transform.Rotate(Vector3.right, 90f);
```
- Rotates group around its local axis
- All children rotate together
- Other cubes unaffected (remain siblings)

### Phase 3: Cleanup
```csharp
LorQBRotationHelper.DestroyRotationGroup(rotGroup);
```
- Reparents all children back to LorQB_Root
- Preserves world positions
- Destroys rotation group GameObject
- Flat hierarchy restored

---

## CODE FLOW DIAGRAM

```
Start() in C12_BlueToRed_CORRECTED
	|
	v
Wait for C10 scene build (1.5s)
	|
	v
Validate flat hierarchy
	|
	v
[VALID?] --NO--> Log error, abort
	|
	YES
	v
Create C12_RotGroup
	|
	v
Parent Blue, HBR, Red, Ball to group
	|
	v
Animate rotation (90° over 2s)
	|
	v
Reparent Ball to LorQB_Root
	|
	v
Position Ball at Seat_Red
	|
	v
Destroy C12_RotGroup
	|
	v
Validate flat hierarchy again
	|
	v
[VALID?] --YES--> Log success, done
	|
	NO
	v
Log error (hierarchy corrupted)
```

---

## VALIDATION FLOW

```
ValidateFlatHierarchy()
	|
	v
Find LorQB_Root
	|
	v
[FOUND?] --NO--> Return false
	|
	YES
	v
Check each expected object (Blue, Red, Green, Yellow, Hinges, Ball)
	|
	v
[OBJECT EXISTS?] --NO--> Log error, continue checking
	|
	YES
	v
[IS DIRECT CHILD OF LORQB_ROOT?] --NO--> Log error, flag invalid
	|
	YES
	v
Continue for all objects
	|
	v
[ALL VALID?] --YES--> Return true
	|
	NO
	v
Return false
```

---

## COMPARISON: BLENDER vs UNITY

### Blender (C10_scene_build.py)
```python
# Creates cubes as independent objects
bpy.ops.mesh.primitive_cube_add(location=location)
cube = bpy.context.object

# Sets pivot to hinge location (but cube remains scene-level)
bpy.context.scene.cursor.location = hinge_location
bpy.ops.object.origin_set(type='ORIGIN_CURSOR')

# Rotation uses object origin (no parenting)
cube.rotation_euler = (angle, 0, 0)
```

### Unity (C10_SceneBuild.cs + LorQBRotationHelper.cs)
```csharp
// Creates cubes as siblings under LorQB_Root
GameObject cube = new GameObject("Cube_Blue");
cube.transform.SetParent(lorqbRoot.transform, false);

// Temporary group at hinge location
GameObject rotGroup = new GameObject("C12_RotGroup");
rotGroup.transform.position = hingeLocation;
cube.transform.SetParent(rotGroup.transform, true);

// Rotation uses temporary group (simulates Blender object-origin)
rotGroup.transform.Rotate(axis, angle);

// Restore independence
cube.transform.SetParent(lorqbRoot.transform, true);
Destroy(rotGroup);
```

**Key insight:** Unity temporary groups mimic Blender object-origin behavior

---

## SUMMARY

### Architecture Principles:
1. **Flat hierarchy** — all cubes are siblings
2. **Independent objects** — no permanent nesting
3. **Temporary groups** — created per-movement, destroyed after
4. **Local pivots** — rotation groups positioned at hinges
5. **Validation** — check before/after each movement

### Movement Pattern:
1. Validate flat hierarchy
2. Create temporary rotation group
3. Animate rotation group
4. Transfer ball
5. Destroy rotation group
6. Validate flat hierarchy again

### Success Criteria:
- ✅ No recursive transform inheritance
- ✅ All cubes remain siblings after movements
- ✅ No cube detachment
- ✅ No transform corruption
- ✅ Validation passes before/after movements

---

**This architecture is REQUIRED for stable LorQB behavior in Unity.**
