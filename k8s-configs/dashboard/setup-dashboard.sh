# Delete dashboard if exists
kubectl delete namespace kubernetes-dashboard

# Setup Kubernetes Dashboard
kubectl create -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/alternative.yaml

#   add ingress and users
mkdir dashboard
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/dashboard/ingress.yaml > dashboard/ingress.yaml
curl -sfL https://raw.githubusercontent.com/mccsoft/backend-frontend-template/master/k8s-configs/dashboard/users.yaml > dashboard/users.yaml
kubectl apply -f dashboard/ingress.yaml
kubectl apply -f dashboard/users.yaml

#   generate token to login
DASHBOARD_TOKEN=$(kubectl -n kubernetes-dashboard get secret/admin-user-secret -o jsonpath="{.data.token}" | base64 --decode)
echo $DASHBOARD_TOKEN > dashboard-token.txt
echo "Your Kuberenetes Dashboard token (also in dashboard-token.txt): $DASHBOARD_TOKEN"
