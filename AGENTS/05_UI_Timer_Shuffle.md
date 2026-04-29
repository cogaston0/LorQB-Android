# Agent 05 — UI, Timer & Shuffle

## Role
Design and implement the HUD (Heads-Up Display) for Level 1, including the shuffle order display, countdown timer, player selector, and optional text mode. All UI is built using Unity UI (uGUI) on a Screen Space — Overlay Canvas.

---

## Core Constraints

- UI system: **Unity uGUI** (Canvas, Image, Text, Button)
- Canvas render mode: **Screen Space — Overlay**
- Text rendering: **TextMeshPro** for all text elements
- Orientation: **Portrait** (locked)
- No 3D UI elements in world space for HUD
- UI must remain readable on screens from 5" to 12" (phones and tablets)
- UI must **not block the cube touch zone** in the center of the screen

---

## HUD Layout

```
┌──────────────────────────────────┐
│  LorQB / Cubolita   [TEXT MODE] │  ← Header bar
├──────────────────────────────────┤
│                                  │
│  Shuffle: [●]→[●]→[●]→[●]       │  ← Shuffle order display (colored dots)
│           Blue  Red  Grn  Yel    │  ← Optional text labels (text mode)
│                                  │
│                                  │
│        [ 3D Game View ]          │  ← Cube + ball area (NO UI here)
│                                  │
│                                  │
├──────────────────────────────────┤
│  Player: [P1 ▼]    Time: 00:00  │  ← Player selector + timer
├──────────────────────────────────┤
│         [◀]    [CubeName]   [▶]  │  ← Rotation controls for selected cube
└──────────────────────────────────┘
```

---

## UI Components

### 5.1 Shuffle Order Display

**Purpose:** Show the sequence the ball must follow (e.g., Blue → Red → Green → Yellow)

**Visual design:**
- Four colored circles (matching cube colors) connected by arrows
- Active cube in sequence is highlighted with a glow or scale pulse
- Completed steps are dimmed/checked
- Colors: Blue `#4488FF`, Red `#FF4444`, Green `#44BB44`, Yellow `#FFDD00`

**Script:** `ShuffleDisplayController.cs`

```csharp
// Namespace: LorQB.UI
// Fields:
[SerializeField] GameObject[] shuffleDotImages   // 4 UI Images (colored circles)
[SerializeField] Color completedColor            // dimmed color for completed steps
[SerializeField] Color activeColor               // highlight color for current step

// Methods:
void SetShuffleOrder(CubeColor[] order)          // initializes display with order array
void AdvanceStep()                               // marks current step complete, highlights next
void ResetDisplay()                              // resets all steps to initial state
```

**Text Mode:** When text mode is ON, TextMeshPro labels appear below each dot showing cube name (Blue, Red, Green, Yellow).

---

### 5.2 Timer

**Purpose:** Track how long the player takes to complete Level 1.

**Display format:** `MM:SS` (e.g., `01:23`)

**Behavior:**
- Timer **starts** when the player confirms their name/selection in the Player Selector
- Timer **pauses** if the app goes to background (OnApplicationPause)
- Timer **stops** when `BallTransferController.OnLevelComplete` fires
- Timer **resets** on scene reload

**Script:** `GameTimerController.cs`

```csharp
// Namespace: LorQB.UI
// Fields:
[SerializeField] TMP_Text timerText

// State:
float elapsedSeconds = 0f
bool isRunning = false

// Methods:
void StartTimer()
void StopTimer()
void PauseTimer()
void ResumeTimer()
void ResetTimer()

// Properties:
float ElapsedSeconds { get; }
string FormattedTime { get; }  // returns "MM:SS"
```

---

### 5.3 Player Selector

**Purpose:** Allow the player to enter or select their name/number before the game starts.

**Implementation:**
- Dropdown list (`TMP_Dropdown`) with preset options: Player 1, Player 2, Player 3, Player 4, Guest
- Optional: tap to open a keyboard input for custom name (uses `TouchScreenKeyboard`)
- A **START** button confirms selection and starts the timer

**Script:** `PlayerSelectorController.cs`

```csharp
// Namespace: LorQB.UI
// Fields:
[SerializeField] TMP_Dropdown playerDropdown
[SerializeField] TMP_InputField customNameField  // optional custom name
[SerializeField] Button startButton

// Events:
event Action<string> OnPlayerConfirmed
    // Fires when START is tapped, passes player name string
```

**Flow:**
```
Scene loads → Player Selector panel is shown (game paused)
    │
    ▼
Player taps name or enters custom name
    │
    ▼
Player taps START
    │
    ├── OnPlayerConfirmed fires
    ├── Player Selector panel hides
    └── GameTimerController.StartTimer() called
```

---

### 5.4 Text Mode Toggle

**Purpose:** Toggle text labels on the shuffle display and cube labels for accessibility.

**Implementation:**
- Toggle button in header bar (`TextModeButton`)
- Button text alternates: "TEXT ON" / "TEXT OFF"
- When ON:
  - Cube name labels appear below each shuffle dot
  - Cube name labels appear on cube faces (world space TextMeshPro or overlay)
- When OFF:
  - Only colored shapes shown

**Script:** `TextModeController.cs`

```csharp
// Namespace: LorQB.UI
// Fields:
[SerializeField] GameObject[] textOnlyElements   // shown only in text mode
[SerializeField] TMP_Text toggleButtonLabel

// Methods:
void ToggleTextMode()
bool IsTextModeOn { get; }
```

---

### 5.5 Rotation Arrow Buttons

**Purpose:** Provide tap-target buttons to rotate the currently selected cube.

**Implementation:**
- Two buttons: `[◀]` (rotate counter-clockwise) and `[▶]` (rotate clockwise)
- Appear only when a cube is selected (hidden otherwise)
- Center label shows the selected cube's name (e.g., "Red")
- Tapping `[◀]` calls `CubeRotationController.RotateTo(currentAngle - 90f)`
- Tapping `[▶]` calls `CubeRotationController.RotateTo(currentAngle + 90f)`
- Buttons are **disabled** (grayed out) while rotation or ball transfer is in progress

**Script:** `RotationButtonController.cs`

```csharp
// Namespace: LorQB.UI
// Fields:
[SerializeField] Button rotateLeftButton
[SerializeField] Button rotateRightButton
[SerializeField] TMP_Text selectedCubeLabel
[SerializeField] GameObject buttonPanel        // hidden when no cube selected

// Methods:
void ShowForCube(CubeIdentifier cube)
void Hide()
void SetButtonsEnabled(bool enabled)
```

---

## Level Complete Screen

When `BallTransferController.OnLevelComplete` fires:
- Timer stops
- A **level complete overlay** appears (fade in):
  - "Level Complete! 🎉"
  - Player name
  - Final time (MM:SS)
  - `[PLAY AGAIN]` button → reloads Level_1 scene
  - `[MAIN MENU]` button → loads MainMenu scene (placeholder for now)

---

## Shuffle Order Generation

The shuffle order is **randomly generated each run** (not predetermined). Rules:
- Always starts with Blue (ball starts in Blue cube)
- All 4 cubes appear exactly once
- Generated by: `Fisher-Yates shuffle` on `[Blue, Red, Green, Yellow]` with Blue forced first

**Script:** `ShuffleOrderGenerator.cs`

```csharp
// Namespace: LorQB.UI
// Methods:
CubeColor[] GenerateOrder()
    // Returns array of 4 CubeColor values, Blue always first
```

---

## Deliverables (Phase 5)

- [ ] Canvas with correct layout created in Level_1 scene
- [ ] Shuffle display shows 4 colored dots with correct colors
- [ ] Shuffle display advances step when ball transfers
- [ ] Timer starts on player confirmation, stops on level complete
- [ ] Player selector dropdown works, fires OnPlayerConfirmed
- [ ] Text mode toggles labels on/off correctly
- [ ] Rotation arrow buttons appear when cube is selected
- [ ] Rotation arrow buttons hidden when no cube selected
- [ ] Rotation arrow buttons disabled during animation
- [ ] Level complete screen shows correct time and player name
- [ ] All text is readable on 5" phone screen (minimum 14sp equivalent)
