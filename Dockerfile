# ---------- збірка ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# спочатку лише файли проєктів, щоб кешувався restore
COPY Lab5.sln ./
COPY Lab5.API/Lab5.API.csproj Lab5.API/
COPY Lab5.Services/Lab5.Services.csproj Lab5.Services/
COPY Lab5.Infrastructure/Lab5.Infrastructure.csproj Lab5.Infrastructure/
RUN dotnet restore Lab5.API/Lab5.API.csproj

COPY . .
RUN dotnet publish Lab5.API/Lab5.API.csproj -c Release -o /app/publish --no-restore

# ---------- запуск ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# каталог для файлу SQLite (у docker-compose підключається як том, щоб дані переживали перезапуск)
RUN mkdir -p /data && chown app:app /data

COPY --from=build /app/publish .

# Секрети та конфігурація в образ не потрапляють: вони передаються під час запуску
# через змінні середовища (див. docker-compose.yml або налаштування хмарної платформи).
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    Hosting__UseHttpsRedirection=false \
    ConnectionStrings__Lab5Database="Data Source=/data/lab5.db"

# Порт 8080 за замовчуванням; якщо платформа задає змінну PORT, застосунок використає її (див. Program.cs)
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Lab5.API.dll"]
