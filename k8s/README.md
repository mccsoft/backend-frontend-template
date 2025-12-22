# Deploy to Kubernetes (k3s)

## Preparation

1. Create a virtual machine that will host your project (if you don't already have Rancher installed, if you do, skip to the next point)
   1. Install Rancher with k3s, e.g. by following [instruction](#install-rancher)
   1. Write down the root credentials somewhere in WIKI / KB
   1. Save `kubeconfig.yaml` file locally & in WIKI / KB
1. Prepare configuration
   1. Create DNS entry and point it to your Virtual Machine
   1. Run `./GenerateDotEnv.ps1 STAGE_NAME` from `scripts` folder. File `/k8s/aks-rancher/stages/STAGE_NAME.env` and `/k8s/aks-rancher/stages/secrets.rancher.STAGE_NAME.env` will be generated for you.
      1. Adjust the variable values in `STAGE_NAME.env`. At least **hostname** and **containers\_\_image** need to be changed.
   1. Run `create-namespace.sh <path_to_kubeconfig.config> <stage>` (from `k8s/aks-rancher` folder). Copy the echoed JSON and address, it will be needed later when setting up Kubernetes Service Connection in Azure DevOps
   1. Setup CI in Azure DevOps.
      1. Add Docker Service Connection (Azure -> Project Settings -> Service Connections -> New Docker Registry). Credentials could be taken:
         1. GitLab: fom [Deploy Tokens](https://gitlab.com/PROJECT_NAME/main/-/settings/repository#js-deploy-tokens) with read_registry & write_registry scopes
         1. Azure Portal: Settings -> Access Keys of `Container registry`
      1. Fill the NAME of this service connection in `./ci/_settings/acr.partial.yml` (connectorACR). Also fill-in `ACR_REGISTRY` and `ACR_REPOSITORY` in the same file
      1. Add Environment to deploy: Go to Pipelines -> Environments, hit "New environment", enter the name of the Stage, choose "Kubernetes", hit "Next". Choose "Generic Provider" and fill in the Secret (JSON from `create-namespace.sh` output, Server URL (URL from `create-namespace.sh` output), Namespace (`PROJECT_NAME-STAGE_NAME`, or just check created `STAGE_NAME.env` file), Cluster name could be the same as namespace). Hit "Validate and create" and ignore the 'Failed to query service connection API: An error occurred while sending the request.' error by clicking "Continue anyway".
   1. Create 2 pipelines in Azure DevOps pointing to `.ci/azure-pipelines.yml` and `.ci/azure-pipelines-pr-tests.yml`, adjust `./ci/settings` parameters if needed
   1. Run build pipeline, it should deploy initial version of your app to Kubernetes
   1. Login to Rancher and change the SECRETS (copy&paste the contents of `secrets.rancher.STAGE_NAME.env` there)

# Install k3s / Rancher

Nice instruction is provided by Google AI assistant via [set up rancher with k3s single node "sslip.io"](https://www.google.com/search?q=set+up+rancher+with+k3s+single+node+"sslip.io") query.

### 1. Install K3s on the Linux Node

SSH into your server and run the K3s installation script. `curl -sfL https://get.k3s.io | sh -` This command installs K3s, starts the service, and sets up the kubeconfig file at /`etc/rancher/k3s/k3s.yaml`.

### 2. Configure kubectl Access

For easier management from your local machine, copy the K3s configuration file and set the correct server URL.

On the server, get the content of your kubeconfig file: `sudo cat /etc/rancher/k3s/k3s.yaml`

Copy this content and save it to a file on your local machine (e.g., `rancher-kubeconfig.yaml`).

Edit the server: line in the local `rancher-kubeconfig.yaml` file to use the actual IP address of your Linux node (e.g., `https://<SERVER_IP>:6443`).

Set the KUBECONFIG environment variable on your local machine: `export KUBECONFIG=/path/to/rancher-kubeconfig.yaml`

### 3. Install cert-manager

Run the following on YOUR_PC (not on the server itself). Make sure that you did `export KUBECONFIG=/path/to/rancher-kubeconfig.yaml` locally and kubeconfig contains SERVER_IP and not the domain name.

```bash
helm repo add rancher-latest https://releases.rancher.com/server-charts/latest

kubectl create namespace cattle-system

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.19.1/cert-manager.crds.yaml

helm repo add jetstack https://charts.jetstack.io

helm repo update

helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --create-namespace
```

### 4. Install Rancher

Install Rancher, replacing <SERVER_IP> with your Linux node's actual IP address and setting a secure bootstrap password.

```bash
helm install rancher rancher-latest/rancher \
  --namespace cattle-system \
  --set hostname=<SERVER_IP>.sslip.io \
  --set replicas=1 \
  --set bootstrapPassword=<YOUR_SECURE_PASSWORD>
```

### 5. Verify the Installation and Access Rancher

Check the status of the Rancher pods. It may take a few minutes for all pods to show as Running.

```bash
kubectl get pods --namespace cattle-system --watch
```

Once all pods are running, open your web browser and navigate to the generated hostname: `https://<SERVER_IP>.sslip.io` You will be prompted to log in using the bootstrapPassword you set during the installation.

## How to add more Nodes

Instructions will follow

# Troubleshooting

1. If `https` has wrong certificate, start troubleshooting by running `kubectl -n YOUR_NAMESPACE describe certificate` (you might face an error like:

   > Failed to create Order: 429 urn:ietf:params:acme:error:rateLimited: Error creating new order :: too many certificates (5) already issued for this exact set of domains in the last 168 hours`).

   If it didn't help, head over to [CertManager troubleshooting guide](https://cert-manager.io/docs/troubleshooting/acme/).
