# Set up development environment

1. Install [latest .Net Core SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks).
2. Install [latest Node.js LTS](https://nodejs.org/en/).
3. Install your IDE of choice. The common choices are:

   1. VSCode for both frontend and backend development (VSCode is high performant and surprisingly good even for backend)
      - if you use VSCode it's recommended to open 2 instances:
        - the one with `root` folder (to work on frontend)
        - the one with `root/webapi` folder (to work on backend)
   1. Rider or Visual Studio for Backend development
   1. WebStorm or Visual Studio Code for frontend development

   You could choose one from above or use your own IDE if you feel productive with it :)

4. Install [git](https://git-scm.com/download/win).
5. Install your git client of choice (we tend to use IDE's git clients or console, but GitKraken or GitExtensions might suit you as well).
6. Install [docker](https://www.docker.com/products/docker-desktop/)
7. Clone repository.
8. Open root project folder in VSCode (if it's one of your chosen IDEs) and install recommended extensions (VSCode should ask you to do that, otherwise just go to Extensions tab and type '@recommended' ).
9. Install [charpier](https://csharpier.com/docs/Editors) extension into VisualStudio/Rider and [prettier](https://prettier.io/docs/en/editors.html) extension into WebStorm/VsCode.

   1. Configure csharpier to reformat on save in Rider/VisualStudio (go to plugin settings and turn on 'Run on Save' flag), WebStorm/VsCode are handled automatically.

10. Run `scripts/postgresql/start_postgres.ps1`. It will start local PostgreSQL in Docker.
11. Run `yarn install` from root folder of the repo. It will install JS libraries. You could run `yarn start-remote` to develop frontend locally pointing to the remote backend.
12. Try to run the backend project (`webapi/MccSoft.TemplateApp.slnx`) as usual (e.g. `F5` from VSCode/Rider/VisualStudio, assuming `MccSoft.TemplateApp.App` is a startup project). Frontend should start automatically together with backend. If you run into issue, consult your teammate :)
    1. You could also run frontend separately from backend:
       1. If you restart backend often, you could speed up startup by running frontend via `yarn start` from `frontend` folder (otherwise backend will start frontend on every restart);
       2. You could use remote backend (that way you don't have to start it locally). Run `yarn start-remote` from `frontend` folder.
13. Go read about automated tests and set up [infrastructure for them](./Auto-tests.md). Run autotests locally, they should all be green.
14. Read [Development Howto](./Development-Howto.md) to know more about day-to-day development.
