# START HERE

## System Order Protocol Established

**C10 → C12 → C13 → Validator**

---

## Immediate Action Required

### 1. Open Unity Editor
Unity will regenerate project files automatically.

### 2. Run Step 1 ONLY
**Menu:** `LorQB > Test Step 1 - C10 Only (Creator)`

### 3. Press Play ▶️

### 4. Confirm
**Do you see:**
- ✅ 4 cubes (Blue, Red, Green, Yellow)?
- ✅ 3 hinges?
- ✅ 1 ball (inside Blue)?
- ✅ Console: "LorQB Scene Built."?

### 5. Report Result
- **If YES** → "Step 1 PASS — proceed to Step 2"
- **If NO** → "Step 1 FAIL — fix C10 before continuing"

---

## Rules

❌ **DO NOT** run Step 2 until Step 1 passes  
❌ **DO NOT** run Step 3 until Step 2 passes  
❌ **DO NOT** add random pieces  

✅ **DO** test in exact order: C10 → C12 → C13  
✅ **DO** stop and fix if any step fails  

---

## Files Ready

| File | Role | Test Step |
|------|------|-----------|
| `C10_SceneBuild.cs` | Creator | Step 1 (FIRST) |
| `C12_BlueToRed.cs` | Manipulator | Step 2 |
| `C13_RedToGreen.cs` | Manipulator | Step 3 |
| `VC13_Validator.cs` | Observer | Step 4 (not restored yet) |

---

## Menu Commands

1. `LorQB > Test Step 1 - C10 Only (Creator)`
2. `LorQB > Test Step 2 - C10 + C12 (Blue to Red)`
3. `LorQB > Test Step 3 - C10 + C13 (Red to Green)`

---

## Documentation

- `QUICK_START.md` — detailed step-by-step instructions
- `SYSTEM_BUILD_PROTOCOL.md` — full protocol with success criteria
- `SYSTEM_ORDER_SUMMARY.md` — complete status report

---

**Next:** Open Unity, run Step 1, report results.

**Stop here until Step 1 confirmed working.**
