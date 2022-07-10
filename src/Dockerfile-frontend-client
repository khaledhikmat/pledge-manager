FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build-env
WORKDIR /app

# Copy everything else and build
COPY ./frontend/client ./frontend/client
COPY ./shared ./shared
RUN dotnet publish -c Release -o out ./frontend/client/client.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS publish
# Use an app directory (as opposed to root) as we might get into a permission issue
WORKDIR /app
COPY --from=build-env /app/out .

FROM nginx:latest AS final
#FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/wwwroot .
COPY ./frontend/client/Settings/remote-appsettings.json ./appsettings.json
COPY ./frontend/client/nginx.conf /etc/nginx/nginx.conf