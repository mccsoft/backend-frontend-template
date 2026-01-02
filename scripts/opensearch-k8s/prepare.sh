#RUN `export KUBECONFIG=./kubeconfig.config` before executing this script

helm repo add opensearch https://opensearch-project.github.io/helm-charts/
helm repo update
#kubectl delete namespace opensearch
kubectl create namespace opensearch
kubectl apply -n opensearch -f .