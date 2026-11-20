# Lab5 — IT Ops: REST API моніторингу серверів (ЛР №5)

Розвиток Lab4: централізована обробка помилок, FluentValidation, EF Core + SQLite, конфігурація
через середовище, health-check, журналювання, Docker, розгортання в хмарі (Render).

| Проєкт | Призначення |
|---|---|
| `Lab5.Services` | моделі, DTO, інтерфейси репозиторію й сервісу, `ServerService` |
| `Lab5.Infrastructure` | `Lab5DbContext` (EF Core, SQLite), `EfServerRepository`, міграції |
| `Lab5.API` | контролер, middleware помилок, валідатори, health checks, `Program.cs` |

## Локальний запуск (VS Code)
```bash
cd Lab5
dotnet dev-certs https --trust
dotnet tool install --global dotnet-ef --version 8.0.11   # один раз

# міграція (створює папку Lab5.Infrastructure/Migrations)
dotnet ef migrations add InitialCreate --project Lab5.Infrastructure --startup-project Lab5.API
dotnet ef database update --project Lab5.Infrastructure --startup-project Lab5.API

dotnet run --project Lab5.API
```
Swagger: https://localhost:7080/swagger. Файл БД: `Lab5.API/lab5.db`.
Якщо міграцію не створено, під час запуску схема створиться через `EnsureCreated` (застосунок це залогує).
F5 у VS Code: конфігурація «Lab5.API (Swagger)».

## Ендпоїнти
`GET/POST /api/servers`, `GET/PUT/DELETE /api/servers/{id}`, `PATCH /api/servers/{id}/status`
(тіло `{ "status": "Online|Offline|Maintenance" }`), `GET /health`, `/health/live`, `/health/ready`,
`GET /debug/throw` (лише Development, для перевірки middleware).
Готові запити: `Lab5.API/Lab5.API.http` (розширення REST Client).

## Конфігурація та секрети
Порядок пріоритету: `appsettings.json` → `appsettings.{Environment}.json` → user-secrets → змінні середовища.
```bash
dotnet user-secrets set "ConnectionStrings:Lab5Database" "Data Source=/шлях/до/lab5.db" --project Lab5.API
# у контейнері / хмарі: ConnectionStrings__Lab5Database=...
```
Реальні паролі й рядки підключення в репозиторій не додавайте.

## Docker
```bash
docker compose up --build -d
docker compose ps
curl http://localhost:8080/health/ready
# Swagger: http://localhost:8080/swagger
docker compose restart          # дані зберігаються в томі lab5-data
docker compose logs -f api
docker compose down             # том залишається; `down -v` видаляє дані
```
Міграції краще створити до збірки образу (`dotnet ef migrations add ...`), тоді в контейнері застосується `Migrate()`.

## Хмара (Render)
Див. `docs/cloud-deploy-render.md` (також є `render.yaml`).
