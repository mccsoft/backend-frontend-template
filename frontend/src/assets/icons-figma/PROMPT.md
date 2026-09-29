# How to export Figma icons into this folder

Source node (all icons live under this frame): https://www.figma.com/design/rbgNeJbg5AuTV6BQLaDSeF/🦁-Kundenportal?node-id=383-4556&t=t3OJ3JwHvGTC8VJx-0

- fileKey: `rbgNeJbg5AuTV6BQLaDSeF`
- root nodeId: `383-4556` (frame "outlined", contains one sub-frame per icon size: 40/32/24/20/16 px)

## Filename convention

`<figma-layer-name>_<size?>__<nodeId-with-dash>.svg`

- Take the layer name exactly as it appears in Figma (lowercase where the existing files already lowercased it, e.g. `Device_24` → `device_24`).
- Variant properties like `direction=left, double=off` become kebab-case joined with the base name, e.g. `direction-left-double-off__25-1224.svg`.
- Replace the `:` in the Figma node id with `-` (e.g. `13694:22717` → `13694-22717`).
- Two underscores (`__`) separate the name from the node id.
- Look at existing files in this folder for the exact pattern before naming a new one.

## Steps to export new/changed icons

1. Call `mcp_figma_mcp_ser_get_metadata` with `fileKey=rbgNeJbg5AuTV6BQLaDSeF` and `nodeId=383-4556` to get the XML tree of every icon symbol/frame under the root node (size sub-frames: 40/32/24/20/16 px). This only returns names, ids, positions — no image data.
2. For every `<symbol>` (and any nested variant frames like `direction=...`, `state=...`, `size=...`) collect: figma node id + layer name.
3. Diff that list against the files already in `frontend/src/assets/icons-figma/` (match by node id suffix in the filename, not by name — names can repeat, e.g. multiple `send_20`). Any node id with no matching file is a new icon to export.
4. If a frame in the metadata XML is self-closing with no children (e.g. `<frame id="8244:94060" name="import_24" .../>`), it's an empty/unfinished placeholder — confirm with a second `get_metadata` call scoped to that node id, and skip exporting it if it still has no children.
5. For each new icon node id, call `mcp_figma_mcp_ser_download_assets` with `fileKey=rbgNeJbg5AuTV6BQLaDSeF`, `nodeId=<id>`, `defaultFormat=svg`. Use the `export` URL from the response (the full node render), not `svgAssets` (only a subset of vector layers).
6. Download each `export` URL with `curl -L -o "frontend/src/assets/icons-figma/<name>__<id>.svg" "<url>"` — the URLs are short-lived, download immediately after the tool call.
7. Read back each downloaded SVG and confirm it matches the existing structure (`<svg width="20" height="20" ...><rect .../><g id="outlined">...<g id="<layer-name>">...<path .../></g></g></svg>`, `fill="#8B8B8B"` for outlined icons) — this confirms it isn't an error page or truncated file.
8. Replace the default fill color (e.g. `fill="#8B8B8B"`, `stroke="#8B8B8B"`) in the downloaded SVG with `currentColor` (`fill="currentColor"` / `stroke="currentColor"`) so the icon inherits color from CSS.
