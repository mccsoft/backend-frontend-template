const {
  override,
  adjustStyleLoaders,
  addWebpackPlugin,
} = require('customize-cra');

const useCamelCaseCssLoader = adjustStyleLoaders(({ use: [, css] }) => {
  if (css && css.options && css.options.modules) {
    css.options.modules.exportLocalsConvention = 'camelCase';
  }
});
const StatoscopeWebpackPlugin = require('@statoscope/webpack-plugin').default;
module.exports = override(
  useCamelCaseCssLoader,
  process.env.STATS
    ? addWebpackPlugin(new StatoscopeWebpackPlugin())
    : undefined,
);
