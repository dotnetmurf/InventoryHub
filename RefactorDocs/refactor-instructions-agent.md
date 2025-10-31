# InventoryHub Refactoring - Agent-Optimized Instructions

## Overview
This is an **agent-optimized hybrid approach** where:
- ✅ **Agent performs**: All code changes, builds, git operations, and automated verification
- ⏸️ **Agent pauses**: At manual testing checkpoints for human verification
- ✅ **Agent continues**: After receiving human confirmation to proceed

## How This Works

### Agent Actions (Automated)
- Edit files with precise string replacements
- Run `dotnet build` after each change
- Execute git commands (branch, commit, merge, tag)
- Detect and report build errors
- Run automated tests (`dotnet test`)

### Human Actions (Manual Checkpoints)
- **CHECKPOINT**: Browser testing (UI verification)
- **CHECKPOINT**: Swagger API testing
- **CHECKPOINT**: End-to-end functional testing
- **DECISION**: Approve merge to master
- **DECISION**: Proceed to next phase

### Communication Protocol
```
Agent: "✅ Step 1.1 complete. Build succeeded."
Agent: "⏸️ CHECKPOINT: Ready to proceed to Step 1.2? (yes/no)"
Human: "yes"
Agent: [Continues with Step 1.2]
```

---

# PHASE 1: SharedModels Integration

## Preparation

**Agent checks**:
- Working directory is clean
- On master branch
- .NET SDK available

**Agent actions**:
```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git status
git checkout master
git checkout -b refactor/phase1-shared-models
```

**Agent reports**: "✅ Branch 'refactor/phase1-shared-models' created"

---

## Step 1.1: Add Project References

**Agent executes**:
```powershell
dotnet add ClientApp/ClientApp.csproj reference SharedModels/SharedModels.csproj
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/ClientApp.csproj
git commit -m "Add SharedModels reference to ClientApp"
```

**Agent reports**: "✅ Step 1.1a complete. ClientApp reference added and builds successfully."

**Agent executes**:
```powershell
dotnet add ServerApp/ServerApp.csproj reference SharedModels/SharedModels.csproj
dotnet build ServerApp/ServerApp.csproj
```

**If build succeeds**:
```powershell
git add ServerApp/ServerApp.csproj
git commit -m "Add SharedModels reference to ServerApp"
```

**Agent reports**: "✅ Step 1.1 complete. Both project references added."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.2? (yes/no)"

---

## Step 1.2: Update ClientApp Using Statements

### Step 1.2.1: Update ProductService.cs

**Agent reads**: `ClientApp/Services/ProductService.cs`

**Agent finds**: `using ClientApp.Models;`

**Agent replaces with**: `using SharedModels;`

**Agent executes**: `dotnet build ClientApp/ClientApp.csproj`

**If build succeeds**:
```powershell
git add ClientApp/Services/ProductService.cs
git commit -m "Update ProductService to use SharedModels"
```

**Agent reports**: "✅ ProductService.cs updated"

---

### Step 1.2.2: Update ProductsStateService.cs

**Agent reads**: `ClientApp/Services/ProductsStateService.cs`

**Agent checks**: Does file contain `using ClientApp.Models`?
- If NO: Skip this file
- If YES: Replace with `using SharedModels;`

**Agent executes**: `dotnet build ClientApp/ClientApp.csproj`

**Agent reports**: Result of this step

---

### Step 1.2.3: Update All Razor Pages

**Agent processes these files**:
- `ClientApp/Pages/Products.razor`
- `ClientApp/Pages/ProductDetails.razor`
- `ClientApp/Pages/CreateProduct.razor`
- `ClientApp/Pages/EditProduct.razor`
- `ClientApp/Pages/DeleteProduct.razor`

**For each file**:
1. Read file
2. Find: `@using ClientApp.Models`
3. Replace with: `@using SharedModels`
4. Save file

**After all files updated**:
```powershell
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/Pages/*.razor
git commit -m "Update all Razor pages to use SharedModels"
```

**Agent reports**: "✅ All Razor pages updated (5 files)"

---

### Step 1.2.4: Update Shared Components

**Agent processes these files**:
- `ClientApp/Shared/ProductCard.razor`
- `ClientApp/Shared/ProductForm.razor`
- `ClientApp/Shared/ErrorAlert.razor` (check only, may not need changes)
- `ClientApp/Shared/ToastContainer.razor` (check only, may not need changes)

**Same pattern**: Find and replace `@using ClientApp.Models` with `@using SharedModels`

**Agent executes**: `dotnet build ClientApp/ClientApp.csproj`

**Agent reports**: "✅ Shared components updated"

---

### Step 1.2.5: Final ClientApp Build

**Agent executes**:
```powershell
dotnet clean ClientApp/ClientApp.csproj
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/
git commit -m "Complete ClientApp migration to SharedModels"
```

**Agent reports**: "✅ Step 1.2 complete. ClientApp fully migrated to SharedModels."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.3? (yes/no)"

---

## Step 1.3: Update ServerApp Using Statements

### Step 1.3.1: Update ProductEndpoints.cs

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs`

**Agent action**: Add `using SharedModels;` after existing using statements (keep `using ServerApp.Models;`)

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Update ProductEndpoints to use SharedModels"
```

**Agent reports**: "✅ ProductEndpoints.cs updated"

---

### Step 1.3.2: Update AppDbContext.cs

**Agent reads**: `ServerApp/Data/AppDbContext.cs`

**Agent action**: Add `using SharedModels;` after existing using statements

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Data/AppDbContext.cs
git commit -m "Update AppDbContext to use SharedModels"
```

**Agent reports**: "✅ AppDbContext.cs updated"

---

### Step 1.3.3: Update Service Files

**Agent processes these files**:
- `ServerApp/Services/CacheService.cs`
- `ServerApp/Services/DbInitializerService.cs`
- `ServerApp/Services/SeedingService.cs`
- `ServerApp/Services/ValidationService.cs`

**For each file**:
1. Read file
2. Add `using SharedModels;` after existing using statements
3. Build: `dotnet build ServerApp/ServerApp.csproj`
4. If successful, commit individually

**Agent reports**: "✅ All service files updated (4 files)"

---

### Step 1.3.4: Update Program.cs

**Agent reads**: `ServerApp/Program.cs`

**Agent action**: Add `using SharedModels;` at the top with other using statements

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Program.cs
git commit -m "Update Program.cs to use SharedModels"
```

**Agent reports**: "✅ Program.cs updated"

---

### Step 1.3.5: Final ServerApp Build

**Agent executes**:
```powershell
dotnet clean ServerApp/ServerApp.csproj
dotnet build ServerApp/ServerApp.csproj
```

**Agent reports**: "✅ Step 1.3 complete. ServerApp fully migrated to SharedModels."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.4 (Remove Duplicate Files)? (yes/no)"

---

## Step 1.4: Remove Duplicate Files

**⚠️ Agent Warning**: "About to delete duplicate model files. Builds must be passing before proceeding."

**Agent verifies**:
```powershell
dotnet build ClientApp/ClientApp.csproj
dotnet build ServerApp/ServerApp.csproj
```

**If both builds pass**, agent proceeds:

### Remove ClientApp Duplicates

**Agent executes**:
```powershell
Remove-Item "ClientApp/Models/Product.cs"
Remove-Item "ClientApp/Models/Category.cs"
Remove-Item "ClientApp/Models/PaginatedList.cs"
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/Models/
git commit -m "Remove duplicate models from ClientApp"
```

**Agent reports**: "✅ ClientApp duplicate models removed"

---

### Remove ServerApp Duplicates

**Agent executes**:
```powershell
Remove-Item "ServerApp/Models/Product.cs"
Remove-Item "ServerApp/Models/Category.cs"
Remove-Item "ServerApp/Models/PaginatedList.cs"
dotnet build ServerApp/ServerApp.csproj
```

**If build succeeds**:
```powershell
git add ServerApp/Models/
git commit -m "Remove duplicate models from ServerApp"
```

**Agent reports**: "✅ ServerApp duplicate models removed"

**Agent reports**: "✅ Step 1.4 complete. All duplicate models removed."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.5? (yes/no)"

---

## Step 1.5: Full Solution Build

**Agent executes**:
```powershell
dotnet build InventoryHub.sln
```

**Agent analyzes output**:
- If 0 errors, 0 warnings: "✅ Solution builds successfully"
- If errors: Reports errors and pauses for human intervention

**If build succeeds**:
```powershell
git add .
git commit -m "Phase 1 complete: SharedModels integration successful"
```

**Agent reports**: "✅ Step 1.5 complete. Full solution builds successfully."

---

## Step 1.6: Runtime Testing (MANUAL CHECKPOINT)

**Agent reports**: "⏸️ MANUAL TESTING REQUIRED"

**Agent provides instructions**:

```
==========================================
CHECKPOINT: Manual Runtime Testing
==========================================

Please perform the following tests:

1. TEST SERVERAPP:
   - Open terminal in ServerApp folder
   - Run: dotnet run
   - Navigate to: http://localhost:5132
   - Verify: Swagger UI loads
   - Test: GET /api/products endpoint
   - Stop: Press Ctrl+C

2. TEST CLIENTAPP:
   - Open terminal in ClientApp folder
   - Run: dotnet run
   - Navigate to: URL shown in terminal
   - Verify: Products page loads
   - Test: View, Create, Edit, Delete operations
   - Check: Browser console (F12) for errors
   - Stop: Press Ctrl+C

==========================================
Please report results:
- Type "pass" if all tests successful
- Type "fail: [description]" if issues found
==========================================
```

**Agent waits for**: Human response

**On "pass"**: Agent continues to Step 1.7

**On "fail: [description]"**: Agent logs issue and asks for guidance

---

## Step 1.7: Phase 1 Completion

**Agent executes**:
```powershell
git status
```

**Agent verifies**: All changes committed

**Agent updates**: `REFACTORING_CHECKLIST.md`

**Agent changes**:
```markdown
## ✅ Phase 1: SharedModels Integration (COMPLETED)

- [x] Create SharedModels project
- [x] Add Product.cs to SharedModels
- [x] Add Category.cs to SharedModels
- [x] Add PaginatedList.cs to SharedModels
- [x] Add SharedModels to solution
- [x] Add ClientApp reference to SharedModels
- [x] Add ServerApp reference to SharedModels
- [x] Update ClientApp using statements
- [x] Update ServerApp using statements
- [x] Delete ClientApp/Models/Product.cs
- [x] Delete ClientApp/Models/Category.cs
- [x] Delete ClientApp/Models/PaginatedList.cs
- [x] Delete ServerApp/Models/Product.cs
- [x] Delete ServerApp/Models/Category.cs
- [x] Delete ServerApp/Models/PaginatedList.cs
- [x] Build solution (verify no errors)
- [x] Run ServerApp (verify API works)
- [x] Run ClientApp (verify UI works)
- [x] Test all CRUD operations
```

**Agent commits**:
```powershell
git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 1 complete"
```

**Agent reports**: "✅ Checklist updated"

---

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 1 to master? (yes/no)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase1-shared-models
git tag phase1-complete
```

**Agent reports**: 
```
✅ Phase 1 Complete!
- Branch: refactor/phase1-shared-models merged to master
- Tag: phase1-complete created
- Commits: [list of commits]
- Files changed: [count]
- Status: SUCCESS
```

**Agent asks**: "⏸️ Push to remote? (yes/no/later)"

**On "yes"**: `git push origin master --tags`

**Agent asks**: "⏸️ Start Phase 2? (yes/no/later)"

---

# PHASE 2: Extract Business Logic (Summary)

## Agent Workflow

**Automated Steps**:
1. Create feature branch
2. Create `IProductBusinessService.cs` interface
3. Create `ProductBusinessService.cs` implementation (from REFACTORING.md template)
4. Register service in Program.cs
5. Refactor each endpoint method one-by-one
6. Build after each change
7. Commit after each successful build

**Manual Checkpoints**:
- After each endpoint refactored: Quick Swagger test
- After all endpoints done: Full integration test

---

# ERROR HANDLING (Agent Automated)

## When Build Fails

**Agent detects**: Non-zero exit code from `dotnet build`

**Agent actions**:
1. Capture error output
2. Analyze error message
3. Attempt common fixes:
   - Run `dotnet restore`
   - Run `dotnet clean && dotnet build`
   - Check for missing using statements
4. Report to human with context

**Agent reports**:
```
❌ BUILD FAILED
Step: 1.2.3 - Update Razor pages
File: ClientApp/Pages/Products.razor
Error: CS0246: The type or namespace name 'Product' could not be found
Likely cause: Missing 'using SharedModels;' directive
Action taken: Reviewing file for missing using statements
```

**Agent asks**: "Need help? (yes/no)"

---

## When Git Operation Fails

**Agent detects**: Git command failure

**Agent actions**:
1. Check working directory status
2. Report uncommitted changes
3. Suggest resolution

**Agent reports**:
```
⚠️ GIT WARNING
Operation: git checkout master
Issue: Uncommitted changes detected
Files: ClientApp/Services/ProductService.cs
Suggestion: Commit or stash changes first
```

**Agent asks**: "Commit changes? (yes/stash/abort)"

---

# PROGRESS TRACKING

**Agent maintains**: Internal state of completed steps

**Agent provides**: Status report on request

**Example Status Report**:
```
==========================================
REFACTORING PROGRESS REPORT
==========================================
Current Phase: Phase 1 - SharedModels Integration
Current Step: 1.3.2 - Update AppDbContext.cs
Overall Progress: 35% (7/20 steps complete)

Completed:
✅ 1.1 - Add Project References
✅ 1.2 - Update ClientApp Using Statements
✅ 1.3.1 - Update ProductEndpoints.cs
⏳ 1.3.2 - Update AppDbContext.cs (in progress)

Remaining:
□ 1.3.3 - Update Service Files
□ 1.4 - Remove Duplicate Files
□ 1.5 - Full Solution Build
□ 1.6 - Runtime Testing
□ 1.7 - Phase 1 Completion

Commits: 7
Builds: 9 (9 successful, 0 failed)
Time Elapsed: [calculated]
==========================================
```

---

# COMMUNICATION PROTOCOL

## Agent Messages

**✅ Success**: Step completed successfully
**⏸️ Checkpoint**: Waiting for human confirmation
**❌ Error**: Problem encountered, needs attention
**⚠️ Warning**: Potential issue, but proceeding
**📝 Info**: Informational message
**🔄 Working**: Currently processing

## Human Responses

**"yes" / "y" / "continue"**: Proceed
**"no" / "n" / "stop"**: Stop and wait
**"skip"**: Skip current step
**"status"**: Show progress report
**"help"**: Show available commands
**"abort"**: Stop refactoring, return to master
**"pass"**: Test passed (at checkpoints)
**"fail: [reason]"**: Test failed (at checkpoints)

---

# SAFETY FEATURES

## Automatic Rollback

**Agent monitors**: Each git commit

**On error**, agent can:
```powershell
git reset --hard HEAD~1  # Undo last commit
```

## Backup Points

**Agent creates tags** before risky operations:
- `pre-phase1-deletion` (before removing duplicate files)
- `pre-phase2-refactor` (before major refactoring)

## Working Directory Checks

**Before each phase**, agent verifies:
- No uncommitted changes
- On correct branch
- All tests passing (if tests exist)

---

# DIFFERENCES FROM MANUAL VERSION

| Aspect | Manual Version | Agent Version |
|--------|---------------|---------------|
| File editing | "Open in VS Code" | Direct file manipulation |
| Build verification | Manual command | Automated after each change |
| Error detection | Manual review | Automated analysis |
| Checkpoints | Implicit | Explicit with prompts |
| Progress tracking | Manual | Automated with reports |
| Rollback | Manual git commands | Automated with safety checks |
| Testing | All manual | Mixed (automated builds, manual UI) |

---

# USAGE INSTRUCTIONS

## To Start Refactoring

**Human**: "Start Phase 1"

**Agent**: Begins executing Phase 1 steps, pausing at checkpoints

## To Resume After Checkpoint

**Human**: "yes" or "continue"

**Agent**: Continues from paused point

## To Get Status

**Human**: "status"

**Agent**: Displays progress report

## To Stop

**Human**: "stop" or "abort"

**Agent**: Commits current work, returns to master

---

# COMPLETION SUMMARY

**At end of each phase**, agent provides:

```
==========================================
PHASE 1 COMPLETION SUMMARY
==========================================
Status: ✅ SUCCESS
Duration: [calculated]
Branch: refactor/phase1-shared-models
Commits: 15
Files Changed: 23
Lines Added: 150
Lines Removed: 300

Build Results:
- Total Builds: 20
- Successful: 20
- Failed: 0

Test Results:
- Solution Build: PASS
- ServerApp Runtime: PASS (manual verification)
- ClientApp Runtime: PASS (manual verification)

Next Phase: Phase 2 - Extract Business Logic
Estimated Time: 4-6 hours
==========================================

Ready to start Phase 2? (yes/no/later)
```

---

**Document Version**: 1.0 Agent-Optimized  
**Created**: October 31, 2025  
**Optimized For**: Hybrid Agent-Human Execution  
**Manual Checkpoints**: Browser testing, UI verification  
**Automated Steps**: All code changes, builds, git operations
