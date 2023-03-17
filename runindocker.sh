#!/bin/bash

env

result="$(dotnet /PackageLister.Console.dll $BEFORE_FILE_LOCATION $AFTER_FILE_LOCATION)"

r=$?
if [ $r -ne 0 ]; then
    echo "Invalid result code"
    echo $result
    exit $r
fi

result="${result//'%'/'%25'}"
result="${result//$'\n'/'%0A'}"
result="${result//$'\r'/'%0D'}"

echo $result
