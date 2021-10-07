#!/bin/sh

# Script injects environment variables that starts with `REACT_APP_*` in minified/uglified/bundled SPA files
# (it literally performs search and replace).
# The technique is taken from https://github.com/zzswang/docker-nginx-react

# find all env start with REACT_APP_
SUBS=$(echo $(env | cut -d= -f1 | grep "^REACT_APP_" | sed -e 's/^/\$/'))

# replace above envs
echo "inject environments ..."
echo $SUBS
for f in `find /webapi/wwwroot -regex ".*\.\(js\|css\|html\|json\|map\)"`; do envsubst "$SUBS" < $f > $f.tmp; mv $f.tmp $f; done
