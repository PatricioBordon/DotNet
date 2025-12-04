API de gestión de libros con .NET 8 + SQLite + JWT + Swagger

## Cómo desplegar con Docker (2 comandos simples)

```bash
# 1. Construir la imagen
docker build -t nubelity-library .

# 2. Ejecutar el contenedor (con base de datos persistente)
docker run -d -p 8080:8080 -v bookdb:/data --name nubelity-library nubelity-library

Swagger UI: http://localhost:8080/swagger/index.html
Credenciales de prueba:
Usuario: admin
Contraseña: 1234