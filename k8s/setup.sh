# --- Uninstall ---
# k3s could be uninstalled by running 
#   /usr/local/bin/k3s-uninstall.sh

[ ! -f ".env" ] && (echo ".env file is not found" && exit 1)
export $(cat ./.env | xargs)
[ -z "$EMAIL" ] && (echo 'EMAIL env. variable is not defined' && exit 1)
[ -z "$NAMESPACE" ] && (echo 'NAMESPACE env. variable is not defined (needed to configure DNS name)' && exit 1)

# This file sets up k3s on fresh VPS:
# 1. Install k3s
# 2. Setup Let's encrypt
# 3. Setup Kubernetes Dashboard
# 4. Create namespace
# 5. Create Secret to authenticate in Docker Registry
# 6. Create config maps (`$NAMESPACE-configmap-secret`)


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
echo "sleep for 60 seconds..."
sleep 60
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s/letsencrypt.yaml > letsencrypt.yaml
envsubst < letsencrypt.yaml > letsencrypt.yaml.tmp && mv letsencrypt.yaml.tmp letsencrypt.yaml
kubectl apply -f letsencrypt.yaml


# 3. Setup Kubernetes Dashboard
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s/dashboard/setup-dashboard.sh | sh -s -


# 4/5/6 creates and initializes namespace
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s/init-namespace.sh | sh -s -
