# Figma snapshot export

Exports a raw snapshot of local Figma variable collections, variables, and styles (paint/text/effect/grid) from the Kundenportal Figma file.

Figma file: https://www.figma.com/design/rbgNeJbg5AuTV6BQLaDSeF/🦁-Kundenportal

File key: `rbgNeJbg5AuTV6BQLaDSeF`

## Prerequisites

- Figma MCP server connected in the coding agent (`use_figma` tool available), with the `figma-use` skill loaded first.
- Edit access to the Figma file above.

## Script (build snapshot, upload, return URL)

1. This is the script that is actually run via `use_figma`.

```js
const variables = await figma.variables.getLocalVariablesAsync();

const variableById = new Map(
  variables.map((variable) => [variable.id, variable]),
);

const serializeBinding = (binding) => ({
  id: binding.id,
  name: variableById.get(binding.id)?.name,
});

const serializeBoundVariables = (boundVariables) => {
  if (!boundVariables) return undefined;

  return Object.fromEntries(
    Object.entries(boundVariables).map(([property, binding]) => [
      property,
      serializeBinding(binding),
    ]),
  );
};

const snapshot = {
  variableCollections: (
    await figma.variables.getLocalVariableCollectionsAsync()
  ).map((collection) => ({
    id: collection.id,
    name: collection.name,
    modes: collection.modes,
    defaultModeId: collection.defaultModeId,
    remote: collection.remote,
    hiddenFromPublishing: collection.hiddenFromPublishing,
  })),

  variables: variables.map((variable) => ({
    id: variable.id,
    key: variable.key,
    name: variable.name,
    variableCollectionId: variable.variableCollectionId,
    resolvedType: variable.resolvedType,
    remote: variable.remote,
    hiddenFromPublishing: variable.hiddenFromPublishing,

    // Variable aliases / other variable bindings
    valuesByMode: Object.fromEntries(
      Object.entries(variable.valuesByMode).map(([modeId, value]) => [
        modeId,
        typeof value === 'object' &&
        value !== null &&
        'type' in value &&
        value.type === 'VARIABLE_ALIAS'
          ? {
              type: value.type,
              id: value.id,
              name: variableById.get(value.id)?.name,
            }
          : value,
      ]),
    ),
  })),

  paintStyles: (await figma.getLocalPaintStylesAsync()).map((style) => ({
    id: style.id,
    key: style.key,
    name: style.name,
    description: style.description,
    remote: style.remote,

    paints: style.paints.map((paint) => ({
      ...paint,

      // Paint properties that can contain variables
      boundVariables: serializeBoundVariables(paint.boundVariables),
    })),

    boundVariables: serializeBoundVariables(style.boundVariables),
  })),

  textStyles: (await figma.getLocalTextStylesAsync()).map((style) => ({
    id: style.id,
    key: style.key,
    name: style.name,
    description: style.description,
    fontSize: style.fontSize,
    fontName: style.fontName,
    letterSpacing: style.letterSpacing,
    lineHeight: style.lineHeight,
    remote: style.remote,
    textDecoration: style.textDecoration,
    paragraphIndent: style.paragraphIndent,
    paragraphSpacing: style.paragraphSpacing,
    textCase: style.textCase,
    textWrapStyle: style.textWrapStyle,

    boundVariables: serializeBoundVariables(style.boundVariables),
  })),

  effectStyles: (await figma.getLocalEffectStylesAsync()).map((style) => ({
    id: style.id,
    key: style.key,
    name: style.name,
    description: style.description,
    remote: style.remote,

    effects: style.effects.map((effect) => ({
      ...effect,

      boundVariables: serializeBoundVariables(effect.boundVariables),
    })),

    boundVariables: serializeBoundVariables(style.boundVariables),
  })),

  gridStyles: (await figma.getLocalGridStylesAsync()).map((style) => ({
    id: style.id,
    key: style.key,
    name: style.name,
    description: style.description,
    remote: style.remote,

    layoutGrids: style.layoutGrids.map((grid) => ({
      ...grid,

      boundVariables: serializeBoundVariables(grid.boundVariables),
    })),

    boundVariables: serializeBoundVariables(style.boundVariables),
  })),
};

return snapshot;
```

2. Fetch the result in fixed-size chunks (`CHUNK = 18000` chars is safe) across multiple `use_figma` calls — each call returns `{ idx, total, chunk: json.slice(idx * CHUNK, idx * CHUNK + CHUNK) }` for a different `idx`, from `0` to `Math.ceil(total / CHUNK) - 1`. Independent calls can be made in parallel.
3. Reassemble locally in order, straight into `snapshot.json` (PowerShell), then skip to validation — steps 2 and 3 below (download/cleanup) don't apply since there's no remote blob:

```powershell
$destDir = "USE_DIRECTORY_WITH_THIS_README"
New-Item -ItemType Directory -Force -Path $destDir | Out-Null

$sb = New-Object System.Text.StringBuilder
foreach ($chunkFilePath in $orderedChunkFiles) { # ordered by idx, from the use_figma tool results
  $obj = Get-Content $chunkFilePath -Raw | ConvertFrom-Json
  [void]$sb.Append($obj.chunk)
}
[System.IO.File]::WriteAllText((Join-Path $destDir "snapshot.json"), $sb.ToString())
```

4. Validate: `Get-Content snapshot.json -Raw | ConvertFrom-Json` should parse without error, and the reconstructed length should equal `total` from step 1.

## Files in this folder

- `snapshot.json` — final downloaded snapshot.
