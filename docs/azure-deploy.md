# Розгортання Lab5.API в Azure App Service (контейнер)

> Команди нижче підготовлено за документацією Azure CLI, але в цьому репозиторії вони не виконувались:
> перевірте їх у своїй підписці. Значення в `<...>` замініть своїми. Паролі та ключі в репозиторій не додавайте.

## 0. Підготовка
```bash
az login
az group create --name rg-lab5-itops --location westeurope
```

## 1. Реєстр контейнерів і збірка образу
```bash
az acr create --resource-group rg-lab5-itops --name <acrName> --sku Basic --admin-enabled true
# збірка образу на стороні Azure (з папки Lab5, де лежить Dockerfile)
az acr build --registry <acrName> --image lab5-api:1.0 .
```

## 2. Служба застосунку (Linux)
```bash
az appservice plan create --resource-group rg-lab5-itops --name plan-lab5 --is-linux --sku B1
az webapp create --resource-group rg-lab5-itops --plan plan-lab5 --name <appName> \
  --deployment-container-image-name <acrName>.azurecr.io/lab5-api:1.0

# доступ до реєстру (облікові дані не зберігайте в коді)
az webapp config container set --resource-group rg-lab5-itops --name <appName> \
  --docker-custom-image-name <acrName>.azurecr.io/lab5-api:1.0 \
  --docker-registry-server-url https://<acrName>.azurecr.io \
  --docker-registry-server-user <acrUser> \
  --docker-registry-server-password <acrPassword>
```
`<acrUser>` і `<acrPassword>` показує команда `az acr credential show --name <acrName>`.

## 3. Налаштування середовища (без зміни коду)
```bash
az webapp config appsettings set --resource-group rg-lab5-itops --name <appName> --settings \
  WEBSITES_PORT=8080 \
  WEBSITES_ENABLE_APP_SERVICE_STORAGE=true \
  ASPNETCORE_ENVIRONMENT=Production \
  ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
  Hosting__UseHttpsRedirection=false \
  Swagger__Enabled=true \
  "ConnectionStrings__Lab5Database=Data Source=/home/lab5.db"
```
- `/home` в App Service для Linux зберігається між перезапусками, тому SQLite-файл не втрачається.
- Для реальної експлуатації замість SQLite використовують керовану БД (Azure SQL / PostgreSQL),
  а рядок підключення беруть із Key Vault або Connection strings служби.

## 4. HTTPS і health-check
```bash
az webapp update --resource-group rg-lab5-itops --name <appName> --https-only true
```
Health-check: портал Azure → App Service → Monitoring → Health check → шлях `/health/live`.

## 5. Перевірка після розгортання
```bash
curl -i https://<appName>.azurewebsites.net/health/live
curl -i https://<appName>.azurewebsites.net/health/ready
```
Swagger: `https://<appName>.azurewebsites.net/swagger`. Журнали: `az webapp log tail --resource-group rg-lab5-itops --name <appName>`.
