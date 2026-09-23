FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Decisore/Decisore.csproj", "Decisore/"]
RUN dotnet restore "Decisore/Decisore.csproj"

COPY . .
WORKDIR /src/Decisore
RUN dotnet publish "Decisore.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Decisore.dll"]
