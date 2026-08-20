---
name: figma-to-react
description: Implement Figma designs as production-ready React and TypeScript components using the existing project architecture, components, SCSS styles, and design system. Use when implementing a UI from a Figma URL or Figma node.
---

# Figma to React

Implement Figma designs as production-ready React + TypeScript + SCSS code.

The existing codebase is the primary implementation environment. The Figma design is the visual source of truth.

Do not blindly generate new components or styles. First understand the existing application architecture and reuse what already exists.

---

## Core principles

1. Figma is the source of truth for visual design.
2. The existing codebase is the source of truth for application architecture.
3. Reuse existing React components whenever possible.
4. Reuse existing SCSS variables, mixins, functions, utilities, and design tokens.
5. Do not introduce a new UI library.
6. Do not duplicate existing components.
7. Do not replace the project's existing styling architecture.
8. Do not modify unrelated code.
9. Prefer small, reusable components.
10. Preserve existing component APIs unless a change is necessary.
11. Keep TypeScript strict and type-safe.
12. The final implementation must be production-ready.

---

# Workflow

Always follow these phases in order.

## Phase 1 — Inspect the repository

Before writing code, inspect the existing project.

Identify:

- package.json
- React version
- TypeScript configuration
- routing
- component structure
- styling architecture
- SCSS structure
- global styles
- design tokens
- reusable UI components
- layout components
- icons
- image/asset handling
- existing page patterns
- testing setup
- linting
- formatting

Look for:

- Button
- Input
- Select
- Checkbox
- Radio
- Modal
- Dialog
- Dropdown
- Tabs
- Card
- Table
- Badge
- Tooltip
- Header
- Navigation
- Sidebar
- Form
- Pagination
- Loading states
- Empty states
- Error states

Do not create a new component until you have searched for an existing equivalent.

---

# Phase 2 — Analyze the Figma design

Use the Figma MCP tools to inspect the provided Figma URL or node.

The Figma URL supplied by the user is the source design.

Extract:

- page structure
- component hierarchy
- spacing
- dimensions
- typography
- colors
- borders
- border radius
- shadows
- icons
- images
- component variants
- states
- responsive behavior
- layout constraints
- Auto Layout
- Figma variables
- design tokens
- reusable Figma components

Use the Figma screenshot when visual verification is necessary.

Do not rely only on textual design information when visual details matter.

---

# Phase 3 — Map Figma to the existing codebase

Create an internal mapping between Figma components and existing application components.

Example:

Figma:

    Button / Primary

Existing code:

    src/components/Button/Button.tsx

Decision:

    Reuse existing Button component.

Another example:

Figma:

    UserCard

Existing code:

    No equivalent component found.

Decision:

    Create UserCard.

Before creating a component, search the repository for:

- exact component names
- similar component names
- matching UI patterns
- existing CSS/SCSS classes
- existing design-system components

Prefer extending an existing component over creating a duplicate.

---

# Phase 4 — Create an implementation plan

Before making substantial changes, determine:

1. Which existing components will be reused.
2. Which new components are required.
3. Which existing components need new props.
4. Which SCSS files need changes.
5. Which new SCSS files are required.
6. Which assets are required.
7. Which routes/pages need modification.
8. What responsive behavior is required.

Keep the plan minimal.

Do not refactor unrelated parts of the application.

---

# Phase 5 — Implement

Implement the design using:

- React
- TypeScript
- existing project conventions
- existing components
- existing SCSS architecture

Follow the existing folder structure.

Typical component structure:

    Component/
    ├── Component.tsx
    ├── Component.scss
    └── index.ts

Only use this structure if it matches the existing project conventions.

---

# React rules

Use functional components.

Prefer explicit TypeScript props.

Example:

    interface UserCardProps {
      user: User;
      onSelect?: (user: User) => void;
    }

    export function UserCard({
      user,
      onSelect,
    }: UserCardProps) {
      ...
    }

Avoid:

- `any`
- unnecessary abstractions
- unnecessary state
- duplicated JSX
- deeply nested conditional rendering
- large monolithic components

Prefer:

- composition
- reusable components
- existing hooks
- existing utilities
- existing application patterns

Do not introduce a new state-management solution.

Do not introduce a new dependency unless absolutely necessary.

---

# SCSS rules

Use the project's existing SCSS architecture.

Prefer existing:

- variables
- CSS custom properties
- mixins
- functions
- breakpoints
- spacing tokens
- typography tokens
- color tokens

Do not hardcode values that already exist as project tokens.

Avoid inline styles.

Do not use:

    style={{ ... }}

unless there is a strong technical reason.

Prefer:

    <div className={styles.container}>

or whatever convention the project already uses.

Follow the project's existing naming convention.

If the project uses BEM, use BEM.

If the project uses CSS Modules, use CSS Modules.

If the project uses global SCSS, follow that architecture.

Do not introduce a different styling approach.

---

# Figma dimensions

Do not blindly copy every pixel value from Figma.

First determine whether the value corresponds to an existing design token.

For example:

Figma:

    padding: 16px

If the project has:

    $spacing-md: 16px;

use:

    $spacing-md

instead of:

    16px

Similarly reuse existing:

- colors
- typography
- border radius
- shadows
- breakpoints
- spacing

If no token exists, use the Figma value.

---

# Typography

Match the Figma design using the existing typography system.

Inspect:

- font family
- font size
- font weight
- line height
- letter spacing
- text transformation

Prefer existing typography classes/tokens.

Do not introduce a new font unless the Figma design explicitly requires one and the project supports it.

---

# Colors

Use existing design tokens whenever possible.

For example:

    color: var(--color-text-primary);

instead of:

    color: #222222;

If the Figma color does not correspond to an existing token, use the Figma value.

Do not invent colors.

---

# Responsive behavior

Determine responsive behavior from:

- Figma variants
- Figma Auto Layout
- constraints
- available desktop/mobile frames
- existing application breakpoints

Use the project's existing breakpoints.

Do not invent a completely new responsive system.

If Figma contains only desktop design:

- infer reasonable responsive behavior
- follow existing project patterns
- avoid excessive media queries

The implementation should work at:

- mobile
- tablet
- desktop

when appropriate for the application.

---

# Assets

Use the Figma MCP asset capabilities when assets are required.

Prefer existing project assets when they already exist.

Do not recreate existing assets.

For images:

1. Check whether the repository already contains the image.
2. Check whether an equivalent asset exists in the project's asset directory.
3. If not, obtain the required Figma asset using the appropriate Figma MCP tool.
4. Store it according to the project's existing asset conventions.

Do not embed large base64 images directly into React source code.

Do not create fake placeholder assets when the actual Figma asset is available.

---

# Icons

Before creating or downloading an icon:

1. Search the project for the icon.
2. Search existing icon components.
3. Search the existing icon library.
4. Reuse the existing implementation.

Do not add a new icon library just for one icon.

If the project already uses SVG components, follow that convention.

---

# Forms

When implementing forms:

- reuse existing form components
- reuse existing validation
- reuse existing form hooks
- reuse existing error handling
- reuse existing labels
- reuse existing input components

Do not create a new form framework.

Preserve existing validation behavior.

---

# Accessibility

The implementation must be accessible.

Check:

- semantic HTML
- button vs div
- labels
- form controls
- keyboard navigation
- focus states
- ARIA attributes when required
- alt text
- heading hierarchy
- sufficient color contrast

Do not use clickable `<div>` elements when a semantic `<button>` or `<a>` is appropriate.

Do not remove existing accessibility behavior.

---

# Loading / empty / error states

If the existing application has patterns for:

- loading
- empty
- error
- disabled

reuse them.

If the Figma design contains these states, implement them.

Do not invent a new state-management pattern.

---

# Routing

If the Figma design represents a new page:

1. Inspect the existing routing.
2. Follow the existing routing convention.
3. Add the minimum required route.
4. Do not change unrelated routes.

---

# Data

Do not invent backend APIs.

If the page requires data:

1. Search the existing application for the relevant API/service.
2. Reuse existing types.
3. Reuse existing API clients.
4. Reuse existing hooks.

If the backend integration does not exist, create only the minimum interface required for the UI and clearly identify the missing integration.

Do not silently invent API endpoints.

---

# Component reuse

Reuse existing components whenever possible.

For example, if the repository contains:

    Button
    Input
    Select
    Modal
    Card

and the Figma design contains equivalent elements:

    Figma Button → existing Button
    Figma Input → existing Input
    Figma Select → existing Select
    Figma Modal → existing Modal
    Figma Card → existing Card

Do not create:

    FigmaButton
    FigmaInput
    FigmaSelect
    FigmaModal
    FigmaCard

unless the existing components genuinely cannot represent the design.

---

# Component extension

If an existing component is close but not sufficient:

1. Determine whether adding a prop is appropriate.
2. Preserve existing behavior.
3. Avoid breaking existing usages.
4. Keep the API backwards compatible where possible.

Example:

Existing:

    <Button variant="primary">

Figma requires:

    <Button variant="primary" size="large">

Prefer extending:

    Button

rather than creating:

    FigmaLargePrimaryButton

---

# Avoid over-engineering

Do not:

- refactor unrelated components
- rename unrelated files
- migrate styling systems
- introduce new libraries
- rewrite existing components unnecessarily
- create excessive abstractions
- create generic components without a real reuse case

Implement only what is necessary for the Figma design.

---

# Verification

After implementation, inspect the generated UI against the Figma design.

Check:

## Layout

- width
- height
- spacing
- alignment
- positioning
- responsive behavior

## Typography

- font
- size
- weight
- line height
- letter spacing

## Visuals

- colors
- borders
- radius
- shadows
- icons
- images

## Components

- correct variants
- correct states
- correct interactions

## Accessibility

- semantic HTML
- labels
- keyboard behavior
- focus states
- alt text

---

# Run project checks

After implementation run the existing project checks.

Prefer the commands already defined in package.json.

Typical checks may include:

    npm run lint

    npm run typecheck

    npm test

    npm run build

Do not assume these commands exist.

Inspect package.json first.

If a check fails:

1. Determine whether the failure was caused by your changes.
2. Fix the issue if it is related to the implementation.
3. Do not modify unrelated code merely to make the check pass.

---

# Visual verification

If the environment allows running the application:

1. Start the development server.
2. Open the implemented page.
3. Compare the UI with the Figma design.
4. Fix visual differences.

Pay particular attention to:

- spacing
- typography
- container width
- alignment
- responsive behavior
- component sizes
- colors
- borders
- shadows

Do not stop after the first generated implementation.

---

# Final response

After implementation, provide a concise summary containing:

## Implemented

List the major components/pages implemented.

## Reused

List important existing components reused.

## Created

List newly created components.

## Changed

List important existing files modified.

## Validation

Report which checks were executed and whether they passed.

Example:

    Implemented:
    - User profile page
    - Responsive user card
    - Mobile navigation

    Reused:
    - Button
    - Input
    - Modal
    - PageLayout

    Created:
    - UserCard

    Validation:
    - TypeScript: passed
    - ESLint: passed
    - Tests: passed

If something could not be verified, explicitly state it.

---

# Important rules

Never:

- generate a completely separate UI system
- duplicate existing components
- replace existing SCSS architecture
- introduce unnecessary dependencies
- invent APIs
- invent design tokens
- use placeholder assets when real Figma assets are available
- modify unrelated files
- skip repository inspection
- skip Figma inspection

Always:

- inspect first
- reuse existing code
- use Figma MCP
- follow project conventions
- preserve existing architecture
- verify the implementation
- run available checks
