# Intro
We use Azure Pipelines to do a Continous Integration / Continous Delivery.
So for exact details you could just take a look at [azure-pipelines.yml](https://github.com/mcctomsk/backend-frontend-template/blob/master/azure-pipelines.yml) at the root of the repository.

# Overview

Build consists of 5 stages:

![](https://github.com/mcctomsk/backend-frontend-template/raw/master/.wiki/ci-cd.png)

1. The Build stage, which results in a Docker image pushed to some Container Registry (usually, GitLab).
1. DeployToDev stage, which deploys the image to Dev stage. This Stage is run automatically once Build completes.
1. TagDevSources which puts a `dev` tag on a Git Commit that was built and deployed.
1. DeployToProd stage, which deploys the built image to Prod stage. This Stage requires Approvement from Project Administrator to be run. To start the deployment one needs to click on 'Retry Stage', and then 'Approve' in the confirmation window.

![](https://github.com/mcctomsk/backend-frontend-template/raw/master/.wiki/ci-cd-approval.png)
1. TagProdSources puts a `prod` tag on Git Commit that was deployed. So you could actually know by looking at the Git Repo what's currently deployed to Dev and to Prod.

Technically DeployToDev and DeployToProd are identical, the only difference is domain/PC to which image is deployed.
Let's take a closer look at each Stage.

## Build
The following steps are performed during build:
1. Restore nuget/node packages
1. Run frontend/backend tests
1. Set version in package.json to be equal to the Build Id (see `BUILD_NUMBER: 0.1.$(Build.BuildId)` at the top of [azure-pipelines.yml](https://github.com/mcctomsk/backend-frontend-template/blob/master/azure-pipelines.yml). The '0.1.' part could and should be corrected depending on the current version of your project.
1. Run `yarn build` from the root of the Repository. Btw, you could also `yarn build` from your working PC to check if it works. Yarn builds does the following:
    1. Run typechecking on Frontend project
    1. Publishes Backend project.
    1. Builds Frontend project.
    1. Copies the `build` folder of frontend project to the `wwwroot` folder of the published backend project.
1. Builds and tags the Docker image. You could do this on your PC by running `yarn docker-build`, or `build-with-docker` (if you want to build backend, frontend and image at once). [Dockerfile](https://github.com/mcctomsk/backend-frontend-template/blob/master/webapi/src/MccSoft.TemplateApp.App/Dockerfile) is taken from the webapi/src/App project.
1. Pushes the built image to Container Registry.

## Deploy To Dev / Deploy To Prod
For Deploy to work you need to set up a PC that will host your project. Instructions on setting it up are available in [README.md](https://github.com/mcctomsk/backend-frontend-template#set-up-hosting-server-droplet-on-digital-ocean).

As a result, we should have a linux host with new user which runs Azure Agent and is added to Environments of a project.
On that host we expect to see `~/.env.base` file with basic environment variables defined (like Connection String, Sentry URL, clients/secrets, etc.). There are instructions on generating this file in [README.md](https://github.com/mcctomsk/backend-frontend-template#set-up-hosting-server-droplet-on-digital-ocean) as well.

'Deploy To Dev' and 'Deploy To Prod' stages are the same (except for Environment VM) and they basically do the following:
1. SSH into the Stage
1. Downloads the [docker-compose.yaml](https://github.com/mcctomsk/backend-frontend-template/blob/master/docker-compose.yaml) from Build Artifacts and copies it to HOME directory.
1. Renames the `~/.env.base` file into `~/.env` (so it will be consumed by docker-compose) and appends the Docker Image version to this file.
1. Runs `docker-compose up -d`.

The default docker-compose runs the following Containers:
1. Postgres container that runs your DB. On Production stages it's better NOT to use this, and use external managed Postgres instead. 
1. Main container that runs your backend/frontend (frontend was copied to `wwwroot` as part of the build).
1. NGINX container that proxies everything to your Main container and also performs GZip/SSL termination. You could configure your nginx via [nginx](https://github.com/mcctomsk/backend-frontend-template/blob/master/nginx) folder in your repo.
1. Acme container, which creates SSL certificates and injects them into the NGINX. Certificates are generated via Lets Encrypt.