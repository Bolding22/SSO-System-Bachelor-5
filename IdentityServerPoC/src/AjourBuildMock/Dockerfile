FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/AjourBuildMock/AjourBuildMock.csproj", "AjourBuildMock/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "AjourBuildMock/AjourBuildMock.csproj"
COPY . /.
WORKDIR "AjourBuildMock"
RUN dotnet build "AjourBuildMock.csproj" -o /app/build

FROM build AS publish
RUN dotnet publish "AjourBuildMock.csproj" -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AjourBuildMock.dll"]
