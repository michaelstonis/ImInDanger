# Tasks — Build Fix Example

## Objective
Fix the broken CI build caused by TypeScript upgrade from 4.x to 5.x.

## Success Criteria
Build succeeds (`npm run build`), all tests pass (`npm test`), no type errors (`npx tsc --noEmit`)

## Task List

### 1. Fix Strict Null Checks in UserController
- **Status:** [ ]
- **Files:** `src/controllers/UserController.ts`
- **Description:** TypeScript 5.x has stricter null checking. Fix all `TS2532: Object is possibly 'undefined'` errors in UserController. Add proper null guards — do NOT use non-null assertions (`!`) or `any` casts.
- **Validation:** `npx tsc --noEmit 2>&1 | grep -c "UserController" | grep "^0$"`
- **Notes:** Check the error log at `.loop-logs/build-errors.txt` for specific line numbers

### 2. Update Deprecated API Calls in AuthService
- **Status:** [ ]
- **Files:** `src/services/AuthService.ts`
- **Description:** Replace deprecated `ts.createSourceFile` calls with the new API. The TypeScript 5.x changelog lists the replacements. Also fix the `TS2345` type mismatch on the token validation return type.
- **Validation:** `npx tsc --noEmit 2>&1 | grep -c "AuthService" | grep "^0$"`

### 3. Fix Module Resolution Errors
- **Status:** [ ]
- **Files:** `tsconfig.json`
- **Description:** TypeScript 5.x changed default module resolution. Update `tsconfig.json` to use `"moduleResolution": "bundler"` or fix import paths that rely on the old resolution algorithm. Do NOT change `"strict": true`.
- **Validation:** `npx tsc --noEmit 2>&1 | grep -c "TS2307" | grep "^0$"`
- **Notes:** Be careful not to break existing imports

### 4. Fix Enum Usage Patterns
- **Status:** [ ]
- **Files:** `src/types/enums.ts`, `src/utils/helpers.ts`
- **Description:** TypeScript 5.x is stricter about enum member access. Fix `TS2551` errors where enum members are accessed with string indexing. Use proper enum member access syntax.
- **Validation:** `npx tsc --noEmit 2>&1 | grep -c "TS2551" | grep "^0$"`

### 5. Full Build Verification
- **Status:** [ ]
- **Depends on:** All previous tasks
- **Description:** Run complete build, test suite, and type check to verify the TypeScript 5.x upgrade is fully resolved.
- **Validation:** `npm run build && npm test && npx tsc --noEmit`
