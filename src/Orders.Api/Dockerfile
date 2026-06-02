FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Orders.sln ./
COPY src/Orders.Domain/Orders.Domain.csproj src/Orders.Domain/
COPY src/Orders.Application/Orders.Application.csproj src/Orders.Application/
COPY src/Orders.Infrastructure/Orders.Infrastructure.csproj src/Orders.Infrastructure/
COPY src/Orders.Api/Orders.Api.csproj src/Orders.Api/

RUN dotnet restore src/Orders.Api/Orders.Api.csproj

COPY . .
RUN dotnet publish src/Orders.Api/Orders.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
