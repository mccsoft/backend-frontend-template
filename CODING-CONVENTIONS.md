# Coding Conventions

This document outlines shared coding conventions across the entire project.

## General Principles

### Readability First
- Code is read much more often than it's written
- Choose clarity over brevity
- Use meaningful names that express intent
- Add comments to explain "why", not "what"

### Consistency
- Follow established patterns in the codebase
- When in doubt, look at similar existing code
- Conventions apply across frontend and backend

### Type Safety
- Use language type systems effectively
- Avoid type system workarounds (`any`, `object`, etc.)
- Validate inputs at boundaries (API, user input)

## File and Directory Naming

### Frontend
- **Components**: PascalCase (`UserProfile.tsx`, `FormInput.tsx`)
- **Hooks**: camelCase starting with `use` (`useUserData.ts`, `useFormSubmit.ts`)
- **Utilities**: camelCase (`helpers.ts`, `validators.ts`)
- **Styles**: Match component name or PascalCase (`UserProfile.scss`)
- **Types**: PascalCase or suffix with `.types.ts` (`types.ts`, `UserTypes.ts`)

### Backend
- **Classes/Types**: PascalCase (`UserService.cs`, `CreateUserRequest.cs`)
- **Namespaces**: PascalCase with dots (`MccSoft.TemplateApp.Domain.Features.Users`)
- **Files**: Match class name (`UserService.cs` for class `UserService`)
- **Interfaces**: PascalCase, typically `I` prefix (`IUserRepository.cs`)

### General
- Use kebab-case for feature directories: `user-management/`, `auth-flow/`
- Keep directory names short, descriptive, lowercase
- Group related files together by feature

## Naming Variables and Functions

### Variables
- Descriptive, noun-based names
- Avoid single letters except in loops (`i`, `j`, `x`, `y`)
- Boolean variables: prefix with `is`, `has`, `can`, `should`

```typescript
// Good
const isUserActive = user.status === 'active';
const hasPermission = user.roles.includes('admin');

// Avoid
const active = user.status === 'active';
const usr = getUser();
```

### Functions and Methods
- Verb-based names describing the action
- `get*` for retrieving data
- `set*` for modifying data
- `is*/has*/can*` for boolean checks
- `handle*` for event handlers
- `async` methods suffix with `Async` (optional in TypeScript, recommended in C#)

```typescript
// Good Frontend
const handleClick = () => { };
const getUserById = (id) => { };
const isValidEmail = (email) => { };

// Good Backend
public async Task<User> GetUserByIdAsync(long id) { }
public void UpdateUserStatus(long userId) { }
public bool IsEmailUnique(string email) { }
```

### Constants
- ALL_CAPS_WITH_UNDERSCORES

```typescript
const MAX_RETRY_ATTEMPTS = 3;
const DEFAULT_PAGE_SIZE = 50;
```

## Imports and Exports

### Frontend (TypeScript)
- Group imports: React, libraries, local components, styles
- Use named exports for components

```typescript
import React from 'react';
import { Box, Button } from '@mui/material';
import { useQuery } from '@tanstack/react-query';

import { UserService } from '../services/UserService';
import { formatDate } from '../helpers/dateHelpers';
import './UserCard.scss';

export const UserCard: React.FC<UserCardProps> = ({ userId }) => {
  // component
};
```

### Backend (C#)
- Group using statements by namespace (System, third-party, local)
- Alphabetical within groups

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MccSoft.TemplateApp.Domain.Features.Users;
using MccSoft.TemplateApp.Database;
```

## Comments and Documentation

### When to Comment
- **Why, not what**: Explain business logic or non-obvious decisions
- **Workarounds**: Explain temporary solutions and ticket references
- **Complex algorithms**: Break down approach
- **Non-obvious parameter usage**: Clarify intent

### When NOT to Comment
- Self-explanatory code with good naming
- Redundant comments that repeat the code
- Commented-out code (use version control)

### Example Comments
```typescript
// Good: Explains the why
// Avoid creating new Date() here; cookies need specific format (RFC 1123)
const formattedDate = date.toUTCString();

// Good: Clarifies non-obvious behavior
// hasUnreadMessages should be recalculated AFTER socket reconnection
const [hasUnreadMessages, setHasUnreadMessages] = useState(false);

// Avoid: Redundant comment
// Set value to true
setValue(true);
```

### Documentation Comments

**Frontend (JSDoc-style for complex functions)**
```typescript
/**
 * Fetches user data and calculates aggregate statistics
 * @param userId - The unique identifier of the user
 * @returns Promise resolving to user stats
 * @throws UserNotFoundError if user doesn't exist
 */
export const getUserStats = async (userId: string): Promise<UserStats> => {
  // ...
};
```

**Backend (XML documentation)**
```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="userId">The unique identifier of the user</param>
/// <returns>The user entity, or null if not found</returns>
/// <exception cref="ArgumentException">Thrown if userId is invalid</exception>
public async Task<User?> GetUserByIdAsync(long userId)
{
    // ...
}
```

## Error Handling

### Error Messages
- Be specific and actionable
- Include context (user ID, resource name, etc.)
- Distinguish user-facing vs. system errors

```typescript
// Good: Specific and actionable
throw new Error(`Failed to create user with email "${email}": User already exists`);

// Avoid: Too generic
throw new Error('Error occurred');
```

### Logging Errors
- Always include error context
- Use appropriate log levels
- Include stack traces in non-production

```typescript
logger.error('User creation failed', {
  email,
  cause: error.message,
  stack: error.stack,
});
```

## Code Structure and Readability

### Function Length
- Keep functions focused (single responsibility)
- Extract logic into smaller functions
- Aim for readability over cleverness

### Nesting Depth
- Avoid deeply nested conditions (max 3 levels)
- Extract to separate functions
- Use early returns/guards

```typescript
// Good: Flat structure with early returns
function processUser(user: User): void {
  if (!user) {
    return;
  }
  
  if (user.isInactive) {
    logger.warn('Skipping inactive user');
    return;
  }
  
  // main logic
}

// Avoid: Deep nesting
function processUser(user: User): void {
  if (user) {
    if (!user.isInactive) {
      // main logic
    } else {
      logger.warn('Skipping inactive user');
    }
  }
}
```

### Line Length
- Keep lines reasonably short (80-120 characters)
- Break long lines logically
- Align for readability

```typescript
// Good: Readable line breaks
const result = await userService.updateUserProfile(
  userId,
  { email, fullName, preferences }
);

// Avoid: Too long
const result = await userService.updateUserProfile(userId, { email, fullName, preferences });
```

## Performance Considerations

### Optimization
- Write clear code first, optimize later
- Profile before optimizing (don't guess)
- Document performance-critical decisions

### Caching
- Cache expensive operations explicitly
- Provide cache invalidation strategy
- Avoid cache stampede scenarios

## Security Considerations

### Secrets
- Never hardcode secrets, API keys, or credentials
- Use environment variables or secret management
- Don't log sensitive data

### Input Validation
- Validate all external inputs (API requests, user input)
- Sanitize strings for display (prevent XSS)
- Validate data types and ranges

## Testing

### Test Naming
- Describe what is being tested
- Include expected behavior
- Use `test('should ...')` or `test('it ...')` patterns

```typescript
test('should return error when email already exists', () => { });
test('it saves user with all required fields', () => { });
```

### Assertions
- One logical assertion per test when possible
- Use specific assertions (not generic truthy checks)
- Include meaningful assertion messages

```typescript
// Good: Specific assertion
expect(user.email).toBe('test@example.com');

// Avoid: Generic
expect(user).toBeTruthy();
```

## Version Control Commits

### Commit Messages
- Descriptive subject line (50 characters max)
- Reference issues/tickets when applicable
- Use imperative mood: "Add feature" not "Added feature"

```
feat: add user profile update endpoint
fix: resolve null reference in user service
docs: update authentication guidelines
```

### Pull Requests
- Descriptive title
- Reference related issues
- Explain changes and why
- Keep PRs focused on single feature/fix
