FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY worker/src/RebusExemplo.csproj worker/src/
COPY shared/StudyCase.Contracts/StudyCase.Contracts.csproj shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/StudyCase.Domain.csproj shared/StudyCase.Domain/
RUN dotnet restore worker/src/RebusExemplo.csproj

COPY worker/src/ worker/src/
COPY shared/StudyCase.Contracts/ shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/ shared/StudyCase.Domain/
RUN dotnet publish worker/src/RebusExemplo.csproj -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app .

ENV DB_HOST=postgres
ENV DB_PORT=5432
ENV DB_NAME=rebus
ENV DB_USER=rebus
ENV DB_PASS=rebus_pass

ENTRYPOINT ["dotnet", "RebusExemplo.dll"]