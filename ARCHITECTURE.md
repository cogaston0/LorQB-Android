# LorQB Unity Architecture — CORRECTED FLAT HIERARCHY

## CRITICAL CHANGE: RECURSIVE HIERARCHY → FLAT HIERARCHY

### PROBLEM (OLD ARCHITECTURE)

**Previous nested hierarchy:**
```
Blue
└── HBR
	└── Red
		└── HRG
			└── Green
				└── HGY
					└── Yellow
```

**Issues caused:**
- Recursive transform inheritance
- Rotating Blue rotates entire chain
- Rotating Red corrupts Green/Yellow spaces
- Pivot axes drift during movement
- Local hinge behavior becomes mathematically unstable
- C12/C13/C14/C15 movements cause detachment and corruption

### SOLUTION (NEW ARCHITECTURE)

**Flat sibling hierarchy:**
```
LorQB_Root
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

**Benefits:**
- No recursive transform inheritance
- All cubes remain siblings
- Hinges remain independent objects
- Rotations affect ONLY intended local cube groups
- No permanent transform inheritance chains
- No nested hinge spaces
- No cube detachment during animation
- Stable local-space rotations

---

## MOVEMENT APPROACH (C12-C15)

### OLD (INCORRECT) APPROACH
- Permanent nested parenting
- Rotate parent cube → entire chain rotates
- Unstable pivot behavior
- Transform corruption

### NEW (CORRECT) APPROACH
For each C-series move:

1. **Create temporary runtime rotation group**
   - Group contains ONLY cubes/hinges needed for that specific move
   - Group pivot positioned at hinge location
   - Temporarily parent required objects to group

2. **Rotate group locally**
   - Rotation affects only objects in temporary group
   - Other cubes remain independent

3. **Destroy temporary group**
   - Reparent all objects back to LorQB_Root
   - Restore flat hierarchy

### EXAMPLE: C12 (Blue → Red transfer)

```csharp
// 1. Create temporary rotation group
GameObject rotGroup = LorQBRotationHelper.CreateC12RotationGroup();
// Group contains: Blue, Hinge_Blue_Red, Red
// Group pivot: Hinge_Blue_Red position

// 2. Rotate group 90° around hinge axis
rotGroup.transform.Rotate(Vector3.right, 90f);

// 3. Destroy group and restore flat hierarchy
LorQBRotationHelper.DestroyRotationGroup(rotGroup);
```

---

## IMPLEMENTATION FILES

### 1. C10_SceneBuild.cs (UPDATED)
**Role:** Scene creator — builds flat hierarchy

**Changes:**
- Creates `LorQB_Root` parent object
- All cubes, hinges, ball, seats are direct children of LorQB_Root
- **REMOVED:** `EstablishChainHierarchy()` method
- All objects positioned with world coordinates
- All objects have identity rotation at creation

**Key method:**
```csharp
void BuildScene()
{
	GameObject lorqbRoot = new GameObject("LorQB_Root");

	// All objects created as children of lorqbRoot
	CreateHollowCube("Cube_Blue", pos, color, sideHole, lorqbRoot);
	CreateHinge("Hinge_Blue_Red", pos, colorA, colorB, lorqbRoot);
	CreateBall(pos, lorqbRoot);
	CreateSeats(lorqbRoot);

	// NO hierarchy setup — flat by design
}
```

### 2. LorQBRotationHelper.cs (NEW)
**Role:** Temporary rotation group management

**Key methods:**
- `CreateRotationGroup()` — Create temp group with specific objects
- `DestroyRotationGroup()` — Destroy group, restore flat hierarchy
- `CreateC12RotationGroup()` — Blue + HBR + Red
- `CreateC13RotationGroup()` — Red + HRG + Green
- `CreateC14RotationGroup()` — Green + HGY + Yellow
- `CreateC15RotationGroup()` — Yellow + Blue (loop closure)
- `ValidateFlatHierarchy()` — Check all objects are siblings

### 3. C12_BlueToRed.cs (NEEDS UPDATE)
**Current status:** Uses old nested hierarchy approach
**Required changes:** Use LorQBRotationHelper temporary groups

### 4. C13_RedToGreen.cs (NEEDS UPDATE)
**Current status:** Uses old nested hierarchy approach
**Required changes:** Use LorQBRotationHelper temporary groups

### 5. C14, C15 (NOT YET CREATED)
**Guidance:** Must use LorQBRotationHelper from the start

---

## DESIGN RULES (MANDATORY)

### DO:
✅ Keep all cubes as siblings under LorQB_Root
✅ Keep all hinges as siblings under LorQB_Root
✅ Create temporary rotation groups for movements
✅ Destroy rotation groups after animation completes
✅ Use `transform.SetParent(lorqbRoot.transform, true)` to restore hierarchy
✅ Position rotation group pivot at hinge location
✅ Validate flat hierarchy before/after movements

### DON'T:
❌ Create permanent parent-child relationships between cubes
❌ Nest hinges under cubes
❌ Rotate parent cubes directly
❌ Use recursive transform chains
❌ Add physics/rigidbodies for hinge behavior
❌ Add armatures or drivers
❌ Assume transform inheritance for movement

---

## COMPARISON WITH BLENDER

### Blender C10_scene_build.py behavior:
- Objects have independent origins
- Pivots set to hinge locations but objects remain scene-level
- Rotations use object-origin mechanics
- No deep parenting hierarchy

### Unity C10_SceneBuild.cs (CORRECTED):
- All objects siblings under LorQB_Root
- Temporary groups simulate local hinge rotations
- No recursive transform inheritance
- Mimics Blender object independence

---

## TESTING CHECKLIST

Before considering architecture stable:

1. ✅ C10 creates flat hierarchy (verify in Unity Hierarchy window)
2. ⏳ C12 uses temporary rotation group (Blue → Red)
3. ⏳ C13 uses temporary rotation group (Red → Green)
4. ⏳ C14 uses temporary rotation group (Green → Yellow)
5. ⏳ C15 uses temporary rotation group (Yellow → Blue)
6. ⏳ All movements complete without cube detachment
7. ⏳ All cubes return to sibling status after each move
8. ⏳ Validate hierarchy after each C-series move

---

## NEXT STEPS (PRIORITY ORDER)

### IMMEDIATE (Architecture Repair):
1. ✅ Update C10_SceneBuild.cs — flat hierarchy
2. ✅ Create LorQBRotationHelper.cs — rotation group utilities
3. ⏳ Update C12_BlueToRed.cs — use rotation helper
4. ⏳ Update C13_RedToGreen.cs — use rotation helper
5. ⏳ Create C14_GreenToYellow.cs — use rotation helper
6. ⏳ Create C15_YellowToBlue.cs — use rotation helper
7. ⏳ Test all C-series moves for stability

### AFTER C12-C15 STABLE:
- Resume T-series (pattern transformations)
- Resume shuffle system
- Resume Android gameplay integration

---

## CONTACT & ARCHITECTURE AUTHORITY

**This architecture is MANDATORY.**

Any script that violates the flat hierarchy rule will cause:
- Transform corruption
- Cube detachment
- Unstable rotations
- Unusable C-series movements

If you encounter issues, **validate flat hierarchy first**:
```csharp
LorQBRotationHelper.ValidateFlatHierarchy();
```
