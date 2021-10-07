/*
  Returns value set in environment variable or defaultValue.
  Environment variables could be set:
  - at build time (TeamCity/AppCenter build configuration), app name must be REACT_APP_*.
  - at runtime (when running via Docker image, e.g. in Azure).
  Usage: getEnvironmentVariableValue('$REACT_APP_SOME_SETTING_NAME');
 */
export function getEnvironmentVariableValue(
  environmentVariableName: string,
): string {
  if (!environmentVariableName.startsWith('$REACT_APP')) {
    // in Docker $REACT_APP_* gets substituted by env. variables
    // by docker via `inject-environment-variables-to-spa.sh`
    // if env variable is not set - return default
    return environmentVariableName;
  }

  const processEnvValue = process.env[environmentVariableName.substr(1)];
  if (processEnvValue) return processEnvValue;

  return '';
}
