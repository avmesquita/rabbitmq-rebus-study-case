FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY mailer/src/Mailer.csproj mailer/src/
COPY shared/StudyCase.Contracts/StudyCase.Contracts.csproj shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/StudyCase.Domain.csproj shared/StudyCase.Domain/
RUN dotnet restore mailer/src/Mailer.csproj

COPY mailer/src/ mailer/src/
COPY shared/StudyCase.Contracts/ shared/StudyCase.Contracts/
COPY shared/StudyCase.Domain/ shared/StudyCase.Domain/
RUN dotnet publish mailer/src/Mailer.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Mailer.dll"]
