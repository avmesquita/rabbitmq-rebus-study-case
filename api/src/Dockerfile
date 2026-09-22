FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY api/src/Api.csproj api/src/
COPY shared/Contracts/Contracts.csproj shared/Contracts/
RUN dotnet restore api/src/Api.csproj
COPY api/src/ api/src/
COPY shared/Contracts/ shared/Contracts/
RUN dotnet publish api/src/Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
