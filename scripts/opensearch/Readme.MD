# How to run Opensearch

This is a small incstruction how to run Opensearch

## Step 1 - Run stack

1. Copy the `opensearch` folder to your VM machine.
1. Adjust `.env` to contain yours VIRTUAL_HOST for generating certificates for your vm host.
1. Change mod for script files: `sudo chmod 777 ./apply-internal-users.sh ./generate-password.sh ./generate-certs.sh`

1. Run the stack (`docker compose up -d`). Make sure it opens in the browser and user/password `admin/admin` works.
1. Change password for `admin` user.

   1. Stop the stack (`docker-compose down`)
   1. Edit `docker-compose.yml` and comment the following line: `- plugins.security.ssl.http.enabled=false` for `node1`
   1. Run `./generate-password.sh`, enter new admin password, save the generated hash somewhere
   1. Edit `internal-users.yml`, change `hash` field for admin user to the hash generated in previous step
   1. Run `./apply-internal-users.sh`
   1. Edit `docker-compose.yml` and uncomment the line: `- plugins.security.ssl.http.enabled=false` for `node1`
   1. Run the stack (`docker compose up -d`). Make sure it opens in the browser and new password for `admin` user works.

## Step 2 - Configure additional users

### User for backend service

1. Open opensearch dashboards
1. Add Tenant for you project (Security -> Tenants)
1. Add Role for writing logs

   1. Open Security tab -> Roles
   1. Create Role with parameters:
      - Name: writing-logs-app
      - Cluster permissions: cluster_manage_index_templates
      - Index: \*
      - Index permissions: indices_all
      - Tenant permissions: YOUR_TENANT_NAME
   1. Go to internal users tab and create a user for writing log from your service

   - Username: templateapp
   - Password
   - Backend-roles: writing-logs-app

   1. Go back to Security tab -> Roles, find created role `writing-logs-app`. Open tab `Mapped users` and add mapping to created user `templateapp`.

1. Add created user to your backend service appsettings
1. Optionally add a user for reading logs (similar to the above)
1. Send some logs from your app
1. Add your logs to Discover
   1. Go to Stack Management -> Index Patterns
   2. Hit 'Create Index pattern'
   3. Define a pattern like `templateapp-*`
   4. It's recommended to setup Default fields for your tenant to be `message`:
      1. Go to Stack Management -> Advanced Settings
      2. Find `Default columns` field
      3. Change the value to `message`
