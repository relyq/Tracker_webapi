## tracker api

https://tracker.relyq.dev/

./scripts folder is copied to build in .csproj

- secrets for the api are stored on env vars
- secrets for python scripts are stored on cron env vars

### required env vars

- ASPNETCORE_URLS
- ASPNETCORE_HTTPS_PORT
- ASPNETCORE_ENVIRONMENT
- ASPNETCORE_CONTENTROOT
- Jwt\_\_Key
- Secrets\_\_SQLConnection
- Secrets\_\_SMTPPassword
- Tracker\_\_BaseUrl
