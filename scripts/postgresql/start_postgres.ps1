docker volume create --name postgres-volume
docker container rm postgres_db
docker run --restart=always -p 5432:5432 --name postgres_db -e POSTGRES_PASSWORD=postgres -v postgres-volume:/var/lib/postgresql/data -d postgres