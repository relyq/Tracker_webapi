#!/bin/bash
rm -rf /home/tracker/tracker/
#rm -rf /home/tracker/publish/appsettings.*
mv ./publish /home/tracker/tracker
mv /home/tracker/tracker/appsettings.pve.json /home/tracker/tracker/appsettings.json
mv /home/tracker/tracker/appsettings.pve.Development.json /home/tracker/tracker/appsettings.Development.json
sudo chmod +x /home/tracker/tracker/Tracker
sudo chmod +x /home/tracker/tracker/scripts/*

### create crontab

# Define the environment variable and cron job
ENV_VAR="Secrets__JanitorPassword=''"
CRON_JOB="@daily /usr/bin/python3 /home/tracker/tracker/scripts/demo_clean.py"

# Combine them into the format cron expects
COMBINED_JOB="$ENV_VAR
$CRON_JOB"

# Get existing crontab (if any), append the new lines if not already present
( crontab -l 2>/dev/null | grep -v -F "$CRON_JOB" ; echo "$COMBINED_JOB" ) | crontab -

sudo systemctl restart tracker.service
