# Refactoring Checklist

Quick reference for implementing recommended refactorings.

## ✅ Phase 1: SharedModels Integration (100% Complete)

**Completed**: October 31, 2025

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

**Estimated Time**: 2-3 hours  
**Actual Time**: ~2.5 hours  
**See**: `SharedModels/MIGRATION_GUIDE.md`

---

## 📋 Phase 2: Extract Business Logic

### Create ProductBusinessService
- [ ] Create `ServerApp/Services/ProductBusinessService.cs`
- [ ] Add IProductBusinessService interface
- [ ] Register service in Program.cs
- [ ] Move GetProducts logic from endpoint
- [ ] Move GetProductById logic from endpoint
- [ ] Move CreateProduct logic from endpoint
- [ ] Move UpdateProduct logic from endpoint
- [ ] Move DeleteProduct logic from endpoint
- [ ] Update ProductEndpoints.cs to use service
- [ ] Test each endpoint after refactoring

**Estimated Time**: 4-6 hours  
**See**: `REFACTORING.md` section 1

---

## 🔒 Phase 3: Consolidate Validation

- [ ] Review all validation usage
- [ ] Update CreateProduct to use ValidationService
- [ ] Update UpdateProduct to use ValidationService
- [ ] Move parameter validation to service layer
- [ ] Consider adding FluentValidation NuGet package
- [ ] Create ProductValidator class (if using FluentValidation)
- [ ] Update error responses to be consistent
- [ ] Test validation error messages

**Estimated Time**: 2-3 hours  
**See**: `REFACTORING.md` section 2

---

## 🎯 Phase 4: Constants Extraction

- [ ] Create `Shared/Constants/AppConstants.cs`
- [ ] Add Cache constants (durations)
- [ ] Add Pagination constants (sizes, defaults)
- [ ] Add API URL constants
- [ ] Update CacheService to use constants
- [ ] Update ClientApp/Program.cs to use constants
- [ ] Update Products.razor page sizes
- [ ] Search for remaining magic numbers
- [ ] Build and test

**Estimated Time**: 1-2 hours  
**See**: `REFACTORING.md` section 4

---

## 🧪 Phase 5: Add Unit Tests

### Setup
- [ ] Create ServerApp.Tests project
- [ ] Create ClientApp.Tests project
- [ ] Add xUnit NuGet package
- [ ] Add Moq NuGet package
- [ ] Add testing dependencies

### ServerApp Tests
- [ ] Test CacheService
- [ ] Test ValidationService
- [ ] Test ProductBusinessService (when created)
- [ ] Test SeedingService
- [ ] Test DbInitializerService

### ClientApp Tests
- [ ] Test ProductService
- [ ] Test ProductsStateService
- [ ] Test ErrorHandlerService
- [ ] Test ToastService

### Integration Tests
- [ ] Create ServerApp.IntegrationTests project
- [ ] Test GET /api/products
- [ ] Test GET /api/product/{id}
- [ ] Test POST /api/product
- [ ] Test PUT /api/product/{id}
- [ ] Test DELETE /api/product/{id}

**Estimated Time**: 8-10 hours  
**See**: `REFACTORING.md` section 14

---

## 🛡️ Phase 6: Security Hardening

### CORS
- [ ] Update Program.cs CORS policy
- [ ] Add environment-specific configuration
- [ ] Restrict origins for production
- [ ] Test with restricted origins

### Input Sanitization
- [ ] Add input sanitization for search terms
- [ ] Validate all user inputs
- [ ] Add anti-XSS measures

### Rate Limiting (Optional)
- [ ] Add rate limiting middleware
- [ ] Configure rate limits
- [ ] Test rate limiting

**Estimated Time**: 1-2 hours  
**See**: `REFACTORING.md` sections 19-21

---

## 🎨 Phase 7: Error Handling Improvement

- [ ] Create ErrorHandlingFilter
- [ ] Apply filter to all endpoints
- [ ] Remove try-catch from individual endpoints
- [ ] Test error scenarios
- [ ] Verify error responses are consistent

**Estimated Time**: 2-3 hours  
**See**: `REFACTORING.md` section 3

---

## 📦 Phase 8: Additional Models Migration

Consider moving these to SharedModels:
- [ ] Evaluate CreateProductRequest for sharing
- [ ] Evaluate UpdateProductRequest for sharing
- [ ] Evaluate ValidationProblemDetails for sharing
- [ ] Move selected models to SharedModels
- [ ] Update references
- [ ] Remove duplicates

**Estimated Time**: 1-2 hours  
**Status**: Optional/Future

---

## 📊 Phase 9: Repository Pattern (Optional)

- [ ] Create IProductRepository interface
- [ ] Create ProductRepository implementation
- [ ] Update ProductBusinessService to use repository
- [ ] Add repository to DI container
- [ ] Test repository
- [ ] Consider Unit of Work pattern

**Estimated Time**: 4-6 hours  
**See**: `REFACTORING.md` sections 7-8  
**Status**: Optional

---

## 🏥 Phase 10: Health Checks (Optional)

- [ ] Add health checks NuGet package
- [ ] Configure health checks in Program.cs
- [ ] Add database health check
- [ ] Add memory cache health check
- [ ] Add /health endpoint
- [ ] Test health endpoint

**Estimated Time**: 1 hour  
**See**: `REFACTORING.md` section 9  
**Status**: Optional

---

## 📈 Progress Tracking

### Overall Progress
```
[####------] 40% Complete (4/10 phases)

Phase 1: [####------] 40% (8/20 tasks)
Phase 2: [----------]  0% (0/10 tasks)
Phase 3: [----------]  0% (0/8 tasks)
Phase 4: [----------]  0% (0/9 tasks)
Phase 5: [----------]  0% (0/18 tasks)
Phase 6: [----------]  0% (0/7 tasks)
Phase 7: [----------]  0% (0/5 tasks)
Phase 8: [----------]  0% (0/5 tasks)
Phase 9: [----------]  0% (0/6 tasks)
Phase 10:[----------]  0% (0/6 tasks)
```

---

## 🎯 Quick Wins (Do These First)

1. ✅ Complete Phase 1 (SharedModels) - Already 40% done!
2. Phase 4 (Constants) - Easy, immediate benefits
3. Phase 6 (CORS Security) - Quick security improvement
4. Phase 3 (Validation) - Consistency improvement

---

## 📝 Notes

- Create a feature branch for each phase
- Commit frequently
- Test after each change
- Update documentation as you go
- Keep `main` branch stable

---

## 🔄 Update This Checklist

As you complete tasks, mark them with [x] and commit the changes:

```bash
git add REFACTORING_CHECKLIST.md
git commit -m "Update refactoring checklist progress"
```

---

**Last Updated**: October 31, 2025  
**Next Review**: After Phase 1 completion
