# CI Follow-Up Items

This document tracks non-blocking issues and improvements for the GitHub Actions CI pipeline.

---

## Submodule Warning: Assets/LorQB-Blender

**Status:** Non-blocking  
**Priority:** Low  
**Discovered:** June 3, 2026

### Issue Description
The GitHub Actions checkout step produces a warning about submodule cleanup:
```
Warning: Failed to remove submodule working tree 'Assets/LorQB-Blender'
```

### Root Cause
- The directory `Assets/LorQB-Blender` exists in the repository
- No `.gitmodules` file is present in the repository root
- Git expects this to be a submodule based on the directory but cannot find configuration

### Impact
- **Build Impact:** None - does not affect build success
- **Runtime Impact:** None - does not affect application functionality
- **CI Impact:** Warning message only, no failure

### Potential Solutions
1. **Add .gitmodules file** - If `Assets/LorQB-Blender` should be a submodule:
   ```
   [submodule "Assets/LorQB-Blender"]
	   path = Assets/LorQB-Blender
	   url = <actual-submodule-url>
   ```

2. **Remove submodule metadata** - If this should NOT be a submodule:
   - Ensure the directory is tracked as regular files
   - Remove any `.git` directory inside `Assets/LorQB-Blender`
   - Commit as normal tracked files

3. **Add workflow ignore** - Suppress the warning if the directory is intentionally in an intermediate state:
   ```yaml
   - name: Checkout Repository
	 uses: actions/checkout@v4
	 with:
	   submodules: false  # Explicitly disable submodule handling
   ```

### Recommendation
Investigate the intent for `Assets/LorQB-Blender` and apply solution #1 or #2 accordingly. This should be addressed during the next maintenance window.

---

## Unity Version in License Workflow

**Status:** ✅ Resolved  
**Resolved:** June 3, 2026

### Previous Issue
The `get-unity-license.yml` workflow was using Unity version `2022.3.20f1` while `build-android.yml` was using `6000.4.5f1`.

### Resolution
Updated `get-unity-license.yml` to use Unity version `6000.4.5f1` to match the build workflow.

### Note
If the Unity license needs to be regenerated for version `6000.4.5f1`, manually trigger the `Get Unity License (Run Once)` workflow from the Actions tab.

---

## EditorBuildSettings Scene Path

**Status:** ✅ Resolved  
**Resolved:** June 3, 2026

### Previous Issue
`EditorBuildSettings.asset` referenced non-existent scene `Assets/Scenes/SampleScene.unity`, causing build failures with error:
```
'Assets/Scenes/SampleScene.unity' is an incorrect path for a scene file
```

### Resolution
Instead of repointing `EditorBuildSettings.asset`, a minimal `Assets/Scenes/SampleScene.unity` was committed directly (origin/main commit `44ef12f`), so the existing reference resolves:
- **Scene Path:** `Assets/Scenes/SampleScene.unity` (unchanged)
- **GUID:** `99c9720ab356a0642a771bea13969a05` (unchanged)

The scene shipped without a `.meta` file, which would have let Unity assign a new random GUID on first import — silently breaking the `EditorBuildSettings.asset` reference again. Added `Assets/Scenes/SampleScene.unity.meta` locally, pinned to the GUID above, to close that gap.

### Follow-Up Consideration
An earlier local (never-pushed) fix took a different approach — repointing `EditorBuildSettings.asset` at `Assets/_Recovery/0 (8).unity` — but that's now superseded by the SampleScene fix above and was discarded to avoid two competing resolutions. The `_Recovery` scene is still sitting there unused; worth deciding later whether it should be deleted or repurposed as the real Level 1 scene once gameplay content is built into it.

---

**Last Updated:** June 3, 2026  
**Maintainer:** CI/CD Team
