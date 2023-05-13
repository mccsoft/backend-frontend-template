## Set up Hosting server (Droplet on Digital Ocean)

These are instructions regarding deploying your app to VM via docker-compose.This is an obsolete way, instructions are available for backwards compatibility.

It's NOT recommended for new projects, and even old ones should better switch to [Kubernetes (k3s)](/k8s/README.md) :).

Instructions:

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
