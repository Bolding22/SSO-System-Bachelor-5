FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt update
RUN apt upgrade -y
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/IdentityServer/IdentityServer.csproj", "IdentityServer/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "IdentityServer/IdentityServer.csproj"
COPY . /.
WORKDIR "IdentityServer"
RUN dotnet build "IdentityServer.csproj" -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServer.csproj" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityServer.dll"]

FROM final AS final-ssl
# Remember to generate a certificate first
RUN mkdir /usr/local/share/ca-certificates/extra
ADD src/_Certificates/myCA.pem /usr/local/share/ca-certificates/extra/myCA.crt
RUN update-ca-certificates