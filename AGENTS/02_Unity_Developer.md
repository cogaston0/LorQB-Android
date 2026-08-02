# Agent 02 — Unity Developer

## Role
Set up the Unity Android project, build the Level 1 scene, and create all GameObjects and prefabs. This agent does **not** write gameplay logic scripts — those belong to Agents 03–05.

---

## Responsibilities

### 1. Unity Project Configuration
- Unity version: **2022 LTS** (or latest stable 2022.x)
- Build target: **Android**
- Minimum API level: **Android 8.0 (API 26)**
- Target API level: **Android 13+ (API 33)**
- Scripting backend: **IL2CPP**
- Color space: **Linear**
- Rendering pipeline: **Universal Render Pipeline (URP)**
- Orientation: **Portrait** (locked)

### 2. Folder Structure
Follow the plan defined in `UNITY_PLAN/Folder_Structure.md` exactly.

### 3. Scene Layout — Level 1

**Scene name:** `Level_1`

#### GameObjects to create (no scripts yet — layout only):

```
Level_1 (Scene root)
├── GameManager          (empty GameObject — scripts attached later)
├── Camera               (Main Camera, orthographic or perspective TBD)
├── Lighting
│   └── Directional Light
├── Cubes
│   ├── Cube_Blue        (hollow transparent cube prefab instance)
│   ├── Cube_Red         (hollow transparent cube prefab instance)
│   ├── Cube_Green       (hollow transparent cube prefab instance)
│   └── Cube_Yellow      (hollow transparent cube prefab instance)
├── Ball                 (sphere — ball prefab instance)
├── Hinges
│   ├── Hinge_BlueRed    (empty GameObject at hinge point)
│   ├── Hinge_RedGreen   (empty GameObject at hinge point)
│   └── Hinge_GreenYellow(empty GameObject at hinge point)
└── UI
    └── Canvas           (Screen Space — Overlay)
        ├── ShuffleDisplay
        ├── TimerDisplay
        ├── PlayerSelector
        └── TextModeToggle
```

### 4. Cube Prefab Specification

Each cube is a **hollow transparent cube** built from 6 quad faces:
- Material: URP Transparent / Unlit with cube color tint
- Each face is a separate child GameObject to allow hole cutouts
- **Hole faces:** Two opposing faces per cube have a circular hole (modeled as a quad with alpha cutout or a mesh with opening)
- Hole axis: holes align on the **X axis** (left/right faces) for horizontal ball passage
- Cube scale: `(1, 1, 1)` Unity units

**Cube color tints:**
| Cube | Hex Color |
|------|-----------|
| Blue | `#4488FF` |
| Red | `#FF4444` |
| Green | `#44BB44` |
| Yellow | `#FFDD00` |

### 5. Hinge Setup

- Each hinge is an **empty GameObject** positioned at the **top edge** shared between two adjacent cubes
- Parent-child relationships:
  - `Cube_Red` is a child of `Hinge_BlueRed` (Blue is the anchor)
  - `Cube_Green` is a child of `Hinge_RedGreen` (Red is the anchor)
  - `Cube_Yellow` is a child of `Hinge_GreenYellow` (Green is the anchor)
- Rotation axis for all hinges: **Z axis** (top edge, like opening a lid)
- No physics joints — rotation is handled by Agent 03 scripts

### 6. Camera Setup

- Single main camera
- Position: above and in front of the cube chain to show all 4 cubes
- Projection: **Perspective**, FOV ~60
- Background: dark neutral color (`#1A1A2E`)

### 7. Lighting

- One **Directional Light** at 45° angle
- Intensity: 1.0
- Color: white
- Enable **soft shadows**

### 8. Ball Prefab

- GameObject name: `Ball`
- Mesh: Unity built-in Sphere
- Scale: `(0.4, 0.4, 0.4)` (fits through cube holes)
- Material: URP Lit, white/pearl color (`#F0F0F0`)
- No Rigidbody — position controlled by scripts

---

## Constraints (must not violate)

- **No Rigidbody or Collider** components on Ball or Cubes
- **No Animator** components driven by mecanim state machines
- **No bones or armatures**
- Cube transparency via **URP Alpha Blend** material, not physics materials
- All mesh holes are **geometry-based** (mesh cutout), not particle effects
- **Do not create Level 2 scenes**

---

## Deliverables (Phase 2)

- [ ] Unity project opens without errors
- [ ] `Level_1` scene loads in Editor
- [ ] All 4 cube prefabs created with correct colors and hole geometry
- [ ] Ball prefab created
- [ ] Hinge empty GameObjects positioned correctly
- [ ] Camera shows all 4 cubes in frame
- [ ] UI Canvas with placeholder text objects
- [ ] Scene builds to Android APK (even if gameplay is not wired)
