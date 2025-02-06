# Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 5151

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copie o arquivo .csproj corretamente
COPY ./fiap_5nett_tech.Api/fiap_5nett_tech.Api.CreateContact/fiap_5nett_tech.Api.CreateContact.csproj ./fiap_5nett_tech.Api.CreateContact/

RUN dotnet restore "fiap_5nett_tech.Api.CreateContact/fiap_5nett_tech.Api.CreateContact.csproj"

# Copia o restante do código
COPY . .

WORKDIR /src/fiap_5nett_tech.Api/fiap_5nett_tech.Api.CreateContact

RUN dotnet build "fiap_5nett_tech.Api.CreateContact.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "fiap_5nett_tech.Api.CreateContact.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "fiap_5nett_tech.Api.CreateContact.dll"]
