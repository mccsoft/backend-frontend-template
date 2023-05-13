[ ! -f ".env" ] && (echo ".env file is not found" && exit 1)
export $(cat ./.env | xargs)
[ -z "$NAMESPACE" ] && (echo 'NAMESPACE env. variable is not defined (needed to configure DNS name)' && exit 1)

# This file sets up k3s on fresh VPS:
# 1. Create namespace
kubectl create namespace $NAMESPACE

# 2. Create secret config maps (`$NAMESPACE-configmap-secret`)
kubectl -n $NAMESPACE create configmap $NAMESPACE-configmap-secret --from-env-file=.env || (kubectl -n $NAMESPACE create configmap $NAMESPACE-configmap-secret --from-env-file=.env -o yaml --dry-run=client | kubectl replace -f -)
