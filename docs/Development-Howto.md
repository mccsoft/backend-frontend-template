
# Day-to-day development workflow
1. Take a task/feature, move it to 'In Progress' on the Board
2. Create a branch for your feature/task
   1. Feature branches are generally named like 'feature/12345-add-sorting-to-products', where '12345' is the Id of your Task/UserStory with short description of what it does
3. Design a backend API interface
   1. Try to use [REST](./REST.md), but don't be too dogmatic. If non-REST interface suits better, go for it (but first make sure, that non-REST REALLY suits better and is more understandable)
   2. Discuss API with colleagues if in doubt
4. Implement backend logic
5. Autogenerate typescript client
6. Implement frontend logic
7. Write **at least** a single Component auto-test per API endpoint, more tests (including unit-tests) are better.
8. Create a Pull Request
9. Wait for it to be approved, answer & resolve all comments, make necessary changes to the code.
10. Merge PR **using squash**
11. Wait for your changes to be built on CI & deployed.
    1. Don't merge & go home! :) Wait for your merge to be built by CI. Better postpone merging till tomorrow morning (because if your changes break on CI and you are offline, your colleagues will be angry!)


# Development principles
1. Common sense above all. If some principle/rule goes against the common sense, don't do it, ask colleagues why/what-to-do.
2. We tend to use a lot of [Trunk-based-development](https://trunkbaseddevelopment.com) principles
   1. Create a branch per task/feature/functionality (branch name is like `feature/12345-add-sorting-to-products` where 12345 is the Id of your Task/UserStory), create a Pull Request when finished, merge **using squash** when PR is approved and tests are green.
      1. Some small changes could be pushed to master directly (with over-the-shoulder code review)
   2. We try to keep branches short-lived (e.g. 1-week is too long) & pull requests small (big PRs are hard to review)
   3. We run autotests for feature-branches (so wait for them to be green before merging your PR)
   4. We don't deploy to PROD on each commit, but we deploy to DEV, and we strive to keep the master branch ready-to-be-deployed.
   5. We use feature flags (and hide new not-finished functionality behind the feature flag) to keep the feature-branches short and master deployable.
3. When designing API/class follow the principle of the least astonishment. The API/behaviour you are building should not surprise the user or your fellow developer.
   1. If you are doing something non-obvious, leave a comment explaining other simpler solutions that you've tried, and why they didn't work out.
4. Don't build bicycles.
   1. If something similar to your feature exists in the codebase already - check it out and consider implementing it in a similar way.
   2. If you think about building something complex solution - maybe there's a 3rd party library for that (or at least, someone shared solutions to a similar problem in blog-post). Search for them.
5. Write SIMPLE code (it's actually harder than writing complex code :)). Make sure it's understandable for your colleagues and your future self.
6. Follow the SRP (single responsibility principle). Try to keep your classes/functions small, so that it's easy to grasp what they are doing.
7. Try to write self-documenting code (create good names for your classes/functions/variables). Comments should explain **why** you are doing something, not **what** are you doing (if you are going to write a comment like `/* attach device to a patient */`, just extract the code into a function named `AttachDeviceToPatient`)
   1. XML-comments to public functions/classes (if you are writing a library) are exceptions for this case.

# General
1. Try to use [REST API](./REST.md)
2. Read about [DateTime handling](./DateTime-handling.md) in backend
3. For pages with filters/sorting, store the filters in the URL.
   1. It should be possible to copy the URL, send it to the colleague, and if he opens the page, he should see the same thing.
   2. It should be possible to press F5 and see more-or-less the same result
   3. [Detailed how to](./details/Filter-Sorting.md) is also available.
4. According to [twelve-factor-app](https://12factor.net/config) principles, we allow overriding config parameters via environment variables at runtime.
   1. For backend you could easily override anything specified in `appsettings.json` using env. variables. E.g. `Email__Host=smtp.sendpulse.com`.
   2. For frontend you could add environment variables in several steps:
      1. Add variable to [.env](../frontend/.env) file
      2. Add variable **with type STRING** to [env.d.ts](../frontend/types/env.d.ts)
      3. Use normal ways to access env. variables ([provided by vite](https://vitejs.dev/guide/env-and-mode.html#env-files)), e.g. `import.meta.env.REACT_APP_SENTRY_DSN`
      4. Env. variables should have a prefix `REACT_APP`
      5. We use [import-meta-env](https://github.com/iendeavor/import-meta-env) plugin so you could override these variables at runtime as well.
      6. On hosting machine, when Docker image starts run `npx @import-meta-env/cli --example .env` which will use env. variables to override previously defined values.

# Backend
1. Do not use `Controllers` and `Services` folders. Rather create a folder in `Features` folder (with a name of your feature), and put your Controllers, Services and Dtos there.
  - it puts everything related to a feature in one place, which helps in refactoring and understanding


# Frontend
1. Use redux for client state only (and it's ok to not use redux at all :))
   1. Do not store Form state in redux (use `react-hook-form`)
   2. Do not store http-request-cache in redux (use `react-query`)
2. Routing: we use [react-router v6](https://reactrouter.com/docs/en/v6/).
   1. Define your route. We use a small [createLink](../frontend/src/application/constants/links.ts) wrapper to add typings to URLs.
      1. Add e.g. `WorkItemDetails: createLink('/projects/:id')}`
      2. Handle this route at [RootPage](../frontend/src/pages/authorized/RootPage.tsx) ```<Route path={Links.WorkItemDetails.route} element={<YOUR_PAGE_COMPONENT />} />```
      3. Within the page component you could access URL parameters using `Links.WorkItemDetails.useParams()`
   2. Optional parameters unfortunately are not supported. You have to define separate route for each optional parameter and `Links.WorkItemDetails.useMatch()` to get the values.
3. We tend NOT to use default exports. Please export and use components via named exports (i.e. `export const MyPage = () => <div>blablabla</div>`). Only use default exports with lazy-loading (via `React.lazy`, or better using the [lazyRetry helper](../frontend/src/helpers/retry-helper.tsx))

### FAQ
1. You could pull updates from Template into your project by running `yarn pull-changes-from-template` (it will actually run [scripts/pull-changes-from-template.js](../scripts/pull-template-changes.js)).
   1. It will clone the template repo next to your project folder, rename according to your project and copy Lib folder and other files that are not meant to be changed.
   2. You could examine/compare some other files and copy them to your project manually
   3. Check the changes before commiting them to your repo!
