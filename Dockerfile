# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
RUN apk update && apk upgrade && apk add --no-cache bash git openssh
WORKDIR /app
COPY --from=build-env /app/out .
COPY localhost.pfx .
ENTRYPOINT ["dotnet", "UAF_Frontend_Registration.dll"]
