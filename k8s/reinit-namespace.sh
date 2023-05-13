[ ! -f ".env" ] && (echo ".env file is not found" && exit 1)
export $(cat ./.env | xargs)
[ -z "$NAMESPACE" ] && (echo 'NAMESPACE env. variable is not defined (needed to configure DNS name)' && exit 1)

# This file sets up k3s on fresh VPS:
# 1. Delete namespace
# 2. Run script that initializes namespace from scratch

kubectl delete namespace $NAMESPACE

curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s/init-namespace.sh | sh -s -
