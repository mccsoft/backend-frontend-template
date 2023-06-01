# React Router

We use react-router v6, but currently stick to version 6.3.0 (please DO NOT update it).

## Reasoning

v6.3.0 is the [last one](https://github.com/remix-run/react-router/issues/8139#issuecomment-1262630360) to support [useBlocker](/frontend/src/helpers/router/useBlocker.ts)/[useCallbackPrompt](/frontend/src/helpers/router/useCallbackPrompt.ts) (ability to show confirmation when user leaves the page).

Starting from v6.4.0 `useBlocker` is only available when using [Data API](https://reactrouter.com/en/main/routers/picking-a-router#using-v64-data-apis), which we currently find inconvenient:

1. It breaks components independence, since you must define all routes in one place (and can't delegate the complex routing to a [nested components](https://reactrouter.com/en/main/start/overview#nested-routes)).
2. It suggests using [loaders](https://reactrouter.com/en/main/route/loader) to load data in Router, rather than within a component. While it might be good for overall UX/performance, it complicates the development, since data fetching now happens outside of component.
3. It overcomplicates things (with current workflow and multiple routers the usage is simple and clear, with forcing a single router it's complicated since you have to think of `<Outlets />` instead if just rendering a `<Switch>`/`<Route>` wherever needed)

We might reconsider these points in the future (since another major router - [@tanstack/router](https://github.com/tanstack/router) seems to go the same way), but this is the state as it is now.
