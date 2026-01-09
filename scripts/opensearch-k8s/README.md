# How to run Opensearch

This is a small instruction how to run Opensearch in kubernetes

## Step 1 - Run stack

1. Adjust URL in [08-ingress.yaml](./08-ingress.yaml) (i.e. change logs.mcc-soft.de to something else)
1. Change `OPENSEARCH_INITIAL_ADMIN_PASSWORD` in [01-stateful-set.yaml](./01-stateful-set.yaml) to something else. Do not commit to repo
1. `Copy kubeconfig.config` from your k8s installation, run `export KUBECONFIG=./kubeconfig.config`
1. Run `./prepare.sh`
1. That's it. Your dashboards are exposed at https://YOUR_HOST, you can feed logs to https://YOUR_HOST/data

## Step 2 - Configure additional users

### User for backend service

1. Open opensearch dashboards
1. Add Tenant for you project (Security -> Tenants)
1. Add Role for writing logs

   1. Open Security tab -> Roles
   1. Create Role with parameters:
      - Name: templateapp-dev-write
      - Cluster permissions: cluster_manage_index_templates, cluster_monitor
      - Index: \*
      - Index permissions: indices_all
      - Tenant permissions: YOUR_TENANT_NAME
   1. Go to internal users tab and create a user for writing log from your service

   - Username: templateapp-dev
   - Password
   - Backend-roles: templateapp-dev-write

   1. Go back to Security tab -> Roles, find created role `templateapp-dev-write`. Open tab `Mapped users` and add mapping to created user `templateapp`.

1. Add created user to your backend service appsettings
1. Optionally add a user for reading logs (similar to the above)
1. Send some logs from your app
1. Add your logs to Discover
   1. Make sure you switched to your tenant in Dashboard
   1. Go to Stack Management -> Index Patterns
   1. Hit 'Create Index pattern'
   1. Define a pattern like `templateapp-*`
   1. It's recommended to setup Default fields for your tenant to be `message`:
      1. Go to Stack Management -> Advanced Settings
      2. Find `Default columns` field
      3. Change the value to `message`
