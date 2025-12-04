FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BookManagementApi/*.csproj ./BookManagementApi/
COPY BookManagementApi.Tests/*.csproj ./BookManagementApi.Tests/
RUN dotnet restore BookManagementApi/BookManagementApi.csproj

COPY BookManagementApi ./BookManagementApi/
COPY BookManagementApi.Tests ./BookManagementApi.Tests/

WORKDIR /src/BookManagementApi
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN mkdir /data

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ConnectionStrings__DefaultConnection="Data Source=/data/bookmanagement.db"

COPY --from=build /app/publish .

COPY BookManagementApi/bookmanagement.db /data/bookmanagement.db

RUN chown app:app /data /data/bookmanagement.db
RUN chmod 666 /data/bookmanagement.db
USER app

ENTRYPOINT ["dotnet", "BookManagementApi.dll"]