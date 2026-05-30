# Quick Start — System Order Testing

## ⚠️ CRITICAL: Test in Order

**Do NOT skip steps. Each must succeed before proceeding.**

---

## Step 1: C10 Only (Creator Test)

### In Unity Editor:
1. Open Unity Editor
2. Menu: `LorQB > Test Step 1 - C10 Only (Creator)`
3. **Press Play** ▶️

### Expected Result:
```
[LorQB Setup] STEP 1: Testing C10 Creator only
[LorQB Setup] ✅ C10_SceneBuild attached
LorQB Scene Built.
```

### Visual Confirmation:
- ✅ 4 cubes appear (Blue, Red, Green, Yellow)
- ✅ 3 hinges appear
- ✅ 1 ball appears (inside Blue cube)
- ✅ Cubes are transparent/glassy
- ✅ Cubes have circular holes

### If Failed:
**STOP. Fix C10 before proceeding.**

---

## Step 2: C12 Test (First Manipulator)

### Prerequisites:
- ✅ Step 1 passed (cubes visible)

### In Unity Editor:
1. Menu: `LorQB > Test Step 2 - C10 + C12 (Blue to Red)`
2. **Press Play** ▶️

### Expected Result:
```
[LorQB Setup] STEP 2: Testing C10 + C12 (Blue → Red)
[LorQB Setup] ✅ C10_SceneBuild + C12_BlueToRed attached
LorQB Scene Built.
(C12 transfer logs)
```

### Visual Confirmation:
- ✅ Scene builds (C10 runs)
- ✅ Ball moves from Blue → Red
- ✅ Hinge_Blue_Red rotates (X-axis)
- ✅ Ball ends in Red cube

### If Failed:
**STOP. Fix C12 before proceeding.**

---

## Step 3: C13 Test (Second Manipulator)

### Prerequisites:
- ✅ Step 1 passed (cubes visible)
- ✅ Step 2 passed (C12 worked)

### In Unity Editor:
1. Menu: `LorQB > Test Step 3 - C10 + C13 (Red to Green)`
2. **Press Play** ▶️

### Expected Result:
```
[LorQB Setup] STEP 3: Testing C10 + C13 (Red → Green)
[LorQB Setup] ✅ C10_SceneBuild + C13_RedToGreen attached
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

### Visual Confirmation:
- ✅ Scene builds (C10 runs)
- ✅ Ball moves from Red → Green
- ✅ Hinge_Red_Green rotates (Y-axis)
- ✅ Ball ends in Green cube

### If Failed:
**STOP. Fix C13 before proceeding.**

---

## Step 4: Validator Test (Observer)

### Prerequisites:
- ✅ Step 1 passed
- ✅ Step 2 passed
- ✅ Step 3 passed

### Status:
❌ **Not yet restored** — waiting for C10/C12/C13 to work first

---

## 🔧 Current Status

| System | File | Status | Test Step |
|--------|------|--------|-----------|
| Creator | `C10_SceneBuild.cs` | ✅ Ready | Step 1 |
| Manipulator 1 | `C12_BlueToRed.cs` | ✅ Ready | Step 2 |
| Manipulator 2 | `C13_RedToGreen.cs` | ✅ Ready | Step 3 |
| Observer | `VC13_Validator.cs` | ❌ Not restored | Step 4 |

---

## 📋 Test Checklist

- [ ] **Step 1:** C10 creates scene visibly
- [ ] **Step 2:** C12 moves ball Blue → Red
- [ ] **Step 3:** C13 moves ball Red → Green
- [ ] **Step 4:** Validator observes only (not restored yet)

---

## 🚫 Rules

1. **NO random piece adding** — follow exact order
2. **NO C13 until C10 creates scene** — must see cubes first
3. **NO Validator until C12/C13 work** — manipulators must prove themselves
4. **Each step must PASS before next** — stop and fix if failed

---

## 🎯 Next Action

1. **Open Unity Editor**
2. **Run:** `LorQB > Test Step 1 - C10 Only (Creator)`
3. **Press Play**
4. **Report:** Do cubes appear? (Yes/No)

**If Yes → proceed to Step 2**  
**If No → fix C10 before continuing**

---

**Start with Step 1 only. Report results.**
