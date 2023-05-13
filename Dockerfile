# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
RUN apt-get update && apt-get install -y python3
WORKDIR /publish
COPY --from=build /publish .
RUN chmod +x scripts/*
EXPOSE 7004
ENTRYPOINT ["dotnet", "Tracker.dll"]
