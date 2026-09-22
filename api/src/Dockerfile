FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY api/src/Api.csproj api/src/
COPY shared/StudyCase.Contracts/StudyCase.Contracts.csproj shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/StudyCase.Domain.csproj shared/StudyCase.Domain/
RUN dotnet restore api/src/Api.csproj
COPY api/src/ api/src/

RUN dotnet restore api/src/Api.csproj

COPY shared/StudyCase.Contracts/ shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/ shared/StudyCase.Domain/

RUN dotnet publish api/src/Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
