#!/bin/sh

docker exec -it opensearch-node1 sh -c "cd /usr/share/opensearch/plugins/opensearch-security/tools/ && \

  sh securityadmin.sh  \
    -f /usr/share/opensearch/config/opensearch-security/internal_users.yml \
    -t internalusers \
    -icl -nhnv \
    -cacert /usr/share/opensearch/config/root-ca.pem \
    -cert /usr/share/opensearch/config/kirk.pem \
    -key /usr/share/opensearch/config/kirk-key.pem \
  "

