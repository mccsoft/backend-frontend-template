[![Build Status](https://dev.azure.com/mcctomsk/Backend-Frontend-Template/_apis/build/status/mccsoft.backend-frontend-template?branchName=master)](https://dev.azure.com/mcctomsk/Backend-Frontend-Template/_apis/build/status/mccsoft.backend-frontend-template?branchName=master) [![MIT](https://img.shields.io/dub/l/vibe-d.svg)](https://opensource.org/licenses/MIT) [![NET7](https://img.shields.io/badge/-.NET%207.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## What's that

This is a template to set up a backend ASP.NET Core project with React frontend. You could check it out at [https://template.mcc-soft.de](https://template.mcc-soft.de) (credentials: admin/BSjo1M8jU760).

Developers are encouraged to start with [setting up the environment](docs/Setting-up-development-environment.md) and proceed to [development how-to](docs/Development-Howto.md).

Template contains (you could also check [CHANGELOG](./docs/CHANGELOG.md) for latest changes):

1. ASP.NET Core backend (.NET 7)
   1. OpenIddict authentication (with support for Social Networks authentication)
   2. Guidance/example on how to structure backend
   3. Helpers for easy implementation of REST API for CRUD (with PATCH instead of UPDATE)
   4. Configured Domain events to apply DDD principles
   5. Localization (i18next), background jobs (Hangfire), etc.
   6. Audit logging for DB entities
   7. Logging via Serilog to:
      1. [OpenSearch](https://logs.mcc-soft.de) (credentials: backend-frontend-template/$a55IVHBhAJ@)
      2. [Loggly](https://loggly.com) (free up to 250MB/day). You could check out logs [yourself](https://mcctemplateapp.loggly.com/) (credentials: mcc.template.app@gmail.com/dBAeFm7_y5mq3)
1. React frontend
   1. CRA-based template
   1. API client autogeneration (including react-query hooks)
   1. UI Kit to simplify app restyling
   1. Guidance/examples on setting up forms (react-hook-form), tables (with sorting/paging) and filters
   1. Support for injecting environment variables (without rebuilding the app)
   1. Redux with persistence
   1. Routing with react-router
   1. Error-boundaries which works with suspense/react-query/code splitting
   1. Pre-built authentication UI
1. Build-script to build a single Docker container from backend and frontend
   1. package.json based build script (just run `yarn build` from repo root)
1. Azure-pipelines CI script which
   1. Builds Docker Image
   1. Uploads it to Container Registry (on Digital Ocean)
   1. Deploys Image to preconfigured agent
1. Instructions about configuring an agent to deploy aforementioned Docker Image on any linux PC (Digital Ocean prefered :))
1. Kubernetes deployment files and guidance regarding deploy to kubernetes (in progress)

## Setting up new project from template

Before using the template for your brand-new app run `yarn rename -n YourProjectName`. Additionally you can use `-c YourCompanyName` to change solution's root namespace. Use CamelCase for your project name. The [script](./scripts/rename-script.js) does the following (you could check the [script](./scripts/rename-script.js) and do this manually if you like to):

1. Rename all files/folders from 'TemplateApp' to your project name.
1. Change all namespaces, classnames, database names, dockerfiles and deployment scripts to suit your project
1. Adjust SPA port number to something random, so that it doesn't clash with other projects (in package.json: 'start' script, PORT=3011; in main app .csproj: `<SpaProxyServerUrl>http://localhost:3011/</SpaProxyServerUrl>)

Create a new git repository and copy everything (except `.git` folder) to it. Don't forget to change the favicons/tiles in `frontend/public` folder to the logo of your project (use https://realfavicongenerator.net/ to create favicons in all required sizes)

## How to run locally

1. Check that connection string in appsettings is correct. Default connection string assumes that you have Postgres running on localhost on standard port with user/pass: postgres/postgres. Adjust connection string if the assumption is wrong.
   1. Make sure the target database exists (or that the postgres user has rights to create databases).
1. Run backend (Open `webapi/MccSoft.TemplateApp.sln` in Rider/Visual Studio and hit `F5`)
1. Run frontend (`yarn start` from `frontend` folder)
1. Frontend will open up in browser. Use that url to test the app!
1. (To authorize, you could use a preconfigured user. Login/Password is in `appsettings.json` under `DefaultUser` section)

## How to set up CI/CD

1. First of all, you need to create an email for your project. It will be needed for registering in 3rd party systems that the project will use (e.g. Sentry and Loggly)
2. Go to Azure and create Pipeline. Specify `.ci/azure-pipelines.yml` as the source of pipeline.
3. Set up Container Registry to push images to.
   1. You could use GitLab Container Registry (since it's private and free)
      1. Register a user in GitLab (using project email)
      1. Create a personal access token with read/write access to container registry at https://gitlab.com/-/profile/personal_access_tokens
      1. Add secret variable `DOCKER_TOKEN` to a pipeline (with value generated on previous step)
      1. Adjust `DOCKER_REGISTRY` (e.g. `registry.gitlab.com/mcctemplateapp1/main`) and `DOCKER_USER` (e.g. `mcc.template.app@gmail.com`) variables in pipeline.
4. (optional) If you want to add git Tags to your sourcecode when deploying to DEV/PROD, you need to do the following:
   1. Give access for WRITING to Repo for your Pipeline. Instructions: https://elanderson.net/2020/04/azure-devops-pipelines-manual-tagging/
   2. Set TAG_SOURCES_DEV and TAG_SOURCES_PROD variables in [base.partial.yml](.ci/_settings/base.partial.yml).
5. Run your pipeline. The first Stage (build) should pass.
6. Disable Pipeline notifications in Azure (i.e. 'Run stage waiting for approval' and 'Manual validation Pending') https://dev.azure.com/mccsoft/TemplateApp/_settings/notifications. Also disable them in your personal profile: https://dev.azure.com/mccsoft/_usersSettings/notifications
7. Pipeline contains 2 stages for deploying to DEV and PROD. You could add new deployment stages by copying existing once.

## Set up Hosting server (Droplet on Digital Ocean)

1. Create a virtual machine that will host your project.
   1. The preferable way is to create a Droplet on Digital Ocean.
      1. Create an account in DO (or use your existing account, you don't need to use a project email here), then create a new Team for new project (or ask your manager to do so and invite you to the Team)
      1. Create new droplet on Digital ocean, use 'Docker' as image, generate random secure password.
   1. Alternatively, set up any linux machine with root access. Install `docker` and `docker-compose`.
      1. To install `docker` follow [instructions](https://docs.docker.com/engine/install/ubuntu/#install-using-the-repository):
      1. To install `docker-compose` (at least 1.29.0 is required to use profiles): `sudo apt install pip && pip install docker-compose;`
   1. Write down the passwords somewhere in WIKI
1. SSH into droplet/VM and run the following (line by line):
   ```
   apt install mc
   adduser azure (generate and remember the password)
   usermod -aG sudo azure
   sudo visudo (add the following as the last line: `azure ALL=(ALL) NOPASSWD: ALL`, click Ctrl+X, save the changes). It will disable password prompt for azure user.
   su -l azure
   ```
1. Go to Azure DevOps, Pipelines -> Environment. Add Environment (name it 'DEV'), add virtual machine (Linux). Copy the script and run inside Droplet under `azure` user account (if you'd like to deploy more than one project to the same Droplet you could change $HOSTNAME in the script to something project-specific).
1. You should see your droplet in Environments in Azure.
1. (If it's a PROD environment, you could add Approval Checks in Azure Dev Ops (so that only manager could deploy to PROD))
1. Generate configuration (by running `./GenerateDotEnv.ps1` from `scripts` folder. File `.env.base` will be generated for you
1. Copy generated `env.base` file to HOME directory of `azure` user.
1. Adjust the file (change the Email, VirtualHost, Sentry and Loggly URLs).
1. (You could also grab Postgres password from this file to connect to Postgres later from your PC)
1. Optional: ONLY if you used Digital Ocean container registry. Make sure that **root** user has access to Container Registry which stores the images.
   1. To do that (in case of Digital Ocean Container Registry) run the following:
      ```
      sudo snap install doctl
      sudo snap connect doctl:dot-docker
      doctl auth init -t (INSERT DIGITAL OCEAN API TOKEN HERE)
      doctl registry login
      ```
