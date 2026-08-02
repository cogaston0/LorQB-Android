# LorQB-Android

Android Unity version of **LorQB / Cubolita** — an educational cube-and-ball game.

---

## Game Concept

LorQB / Cubolita is a single-player educational puzzle game designed for Android devices.

**Level 1** features:
- 4 hollow transparent cubes: **Blue**, **Red**, **Green**, and **Yellow**
- 1 ball that travels between cubes
- A **digital shuffle order** displayed on-screen (e.g. Blue → Red → Green → Yellow)
- A countdown **timer** tracking the player's speed
- A **player selector** (choose player name / number before starting)
- Optional **text mode** overlay (accessibility / learning aid)

### Core Rules
- The ball moves from cube to cube **following the shuffle order**.
- The ball can only pass through **aligned holes** between adjacent cubes.
- Cubes are connected by **3 top hinges only** — no detachment is allowed.
- Cube rotation is **controlled and scripted** (no physics engine).
- Ball transfer is triggered **only when holes are aligned**.
- No bones, armatures, or Blender drivers are used.

---

## Repository Layout

```
LorQB-Android/
├── AGENTS/             # Agent role definitions and responsibilities
├── DESIGN/             # Game design documents
├── UNITY_PLAN/         # Unity project folder structure plan
├── TESTING/            # Test checklists
└── README.md
```

---

## Current Status

> **Phase:** Planning only — no Unity scenes or C# scripts yet.

### Planning Files
| File | Purpose |
|------|---------|
| `AGENTS/01_Project_Manager.md` | Project oversight, milestones, and agent coordination |
| `AGENTS/02_Unity_Developer.md` | Scene setup, prefabs, camera, and lighting plan |
| `AGENTS/03_Movement_Logic.md` | Cube rotation, hinge constraints, and ball transfer logic |
| `AGENTS/04_Touch_Control.md` | Android touch input system design |
| `AGENTS/05_UI_Timer_Shuffle.md` | HUD, timer, shuffle display, and player selector UI |
| `AGENTS/06_Testing.md` | QA strategy and test execution plan |
| `DESIGN/Level_1_Game_Design.md` | Full Level 1 game design specification |
| `UNITY_PLAN/Folder_Structure.md` | Unity Assets folder layout and naming conventions |
| `TESTING/Level_1_Test_Checklist.md` | Functional test checklist for Level 1 |

---

## Development Constraints

- Target platform: **Android** (Unity 2022 LTS or later)
- **No physics engine** (Rigidbody/Collider) for cube or ball movement
- **Scripted rotations** only (transform-based, eased)
- **No bones, armatures, or drivers**
- **No Level 2** implemented yet
- Ball transfer only on hole alignment (angle threshold check)
- Touch controls must support tap, swipe, and long-press gestures

---

## Getting Started

> Unity project setup instructions will be added when the implementation phase begins.

