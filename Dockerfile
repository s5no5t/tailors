FROM node:lts-alpine as node-build-env
WORKDIR /App

# Copy everything
COPY ./src/Tailors.Web ./
RUN npm ci
RUN npm run build:css

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
COPY --from=node-build-env /App/wwwroot/css/site.css /App/src/Tailors.Web/wwwroot/css/site.css 
# Restore as distinct layers
ENV HUSKY="0"
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out ./src/Tailors.Web/Tailors.Web.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "Tailors.Web.dll"]
