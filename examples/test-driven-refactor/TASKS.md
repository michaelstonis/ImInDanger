# Tasks — Test-Driven Refactoring Example

## Objective
Refactor the `UserService` class to use dependency injection instead of direct database calls, guided by existing tests.

## Success Criteria
All tests pass (`npm test`), build succeeds (`npm run build`), no lint errors (`npm run lint`)

## Task List

### 1. Create UserRepository Interface
- **Status:** [ ]
- **Files:** `src/repositories/UserRepository.ts`
- **Description:** Create a `UserRepository` interface with methods: `findById(id: string)`, `findByEmail(email: string)`, `save(user: User)`, `delete(id: string)`. Each method should return a Promise. This interface will be the abstraction layer between UserService and the database.
- **Validation:** `npx tsc --noEmit`
- **Notes:** Follow existing interface patterns in the codebase

### 2. Implement PostgresUserRepository
- **Status:** [ ]
- **Depends on:** Task 1
- **Files:** `src/repositories/PostgresUserRepository.ts`
- **Description:** Implement the `UserRepository` interface using the existing Postgres connection pool. Move the SQL queries currently in `UserService` into this class.
- **Validation:** `npx tsc --noEmit`

### 3. Create MockUserRepository for Tests
- **Status:** [ ]
- **Depends on:** Task 1
- **Files:** `tests/mocks/MockUserRepository.ts`
- **Description:** Create an in-memory mock implementation of `UserRepository` for unit testing. Store data in a Map. Implement all interface methods.
- **Validation:** `npx tsc --noEmit`

### 4. Refactor UserService Constructor
- **Status:** [ ]
- **Depends on:** Task 1
- **Files:** `src/services/UserService.ts`
- **Description:** Modify `UserService` constructor to accept a `UserRepository` parameter via dependency injection. Replace all direct database calls with repository method calls.
- **Validation:** `npx tsc --noEmit`
- **Notes:** Do NOT modify any test files in this step

### 5. Update UserService Tests
- **Status:** [ ]
- **Depends on:** Task 3, Task 4
- **Files:** `tests/services/UserService.test.ts`
- **Description:** Update test setup to inject `MockUserRepository` into `UserService`. Ensure all existing test assertions remain unchanged — only the setup/teardown should change.
- **Validation:** `npm test -- --grep "UserService"`

### 6. Update Application Bootstrap
- **Status:** [ ]
- **Depends on:** Task 2, Task 4
- **Files:** `src/app.ts`
- **Description:** Update the application entry point to create `PostgresUserRepository` and inject it into `UserService`.
- **Validation:** `npm run build`

### 7. Full Validation Smoke Test
- **Status:** [ ]
- **Depends on:** All previous tasks
- **Description:** Run complete test suite, build, and lint to verify nothing is broken.
- **Validation:** `npm test && npm run build && npm run lint`
