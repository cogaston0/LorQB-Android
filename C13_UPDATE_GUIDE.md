# C13 UPDATE GUIDE

## NEXT TASK: Update C13_RedToGreen.cs

Now that C12 is corrected, C13 needs the same architecture fix.

---

## CURRENT C13 ISSUES (Same as old C12)

❌ Uses nested hierarchy (cubes parented to hinges)  
❌ Direct hinge rotation with children  
❌ No flat hierarchy validation  
❌ Creates recursive transform inheritance  
❌ Violates corrected architecture rules

---

## REQUIRED CHANGES

### 1. Add Validation
**Before movement:**
```csharp
if (!LorQBRotationHelper.ValidateFlatHierarchy())
{
	Debug.LogError("[C13] Flat hierarchy validation FAILED!");
	yield break;
}
```

**After movement:**
```csharp
if (LorQBRotationHelper.ValidateFlatHierarchy())
{
	Debug.Log("[C13] ✅ Movement complete — architecture stable");
}
```

### 2. Create Rotation Group
**Replace:**
```csharp
// OLD (remove):
cubeRed.transform.SetParent(hingeRedGreen.transform, true);
ball.transform.SetParent(cubeRed.transform, true);
```

**With:**
```csharp
// NEW (corrected):
GameObject rotGroup = LorQBRotationHelper.CreateC13RotationGroup();
if (rotGroup == null)
{
	Debug.LogError("[C13] Failed to create rotation group!");
	yield break;
}
ball.transform.SetParent(rotGroup.transform, true);
```

### 3. Rotate Group (Not Hinge)
**Replace:**
```csharp
// OLD (remove):
hingeRedGreen.transform.localRotation = Quaternion.Euler(...);
```

**With:**
```csharp
// NEW (corrected):
rotGroup.transform.localRotation = Quaternion.Euler(...);
```

### 4. Cleanup After Animation
**Add before final yield:**
```csharp
// Destroy rotation group — restore flat hierarchy
LorQBRotationHelper.DestroyRotationGroup(rotGroup);
Debug.Log("[C13] Rotation group destroyed — flat hierarchy restored");
```

---

## KEY DIFFERENCES: C12 vs C13

### Hinge Axis:
- **C12:** `Vector3.right` (X-axis) — Blue/Red hinge on right edge
- **C13:** `Vector3.forward` (Y-axis) — Red/Green hinge on bottom edge

### Rotation Direction:
- **C12:** Rotates around X-axis (Euler angle X component)
- **C13:** Rotates around Y-axis (Euler angle Y or Z component depending on Unity convention)

### Rotation Group:
- **C12:** `CreateC12RotationGroup()` — Blue, Hinge_Blue_Red, Red
- **C13:** `CreateC13RotationGroup()` — Red, Hinge_Red_Green, Green

### Seat Positions:
- **C12:** `seatBlueWorld`, `seatRedWorld`
- **C13:** `seatRedWorld`, `seatGreenWorld`

---

## C13 ROTATION DETAILS

### Hinge_Red_Green:
- **Position:** (0.0, 1.0, -0.51) — bottom edge
- **Axis:** Vector3.forward (Z-axis in Unity) OR Vector3.up (Y-axis) depending on scene orientation
- **Connects:** Red (right) ↔ Green (left)

### Expected Rotation:
If following Blender convention and Unity coordinate system:
- **Phase 1:** 0° → -90° (opening)
- **Phase 2:** -90° → -180° (transfer position)
- **Return:** -180° → 0° (closing)

**IMPORTANT:** Check which axis component to use:
- If hinge runs along Y-axis (up/down), rotate around Y
- If hinge runs along X-axis (left/right), rotate around X
- If hinge runs along Z-axis (forward/back), rotate around Z

In your scene, Hinge_Red_Green is at Y=1.0, positioned along the bottom edge (X varies, Z=-0.51), so it likely rotates around **Z-axis** (forward).

---

## STEP-BY-STEP C13 UPDATE PROCESS

### Step 1: Backup Current C13
(Optional, but recommended)
```bash
cp Assets\Scripts\C13_RedToGreen.cs Assets\Scripts\C13_RedToGreen_OLD.cs
```

### Step 2: Update C13 File
Follow the C12 pattern:
1. Add validation before movement
2. Replace parenting with rotation group creation
3. Replace hinge rotation with rotation group rotation
4. Add rotation group cleanup
5. Add validation after movement

### Step 3: Verify Build
```
Run Build → Should compile successfully
```

### Step 4: Test in Unity
```
Menu: LorQB > Test Step 3 - C10 + C13 (Red to Green)
Press Play
Check Console for validation messages
```

### Step 5: Remove OLD Backup
After successful test:
```bash
rm Assets\Scripts\C13_RedToGreen_OLD.cs
```

---

## C13 TEMPLATE (Skeleton)

```csharp
IEnumerator RunC13Transfer()
{
	// 1. Validate before
	if (!LorQBRotationHelper.ValidateFlatHierarchy())
	{
		Debug.LogError("[C13] Validation FAILED!");
		yield break;
	}

	// 2. Get references
	GameObject ball = GameObject.Find("Ball");
	GameObject red = GameObject.Find("Cube_Red");
	GameObject green = GameObject.Find("Cube_Green");
	// ... check nulls

	// 3. Create rotation group
	GameObject rotGroup = LorQBRotationHelper.CreateC13RotationGroup();
	ball.transform.SetParent(rotGroup.transform, true);
	ball.transform.position = seatRed;

	// 4. Phase 1 rotation (0° → -90°)
	// Rotate rotGroup, NOT hinge
	while (elapsed < PHASE1_DURATION)
	{
		// ... rotation logic
		rotGroup.transform.localRotation = Quaternion.Euler(0f, 0f, angle); // or (0, angle, 0)
	}

	// 5. Phase 2 rotation (-90° → -180°)
	// ... same pattern

	// 6. Ball transfer
	GameObject lorqbRoot = GameObject.Find("LorQB_Root");
	ball.transform.SetParent(lorqbRoot.transform, true);
	ball.transform.position = seatGreen;

	// 7. Return rotation (-180° → 0°)
	// ... same pattern

	// 8. Cleanup
	LorQBRotationHelper.DestroyRotationGroup(rotGroup);

	// 9. Validate after
	if (LorQBRotationHelper.ValidateFlatHierarchy())
	{
		Debug.Log("[C13] ✅ Movement complete — architecture stable");
	}
}
```

---

## AFTER C13 COMPLETE

### Next Files to Create:
1. **C14_GreenToYellow.cs**
   - Use `CreateC14RotationGroup()`
   - Rotate around X-axis (like C12)
   - Transfer Green → Yellow

2. **C15_YellowToBlue.cs**
   - Use `CreateC15RotationGroup()`
   - Rotate around appropriate axis (top edge)
   - Transfer Yellow → Blue (closes loop)

---

## TESTING CHECKLIST

After updating C13:

- [ ] C13 file compiles successfully
- [ ] Menu "LorQB > Test Step 3 - C10 + C13" works
- [ ] Validation passes before movement
- [ ] Red and Green cubes rotate together
- [ ] Ball transfers from Red to Green
- [ ] Rotation group destroyed after animation
- [ ] All cubes remain siblings under LorQB_Root
- [ ] Validation passes after movement
- [ ] No cube detachment
- [ ] No transform corruption

---

**Ready to update C13? Follow the C12 pattern exactly.**
