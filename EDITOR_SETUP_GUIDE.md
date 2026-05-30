# Unity Editor Scene Setup — Automated

## ✅ Build Status: **SUCCESSFUL**

---

## What Was Created

### `Assets/Scripts/Editor/LorQBSceneSetup.cs`

**Unity Editor automation script** that handles scene wiring with one menu click.

**What it does:**
1. Creates `LorQB_Runtime` GameObject (if not exists)
2. Attaches `LorQBSceneBuilder` component
3. Attaches `C13_PilotRunner` component  
4. Saves the scene automatically
5. Selects `LorQB_Runtime` in hierarchy for visibility

---

## How to Use

### **In Unity Editor:**

1. **Open any scene** (or create new scene: `File > New Scene`)

2. **Click menu:** `LorQB > Setup Scene for C13 Test`

3. **Wait for console confirmation:**
   ```
   [LorQB Setup] Starting scene setup...
   [LorQB Setup] Created LorQB_Runtime GameObject
   [LorQB Setup] Attached LorQBSceneBuilder component
   [LorQB Setup] Attached C13_PilotRunner component
   [LorQB Setup] Scene saved: Assets/YourScene.unity
   [LorQB Setup] ✅ Scene setup complete!
   [LorQB Setup] Press Play to run C13 test
   ```

4. **Press Play** ▶️

---

## What You'll See in Hierarchy

```
Scene
├── Main Camera
├── Directional Light
└── LorQB_Runtime ← (automatically created & selected)
	├── LorQBSceneBuilder (component)
	└── C13_PilotRunner (component)
```

---

## Console Output When Playing

```
[LorQB Setup] Press Play to run C13 test
[C13] PilotRunner loaded
[C13] C13_RedToGreen component added
LorQB Scene Built.
[C13] Red-to-Green test started
[C13] Scene objects found
=== C13 Start: Red → Green ===
[C13] Ball positioned in Red seat
[C13] PHASE 1 — Rotating HRG 0° → 90°
[C13] PHASE 2 — Rotating HRG 90° → 180°
[C13] Ball transferred Red → Green
[C13] RETURN — Rotating HRG 180° → 0°
=== C13 Complete: Red → Green ===
```

---

## Scene Workflow

### First Time Setup:
1. `File > New Scene` (Basic template)
2. `LorQB > Setup Scene for C13 Test` (menu command)
3. Save dialog appears → choose location (e.g., `Assets/Scenes/LorQB_C13_Test.unity`)
4. Press Play

### Subsequent Uses:
1. Open existing scene
2. `LorQB > Setup Scene for C13 Test` (re-runs safely, won't duplicate)
3. Press Play

---

## Technical Notes

### Idempotent Operation
- Running the setup multiple times is **safe**
- Won't create duplicate components
- Won't create duplicate GameObjects
- Will log "already attached" if components exist

### Scene Saving
- **If scene is new:** prompts for save location
- **If scene exists:** auto-saves to current path
- Marks scene as dirty to ensure Unity knows it changed

### Editor Assembly
- Script lives in `Assets/Scripts/Editor/` folder
- Compiled into `Assembly-CSharp-Editor.csproj`
- Only runs in Unity Editor (not in builds)
- Uses `UnityEditor` namespace for menu integration

---

## File Structure After Setup

```
Assets/
├── Scenes/
│   └── LorQB_C13_Test.unity  [saved scene]
├── Scripts/
│   ├── C12_BlueToRed.cs
│   ├── C13_RedToGreen.cs
│   ├── C13_PilotRunner.cs
│   ├── LorQBSceneBuilder.cs
│   └── Editor/
│       └── LorQBSceneSetup.cs  [NEW - automation]
```

---

## What This Script Does NOT Do

❌ Does NOT touch hinge logic  
❌ Does NOT modify existing components  
❌ Does NOT change scene hierarchy (beyond adding LorQB_Runtime)  
❌ Does NOT override user settings  

✅ **Only scene wiring and component attachment**

---

## Troubleshooting

### "Menu item not appearing"
- Restart Unity Editor
- Check console for script compilation errors
- Verify file is in `Assets/Scripts/Editor/` folder

### "Components not attached"
- Check console logs for error messages
- Verify `LorQBSceneBuilder.cs` and `C13_PilotRunner.cs` exist
- Rebuild scripts (`Assets > Reimport All`)

### "Scene not saving"
- Check if scene has write permissions
- Try manually saving first: `File > Save Scene`
- Check console for save errors

---

## Next Steps

1. **Open Unity Editor**
2. **Run:** `LorQB > Setup Scene for C13 Test`
3. **Press Play** ▶️
4. **Observe:**
   - Scene builds automatically (cubes, hinges, ball)
   - C13 test runs automatically
   - Ball moves Red → Green
   - Console logs progress

---

**Status:** Ready for one-click setup! ✅

**Menu location:** `LorQB > Setup Scene for C13 Test`
