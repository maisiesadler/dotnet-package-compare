#!/bin/bash

echo Comparing files $BEFORE_FILE_LOCATION and $AFTER_FILE_LOCATION

dotnet /PackageLister.Console.dll $BEFORE_FILE_LOCATION $AFTER_FILE_LOCATION

EOF=$(dd if=/dev/urandom bs=15 count=1 status=none | base64)
echo "DIFF_OUTPUT<<$EOF" >> $GITHUB_ENV
dotnet /PackageLister.Console.dll $BEFORE_FILE_LOCATION $AFTER_FILE_LOCATION >> $GITHUB_ENV
echo "$EOF" >> $GITHUB_ENV
