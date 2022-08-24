docker volume create --name mssql-volume
docker container rm mssql_db
docker run --restart=always -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 --name mssql_db --hostname mssql_db -d mcr.microsoft.com/mssql/server:2022-latest 