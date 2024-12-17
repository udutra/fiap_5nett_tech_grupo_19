FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
EXPOSE 5092

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["fiap_5nett_tech.Api.Contact.Update/fiap_5nett_tech.Api.Contact.Update.csproj", "fiap_5nett_tech.Api.Contact.Update/"]
RUN dotnet restore "fiap_5nett_tech.Api.Contact.Update/fiap_5nett_tech.Api.Contact.Update.csproj"
COPY . .
WORKDIR "/src/fiap_5nett_tech.Api.Contact.Update"
RUN dotnet build "fiap_5nett_tech.Api.Contact.Update.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "fiap_5nett_tech.Api.Contact.Update.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "fiap_5nett_tech.Api.Contact.Update.dll"]
