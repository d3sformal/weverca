#!/bin/sh
#
# addcgipath.sh      Adds path for PHP CGI exec when running
#                    with suEXEC for example
#
# Author:            Olivier Cahagne
#

if test $# -eq 0
then
  echo "Missing first parameter (path to add)"
  echo "Usage: addcgipath <path to add>"
  echo "Example: addcgipath /usr/local/bin/php"
  exit 0
fi

if test -d bak
then
  echo 'Directory bak is already there'
else
  mkdir bak
fi

for i in action.php delete.php download.php index.php logout.php \
  send.php
  do
  mv $i bak/$i
  (echo '#!'$1''; cat bak/$i) > $i
  done;
