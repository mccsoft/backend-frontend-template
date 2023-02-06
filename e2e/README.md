# Structure

## `infrastructure` folder

Contains base fixtures (e.g. `authenticatedTest` which authorizes the user)

## `page-objects` folder

Contains Page Objects for components and pages

## `storybook` folder

Contains tests that run over Storybook stories. We automatically generate a test per each Story, which basically takes a screenshot and verifies that it didn't change.

- [get-all-stories.test.ts](./storybook/get-all-stories.test.ts) - contains a single test that creates a [componentList.json](./storybook/componentList.json) file with all existing Stories. Run this test everytime you added a new Story (otherwise it will fail on CI).
- [storybook-smart.test.ts](./storybook/storybook-smart.test.ts) - contains screenshot tests for every Story from [componentList.json](./storybook/componentList.json). If the Story contains a single button, then we create another snapshot after the button is pressed.

## `tests` folder

Regular tests are located here.
