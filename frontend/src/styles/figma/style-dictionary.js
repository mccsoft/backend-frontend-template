import { transforms } from 'style-dictionary/enums';
import { getReferences } from 'style-dictionary/utils';
import StyleDictionary from 'style-dictionary';
import { readFile } from 'node:fs/promises';

const normalizeTokens = (value) => {
  if (Array.isArray(value)) return value.map(normalizeTokens);
  if (value && typeof value === 'object') {
    return Object.fromEntries(
      Object.entries(value).map(([key, child]) => [
        key.trim(),
        normalizeTokens(child),
      ]),
    );
  }
  if (typeof value === 'string' && /^\{.*\}$/.test(value)) {
    return value.replace(/\s+\/\s+/g, '.');
  }
  return value;
};

// Like lodash's `kebabCase`, but keeps a letter directly followed by digits
// as one word (e.g. `p1`, `h2`) instead of splitting it into `p-1`, `h-2`.
function kebabCase(str) {
  return str
    .replace(/[#&+\(\)<>\[\]%]/g, '-')
    .replace(/([a-z0-9])([A-Z])/g, '$1-$2')
    .replace(/[\s_]+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-+|-+$/g, '')
    .toLowerCase();
}

// Same as the built-in `name/kebab` transform, but drops the leading collection/style-group
// path segment, e.g. `colors.white` becomes `--white` instead of `--colors-white`.
// Exception: 'Effect-styles' prefix is preserved.
StyleDictionary.registerTransform({
  name: 'name/kebab-strip-prefix',
  type: 'name',
  transform: (token, config) => {
    // Keep 'Effect-styles' prefix, strip others
    const shouldKeepPrefix = token.path[0] === 'Effect-styles';
    const pathToUse = shouldKeepPrefix ? token.path : token.path.slice(1);
    return kebabCase([config.prefix].concat(pathToUse).join(' '));
  },
});

// Figma's enum values for these fields aren't valid CSS keywords as-is.
const TEXT_TRANSFORM_BY_CASE = {
  ORIGINAL: 'none',
  UPPER: 'uppercase',
  LOWER: 'lowercase',
  TITLE: 'capitalize',
};
const TEXT_DECORATION_BY_VALUE = {
  NONE: 'none',
  UNDERLINE: 'underline',
  STRIKETHROUGH: 'line-through',
};

// One CSS class per typography style token (e.g. `.header-h1`, `.paragraph-p1-reg`).
StyleDictionary.registerFormat({
  name: 'css/typography-classes',
  format: ({ dictionary }) =>
    [
      '/**\n * Do not edit directly, this file was auto-generated.\n */\n\n',
      ...dictionary.allTokens
        .filter((token) => token.original.$type === 'typography')
        .map((token) => {
          // DTCG dimensions (e.g. fontSize) are still `{ value, unit }` objects here, not CSS length strings.
          const formatDimension = (value) =>
            typeof value === 'object' &&
            value !== null &&
            'value' in value &&
            'unit' in value
              ? `${value.value}${value.unit}`
              : value;

          // Render referenced properties as `var(--name)`, others as their (optionally mapped) literal value.
          const cssValue = (prop, mapLiteral) => {
            const originalProp = token.original.$value[prop];
            const [reference] =
              typeof originalProp === 'string'
                ? getReferences(originalProp, dictionary.tokens, {
                    usesDtcg: true,
                  })
                : [];
            if (reference) return `var(--${reference.name})`;
            const value = formatDimension(token.$value[prop]);
            return mapLiteral ? mapLiteral(value) : value;
          };

          // Skip properties whose value is the browser default, to keep the generated CSS lean.
          const isZeroDimension = (value) =>
            typeof value === 'string' && /^0(\.0+)?[a-z%]*$/i.test(value);
          const declaration = (name, value, isDefault) =>
            isDefault(value) ? [] : [`  ${name}: ${value};`];

          const textTransform = cssValue(
            'textCase',
            (value) => TEXT_TRANSFORM_BY_CASE[value] ?? value,
          );
          const textDecoration = cssValue(
            'textDecoration',
            (value) => TEXT_DECORATION_BY_VALUE[value] ?? value,
          );
          const textIndent = cssValue('paragraphIndent');
          const marginBottom = cssValue('paragraphSpacing');

          const lines = [
            `  font-family: ${cssValue('fontFamily')};`,
            `  font-style: ${cssValue('fontStyle')};`,
            `  font-weight: ${cssValue('fontWeight')};`,
            `  font-size: ${cssValue('fontSize')};`,
            `  line-height: ${cssValue('lineHeight')};`,
            `  letter-spacing: ${cssValue('letterSpacing')};`,
            ...declaration(
              'text-transform',
              textTransform,
              (value) => value === 'none',
            ),
            ...declaration(
              'text-decoration',
              textDecoration,
              (value) => value === 'none',
            ),
            ...declaration('text-indent', textIndent, isZeroDimension),
            ...declaration('margin-bottom', marginBottom, isZeroDimension),
          ];
          return `.${token.name} {\n${lines.join('\n')}\n}`;
        }),
    ].join('\n\n'),
});

// Figma variable modes (e.g. `light theme` / `dark theme`) become `[data-theme='...']`
// overrides, matching the selector convention already used in src/app/styles/*-theme.scss.
const themeSelectorForMode = (mode) =>
  `[data-theme='${kebabCase(mode.replace(/\s*theme$/i, ''))}']`;

// Resolves a raw `$extensions.mode` value, which is either a literal (e.g. a hex color)
// or a Figma reference string like `{Primitives.Dark theme.Navy blue.700}`.
const resolveModeValue = (rawValue, tokenByPath) => {
  const match = /^\{(.+)\}$/.exec(rawValue);
  if (!match) return rawValue;
  const referenced = tokenByPath.get(match[1]);
  return referenced ? `var(--${referenced.name})` : rawValue;
};

// Same output `formats.cssVariables` with `outputReferences: true` would produce for a
// single (non-composite) token: prefer `var(--name)` over the resolved literal value.
const resolveTokenValue = (token, dictionary) => {
  const originalValue = token.original.$value;
  const [reference] =
    typeof originalValue === 'string'
      ? getReferences(originalValue, dictionary.tokens, { usesDtcg: true })
      : [];
  return reference ? `var(--${reference.name})` : token.$value;
};

// `:root` variables plus `[data-theme='...']` overrides for tokens whose value differs by mode.
StyleDictionary.registerFormat({
  name: 'css/variables-with-modes',
  format: ({ dictionary }) => {
    const tokenByPath = new Map(
      dictionary.allTokens.map((token) => [token.path.join('.'), token]),
    );

    const overridesByMode = new Map();
    dictionary.allTokens.forEach((token) => {
      const modes = token.original.$extensions?.mode ?? {};
      if (Object.keys(modes).length < 2) return;

      Object.entries(modes).forEach(([mode, rawValue]) => {
        if (rawValue === token.original.$value) return; // matches the default already in :root
        if (!overridesByMode.has(mode)) overridesByMode.set(mode, []);
        overridesByMode
          .get(mode)
          .push(
            `  --${token.name}: ${resolveModeValue(rawValue, tokenByPath)};`,
          );
      });
    });

    const root = [
      ':root {',
      ...dictionary.allTokens.map(
        (token) =>
          `  --${token.name}: ${resolveTokenValue(token, dictionary)};`,
      ),
      '}',
    ].join('\n');

    const modeBlocks = Array.from(overridesByMode.entries()).map(
      ([mode, declarations]) =>
        [`${themeSelectorForMode(mode)} {`, ...declarations, '}'].join('\n'),
    );

    return [
      '/**\n * Do not edit directly, this file was auto-generated.\n */',
      root,
      ...modeBlocks,
    ].join('\n\n');
  },
});

const CSS_TRANSFORMS = [
  transforms.attributeCti,
  'name/kebab-strip-prefix',
  transforms.timeSeconds,
  transforms.htmlIcon,
  transforms.sizeRem,
  transforms.colorCss,
  transforms.assetUrl,
  transforms.fontFamilyCss,
  transforms.cubicBezierCss,
  transforms.strokeStyleCssShorthand,
  transforms.borderCssShorthand,
  transforms.transitionCssShorthand,
  transforms.shadowCssShorthand,
];

const styleDictionary = new StyleDictionary({
  log: {
    verbosity: 'verbose',
  },
  tokens: normalizeTokens(
    JSON.parse(
      await readFile(new URL('./tokens.json', import.meta.url), 'utf8'),
    ),
  ),
  platforms: {
    css: {
      transforms: [...CSS_TRANSFORMS, transforms.typographyCssShorthand],
      buildPath: 'src/styles/figma/',
      files: [
        {
          destination: 'variables.css',
          format: 'css/variables-with-modes',
        },
      ],
    },
    cssClasses: {
      // No typography shorthand here, so `format` sees the individual font properties.
      transforms: CSS_TRANSFORMS,
      buildPath: 'src/styles/figma/',
      files: [
        {
          destination: 'typography.css',
          format: 'css/typography-classes',
        },
      ],
    },
  },
});

await styleDictionary.buildAllPlatforms();
