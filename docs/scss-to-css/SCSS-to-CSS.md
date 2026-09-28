1. Use AI prompt in `variables.scss` file: `convert this file to css variables then use css variables in all scss.module files`

2. search and replace:

```
@extend \.(.*)
composes: $1 from global
```

```
@use 'src/styles/base' as *;
""
```

3. Rename all `*.scss` files to `*.css`
4. Adjust `razor.scss` (css generation for backend): remove all imports, adjust `build-css-for-razor` script in `package.json` (to use concat-cli)
