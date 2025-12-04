FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BookManagementApi.csproj", "./"]
RUN dotnet restore "BookManagementApi.csproj"

COPY . .
WORKDIR /src
RUN dotnet build "BookManagementApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookManagementApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "BookManagementApi.dll"]