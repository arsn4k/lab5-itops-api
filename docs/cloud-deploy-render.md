# Розгортання Lab5.API у хмарі (Render, Docker)

Платформа: **Render** (render.com), тип служби Web Service, мова Docker, безкоштовний план (Free).
Render збирає Docker-образ за нашим `Dockerfile` просто з репозиторію GitHub.

> Інструкцію складено за документацією Render. Перевірте кроки в своєму акаунті.
> Паролі та ключі в репозиторій не додавайте.

## Обмеження безкоштовного плану (їх треба знати й вказати у звіті)
- Служба «засинає» після 15 хвилин без запитів; перший запит після цього триває близько хвилини.
- Файлова система тимчасова: **файл SQLite зникає** при перезапуску, повторному розгортанні або «засинанні».
  Для постійного зберігання потрібні Render Disk (платно) або керована БД (Render Postgres, потрібно змінити провайдер EF Core).
- Для лабораторної цього достатньо: API, health-check, Swagger і CRUD працюють, дані живуть, поки служба активна.

## 1. Репозиторій на GitHub
Створіть на github.com порожній репозиторій (New repository, без README), назва, наприклад, `lab5-itops-api`.
У PowerShell з папки **Lab5** (вона має бути коренем репозиторію):
```powershell
git init
git add .
git commit -m "Lab5: IT Ops API"
git branch -M main
git remote add origin https://github.com/<ВАШ_ЛОГІН>/lab5-itops-api.git
git push -u origin main
```
`.gitignore` уже виключає bin/obj і файли баз даних. Папка `Migrations` (якщо створена) має потрапити в репозиторій.

## 2. Створення служби в Render
1. Зареєструйтесь на render.com (зручно через GitHub).
2. **New +** → **Web Service** → підключіть репозиторій `lab5-itops-api`.
3. Заповніть поля:
   - **Name**: `lab5-itops-<ваше прізвище>` (з нього буде адреса `https://<name>.onrender.com`)
   - **Language**: **Docker**
   - **Branch**: `main`
   - **Region**: найближчий (наприклад, Frankfurt)
   - **Dockerfile Path**: `./Dockerfile` (за замовчуванням)
   - **Instance Type**: **Free**
4. **Advanced** → **Environment Variables**: додайте `Swagger__Enabled` = `true`.
   (Інші налаштування — Production, пересилання заголовків, вимкнений редирект на HTTPS, рядок підключення — вже задані в Dockerfile.)
5. **Advanced** → **Health Check Path**: `/health/live`.
6. **Create Web Service**. Render збере образ і запустить контейнер (5–10 хвилин). Хід видно на вкладці **Logs**.

Порт: Render передає застосунку змінну `PORT` (за замовчуванням 10000), а `Program.cs` автоматично починає слухати саме цей порт.
HTTPS: SSL завершується на балансувальнику Render, а HTTP-запити перенаправляються на HTTPS, тому в застосунку редирект вимкнено.

## 3. Перевірка після розгортання
У PowerShell (`curl.exe`, а не `curl`) або в браузері:
```powershell
curl.exe -i https://<name>.onrender.com/health/live
curl.exe -i https://<name>.onrender.com/health/ready
```
Swagger: `https://<name>.onrender.com/swagger`. Створіть сервер (POST) і перевірте GET.
Журнали (JSON) дивіться на вкладці **Logs** служби.

## Альтернатива без ручного налаштування
У репозиторії є `render.yaml`. Можна вибрати **New +** → **Blueprint**, і Render створить службу за цим файлом.

## Інші хмарні сервіси
Методичка допускає будь-який хмарний сервіс. Той самий Docker-образ можна розгорнути на Fly.io, Railway, Koyeb або Google Cloud Run
(вони можуть вимагати прив'язку банківської картки). Змінюються лише кроки створення служби, а налаштування (змінні, health-check `/health/live`) однакові.
