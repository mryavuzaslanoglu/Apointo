FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY src/Apointo.Domain/Apointo.Domain.csproj src/Apointo.Domain/
COPY src/Apointo.Application/Apointo.Application.csproj src/Apointo.Application/
COPY src/Apointo.Infrastructure/Apointo.Infrastructure.csproj src/Apointo.Infrastructure/
COPY src/Apointo.Api/Apointo.Api.csproj src/Apointo.Api/

RUN dotnet restore

COPY . .
WORKDIR /src/src/Apointo.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Apointo.Api.dll"]