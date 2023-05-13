# Deploy to Kubernetes (k3s)

## Preparation

1. Create a virtual machine that will host your project.
   1. Write down the root credentials somewhere in WIKI
   1. Install `docker` (`curl -fsSL https://get.docker.com | sh -s`) if it's not installed already
1. Prepare configuration
   1. Create DNS entry and point it to your Virtual Machine
   1. Run `./GenerateDotEnv.ps1` from `scripts` folder. File `.env` will be generated for you
      1. Adjust the variable values. At least **EMAIL** needs to be changed.
      1. Adjust [dev.env](/.ci/.env/dev.env) and [prod.env](/.ci/.env/prod.env) files (change at least **VIRTUAL_HOST** and **General\_\_SiteUrl**). You could also put some non-secret variable values into these files. They will be applied on corresponding environments.
1. Create a folder on VM, e.g. `/home/k3s`. Copy `.env` file to that folder (you could run `nano` and copy&paste the contents).

## Setup

1. Authenticate in your docker registry (if it's private). Run `docker login https://YOUR_DOCKER_REGISTRY`. Run `docker pull YOUR_FULL_IMAGE_URL` to make sure everything works (you could skip this step if you run `docker login` before on this VM).
1. Run `curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s/setup.sh | /bin/bash -s -` from folder containing `.env` file. This will setup k3s, Kubernetes Dashboard (at `https://VIRTUAL_HOST/kube-dashboard`) and letsencrypt.
   1. Grab the contents of `dashboard-token.txt`, it contains the token you could use to login in Kubernetes Dashboard.
1. Create Secret File in Azure with your Kubernetes Config. To do so:
   1. Grab the `cat /etc/rancher/k3s/k3s.yaml` from your VM
   1. Save it locally as `k3s-dev.yaml` (or `k3s-prod.yaml` if it's PROD environment)
   1. Change `server: https://localhost:6443` to `server: https://SERVER_IP_ADDRESS:6443` (it's important to put IP, not the Hostname there)
   1. Add it to Azure Secret files.
1. Run your pipeline. Everything should be deployed.

# Reinitialize kubernetes

1. If you want to change something in `.env` (e.g. **VIRTUAL_HOST**, **EMAIL**, or some other secret), change it in VM. Then repeat the [setup](#setup) steps.
1. If you want to completely reinitialize whole kubernetes cluster, run the following:
   1. `/usr/local/bin/k3s-uninstall.sh` to uninstall everything
   1. Repeat the [setup](#setup) steps

# How to add more Nodes

Instructions will follow

# Troubleshooting

1. If `https` has wrong certificate, start troubleshooting by running `kubectl -n YOUR_NAMESPACE describe certificate` (you might face an error like:

   > Failed to create Order: 429 urn:ietf:params:acme:error:rateLimited: Error creating new order :: too many certificates (5) already issued for this exact set of domains in the last 168 hours`).

   If it didn't help, head over to [CertManager troubleshooting guide](https://cert-manager.io/docs/troubleshooting/acme/).
