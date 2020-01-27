FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
WORKDIR /app

COPY webapp/webapp.csproj ./
RUN dotnet restore

COPY ./webapp ./
RUN dotnet publish -c Release -o out

FROM base AS final
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "webapp.dll" ]