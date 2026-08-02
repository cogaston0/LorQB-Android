# Unity Project — Folder Structure Plan

## Overview

This document defines the folder structure for the Unity Android project for LorQB / Cubolita. All agents must follow this structure exactly. Do not create folders outside this plan without updating this document first.

---

## Unity Version & Settings

| Setting | Value |
|---------|-------|
| Unity Version | 2022 LTS (2022.3.x latest) |
| Build Target | Android |
| Render Pipeline | Universal Render Pipeline (URP) |
| Scripting Backend | IL2CPP |
| .NET Target | .NET Standard 2.1 |
| Min Android API | 26 (Android 8.0) |
| Target Android API | 33 (Android 13) |

---

## Root Unity Project Folder

```
LorQB-Android/
├── Assets/                      ← All game content lives here
├── Packages/                    ← Unity Package Manager manifests
├── ProjectSettings/             ← Unity project settings (versioned)
├── AGENTS/                      ← Planning documents (not imported by Unity)
├── DESIGN/                      ← Design documents (not imported by Unity)
├── TESTING/                     ← Test checklists (not imported by Unity)
└── README.md
```

---

## Assets Folder Structure

```
Assets/
│
├── _LorQB/                      ← All game-specific assets (underscore = top of list)
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity       ← Placeholder scene (empty for now)
│   │   └── Level_1.unity        ← Level 1 main scene
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs           ← Central coordinator
│   │   │   ├── CubeIdentifier.cs        ← Enum/component to identify cubes
│   │   │   └── CubeColor.cs             ← CubeColor enum (Blue, Red, Green, Yellow)
│   │   │
│   │   ├── Movement/
│   │   │   ├── CubeRotationController.cs
│   │   │   ├── HoleAlignmentDetector.cs
│   │   │   └── BallTransferController.cs
│   │   │
│   │   ├── Input/
│   │   │   └── TouchInputManager.cs
│   │   │
│   │   └── UI/
│   │       ├── ShuffleDisplayController.cs
│   │       ├── ShuffleOrderGenerator.cs
│   │       ├── GameTimerController.cs
│   │       ├── PlayerSelectorController.cs
│   │       ├── TextModeController.cs
│   │       ├── RotationButtonController.cs
│   │       └── LevelCompleteController.cs
│   │
│   ├── Prefabs/
│   │   ├── Cubes/
│   │   │   ├── Cube_Blue.prefab
│   │   │   ├── Cube_Red.prefab
│   │   │   ├── Cube_Green.prefab
│   │   │   └── Cube_Yellow.prefab
│   │   ├── Ball/
│   │   │   └── Ball.prefab
│   │   └── UI/
│   │       ├── HUD_Canvas.prefab
│   │       └── LevelComplete_Panel.prefab
│   │
│   ├── Materials/
│   │   ├── Cubes/
│   │   │   ├── Mat_Cube_Blue.mat
│   │   │   ├── Mat_Cube_Red.mat
│   │   │   ├── Mat_Cube_Green.mat
│   │   │   └── Mat_Cube_Yellow.mat
│   │   └── Ball/
│   │       └── Mat_Ball.mat
│   │
│   ├── Meshes/
│   │   └── Cubes/
│   │       └── HollowCubeFace.fbx       ← Single reusable cube face mesh with hole
│   │
│   ├── Textures/
│   │   └── UI/
│   │       ├── ShuffleDot_Blue.png
│   │       ├── ShuffleDot_Red.png
│   │       ├── ShuffleDot_Green.png
│   │       └── ShuffleDot_Yellow.png
│   │
│   ├── Fonts/
│   │   └── NunitoSans-Regular SDF.asset  ← TextMeshPro font asset
│   │
│   └── Audio/                            ← Placeholder folder (no audio in v1)
│       └── .gitkeep
│
├── Settings/
│   ├── UniversalRenderPipelineAsset.asset
│   └── URPRenderer.asset
│
└── Tests/
    ├── EditMode/
    │   ├── MovementLogicTests.cs
    │   ├── ShuffleOrderTests.cs
    │   └── InputSystemTests.cs
    └── PlayMode/
        └── Level1IntegrationTests.cs
```

---

## Naming Conventions

### GameObjects (in Hierarchy)
| Type | Convention | Example |
|------|-----------|---------|
| Cube | `Cube_[Color]` | `Cube_Blue` |
| Hinge | `Hinge_[ColorA][ColorB]` | `Hinge_BlueRed` |
| Ball | `Ball` | `Ball` |
| Hole axis marker | `HoleAxis` | `HoleAxis` (child of cube face) |
| Manager | `[Name]Manager` | `GameManager` |
| Canvas | `[Name]Canvas` | `HUDCanvas` |

### Scripts (C#)
| Type | Convention | Example |
|------|-----------|---------|
| MonoBehaviour | `[Name]Controller` or `[Name]Manager` | `CubeRotationController` |
| Data / enum | `[Name]` (PascalCase) | `CubeColor`, `SwipeDirection` |
| Namespace | `LorQB.[Module]` | `LorQB.Movement`, `LorQB.UI` |

### Prefabs
| Convention | Example |
|-----------|---------|
| `[Type]_[Name].prefab` | `Cube_Blue.prefab`, `Ball.prefab` |

### Materials
| Convention | Example |
|-----------|---------|
| `Mat_[Type]_[Name].mat` | `Mat_Cube_Blue.mat`, `Mat_Ball.mat` |

### Scenes
| Convention | Example |
|-----------|---------|
| PascalCase with underscore | `Level_1.unity`, `MainMenu.unity` |

---

## Scene Hierarchy (Level_1)

```
Level_1
├── GameManager
├── Camera
│   └── Main Camera
├── Lighting
│   └── Directional Light
├── Cubes
│   ├── Cube_Blue               ← Anchor (does not rotate)
│   ├── Hinge_BlueRed           ← Empty GO at top edge Blue/Red
│   │   └── Cube_Red            ← Child of hinge, rotates around hinge Z
│   ├── Hinge_RedGreen          ← Empty GO at top edge Red/Green
│   │   └── Cube_Green
│   └── Hinge_GreenYellow       ← Empty GO at top edge Green/Yellow
│       └── Cube_Yellow
├── Ball
└── HUDCanvas
    ├── HeaderBar
    │   ├── TitleText
    │   └── TextModeButton
    ├── ShuffleDisplay
    │   ├── ShuffleDot_0        ← Blue (always first)
    │   ├── Arrow_0
    │   ├── ShuffleDot_1
    │   ├── Arrow_1
    │   ├── ShuffleDot_2
    │   ├── Arrow_2
    │   └── ShuffleDot_3
    ├── BottomHUD
    │   ├── PlayerSelectorPanel
    │   │   ├── PlayerDropdown
    │   │   └── StartButton
    │   └── TimerDisplay
    ├── RotationControls
    │   ├── RotateLeftButton
    │   ├── SelectedCubeLabel
    │   └── RotateRightButton
    └── LevelCompletePanel      ← Hidden by default, shown on level complete
        ├── LevelCompleteText
        ├── PlayerNameText
        ├── FinalTimeText
        ├── PlayAgainButton
        └── MainMenuButton
```

---

## Package Dependencies

| Package | Purpose | Source |
|---------|---------|--------|
| Universal RP | Rendering | Unity Registry |
| TextMeshPro | Text rendering | Unity Registry |
| Input System | Touch input | Unity Registry |
| Unity Test Framework | Unit/integration tests | Unity Registry |

No third-party paid packages. No Asset Store dependencies in Level 1.

---

## .gitignore Notes

The existing `.gitignore` correctly excludes Unity build artifacts. Ensure:
- `Assets/` (including .meta files) IS committed
- `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/` are NOT committed
- `*.apk`, `*.aab` are NOT committed
- `ProjectSettings/` IS committed (required for team collaboration)
- `Packages/packages-lock.json` IS committed (reproducible builds)
