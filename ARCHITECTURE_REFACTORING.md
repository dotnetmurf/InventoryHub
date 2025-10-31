# Refactoring Architecture Diagram

## Current Architecture (Before Refactoring)

```
┌─────────────────────────────────────────────────────────────┐
│                       ClientApp                              │
│                                                              │
│  ┌────────────┐      ┌──────────────┐                      │
│  │   Pages    │─────▶│   Services   │                      │
│  │  (Razor)   │      │              │                      │
│  └────────────┘      │ ProductService                      │
│                      │ StateService  │                      │
│                      │ ErrorHandler  │                      │
│                      └──────┬───────┘                      │
│                             │                               │
│                      ┌──────▼───────┐                      │
│                      │    Models    │ (DUPLICATED!)       │
│                      │  Product     │                      │
│                      │  Category    │                      │
│                      │  Paginated   │                      │
│                      └──────────────┘                      │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                       ServerApp                              │
│                                                              │
│  ┌──────────────┐                                           │
│  │  Program.cs  │                                           │
│  │  (Startup)   │                                           │
│  └──────┬───────┘                                           │
│         │                                                    │
│  ┌──────▼────────────────────────────┐                     │
│  │     ProductEndpoints.cs           │                     │
│  │                                    │                     │
│  │  ❌ HTTP handling + business logic │                     │
│  │  ❌ Query building                 │                     │
│  │  ❌ Caching logic                  │                     │
│  │  ❌ Validation                     │                     │
│  │  ❌ Error handling (repetitive)    │                     │
│  └──────┬────────────┬────────────────┘                     │
│         │            │                                       │
│  ┌──────▼──────┐  ┌─▼──────────┐                           │
│  │   Models    │  │  Services  │                           │
│  │  Product    │  │  Cache     │                           │
│  │  Category   │  │  Validation│                           │
│  │  Paginated  │  │  DbInit    │                           │
│  │ (DUPLICATE!)│  └────────────┘                           │
│  └─────────────┘                                            │
│         │                                                    │
│  ┌──────▼─────────┐                                         │
│  │  AppDbContext  │                                         │
│  └────────────────┘                                         │
└─────────────────────────────────────────────────────────────┘
```

## Target Architecture (After Refactoring)

```
┌─────────────────────────────────────────────────────────────┐
│                       ClientApp                              │
│                                                              │
│  ┌────────────┐      ┌──────────────┐                      │
│  │   Pages    │─────▶│   Services   │                      │
│  │  (Razor)   │      │              │                      │
│  └────────────┘      │ ProductService                      │
│                      │ StateService  │                      │
│                      │ ErrorHandler  │                      │
│                      └──────┬───────┘                      │
│                             │                               │
│                             │ ┌──────────────────┐         │
│                             └▶│  using SharedModels│         │
│                               └──────────────────┘         │
└──────────────────────────────────────────────────────────────┘
                               │
                               │
        ┌──────────────────────▼─────────────────────────┐
        │          SharedModels (New!)                   │
        │                                                 │
        │  ✅ Product.cs                                 │
        │  ✅ Category.cs                                │
        │  ✅ PaginatedList.cs                           │
        │  ✅ CreateProductRequest.cs (Future)           │
        │  ✅ UpdateProductRequest.cs (Future)           │
        │                                                 │
        │  Single Source of Truth for DTOs               │
        └──────────────────────┬─────────────────────────┘
                               │
                               │ HTTP
┌──────────────────────────────▼──────────────────────────────┐
│                       ServerApp                              │
│                                                              │
│  ┌──────────────┐                                           │
│  │  Program.cs  │                                           │
│  │  (Startup)   │                                           │
│  └──────┬───────┘                                           │
│         │                                                    │
│  ┌──────▼────────────────────────┐                         │
│  │  ProductEndpoints.cs (SLIM!)  │                         │
│  │                                │                         │
│  │  ✅ HTTP handling only         │                         │
│  │  ✅ Delegates to services      │                         │
│  │  ✅ Consistent error handling  │                         │
│  └──────┬────────────────────────┘                         │
│         │                                                    │
│  ┌──────▼─────────────────┐                                │
│  │  ProductBusinessService │  (NEW!)                       │
│  │                          │                                │
│  │  ✅ Business logic       │                                │
│  │  ✅ Query building       │                                │
│  │  ✅ Caching coordination │                                │
│  │  ✅ Validation calls     │                                │
│  │  ✅ TESTABLE!           │                                │
│  └──────┬──────────────────┘                                │
│         │                                                    │
│  ┌──────▼──────┐  ┌───────────┐  ┌──────────────┐         │
│  │   Models    │  │  Services │  │  Repositories│ (Future)│
│  │  (Request)  │  │  Cache    │  │  IProductRepo│         │
│  │  (Params)   │  │  Validate │  │  ProductRepo │         │
│  └─────────────┘  └───────────┘  └──────┬───────┘         │
│                                          │                  │
│                                   ┌──────▼─────────┐        │
│                                   │  AppDbContext  │        │
│                                   └────────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

## Key Improvements

### 1. Shared Models Layer
```
Before: ClientApp.Models.Product ≠ ServerApp.Models.Product
After:  SharedModels.Product (single definition)
```

### 2. Service Layer Extraction
```
Before: 
  Endpoint ──> Database (60+ lines per endpoint)

After:
  Endpoint ──> BusinessService ──> Database
    (15 lines)    (Business Logic)
```

### 3. Consistent Validation
```
Before:
  ❌ Inline validation
  ❌ ValidationService (unused)
  ❌ Different patterns

After:
  ✅ ValidationService.TryValidate() everywhere
  ✅ Single pattern
  ✅ Consistent responses
```

### 4. Error Handling
```
Before:
  try { ... } catch { logger.LogError(); return Problem(); }
  try { ... } catch { logger.LogError(); return Problem(); }
  try { ... } catch { logger.LogError(); return Problem(); }

After:
  [ErrorHandlingFilter]  // Applied once to all endpoints
```

## Data Flow Comparison

### Before (Current)
```
User → Blazor Page → ProductService → HTTP → 
  → ProductEndpoints (business logic + caching + validation + DB access) → 
  → Response
```

### After (Target)
```
User → Blazor Page → ProductService → HTTP → 
  → ProductEndpoints → ProductBusinessService → 
  → Cache/Validation/Repository → Database → 
  → Response
```

## Benefits Visualization

```
┌─────────────────────┬──────────────┬──────────────┐
│   Metric            │   Before     │    After     │
├─────────────────────┼──────────────┼──────────────┤
│ Duplicate Code      │   ~500 LOC   │    0 LOC     │
│ Endpoint Length     │   40-60 LOC  │   15-25 LOC  │
│ Test Coverage       │      0%      │    60-80%    │
│ Magic Values        │     15+      │      0       │
│ Validation Patterns │      3       │      1       │
│ Testability         │     Low      │    High      │
│ Maintainability     │    Good      │  Excellent   │
└─────────────────────┴──────────────┴──────────────┘
```

## Migration Path

```
Phase 1: Foundation
  [✓] Create SharedModels project
  [ ] Add project references
  [ ] Migrate DTOs
  [ ] Remove duplicates

Phase 2: Separation
  [ ] Create BusinessService
  [ ] Extract endpoint logic
  [ ] Update endpoints
  
Phase 3: Quality
  [ ] Add unit tests
  [ ] Consolidate validation
  [ ] Extract constants
  
Phase 4: Security
  [ ] Secure CORS
  [ ] Add rate limiting
  [ ] Input sanitization
```

## Decision Tree: When to Use Each Layer

```
                    ┌────────────────┐
                    │ Incoming HTTP  │
                    │    Request     │
                    └────────┬───────┘
                             │
                    ┌────────▼────────┐
                    │   Endpoint      │ ─── HTTP concerns only
                    │ - Route binding │     No business logic
                    │ - Auth check    │
                    └────────┬────────┘
                             │
                    ┌────────▼────────────┐
                    │  Business Service   │ ─── Business rules
                    │ - Validation        │     Orchestration
                    │ - Business logic    │     Testable
                    │ - Caching decisions │
                    └────────┬────────────┘
                             │
                    ┌────────▼────────┐
                    │   Repository    │ ─── Data access
                    │ - Query building│     Abstraction
                    │ - EF queries    │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │   DbContext     │ ─── Raw DB access
                    └─────────────────┘
```

---

**Note**: This is a high-level architectural view. See `REFACTORING.md` for detailed code examples and implementation guidance.
