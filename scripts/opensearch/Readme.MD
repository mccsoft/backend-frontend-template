# How to run Opensearch

This is a small incstruction how to run Opensearch

## Step 1 - Run stack
1. Copy the `opensearch` folder to your VM machine.
1. Ensure that `GenerateOpensearchCerts.sh` endings are not windows alike. You can use `dos2unix` tool.
1. Change mod for script file.

    ```
    sudo chmod 777 ./GenerateOpensearchCerts.sh
    ```
1. Run the script to generate certificates

   ```
   sudo ./GenerateOpensearchCerts.sh
   ```

   After run, you'll have 6 certificates: root-ca, node and admin and keys for them.
1. Adjust `.env` to contain yours VIRTUAL_HOST and EMAIL for generating certificates for your vm host.
1. If it neccessary regenerate passwords for built in users in `internal_users.yml`.

   It'll be a bit treacky because, you should run docker compose, for running hash-generation tool -> pls check the [url](https://opensearch.org/docs/latest/security-plugin/configuration/yaml/#internal_usersyml).
   NOTICE, if you will run once stack, you should remove volume of opensearch, if you want automatically regeneration of security index, otherwise you will have to do it manualy, see example below.

    ```sh
    # Run these command in opensearch docker container

    cd ./plugins/opensearch-security/tools/

    # This command will regenerate opensearch-security index, if you want to regenerate only internal users use command below
    ./securityadmin.sh -cd ../../../config/opensearch-security/ -icl -nhnv \
    -cacert ../../../config/root-ca.pem \
    -cert ../../../config/admin.pem \
    -key ../../../config/admin-key.pem

    # Change internal users only
    ./securityadmin.sh -f ../../../config/opensearch-security/internal_users.yml \
    -t internalusers \
    -icl \
    -nhnv \
    -cacert ../../../config/root-ca.pem \
    -cert ../../../config/admin.pem \
    -key ../../../config/admin-key.pem
    ```
1. Run `docker-compose up -d`

## Step 2 - Configure additional users

### User for backend service

1. Open opensearch dashboards
1. Add Tenant for you project (Security -> Tenants)
2. Add Role for writing logs
   1. Open Security tab -> Roles
   1. Create Role with parameters:
      * Name: writing-logs-app
      * Cluster permissions: cluster_manage_index_templates
      * Index: *
      * Index permissions: indices_all
      * Tenant permissions: YOUR_TENANT_NAME
   1. Go to internal users tab and create a user for writing log from your service
     * Username: templateapp
     * Password
     * Backend-roles: writing-logs-app

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
