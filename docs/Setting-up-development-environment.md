# Set up development environment
1. Install your IDE of choice. We tend to use:
    1. Rider or Visual Studio for Backend development
    1. WebStorm or Visual Studio Code for frontend development

   You could choose one from above or use your own IDE if you feel productive with it :)
2. Install [charpier](https://csharpier.com/docs/Editors) extension into VisualStudio/Rider and [prettier](https://prettier.io/docs/en/editors.html) extension into WebStorm/VsCode.
   1. Configure csharpier to reformat on save in Rider/VisualStudio (go to plugin settings and turn on 'Run on Save' flag), WebStorm/VsCode are handled automatically.
3. Install [git](https://git-scm.com/download/win).
4. Install your git client of choice (we tend to use IDE's git clients or console, but GitKraken or GitExtensions might suit you as well).
5. Clone repository.
6. Install [docker](https://www.docker.com/products/docker-desktop/)
7. Run `scripts/postgresql/start_postgres.ps1`. It will start local PostgreSQL in Docker.
8. Install latest stable [.net sdk](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks) (.NET 6 at the time of writing).
9. Run `yarn install` from root folder of the repo. It will install JS libraries. You could run `yarn start-remote` to develop frontend locally pointing to the remote backend.
10. Try to run the backend project (`webapi/MccSoft.TemplateApp.sln`) as usual (e.g. `F5` from Rider, assuming `MccSoft.TemplateApp.App` is a startup project). You should have backend+frontend If you run into issue, consult your teammate :)
11. Go read about automated tests and set up [infrastructure for them](./Auto-tests.md). Run autotests locally, they should all be green.
12. Read [Development Howto](./Development-Howto.md) to know more about day-to-day development.
