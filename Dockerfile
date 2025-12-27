FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "miniHttpServer/miniHttpServer.csproj"

RUN dotnet publish "miniHttpServer/miniHttpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 1234

ENTRYPOINT ["dotnet", "miniHttpServer.dll"]