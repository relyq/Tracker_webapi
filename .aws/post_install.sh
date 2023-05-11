#!/bin/bash
chown -R tracker:tracker /opt/tracker/api
chmode -R 0755 /opt/tracker/api
mv /opt/tracker/api/appsettings.aws.json /opt/tracker/api/appsettings.json
mv /opt/tracker/api/appsettings.aws.Development.json /opt/tracker/api/appsettings.Development.json
chmod +x /opt/tracker/api/scripts/*.py
chmod +x /opt/tracker/api/Tracker