FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["fiap_5nett_tech.Api.Contact.Create/fiap_5nett_tech.Api.Contact.Create.csproj", "fiap_5nett_tech.Api.Contact.Create/"]
RUN dotnet restore "fiap_5nett_tech.Api.Contact.Create/fiap_5nett_tech.Api.Contact.Create.csproj"
COPY . .
WORKDIR "/src/fiap_5nett_tech.Api.Contact.Create"
RUN dotnet build "fiap_5nett_tech.Api.Contact.Create.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "fiap_5nett_tech.Api.Contact.Create.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "fiap_5nett_tech.Api.Contact.Create.dll"]