# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=1
ENV NUGET_CERT_REVOCATION_MODE=offline
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ENV NUGET_HTTP_CACHE_PATH=/tmp/nuget-http-cache
ENV NUGET_SCRATCH=/tmp/nuget-scratch
COPY nuget.config ./
COPY ProjectManagementSystem.Database/ProjectManagementSystem.Database.csproj ProjectManagementSystem.Database/
COPY ProjectManagementSystem.API/ProjectManagementSystem.API.csproj ProjectManagementSystem.API/
RUN --network=host \
    --mount=type=bind,from=nuget,target=/root/.nuget/packages,readonly \
    dotnet restore --disable-parallel --ignore-failed-sources ProjectManagementSystem.API/ProjectManagementSystem.API.csproj
COPY ProjectManagementSystem.Database/ ProjectManagementSystem.Database/
COPY ProjectManagementSystem.API/ ProjectManagementSystem.API/
RUN --mount=type=bind,from=nuget,target=/root/.nuget/packages,readonly \
    dotnet publish ProjectManagementSystem.API/ProjectManagementSystem.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir -p /app/logs /app/uploads
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV LogsPath=/app/logs
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectManagementSystem.API.dll"]
