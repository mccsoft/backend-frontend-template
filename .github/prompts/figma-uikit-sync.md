---
name: figma-uikit-sync
description: Regenerate uikit-figma React components after a Figma design update
agent: agent
---

Use this prompt whenever the Figma `components` node has been updated and the generated React kit in `frontend/src/components/uikit-figma` needs to catch up.

## 1. Inspect the Figma node

- Figma file: `https://www.figma.com/design/rbgNeJbg5AuTV6BQLaDSeF/🦁-Kundenportal`

## 3. Map Figma families to React components

- Do NOT reuse components from `frontend/src/components/uikit` — `uikit-figma` must stay self-contained.
- For each Figma family, decide: new component, renamed prop, or no change.
- Keep one exported component (plus its prop type) per logical UI element, matching existing naming (`FigmaActionButton`, `FigmaTextField`, `FigmaChip`, etc.).

## 4. Implement / update components

- Find components under `https://www.figma.com/design/rbgNeJbg5AuTV6BQLaDSeF/🦁-Kundenportal?node-id=2-22&p=f&m=dev`
- Files live in `frontend/src/components/uikit-figma/`, split by concern:
  - `FigmaUiKit.shared.tsx` — shared primitives (`FigmaLink`, `FigmaSpinner`, `FigmaIconButton`, `CommonProps`)
  - `FigmaUiKit.controls.tsx` — buttons, text field, file/field input
  - `FigmaUiKit.selection.tsx` — checkbox, radio, chip, tab
  - `FigmaUiKit.navigation.tsx` — list item, breadcrumbs, page title
  - `FigmaUiKit.surfaces.tsx` — menu card, cart, modal header/footer, editor controls, footers, instruction step, group
  - `FigmaUiKit.feedback.tsx` — scrollbar, cursor, loader, toast, notification item, print data line
  - `FigmaUiKit.tsx` — barrel file only (`export * from './FigmaUiKit.*'`), no component logic here
  - `FigmaUiKit.module.scss` — all component styles
  - `index.ts` — `export * from './FigmaUiKit'`
- New Figma families get a new file section (or a new file if the concern doesn't fit existing groups) following the same pattern: typed props extending `CommonProps`, functional component, `clsx` for class composition.
- Renamed/restyled Figma variants: update the existing component's prop union types and the matching SCSS classes instead of creating duplicates.

## 5. Apply styling rules

- Colors: use CSS variables from `tokens-export.css` only (e.g. `var(--teal-800)`, `var(--ink-blue-900)`, `var(--red-700)`) — no hardcoded hex values, no fallback values baked into `var(...)`.
- Text formatting: never hardcode a Figma typography class as a string literal (no `'paragraph-p1-reg-line'`). Import `../../styles/figma/components.module.css` (a CSS module by filename convention, no `?module` query needed) in each file that needs it, then reference the class via the imported object (e.g. `clsx(styles.someClass, typography.paragraphP1RegLine)`). Let the SCSS module handle layout/spacing/color only; let the imported `components.module.css` classes handle font-family/weight/size/line-height/letter-spacing.

## 6. Validate

- Run diagnostics on `frontend/src/components/uikit-figma` (get_errors or equivalent) and fix any TypeScript/ESLint issues.
- Run `yarn --cwd frontend typecheck` and confirm it exits cleanly.
- Grep `frontend/src/components/uikit-figma` for `../uikit` to confirm no accidental dependency crept back in.
- Grep the module SCSS for raw hex colors (`#[0-9a-fA-F]{3,6}`) to confirm everything routes through token variables.

## 7. Report

- Summarize which Figma families were added/changed/removed and which files were touched.
- Call out any Figma family that has no reasonable React equivalent yet and needs a follow-up decision.
