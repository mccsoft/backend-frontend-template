#!/usr/bin/env bash
#
# Sets up COMPOSE_PROFILES env. variable to enable/disable postgres and nginx services in docker-compose
# Configures CONNECTION_STRING env. variable (if it's empty) to use default Postgres connection string with passed password
#

COMPOSE_PROFILES=""

if [ -z "$DISABLE_LOCAL_POSTGRES" ]
then
    COMPOSE_PROFILES="$COMPOSE_PROFILES,db"
fi

if [ -z "$DISABLE_LOCAL_NGINX" ]
then
    COMPOSE_PROFILES="$COMPOSE_PROFILES,nginx"
fi

echo "COMPOSE_PROFILES=$COMPOSE_PROFILES";
echo "COMPOSE_PROFILES=$COMPOSE_PROFILES" >> $HOME/.env;

DEFAULT_CONNECTION_STRING="Server=postgres;Database=${POSTGRES_DATABASE:-postgres};Port=5432;Username=${POSTGRES_USER:-postgres};Password=$POSTGRES_PASSWORD;Pooling=true;Keepalive=5;Command Timeout=60;Trust Server Certificate=true"

if [ -z "$CONNECTION_STRING" ]
then
  echo "CONNECTION_STRING=$DEFAULT_CONNECTION_STRING" >> $HOME/.env;
fi

