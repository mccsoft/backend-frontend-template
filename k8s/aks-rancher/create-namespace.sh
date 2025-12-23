#!/bin/bash

if [ -z "$1" ]; then
  echo -e "${RED}Error:${NC} path to kubeconfig not provided name not provided."
  echo "Usage: $0 <path_to_kubeconfig.config> <stage>"
  echo "Example: $0 ./kubeconfig.config dev"
  exit 1
fi

if [ -f "$1" ]; then
    echo "$1 found."
else
    echo "$1 does not exist or is not a regular file."
    exit 1
fi

if [ -z "$2" ]; then
  echo -e "${RED}Error:${NC} stage is not provided."
  echo "Usage: $0 <path_to_kubeconfig.config> <stage>"
  echo "Example: $0 ./kubeconfig.config dev"
  exit 1
fi

if [ -f "stages/$2.env" ]; then
    echo "Config for stage $2 found."
else
    echo "Config for stage $2 does not exist (should be at `stages/$2.env`)"
    exit 1
fi
source "stages/$2.env"
export KUBECONFIG=$1

kubectl create namespace ${namespace}
kubectl create serviceaccount ${namespace}-sa -n ${namespace}

kubectl create clusterrolebinding ${namespace}-sa-binding --clusterrole=cluster-admin --serviceaccount=${namespace}:${namespace}-sa

kubectl apply -n ${namespace} -f - <<EOF
apiVersion: v1
kind: Secret
metadata:
  name: "${namespace}-sa-token"
  annotations:
    kubernetes.io/service-account.name: "${namespace}-sa"
type: kubernetes.io/service-account-token
EOF

kubectl get secret "${namespace}-sa-token" -n ${namespace} -o json
kubectl config view --minify -o jsonpath={.clusters[0].cluster.server}
