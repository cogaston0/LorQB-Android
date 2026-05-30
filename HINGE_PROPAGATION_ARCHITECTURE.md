# HINGE PROPAGATION ARCHITECTURE
**LorQB-Android | Unity 6 | Kinematic Chain System**

---

## CORE LAW

> **Connected cubes must behave as ONE continuous kinematic chain at all times.**
> **The ball may transfer only through aligned holes.**
> **The ball must never leave the cube system during gameplay.**

---

## PROBLEM STATEMENT

Current system failures (C13, T03, T04, C14, C15):
- Cubes detach during rotation
- Independent transforms cause world matrix divergence
- Temporary reparenting breaks chain integrity
- Each script reinvents hierarchy → inconsistent behavior
- No central authority enforcing mechanical constraints

**Root Cause:**  
Treating cubes as independent animated objects instead of a **connected kinematic graph**.

---

## ARCHITECTURAL PRINCIPLES

### 1. GRAPH-BASED MECHANICAL PROPAGATION

LorQB is NOT:
- ❌ Independent cube animation
- ❌ Free local rotations
- ❌ Temporary reparenting
- ❌ Physics-based simulation

LorQB IS:
- ✅ A kinematic chain graph
- ✅ Rigid-body rotation propagation
- ✅ Graph-traversal transform updates
- ✅ Constraint-based movement

### 2. SINGLE SOURCE OF TRUTH

One canonical hierarchy:
```
Blue ↔ [HBR] ↔ Red ↔ [HRG] ↔ Green ↔ [HGY] ↔ Yellow
```

Every movement script queries this graph.  
No script modifies hierarchy directly.

### 3. NO DETACHMENT EVER

Constraint: `distance(CubeA, CubeB) ≤ MAX_HINGE_STRETCH`

Violated = **system halt** + error log.

---

## CORE COMPONENTS

### 1. HingeGraph

**Responsibility:**  
Maintain the canonical connection graph.

**Data Structure:**
```csharp
Dictionary<string, HingeConnection> hinges;

class HingeConnection {
	string hingeName;           // "Hinge_Red_Green"
	GameObject hingeObject;
	CubeNode anchorCube;       // stationary side (Red)
	CubeNode movingCube;       // rotates (Green)
	Vector3 rotationAxis;      // local axis (X or Y)
	RotationGroup downstreamGroup;
}

class CubeNode {
	string cubeName;           // "Cube_Red"
	GameObject cubeObject;
	List<HingeConnection> connectedHinges;
}
```

**Methods:**
- `GetDownstreamCubes(hingeName)` → returns all cubes that must move with this hinge
- `ValidateChainIntegrity()` → checks no detachment
- `GetRotationGroup(hingeName)` → returns RotationGroup for this hinge

---

### 2. RotationGroup

**Responsibility:**  
Define which cubes rotate together as ONE rigid assembly.

**Example:**

When `Hinge_Red_Green` rotates:
```
Anchor: Red (stationary)
Moving Group: [Green, Yellow]
```

When `Hinge_Green_Yellow` rotates:
```
Anchor: Green (stationary)
Moving Group: [Yellow]
```

**Critical Rule:**  
Rotating a hinge rotates **ALL downstream cubes** simultaneously.

**Data Structure:**
```csharp
class RotationGroup {
	GameObject anchorCube;
	List<GameObject> movingCubes;
	Vector3 pivotPoint;        // world space
	Vector3 rotationAxis;      // world space

	void ApplyRotation(float angleDelta);
}
```

**Method: ApplyRotation**
```csharp
void ApplyRotation(float angleDelta) {
	foreach (GameObject cube in movingCubes) {
		// Rotate around pivot as rigid body
		cube.transform.RotateAround(pivotPoint, rotationAxis, angleDelta);
	}
}
```

---

### 3. Anchor Cube vs Moving Cube Set

**Rule:**  
Every hinge has:
1. **Anchor cube** (stationary during rotation)
2. **Moving cube set** (all downstream cubes)

**Level 1 Example:**

| Hinge | Anchor | Moving Set |
|-------|--------|------------|
| HBR   | Blue   | Red, Green, Yellow |
| HRG   | Red    | Green, Yellow |
| HGY   | Green  | Yellow |

**Direction Matters:**

C13 (Red → Green):
- HRG rotates
- Anchor: Red
- Moving: Green, Yellow

T03 (Red → Yellow):
- HGY rotates first (Yellow moves)
- Then HRG rotates (Green + Yellow move)

---

### 4. Downstream Propagation

**Definition:**  
When a hinge rotates, ALL cubes downstream in the chain rotate together.

**Graph Traversal:**
```
Starting from hinge H:
1. Identify anchor cube A
2. Traverse graph away from A
3. Collect all connected cubes → moving set
4. Apply rotation to entire set as rigid body
```

**Example: C13 (Red → Green)**

```
Graph: Blue ↔ HBR ↔ Red ↔ HRG ↔ Green ↔ HGY ↔ Yellow

Rotating HRG:
- Anchor: Red
- Traverse right from Red
- Collect: [Green, Yellow]
- Rotate both around HRG pivot
```

**Example: T03 (Red → Yellow)**

```
Step 1: Rotate HGY
- Anchor: Green
- Moving: [Yellow]

Step 2: Rotate HRG
- Anchor: Red
- Moving: [Green, Yellow] (Yellow already moved with Green)
```

---

### 5. Ball Transfer Rule

**Constraint:**  
Ball can only transfer when:
1. Source cube hole is aligned with destination cube hole
2. Hinge connecting them is at transfer gate angle
3. Ball is inside source cube boundary

**Transfer Gate Angles:**

| Transfer | Hinge | Gate Angle |
|----------|-------|------------|
| Blue → Red | HBR | 180° (X-axis) |
| Red → Green | HRG | 180° (Y-axis) |
| Green → Yellow | HGY | 180° (X-axis) |

**Transfer Sequence:**
1. Rotate hinge to gate angle
2. Validate holes aligned
3. `ball.SetParent(destinationCube)`
4. `ball.position = destinationSeat`
5. Rotate hinge back to 0°

**Critical:**  
Ball NEVER floats in world space.  
Always parented to a cube OR in transfer (but transfer is instant snap).

---

### 6. No-Detachment Validation

**Continuous Check (every frame during movement):**

```csharp
bool ValidateChainIntegrity() {
	foreach (HingeConnection hinge in hinges.Values) {
		float distance = Vector3.Distance(
			hinge.anchorCube.cubeObject.transform.position,
			hinge.movingCube.cubeObject.transform.position
		);

		if (distance > MAX_HINGE_STRETCH) {
			Debug.LogError($"DETACHMENT: {hinge.hingeName} stretched to {distance}");
			return false;
		}
	}
	return true;
}
```

**Action on Failure:**
- Halt all movement
- Log error with exact cube positions
- Optionally: attempt snap-back correction

**Constants:**
```csharp
const float MAX_HINGE_STRETCH = 1.2f;  // cubes are 1.02 apart normally
const float VALIDATION_FREQUENCY = 0.016f; // every frame at 60fps
```

---

## CENTRAL HINGE SOLVER

**Responsibility:**  
Single authority that all C/T scripts use for movement.

**File:** `HingeSolver.cs`

**Public Interface:**
```csharp
class HingeSolver : MonoBehaviour {
	// Singleton
	public static HingeSolver Instance;

	// Core methods
	public RotationGroup GetRotationGroup(string hingeName);
	public void RotateHinge(string hingeName, float targetAngle, float duration);
	public bool TransferBall(string fromCube, string toCube);
	public bool ValidateChainIntegrity();

	// Query methods
	public Vector3 GetCubePosition(string cubeName);
	public float GetHingeAngle(string hingeName);
	public List<string> GetDownstreamCubes(string hingeName);
}
```

**Movement Scripts Become:**
```csharp
// C13_RedToGreen.cs
IEnumerator RunTransfer() {
	yield return HingeSolver.Instance.RotateHinge("Hinge_Red_Green", 180f, 2f);
	HingeSolver.Instance.TransferBall("Cube_Red", "Cube_Green");
	yield return HingeSolver.Instance.RotateHinge("Hinge_Red_Green", 0f, 2f);
}
```

**Result:**
- No hierarchy manipulation in movement scripts
- No transform math in movement scripts
- Only movement instructions
- All mechanical laws enforced centrally

---

## IMPLEMENTATION PHASES

### Phase 1: Core Infrastructure
1. `HingeGraph.cs` — graph data structure
2. `RotationGroup.cs` — rigid rotation logic
3. `HingeSolver.cs` — central API

### Phase 2: Validation
1. `ChainValidator.cs` — no-detachment checks
2. `TransferValidator.cs` — ball transfer rules
3. Real-time warning system

### Phase 3: Rewrite Movement Scripts
1. Convert C12, C13, C14, C15 to use HingeSolver
2. Convert T03, T04 to use HingeSolver
3. Remove all transform math from movement scripts

### Phase 4: Level 2 Support
1. Extend HingeGraph for 3D lattice
2. Multi-layer rotation groups
3. Complex transfer paths

---

## NON-NEGOTIABLE CONSTRAINTS

1. ✅ No cube ever detaches (distance check every frame)
2. ✅ Ball always enclosed (except start/end of turn)
3. ✅ Only aligned holes allow transfer
4. ✅ No physics engine (kinematic only)
5. ✅ No armatures (pure transform math)
6. ✅ One canonical hierarchy (HingeGraph is source of truth)
7. ✅ All rotations are rigid-body (RotateAround pivot)
8. ✅ No local/world mismatch (everything in world space)
9. ✅ Rotation groups include ALL downstream cubes
10. ✅ Movement scripts only issue commands (no direct transform manipulation)

---

## SUCCESS CRITERIA

✅ C13 runs without detachment  
✅ T03/T04 run without detachment  
✅ Yellow never moves during C13  
✅ Blue passive carry survives all movements  
✅ Ball never leaves cube boundaries  
✅ Validator passes all 9 checkpoints  
✅ Adding new movement only requires adding commands (no new math)  
✅ Level 2 extension is possible without rewrite  

---

## FAILURE MODES TO PREVENT

❌ Cube detachment (current problem)  
❌ Inconsistent hierarchy across scripts  
❌ Local rotation accumulation errors  
❌ Ball floating in world space  
❌ Hinge ownership ambiguity  
❌ Independent cube transforms  
❌ Temporary reparenting breaks  

---

## NEXT STEPS

1. **Review this document** — confirm architecture is correct
2. **Build Phase 1** — HingeGraph, RotationGroup, HingeSolver
3. **Test with C13** — validate no detachment
4. **Expand validator** — real-time monitoring
5. **Rewrite all movement scripts** — use HingeSolver API only
6. **Stabilize Level 1** — all C/T movements validated
7. **Design Level 2 extension** — 3D lattice support

---

## REFERENCES

- Original patent: "4 cubes joined by 3 hinges on top only"
- Blender constraints: COPY_TRANSFORMS for ball only
- Image evidence: detachment at HRG during C13
- Repeated failures: T03, T04, C14, C15 all show same pattern

---

**END OF ARCHITECTURE DOCUMENT**

*Next action: Review → Build Phase 1 → Never patch individual scripts again*
