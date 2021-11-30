#!/bin/bash

# Script injects environment variables that starts with `REACT_APP_*` in minified/uglified/bundled SPA files
# (it literally performs search and replace of REACT_APP_BLABLABLA:"ENV VARIABLE VALUE IS PUT HERE").
# The technique is taken from https://github.com/zzswang/docker-nginx-react

set -x

# find all env start with REACT_APP_
export SUBS=$(echo $(env | cut -d= -f1 | grep "^REACT_APP_"))
echo "inject environments ..."

for VARIABLE in $SUBS; do
  VALUE="${!VARIABLE}"
  for f in `find /webapi/wwwroot -regex ".*\.\(js\|css\|html\|json\|map\)"`; do
    sed -i "s~$VARIABLE:\"[^\"]*\"~$VARIABLE:\"$VALUE\"~g" $f;
  done
done
