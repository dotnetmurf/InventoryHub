# Phase 1: SharedModels Integration - Agent Instructions

## Overview
This phase migrates ClientApp and ServerApp to use the unified SharedModels project, eliminating DTO duplication.

**Estimated Time**: 2-3 hours  
**Risk Level**: Medium (involves file deletion)  
**Rollback Strategy**: Git branch allows complete rollback

---

## Pre-Phase Checklist

**Agent verifies**:
- [ ] Working directory is clean (`git status`)
- [ ] On master branch
- [ ] .NET SDK available
- [ ] SharedModels project exists with Product, Category, PaginatedList

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

### Add ClientApp Reference

**Agent executes**:
```powershell
dotnet add ClientApp/ClientApp.csproj reference SharedModels/SharedModels.csproj
dotnet build ClientApp/ClientApp.csproj
```

**Expected result**: Build succeeds (may have warnings about unused using statements)

**If build succeeds**:
```powershell
git add ClientApp/ClientApp.csproj
git commit -m "Add SharedModels reference to ClientApp"
```

**Agent reports**: "✅ ClientApp reference added and builds successfully"

---

### Add ServerApp Reference

**Agent executes**:
```powershell
dotnet add ServerApp/ServerApp.csproj reference SharedModels/SharedModels.csproj
dotnet build ServerApp/ServerApp.csproj
```

**Expected result**: Build succeeds

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

**Agent reads**: `ClientApp/Services/ProductService.cs` (lines 1-10 to find using statements)

**Agent finds and replaces**:
```csharp
// OLD:
using ClientApp.Models;

// NEW:
using SharedModels;
```

**Agent executes**: `dotnet build ClientApp/ClientApp.csproj`

**If build succeeds**:
```powershell
git add ClientApp/Services/ProductService.cs
git commit -m "Update ProductService to use SharedModels"
```

**Agent reports**: "✅ ProductService.cs updated"

---

### Step 1.2.2: Update ProductsStateService.cs

**Agent reads**: `ClientApp/Services/ProductsStateService.cs` (lines 1-10)

**Agent checks**: Does file contain `using ClientApp.Models`?

**If YES**, agent replaces:
```csharp
// OLD:
using ClientApp.Models;

// NEW:
using SharedModels;
```

**If NO**: Agent reports "📝 ProductsStateService.cs does not reference ClientApp.Models, skipping"

**Agent executes**: `dotnet build ClientApp/ClientApp.csproj`

**If build succeeds and changes made**:
```powershell
git add ClientApp/Services/ProductsStateService.cs
git commit -m "Update ProductsStateService to use SharedModels"
```

**Agent reports**: Result of this step

---

### Step 1.2.3: Update All Razor Pages

**Agent processes these files**:
1. `ClientApp/Pages/Products.razor`
2. `ClientApp/Pages/ProductDetails.razor`
3. `ClientApp/Pages/CreateProduct.razor`
4. `ClientApp/Pages/EditProduct.razor`
5. `ClientApp/Pages/DeleteProduct.razor`

**For each file**:
- Read file (lines 1-15 to find @using directives)
- Find: `@using ClientApp.Models`
- Replace with: `@using SharedModels`

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
1. `ClientApp/Shared/ProductCard.razor`
2. `ClientApp/Shared/ProductForm.razor`
3. `ClientApp/Shared/ErrorAlert.razor` (check only)
4. `ClientApp/Shared/ToastContainer.razor` (check only)

**For each file**:
- Read file (lines 1-15)
- Check for `@using ClientApp.Models`
- If found, replace with `@using SharedModels`
- If not found, skip

**After all files processed**:
```powershell
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/Shared/*.razor
git commit -m "Update Shared components to use SharedModels"
```

**Agent reports**: "✅ Shared components updated (X files changed)"

---

### Step 1.2.5: Final ClientApp Build

**Agent executes**:
```powershell
dotnet clean ClientApp/ClientApp.csproj
dotnet build ClientApp/ClientApp.csproj
```

**Agent verifies**: 0 errors

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

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs` (lines 1-20 to find using statements)

**Agent action**: Add `using SharedModels;` after existing using statements

**Note**: Keep `using ServerApp.Models;` for now (PaginationParams, ValidationProblemDetails, etc.)

**Example**:
```csharp
using ServerApp.Data;
using ServerApp.Services;
using ServerApp.Models;  // Keep this
using SharedModels;      // Add this
using Microsoft.EntityFrameworkCore;
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Update ProductEndpoints to use SharedModels"
```

**Agent reports**: "✅ ProductEndpoints.cs updated"

---

### Step 1.3.2: Update AppDbContext.cs

**Agent reads**: `ServerApp/Data/AppDbContext.cs` (lines 1-15)

**Agent action**: Add `using SharedModels;` after existing using statements

**Example**:
```csharp
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;  // Keep for now
using SharedModels;      // Add this
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Data/AppDbContext.cs
git commit -m "Update AppDbContext to use SharedModels"
```

**Agent reports**: "✅ AppDbContext.cs updated"

---

### Step 1.3.3: Update Service Files

**Agent processes these files** (one at a time):
1. `ServerApp/Services/CacheService.cs`
2. `ServerApp/Services/DbInitializerService.cs`
3. `ServerApp/Services/SeedingService.cs`
4. `ServerApp/Services/ValidationService.cs`

**For each file**:
1. Read file (lines 1-15)
2. Add `using SharedModels;` after existing using statements
3. Build: `dotnet build ServerApp/ServerApp.csproj`
4. If successful, commit individually:
   ```powershell
   git add ServerApp/Services/[FileName].cs
   git commit -m "Update [FileName] to use SharedModels"
   ```

**Agent reports**: "✅ All service files updated (4 files)"

---

### Step 1.3.4: Update Program.cs

**Agent reads**: `ServerApp/Program.cs` (lines 1-20)

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

**Agent verifies**: 0 errors

**Agent reports**: "✅ Step 1.3 complete. ServerApp fully migrated to SharedModels."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.4 (Remove Duplicate Files)? (yes/no)"

---

## Step 1.4: Remove Duplicate Files

**⚠️ Agent Warning**: "About to delete duplicate model files. Both apps must build successfully."

### Pre-Deletion Verification

**Agent verifies**:
```powershell
dotnet build ClientApp/ClientApp.csproj
dotnet build ServerApp/ServerApp.csproj
```

**If both builds pass**, agent creates safety tag:
```powershell
git tag pre-phase1-deletion
```

**Agent reports**: "✅ Safety tag created. Proceeding with file deletion."

---

### Remove ClientApp Duplicates

**Agent executes**:
```powershell
Remove-Item "ClientApp/Models/Product.cs"
Remove-Item "ClientApp/Models/Category.cs"
Remove-Item "ClientApp/Models/PaginatedList.cs"
```

**Agent verifies files deleted**:
```powershell
Test-Path "ClientApp/Models/Product.cs"  # Should return False
```

**Agent builds**:
```powershell
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/Models/
git commit -m "Remove duplicate models from ClientApp"
```

**Agent reports**: "✅ ClientApp duplicate models removed (3 files)"

---

### Remove ServerApp Duplicates

**Agent executes**:
```powershell
Remove-Item "ServerApp/Models/Product.cs"
Remove-Item "ServerApp/Models/Category.cs"
Remove-Item "ServerApp/Models/PaginatedList.cs"
```

**Agent verifies files deleted**:
```powershell
Test-Path "ServerApp/Models/Product.cs"  # Should return False
```

**Agent builds**:
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**If build succeeds**:
```powershell
git add ServerApp/Models/
git commit -m "Remove duplicate models from ServerApp"
```

**Agent reports**: "✅ ServerApp duplicate models removed (3 files)"

**Agent reports**: "✅ Step 1.4 complete. All duplicate models removed (6 files total)."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 1.5? (yes/no)"

---

## Step 1.5: Full Solution Build

**Agent executes**:
```powershell
dotnet clean InventoryHub.sln
dotnet build InventoryHub.sln
```

**Agent analyzes output**:
- Count errors: Should be 0
- Count warnings: Document for review

**If build succeeds**:
```powershell
git add .
git commit -m "Phase 1 complete: SharedModels integration successful"
```

**Agent reports**: 
```
✅ Step 1.5 complete
- Solution builds successfully
- 0 errors, X warnings
- Ready for runtime testing
```

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
   - Test: GET /api/products (should return products)
   - Test: POST /api/products (create a test product)
   - Test: PUT /api/products/{id} (update the test product)
   - Test: DELETE /api/products/{id} (delete the test product)
   - Stop: Press Ctrl+C

2. TEST CLIENTAPP:
   - Open new terminal in ClientApp folder
   - Run: dotnet run
   - Navigate to: URL shown in terminal (likely http://localhost:5058)
   - Verify: Home page loads
   - Navigate to: Products page
   - Test: View product list
   - Test: View product details
   - Test: Create new product
   - Test: Edit product
   - Test: Delete product
   - Check: Browser console (F12) for JavaScript errors
   - Stop: Press Ctrl+C

3. INTEGRATION TEST:
   - Run both apps simultaneously
   - Create product in ClientApp
   - Verify it appears in Swagger API
   - Edit in Swagger
   - Verify changes in ClientApp

==========================================
Please report results:
- Type "pass" if all tests successful
- Type "fail: [description]" if issues found
==========================================
```

**Agent waits for**: Human response

**On "pass"**: Agent continues to Step 1.7

**On "fail: [description]"**: 
- Agent logs issue
- Agent asks: "Roll back to pre-deletion? (yes/no/investigate)"

---

## Step 1.7: Phase 1 Completion

**Agent executes**:
```powershell
git status
```

**Agent verifies**: All changes committed (working tree clean)

---

### Update Checklist

**Agent updates**: `REFACTORING_CHECKLIST.md`

**Agent replaces**:
```markdown
## ⏳ Phase 1: SharedModels Integration (40% Complete - In Progress)
```

**With**:
```markdown
## ✅ Phase 1: SharedModels Integration (100% Complete)

**Completed**: [Current Date]

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

### Merge Decision

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 1 to master? (yes/no/later)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase1-shared-models --no-ff
git tag phase1-complete
```

**Agent reports**: 
```
✅ Phase 1 Complete!

Summary:
- Branch: refactor/phase1-shared-models merged to master
- Tag: phase1-complete created
- Commits: [count from git log]
- Files changed: 23+
- Files deleted: 6 (duplicate DTOs)
- Files modified: 17+
- Status: SUCCESS

Benefits Achieved:
✅ Eliminated DTO duplication
✅ Single source of truth for models
✅ Reduced maintenance burden
✅ Simplified future changes

Branch available for reference: refactor/phase1-shared-models
Safety tag available: pre-phase1-deletion
```

---

### Push to Remote (Optional)

**Agent asks**: "⏸️ Push to remote repository? (yes/no/later)"

**On "yes"**: 
```powershell
git push origin master --tags
```

**Agent reports**: "✅ Changes pushed to remote"

---

### Next Phase

**Agent asks**: "⏸️ Start Phase 2 (Extract Business Logic)? (yes/no/later)"

**On "yes"**: Agent loads `refactor-phase2-agent.md` and begins

**On "no"** or **"later"**: 
```
✅ Phase 1 session complete

To resume later:
1. Ensure on master branch: git checkout master
2. Say "Start Phase 2" to load refactor-phase2-agent.md
3. Agent will create new branch: refactor/phase2-business-logic

Current State:
- Phase 1: ✅ COMPLETE
- Phase 2: ⏳ READY TO START
- All changes committed and merged
```

---

## Error Recovery

### Build Failure During Phase

**If build fails at any step**:

1. **Agent captures error**:
   ```powershell
   dotnet build [project] 2>&1 | Tee-Object -Variable buildOutput
   ```

2. **Agent analyzes error**:
   - Missing using statement → Add it
   - Ambiguous reference → Specify namespace
   - Type not found → Verify SharedModels reference

3. **Agent reports**:
   ```
   ❌ BUILD FAILED
   Step: [step number and name]
   File: [file path]
   Error: [error message]
   Likely cause: [analysis]
   Suggested fix: [recommendation]
   ```

4. **Agent asks**: "Attempt automatic fix? (yes/no/manual)"

---

### Rollback Procedure

**If critical failure occurs**:

**Agent offers**:
```
⚠️ CRITICAL ERROR - Rollback Available

Options:
1. Rollback to last commit: git reset --hard HEAD~1
2. Rollback to pre-deletion: git reset --hard pre-phase1-deletion
3. Abandon phase: git checkout master; git branch -D refactor/phase1-shared-models
4. Manual intervention: [stop and wait for instructions]

Which option? (1/2/3/4)
```

---

## Success Metrics

**Phase 1 is successful when**:
- ✅ All projects build without errors
- ✅ All using statements updated to SharedModels
- ✅ All duplicate DTOs removed (6 files)
- ✅ ServerApp runs and serves API endpoints
- ✅ ClientApp runs and displays UI
- ✅ All CRUD operations work end-to-end
- ✅ No console errors in browser
- ✅ No breaking changes to API contracts

---

## Files Modified Summary

**Expected changes**:
- **ClientApp** (9-11 files):
  - ClientApp.csproj (reference added)
  - Services/ProductService.cs (using statement)
  - Services/ProductsStateService.cs (using statement, maybe)
  - Pages/Products.razor (using statement)
  - Pages/ProductDetails.razor (using statement)
  - Pages/CreateProduct.razor (using statement)
  - Pages/EditProduct.razor (using statement)
  - Pages/DeleteProduct.razor (using statement)
  - Shared/ProductCard.razor (using statement)
  - Shared/ProductForm.razor (using statement)
  
- **ServerApp** (8-10 files):
  - ServerApp.csproj (reference added)
  - Endpoints/ProductEndpoints.cs (using statement)
  - Data/AppDbContext.cs (using statement)
  - Services/CacheService.cs (using statement)
  - Services/DbInitializerService.cs (using statement)
  - Services/SeedingService.cs (using statement)
  - Services/ValidationService.cs (using statement)
  - Program.cs (using statement)

- **Deleted** (6 files):
  - ClientApp/Models/Product.cs
  - ClientApp/Models/Category.cs
  - ClientApp/Models/PaginatedList.cs
  - ServerApp/Models/Product.cs
  - ServerApp/Models/Category.cs
  - ServerApp/Models/PaginatedList.cs

- **Other**:
  - REFACTORING_CHECKLIST.md (progress update)

**Total**: ~25-30 files affected

---

**Document Version**: 1.0  
**Phase**: 1 of 10  
**Prerequisites**: SharedModels project created with DTOs  
**Next Phase**: refactor-phase2-agent.md (Extract Business Logic)
