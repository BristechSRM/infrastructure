#!/bin/bash -euv

cd source
cp -R /code/.nuget/ .
cp -R /code/packages/ .
cp /config/TrelloCreds.config ./TrelloMigrate/TrelloCreds.config
mono packages/FAKE/tools/FAKE.exe build.fsx
cd ..
cp source/build/output/* binaries/
