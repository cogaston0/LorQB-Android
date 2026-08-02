# LorQB System Build Protocol

## ✅ System Order (MUST follow sequentially)

### **1. C10 System — Creator (FIRST)**
- Restores/creates full LorQB scene
- Cubes, ball, seats, hinges, names
- **Must run first and visibly create scene**

### **2. C12 System — First Manipulator (SECOND)**
- Tests Blue → Red only
- Proves C10 objects are found
- **Cannot test until C10 creates scene**

### **3. C13 System — Second Manipulator (THIRD)**
- Tests Red → Green only
- No new creation
- **Cannot test until C10 + C12 work**

### **4. Validator System — Observer (LAST)**
- Watches C12/C13
- Warns only
- No movement, no repair, no creation
- **Cannot test until C10 + C12 + C13 work**

---

## 📋 Test Protocol

### **Step 1: C10 Test (Creator Only)**
1. Open clean Unity
2. Confirm files exist:
   - ✅ `C10_SceneBuild.cs`
   - ✅ `C12_BlueToRed.cs`
   - ✅ `C13_RedToGreen.cs`
   - ❌ `VC13_Validator.cs` (not restored yet)
3. Create one scene object: `LorQB_Runtime`
4. Attach ONLY: `C10_SceneBuild`
5. **Press Play**
6. **CONFIRM:** Cubes appear (Blue, Red, Green, Yellow)
7. **CONFIRM:** Ball appears (in Blue cube)
8. **CONFIRM:** Hinges appear (Blue_Red, Red_Green, Green_Yellow)
9. **CONFIRM:** Console shows: `"LorQB Scene Built."`
10. **Stop**

**⚠️ STOP HERE if cubes don't appear — C10 must work first**

---

### **Step 2: C12 Test (First Manipulator)**
1. Continue from Step 1 (C10 attached)
2. Add component: `C12_BlueToRed`
3. **Press Play**
4. **CONFIRM:** Scene builds (C10 runs)
5. **CONFIRM:** Ball moves Blue → Red
6. **CONFIRM:** Hinge_Blue_Red rotates X-axis
7. **CONFIRM:** Console shows C12 progress logs
8. **Stop**

**⚠️ STOP HERE if C12 doesn't work — fix before C13**

---

### **Step 3: C13 Test (Second Manipulator)**
1. Continue from Step 2 (C10 + C12 attached)
2. **Remove C12_BlueToRed** (or disable component)
3. Add component: `C13_RedToGreen`
4. **Press Play**
5. **CONFIRM:** Scene builds (C10 runs)
6. **CONFIRM:** Ball moves Red → Green
7. **CONFIRM:** Hinge_Red_Green rotates Y-axis
8. **CONFIRM:** Console shows:
   - `"[C13] Scene objects found"`
   - `"[C13] Ball positioned in Red seat"`
   - `"=== C13 Complete: Red → Green ==="`
9. **Stop**

**⚠️ STOP HERE if C13 doesn't work — fix before Validator**

---

### **Step 4: Validator Test (Observer)**
1. Continue from Step 3 (C10 + C13 attached)
2. Add component: `VC13_Validator` (when restored)
3. **Press Play**
4. **CONFIRM:** Warnings appear ONLY when rule breaks
5. **CONFIRM:** Validator never creates objects
6. **CONFIRM:** Validator never moves objects
7. **CONFIRM:** Validator never repairs violations
8. **Stop**

---

## 🚫 Critical Rules

### **Rule 1: No C13 testing until C10 visibly creates scene**
If Step 1 fails (cubes don't appear), DO NOT proceed to C12 or C13.

### **Rule 2: No Validator testing until C10 + C12/C13 work**
If Step 2 or Step 3 fail, DO NOT add Validator.

### **Rule 3: No adding pieces randomly**
Follow the exact order: C10 → C12 → C13 → Validator

### **Rule 4: Each system must prove itself before next**
- C10 must visibly create scene
- C12 must successfully move ball Blue → Red
- C13 must successfully move ball Red → Green
- Only then test Validator

---

## 📁 Current File Status

| File | Status | Role | Test Step |
|------|--------|------|-----------|
| `C10_SceneBuild.cs` | ✅ Exists | Creator | Step 1 |
| `C12_BlueToRed.cs` | ✅ Exists | Manipulator | Step 2 |
| `C13_RedToGreen.cs` | ✅ Exists | Manipulator | Step 3 |
| `VC13_Validator.cs` | ❌ Not restored | Observer | Step 4 |

---

## 🎯 Success Criteria

### **Step 1 Success (C10):**
- [ ] Blue cube visible (top-right, 2 holes)
- [ ] Red cube visible (bottom-right, 1 hole)
- [ ] Green cube visible (bottom-left, 1 hole)
- [ ] Yellow cube visible (top-left, 2 holes)
- [ ] Ball visible (inside Blue)
- [ ] 3 hinges visible
- [ ] Console: "LorQB Scene Built."

### **Step 2 Success (C12):**
- [ ] All Step 1 criteria met
- [ ] Ball transfers Blue → Red
- [ ] Hinge_Blue_Red rotates smoothly
- [ ] Console shows C12 progress

### **Step 3 Success (C13):**
- [ ] All Step 1 criteria met
- [ ] Ball transfers Red → Green
- [ ] Hinge_Red_Green rotates smoothly
- [ ] Console shows C13 progress

### **Step 4 Success (Validator):**
- [ ] All previous criteria met
- [ ] Warnings logged when violations occur
- [ ] No object creation by validator
- [ ] No object movement by validator

---

## 🛠️ Current Action Required

**Status:** Files exist, awaiting Unity Editor project regeneration

**Next Action:**
1. Open Unity Editor
2. Wait for project file regeneration
3. Follow Step 1 (C10 test only)
4. Report results before proceeding to Step 2

---

## 📝 Editor Automation

The `LorQBSceneSetup.cs` editor script will be updated to support step-by-step testing:

- Menu: `LorQB/Test Step 1 - C10 Only` (attach C10 only)
- Menu: `LorQB/Test Step 2 - C10 + C12` (attach C10 + C12)
- Menu: `LorQB/Test Step 3 - C10 + C13` (attach C10 + C13)
- Menu: `LorQB/Test Step 4 - C10 + C13 + Validator` (attach all)

**Each menu command creates clean setup for that step.**

---

**Status:** Protocol established, awaiting Step 1 execution ✅
