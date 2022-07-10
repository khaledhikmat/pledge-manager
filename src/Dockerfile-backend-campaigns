FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build-env
WORKDIR /app

# Copy everything else and build
COPY ./backend/campaigns ./backend/campaigns
COPY ./shared ./shared
RUN dotnet publish -c Release -o out ./backend/campaigns/campaigns.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
# Use an app directory (as opposed to root) as we might get into a permission issue
WORKDIR /app
COPY --from=build-env /app/out .

# Expose ports
EXPOSE 6000/tcp
ENV ASPNETCORE_URLS http://*:6000

# Start
ENTRYPOINT ["dotnet", "campaigns.dll"]
