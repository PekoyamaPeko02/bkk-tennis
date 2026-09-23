FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/BkkTennis.Api/BkkTennis.Api.csproj src/BkkTennis.Api/
RUN dotnet restore src/BkkTennis.Api/BkkTennis.Api.csproj
COPY src/ src/
RUN dotnet publish src/BkkTennis.Api/BkkTennis.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BkkTennis.Api.dll"]
