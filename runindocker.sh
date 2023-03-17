#!/bin/bash

env

result="$(dotnet /PackageLister.Console.dll $BEFORE_FILE_LOCATION $AFTER_FILE_LOCATION)"

r=$?
if [ $r -ne 0 ]; then
    echo "Invalid result code"
    echo $result
    exit $r
fi

echo $result

EOF=$(dd if=/dev/urandom bs=15 count=1 status=none | base64)
    echo "DIFF_OUTPUT<<$EOF" >> $GITHUB_ENV
    echo $result >> $GITHUB_ENV
    echo "$EOF" >> $GITHUB_ENV
