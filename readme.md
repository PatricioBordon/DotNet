.NET SDK 8.0

Ejecutar estos comandos antes:

Remove-Item -Recurse -Force BookManagementApi\bin
Remove-Item -Recurse -Force BookManagementApi\obj
Remove-Item -Recurse -Force BookManagementApi.Tests\bin
Remove-Item -Recurse -Force BookManagementApi.Tests\obj

docker builder prune -f

docker build --no-cache -t bookmanagementapi .

docker run -d -p 8080:8080 -v bookdb:/data --name mi-api bookmanagementapi

docker logs -f mi-api

Ver api en:

http://localhost:8080/swagger/index.html

Usuario: admin
Contraseña: 1234
