# Specify EMAIL as a first parameter 
test env.base || (echo "env.base file is not defined" && exit 1)
export $(cat ./env.base | xargs)
$EMAIL || (echo 'EMAIL env. variable is not defined' && exit 1)

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
echo "nameserver 8.8.8.8" | sudo tee /etc/k3s-resolv.conf
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


# 4. Setup Postgres
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/postgres.yaml > postgres.yaml
envsubst < postgres.yaml > postgres.yaml.tmp && mv postgres.yaml.tmp postgres.yaml
kubectl apply -f postgres.yaml

# 5. Setup App
# import docker secrets
kubectl delete secret docker-registry-secret
test $HOME/.docker/config.json || kubectl create secret generic docker-registry-secret --from-file=.dockerconfigjson=$HOME/.docker/config.json --type=kubernetes.io/dockerconfigjson
# setup configmap
kubectl -n templateapp delete configmap templateapp-main-configmap
kubectl -n templateapp create configmap templateapp-main-configmap --from-env-file=.env
# setup deployment
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/templateapp-app.yaml > templateapp-app.yaml
envsubst < templateapp-app.yaml > templateapp-app.yaml.tmp && mv templateapp-app.yaml.tmp templateapp-app.yaml
kubectl apply -f templateapp-app.yaml

