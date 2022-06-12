# Set up development environment
1. Install your IDE of choice. We tend to use:
    1. Rider or Visual Studio for Backend development
    1. WebStorm or Visual Studio Code for frontend development

   You could choose one from above or use your own IDE if you feel productive with it :)
1. Install [git](https://git-scm.com/download/win).
1. Install your git client of choice (we tend to use IDE's git clients or console, but GitKraken or GitExtensions might suit you as well).
1. Install [docker](https://www.docker.com/products/docker-desktop/)
1. Install latest stable [.net sdk](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks) (.NET 6 at the time of writing).
1. Clone repository (with submodules).
1. Try to run the backend project (`webapi/MccSoft.TemplateApp.sln`) as usual (e.g. `F5` from Rider, assuming `Lmt.Unicorn.App` is a startup project). If you run into issue, consult your teammate :)
1. Go read about automated tests and set up [infrastructure for them](./Auto-tests.md).
