# Set up development environment
1. Install your IDE of choice. We tend to use:
    1. Rider or Visual Studio for Backend development
    1. WebStorm or Visual Studio Code for frontend development

   You could choose one from above or use your own IDE if you feel productive with it :)
2. Install [git](https://git-scm.com/download/win).
3. Install your git client of choice (we tend to use IDE's git clients or console, but GitKraken or GitExtensions might suit you as well).
4. Clone repository.
5. Install [docker](https://www.docker.com/products/docker-desktop/)
6. Run `scripts/postgresql/start_postgres.ps1`. It will start local PostgreSQL in Docker.
7. Install latest stable [.net sdk](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks) (.NET 6 at the time of writing).
8. Run `yarn install` from root folder of the repo. It will install JS libraries. You could run `yarn start-remote` to develop frontend locally pointing to the remote backend.
9. Try to run the backend project (`webapi/MccSoft.TemplateApp.sln`) as usual (e.g. `F5` from Rider, assuming `MccSoft.TemplateApp.App` is a startup project). You should have backend+frontend If you run into issue, consult your teammate :)
10. Go read about automated tests and set up [infrastructure for them](./Auto-tests.md). Run autotests locally, they should all be green.
11. Read [Development Howto](./Development-Howto.md) to know more about day-to-day development.
