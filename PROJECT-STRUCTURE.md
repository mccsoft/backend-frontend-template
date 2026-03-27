# Backend-Frontend Template Project Structure

This document describes the high-level structure and key organizational patterns of this full-stack project.

## Directory Overview

### `/frontend` - React/TypeScript/Vite Frontend

- **Main entry**: `src/index.tsx`
- **Build tool**: Vite
- **Package manager**: Yarn
- **Key technologies**:
  - React 19 with TypeScript
  - Material-UI (MUI) for components
  - React Router for navigation
  - React Query (TanStack Query) for server state
  - Redux Toolkit for client state
  - React Hook Form for forms
  - i18next for internationalization

### `/webapi` - .NET 10 Backend

- **Main solution**: `MccSoft.TemplateApp.slnx`
- **Entry project**: `src/MccSoft.TemplateApp.App`
- **Target framework**: .NET 10
- **Key technologies**:
  - ASP.NET Core
  - Entity Framework Core
  - OpenIddict for authentication
  - Hangfire for background jobs
  - Serilog for logging

### `/e2e` - Playwright End-to-End Tests

- **Test framework**: Playwright
- **Page objects pattern**: `page-objects/`
- **Tests**: `tests/`
- **Infrastructure**: `infrastructure/` for environment setup

### `/k8s` - Kubernetes Deployment

- Kubernetes manifests for deployment
- Rancher-specific configuration in `aks-rancher/`

### `/nginx` - Nginx Configuration

- Reverse proxy configuration
- Virtual hosts setup

### `/docs` - Documentation

- Development guides and setup instructions
- Authentication and deployment documentation
- Architecture patterns and best practices

### `/scripts` - Build and Deployment Scripts

- Environment configuration (`*.env` files)
- Build and deployment automation
- Database scripts in subdirectories

## Key Patterns

### API Integration

- Backend exposes OpenAPI/Swagger documentation
- Frontend uses code generation from OpenAPI spec
- React Query hooks auto-generated for API calls

### Authentication

- OpenIddict-based OpenID Connect
- Social network authentication support
- Token-based with refresh token rotation

### State Management

- Server state: React Query
- Client state: Redux Toolkit with persistence
- Form state: React Hook Form

### Build Process

- Single Docker container build for entire stack
- Backend and frontend built together
- Environment-based configuration injection

## Development Commands

See individual README files or `docs/Development-Howto.md` for complete setup and commands.

## Key Files

- `docker-compose.yaml` - Local development containers
- `package.json` - Root-level build scripts
- `webapi/Directory.Packages.props` - Centralized .NET package management
- `frontend/vite.config.mts` - Frontend build configuration

## Architecture Notes

### Backend

- Uses DDD principles with domain events
- CRUD operations with PATCH support (not full UPDATE)
- Audit logging for database entities
- Structured logging to OpenSearch/Loggly

### Frontend

- Feature-based component organization
- Error boundaries for error handling
- Suspense-based code splitting
- Environment variable injection at runtime (no rebuild needed)
