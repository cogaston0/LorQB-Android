# LorQB-Android — Handoff Brief

**Purpose of this document:** a self-contained briefing so another assistant (or a human) can pick up this project cold, without access to prior chat history. Written 2026-07-25.

---

## 1. What this project is

**LorQB-Android** (aka *Cubolita*) is a Unity Android port of an educational puzzle game: four colored cubes (Blue, Red, Green, Yellow) connected in a chain by three hinges, with a ball that must be passed cube-to-cube through aligned holes, following a shuffled sequence, within a time limit.

Repo: `https://github.com/cogaston0/LorQB-Android` (owner: `cogaston0`, default branch `main`).

The project has two halves that must stay conceptually in sync:

1. **A Blender reference implementation** — Python scripts (`C10`–`C15`, `T01`–`T04`) that prototype the cube/hinge/ball mechanics in Blender before they're ported to Unity. This lives in `Assets/LorQB-Blender/`, which is a **git submodule** pointing at a separate repo.
2. **The Unity/Android app** — C# scripts under `Assets/_LorQB/Scripts/` and `Assets/Scripts/` that port the Blender mechanics into actual gameplay, plus the Android CI/build pipeline (GitHub Actions).

---

## 2. The chain/hinge architecture (core mechanic)

Canonical topology: `Blue ↔ [Hinge_Blue_Red] ↔ Red ↔ [Hinge_Red_Green] ↔ Green ↔ [Hinge_Green_Yellow] ↔ Yellow`

**Critical, non-obvious fact discovered by inspecting the actual code** (documented in `Assets/LorQB-Blender/C_SERIES_ARCHITECTURE.md`): the parent-child hierarchy is **hinge-rooted, not cube-rooted**, and it changes shape at every stage:

- After `C12` (Blue→Red) runs: `Hinge_Blue_Red → Blue`
- After `C13` (Red→Green) runs: `Hinge_Red_Green → Red → Blue` (Blue rides passively on Red)
- `C14` (Green→Yellow) needs: `Hinge_Green_Yellow → Green → Hinge_Red_Green → Red → Blue`

Each stage script must **re-attach the passive chain built by the previous stage** before parenting its own cube to its own hinge. If it doesn't, the previously-attached cubes get orphaned mid-animation — this reads visually as cubes "detaching" from the chain.

### Known/fixed instance: C14 detachment
Root-caused and fixed — see `Assets/LorQB-Blender/C14_DETACHMENT_FIX.md`. Fix: `reset_c14_state()` now does `parent_preserve_world(hrg, green)` before parenting Green to `Hinge_Green_Yellow`, so the Blue+Red chain rides along.

### Broader documented problem
`HINGE_PROPAGATION_ARCHITECTURE.md` (repo root) calls this out as systemic across C13, T03, T04, C14, C15 and proposes a real fix: a central `HingeSolver` / `HingeGraph` authority (Unity-side, C#) that all movement scripts would query instead of each script hand-rolling its own parenting logic. **This central solver has not been built yet** — it's a proposed Phase 1–4 plan, not implemented code.

### Open/unresolved instance: C12 detachment (diagnosed, not fixed)
User reported: running `C12` (`Assets/LorQB-Blender/C_series/C12_blue_to_red.py`) via the "Run C12" panel button inside Blender's **Layout** workspace causes cubes to detach, and the "Reset to Base" button doesn't fix it — even though running the raw script fresh (e.g. in the Scripting workspace) works fine.

**Diagnosis** (see conversation history / re-derivable from code):
- `reset_scene_to_canonical()` (the function behind "Reset to Base", lines ~35–70) only clears animation data, clears the ball's constraints, zeroes the three hinges' rotations, and deletes `Seat_*` empties. **It never touches object parenting.** So "Reset to Base" is a *pose* reset, not a *hierarchy* reset.
- `setup_blue_to_red()`, step 7C: `if blue.parent != hinge: parent_preserve_world(blue, hinge)`. `parent_preserve_world` snapshots whatever `Cube_Blue`'s **current** world transform is at the moment you click "Run" and bakes that in as the new baseline — it has no notion of the "true" canonical position.
- If another script in the chain (C13/C14/T-series) has previously reparented `Cube_Blue`/`Cube_Red` elsewhere and left it mid-animation, C12's "Reset to Base" won't undo that reparenting, and "Run C12" will bake in the drifted position instead of the correct one.
- This is not a Blender-workspace bug per se — the Python logic is identical regardless of which workspace UI panel you click it from. The real variable is **scene state history**: running the raw script fresh vs. clicking the panel button after other C-series scripts have already run in that session produces different starting states.

**Not yet fixed.** Suggested fix direction (not implemented): make `reset_scene_to_canonical()` actually restore canonical parenting (and ideally canonical world transforms) for all four cubes before C12 runs, rather than only zeroing the three named hinges' rotations.

---

## 3. Unity app / gameplay implementation status

Level 1 gameplay was built via a sequence of GitHub Copilot-authored PRs, **all merged (#1–25)**, in this order:
1. Planning docs (`AGENTS/`, `DESIGN/Level_1_Game_Design.md`, `UNITY_BUILD/*.md` — design, movement system, ball system, touch input, sequence/shuffle, timer/scoring, level completion)
2. `GameStateManager`
3. `SequenceManager` (+ `GetCurrentIndex()`)
4. `ValidationController` — validates release gestures, gates ball-transfer attempts
5. `InputController` — touch press/drag/release handling
6. `RotationController` — player-drag cube rotation, clamped to [0°, 180°]
7. `BallTransferController` — `TryTransfer()`, seat/detector logic
8. `LevelCompletionController` — fires `ROUND_COMPLETE` (later disabled in favor of `BallTransferController` being sole authority on completion)
9. `GameManager` — rewritten as a clean orchestration script wiring all the above together

**Conclusion: the core Level 1 gameplay loop (input → rotation → validation → ball transfer → sequence/completion) is functionally implemented.** This is the "done" half of the project.

---

## 4. Android CI / build pipeline status

This is the **active, unfinished** half of the project. After gameplay landed, work shifted to getting GitHub Actions to actually produce a buildable Android APK.

### Timeline of fixes (chronological, most recent last)
- Added devcontainer config, GH Actions build workflow, a Unity-license-request workflow
- Restored `Packages/` and `ProjectSettings/` (needed for CI compilation — UI/TMP package resolution)
- Deleted `RotationButtonController.cs`, `C13_PilotRunner.cs`, `LorQBSceneSetup.cs` — all were blocking CI compilation and were removed rather than fixed
- Fixed missing `GameManager` methods for the Android build
- Removed Unity XR packages from `Packages/manifest.json` (unneeded, was causing CI issues)
- Added, then removed, an explicit `scenes` parameter on the build workflow (settled on relying on `EditorBuildSettings` instead)
- **Added `Assets/Scenes/SampleScene.unity`** (a real minimal scene) so `EditorBuildSettings.asset`'s existing reference to `Assets/Scenes/SampleScene.unity` (guid `99c9720ab356a0642a771bea13969a05`) resolves to something real, instead of the previous dangling reference that caused: `'Assets/Scenes/SampleScene.unity' is an incorrect path for a scene file`

### Reconciliation performed this session
There had been **two competing fixes** for the same "missing scene" problem: an uncommitted local edit that repointed `EditorBuildSettings.asset` at `Assets/_Recovery/0 (8).unity`, versus origin/main's approach of committing a real `SampleScene.unity`. Reconciled by:
1. Fast-forwarded local `main` to `origin/main` (was 6 commits behind — clean FF, no conflicts: `f395a4a..e8f167e`)
2. **Discarded** the local `_Recovery` repoint of `EditorBuildSettings.asset` (superseded)
3. **Found a gap** in origin's `SampleScene.unity` fix: it was committed **without a `.meta` file**. Without one, Unity would assign a fresh random GUID on first import in CI, silently breaking the `EditorBuildSettings.asset` reference all over again. **Created `Assets/Scenes/SampleScene.unity.meta`**, pinned to guid `99c9720ab356a0642a771bea13969a05` to match what `EditorBuildSettings.asset` already expects.
4. Kept the local `.github/workflows/get-unity-license.yml` edit (Unity version `2022.3.20f1` → `6000.4.5f1`, unrelated to the scene issue, matches what `build-android.yml` already uses) — origin never touched this file, no conflict.
5. Rewrote the relevant section of `CI_FOLLOW_UP_ITEMS.md` (repo root, currently **untracked**) to describe the real resolution instead of the abandoned one.

### Current working-tree state (as of HEAD `e8f167e`) — nothing below is committed yet
```
 M .github/workflows/get-unity-license.yml     (Unity version bump, unpushed)
 m Assets/LorQB-Blender                        (submodule dirty — see §5)
?? Assets/Scenes/SampleScene.unity.meta        (new — closes the GUID gap)
?? CI_FOLLOW_UP_ITEMS.md                       (new — CI issue tracker doc)
```
These changes are ready for review/commit but **have not been committed or pushed**, per this project's convention of not auto-committing without explicit request.

---

## 5. Out-of-scope / not yet addressed

- **`Assets/LorQB-Blender` submodule is dirty** — it has uncommitted local modifications (`C10_scene_build.py`, `C14_green_to_yellow.py`, `C15_yellow_to_blue.py` modified; several new untracked files including `C14_DETACHMENT_FIX.md`, `.meta` files, `BLENDER_RELOAD_REQUIRED.md`). This looks like the C14 detachment-fix work described in §2 sitting uncommitted inside the submodule's own working tree. It has not been committed inside the submodule repo, so the parent repo's submodule pointer is technically stale/dirty too. **Untouched this session** — deliberately out of scope for the CI reconciliation.
- **7 stale Copilot branches on GitHub**, none merged into `main`: `copilot/add-c13-red-to-green-script`, `copilot/fix-ci-failure`, `copilot/fix-github-repo-actions-build`, `copilot/fix-unity-github-actions-android-build`, `copilot/lorqb-android-inspection`, `copilot/state-of-ci`, `copilot/unity-android-ci-repair`. Corresponding PRs (#26–30) are closed but unmerged — their intended fixes appear to have been re-applied as direct commits to `main` instead. These branches are likely safe to delete but haven't been.
- **C12 detachment bug** (§2) — diagnosed, not fixed.
- **The `_Recovery/0 (8).unity` scene** is still sitting in the repo unused. Worth deciding later whether to delete it or eventually turn it into the real Level 1 scene (gameplay scripts currently have no scene wiring them together yet — `SampleScene.unity` is a minimal placeholder for CI purposes only, not a playable level).
- **Central `HingeSolver`/`HingeGraph` authority** proposed in `HINGE_PROPAGATION_ARCHITECTURE.md` — not built. Would be the durable fix for the whole family of detachment bugs (C12 and beyond) instead of patching each script individually.

---

## 6. Key files to read first

| File | What it tells you |
|---|---|
| `HINGE_PROPAGATION_ARCHITECTURE.md` | The proposed durable architecture fix for chain detachment (not yet implemented) |
| `Assets/LorQB-Blender/C_SERIES_ARCHITECTURE.md` | Ground-truth parent-chain topology per C-stage, reverse-engineered from code |
| `Assets/LorQB-Blender/C14_DETACHMENT_FIX.md` | Worked example of the detachment bug + fix, in C14 |
| `Assets/LorQB-Blender/C_series/C12_blue_to_red.py` | Where the still-open C12 detachment bug lives |
| `CI_FOLLOW_UP_ITEMS.md` | Running log of CI issues, now up to date |
| `Assets/_LorQB/Scripts/GameManager.cs` | Orchestrates all Level 1 gameplay subsystems |
| `.github/workflows/build-android.yml`, `.github/workflows/get-unity-license.yml` | The CI pipeline |

---

## 7. Suggested next actions (not yet decided/started)

1. Review and commit the working-tree changes in §4 (nothing pushed yet — needs an explicit go-ahead).
2. Decide whether to clean up the 7 stale Copilot branches.
3. Fix the C12 detachment bug the same way C14's was fixed (preserve passive-carry chain across `reset_scene_to_canonical()`).
4. Decide the fate of `Assets/_Recovery/0 (8).unity` vs `Assets/Scenes/SampleScene.unity`.
5. Commit/reconcile the dirty `Assets/LorQB-Blender` submodule state.
