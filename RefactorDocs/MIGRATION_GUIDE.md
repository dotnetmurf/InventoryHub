# SharedModels Migration Guide

## Overview
This guide helps complete the migration from duplicated models to the shared `SharedModels` project.

## Step 1: Add Project References

Run these commands from the solution root:

```powershell
dotnet add ClientApp/ClientApp.csproj reference SharedModels/SharedModels.csproj
dotnet add ServerApp/ServerApp.csproj reference SharedModels/SharedModels.csproj
```

## Step 2: Update Using Statements

### In ClientApp:
Find and replace in these files:
- `ClientApp/Services/ProductService.cs`
- `ClientApp/Services/ProductsStateService.cs`
- `ClientApp/Pages/*.razor` files
- Any other files using `ClientApp.Models`

Replace:
```csharp
using ClientApp.Models;
```

With:
```csharp
using SharedModels;
```

### In ServerApp:
Find and replace in these files:
- `ServerApp/Endpoints/ProductEndpoints.cs`
- `ServerApp/Services/*.cs`
- `ServerApp/Data/AppDbContext.cs`
- Any other files using `ServerApp.Models`

Replace:
```csharp
using ServerApp.Models;
```

With:
```csharp
using SharedModels;
```

## Step 3: Remove Duplicate Files

After verifying compilation and tests pass, delete these files:

### From ClientApp/Models:
- `Product.cs`
- `Category.cs`
- `PaginatedList.cs`

### From ServerApp/Models:
- `Product.cs`
- `Category.cs`
- `PaginatedList.cs`

**Keep these files** (not duplicated):
- `ClientApp/Models/`:
  - `CreateProductRequest.cs`
  - `ErrorResponse.cs`
  - `ProductServiceException.cs`
  - `ToastMessage.cs`
  - `UpdateProductRequest.cs`
  - `UserError.cs`
  - `ValidationException.cs`
  - `ValidationProblemDetails.cs`

- `ServerApp/Models/`:
  - `CreateProductRequest.cs`
  - `PaginationParams.cs`
  - `UpdateProductRequest.cs`
  - `ValidationProblemDetails.cs`

## Step 4: Verify Compilation

```powershell
dotnet build InventoryHub.sln
```

Fix any compilation errors related to:
- Missing using statements
- Type resolution issues
- Namespace conflicts

## Step 5: Test Thoroughly

1. Run ServerApp and verify API endpoints work
2. Run ClientApp and verify all CRUD operations work
3. Test pagination, search, and filtering
4. Verify error handling still works correctly

## Step 6: Update Documentation

Update `README.md` and other documentation to mention the `SharedModels` project.

## Benefits After Migration

- ✅ Single source of truth for DTOs
- ✅ No more manual synchronization between client/server models
- ✅ Compile-time safety for contract changes
- ✅ Easier maintenance and refactoring
- ✅ Reduced code duplication

## Rollback Plan

If issues arise, rollback is simple:
1. Remove project references to SharedModels
2. Restore original using statements
3. Keep duplicate model files in place
4. Rebuild solution

## Future Considerations

Once stable, consider moving these to SharedModels:
- `CreateProductRequest`
- `UpdateProductRequest`
- `ValidationProblemDetails`
- `PaginationParams`

This would further reduce duplication and ensure consistency.
