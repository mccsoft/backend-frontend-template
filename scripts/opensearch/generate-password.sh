#!/bin/sh

docker exec -it opensearch-node1 sh -c "cd /usr/share/opensearch/plugins/opensearch-security/tools/ && \
    ./hash.sh
  "
