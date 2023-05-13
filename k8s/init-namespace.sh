[ ! -f ".env" ] && (echo ".env file is not found" && exit 1)
export $(cat ./.env | xargs)
[ -z "$EMAIL" ] && (echo 'EMAIL env. variable is not defined' && exit 1)
[ -z "$VIRTUAL_HOST" ] && (echo 'VIRTUAL_HOST env. variable is not defined (needed to configure DNS name)' && exit 1)
[ -z "$NAMESPACE" ] && (echo 'NAMESPACE env. variable is not defined (needed to configure DNS name)' && exit 1)
[ -z "$General__SiteUrl" ] && (echo "General__SiteUrl=$VIRTUAL_HOST" >> .env)

# This file sets up k3s on fresh VPS:
# 1. Create namespace
kubectl delete namespace $NAMESPACE
kubectl create namespace $NAMESPACE

# 5. Create Secret to authenticate in Docker Registry
kubectl -n $NAMESPACE delete secret docker-registry-secret
test $HOME/.docker/config.json && kubectl -n $NAMESPACE create secret generic docker-registry-secret --from-file=.dockerconfigjson=$HOME/.docker/config.json --type=kubernetes.io/dockerconfigjson


# 6. Create config maps (`$NAMESPACE-configmap-secret`)
kubectl -n $NAMESPACE create configmap $NAMESPACE-configmap-secret --from-env-file=.env || (kubectl -n $NAMESPACE create configmap $NAMESPACE-configmap-secret --from-env-file=.env -o yaml --dry-run=client | kubectl replace -f -)
kubectl -n $NAMESPACE create configmap $NAMESPACE-configmap
