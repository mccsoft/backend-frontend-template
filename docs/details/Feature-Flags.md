# Feature Flags
[Feature flags](https://martinfowler.com/articles/feature-toggles.html) allows to hide not-yet-ready feature from the user, making it possible to merge not fully completed features to Master and deploy to PROD.

Feature flags align with [twelve-factor-app](https://12factor.net/config) principles (as with any other settings they could be overriden via environment variables on Stage)

Here's the idea:
1. You start to implement a feature
2. You implement some very first steps, and according to [trunk-driven-development](../Development-Howto.md#development-principles) want to merge it to master
3. You need master to be deployable to PROD, but your feature isn't ready to be shown to the user yet (since you just started)
4. You create a feature-flag and turn it ON in DEV or Canary environment, and turn it OFF on Prod (via environment variables on docker container)
5. You add a code to visually hide / disable the feature if feature-flag is off.

# Technical details
## Backend
For backend you should add a feature-flag as a boolean setting in `appsettings.json`.
You then read it from `appsettings` as a usual configuration.
You could easily override anything specified in `appsettings.json` using env. variables. E.g. `Email__Host=smtp.sendpulse.com`.

## Frontend
Check out [env-variables.ts](/frontend/src/application/constants/env-variables.ts) for a reference implementation (`isMiniProfilerEnabled` feature flag).
Steps to add:
1. Add variable to [.env](/frontend/.env) file
2. Add variable **with type STRING** to [env.d.ts](/frontend/types/env.d.ts)
3. Use normal ways to access env. variables ([provided by vite](https://vitejs.dev/guide/env-and-mode.html#env-files)), e.g. `import.meta.env.REACT_APP_SENTRY_DSN`
4. Env. variables should have a prefix `REACT_APP`
5. Set variables when running Docker image on certain stage (e.g. in `Configuration` section of App Service in Azure Portal)
6. We use [import-meta-env](https://github.com/iendeavor/import-meta-env) plugin so you could override these variables at runtime as well.
7. On hosting machine, when Docker image starts run `npx @import-meta-env/cli --example .env` which will use env. variables to override previously defined values.
Steps 6 and 7 are already implemented in a template, so no need to do them for each feature flag.
