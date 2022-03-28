FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/IdentityServerAspNetIdentity/IdentityServerAspNetIdentity.csproj", "IdentityServerAspNetIdentity/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "IdentityServerAspNetIdentity/IdentityServerAspNetIdentity.csproj"
COPY . /.
WORKDIR "IdentityServerAspNetIdentity"
RUN dotnet build "IdentityServerAspNetIdentity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServerAspNetIdentity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityServerAspNetIdentity.dll"]
