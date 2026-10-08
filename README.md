# LocationInfoService

ASP.NET Core (.NET 8) minimal API hosted on [Render](https://render.com). Returns the stored text for a location from PostgreSQL.

## API

`GET /api/locations/{location}`

| Status | Meaning |
|---|---|
| 200 | `{ "location": "Paris", "description": "..." }` |
| 400 | Location is empty or longer than 100 characters |
| 404 | Location not found |

Swagger UI: `/swagger`

## Deploy to Render

1. Push this repository to GitHub.
2. In Render: **New → Blueprint**, select the repository. [render.yaml](render.yaml) creates:
   - `location-info-service` – Docker web service built from [Dockerfile](Dockerfile)
   - `locations-db` – PostgreSQL database, with credentials injected as environment variables
3. Seed the database: open `locations-db` in Render, copy the **External Database URL**, then run:

   ```powershell
   psql "<external-database-url>" -f Database/schema.sql
   ```

Render redeploys automatically on every push to the connected branch. The API is available at `https://location-info-service.onrender.com/api/locations/Paris` (actual subdomain shown in the Render dashboard).

## Configuration

| Setting | Env var |
|---|---|
| `Database:Host` | `Database__Host` |
| `Database:Port` | `Database__Port` |
| `Database:Name` | `Database__Name` |
| `Database:User` | `Database__User` |
| `Database:Password` | `Database__Password` |

Never commit the password; set it via environment variables or `dotnet user-secrets`.

## Run locally

```powershell
docker run -d --name pg -e POSTGRES_PASSWORD=dev -e POSTGRES_DB=locations -p 5432:5432 postgres:16
Get-Content Database/schema.sql | docker exec -i pg psql -U postgres -d locations
$env:Database__Password = "dev"
dotnet run
```
