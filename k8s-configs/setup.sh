[ ! -f ".env" ] && (echo ".env file is not found" && exit 1)
export $(cat ./.env | xargs)
[ -z "$EMAIL" ] && (echo 'EMAIL env. variable is not defined' && exit 1)
[ -z "$VIRTUAL_HOST" ] && (echo 'VIRTUAL_HOST env. variable is not defined (needed to configure DNS name)' && exit 1)
[ -z "$General__SiteUrl" ] && (echo "General__SiteUrl=$VIRTUAL_HOST" >> .env)

# This file sets up k3s on fresh VPS:
# 1. Install k3s
# 2. Setup Let's encrypt
# 3. Setup Kubernetes Dashboard
# 4. Setup Postgres
# 5. Setup App
# 
# --- Uninstall ---
# k3s could be uninstalled by running 
#   /usr/local/bin/k3s-uninstall.sh


# 1. Install k3s 

# On some VPS (for example https://www.mvps.net) external name resolution in CoreDns do not work
# (i.e. if you run `ping google.com` from your Pod, it resolves to some very strange IP address).
# The following lines solve this issue. 
cat /etc/k3s-resolv.conf | grep "nameserver 8.8.8.8" || echo "nameserver 8.8.8.8" | sudo tee /etc/k3s-resolv.conf
curl -sfL https://get.k3s.io | sh -s - --resolv-conf /etc/k3s-resolv.conf

# setup bash autocompletion
cat ~/.bashrc | grep "kubectl completion" || echo "source <(kubectl completion bash)" >> ~/.bashrc
source ~/.bashrc

# 2. Setup Let's encrypt
kubectl apply -f https://github.com/jetstack/cert-manager/releases/download/v1.11.1/cert-manager.yaml
sleep 15000
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/letsencrypt.yaml > letsencrypt.yaml
envsubst < letsencrypt.yaml > letsencrypt.yaml.tmp && mv letsencrypt.yaml.tmp letsencrypt.yaml
kubectl apply -f letsencrypt.yaml


# 3. Setup Kubernetes Dashboard
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/dashboard/setup-dashboard.sh | sh -s -


# kubectl create namespace templateapp

# # 4. Setup Postgres
# curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/postgres.yaml > postgres.yaml
# envsubst < postgres.yaml > postgres.yaml.tmp && mv postgres.yaml.tmp postgres.yaml
# kubectl apply -f postgres.yaml

# # 5. Setup App
# # authenticate in docker registry
# kubectl -n template-app delete secret docker-registry-secret
# test $HOME/.docker/config.json && kubectl -n template-app create secret generic docker-registry-secret --from-file=.dockerconfigjson=$HOME/.docker/config.json --type=kubernetes.io/dockerconfigjson
# # setup configmap
# kubectl -n templateapp create configmap templateapp-main-configmap --from-env-file=.env || (kubectl -n templateapp create configmap templateapp-main-configmap --from-env-file=.env -o yaml --dry-run=client | kubectl replace -f -)
# # setup deployment
# curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/templateapp-app.yaml > templateapp-app.yaml
# envsubst < templateapp-app.yaml > templateapp-app.yaml.tmp && mv templateapp-app.yaml.tmp templateapp-app.yaml
# kubectl apply -f templateapp-app.yaml

