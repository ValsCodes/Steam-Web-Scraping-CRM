# Подробно описание на проекта

## 1) Какво представлява проектът

**Steam Web Scraping CRM** е full-stack система за:

- събиране (scraping) и обработка на данни от Steam Community Market,
- управление на каталожни данни (игри, URL-и, продукти, пиксели, тагове),
- следене на ценови условия (watch/wish логика),
- оперативна работа през административен UI.

Проектът комбинира:

- Angular клиент (`SteamApp.Client`),
- .NET Web API (`SteamApp.Server/SteamApp.WebAPI`),
- SQL Server база чрез EF Core.

---

## 2) Бизнес цел и проблем, който решава

Системата намалява ръчната работа при наблюдение на пазарни оферти в Steam чрез:

1. стандартизирани източници (Game URL конфигурации),
2. филтрация и категоризация (Tag/Pixel/Product правила),
3. автоматизирани проверки (wishlist/watchlist сценарии),
4. централизирано администриране и експорт на данни.

---

## 3) Основни функционални области

## 3.1 Каталог и конфигурация

- **Games**: основни игри/контексти.
- **GameUrls**: URL шаблони/настройки за scraping:
  - batch paging,
  - public API режим,
  - pixel scraping параметри.
- **Products**: артикули за следене.
- **Pixels**: цветови сигнатури (RGB/paint values).
- **Tags**: таксономия за продуктова категоризация.

## 3.2 Релации (M2M)

Системата поддържа и управлява следните many-to-many релации директно от основните форми:

- GameUrl ↔ Product
- GameUrl ↔ Pixel
- Product ↔ Tag

Това е ключово UX решение: операторът конфигурира свързаността в контекст на главния обект, вместо през отделни „join“ екрани.

## 3.3 Оперативно следене

- **WatchList**: активни правила за пазарно наблюдение.
- **WishList**: таргетни условия за известяване/проверка на цена.

## 3.4 Scraping и анализ

- scraping на listing страници,
- scraping през public API,
- pixel-based валидиране на item атрибути,
- helper функционалности за URL encoding/formatting.

## 3.5 Експортиране и отчетност

Ключовите таблици в UI поддържат export към Excel (XLSX), за офлайн анализ/отчетност.

---

## 4) Роли и употреба (типични потребители)

## 4.1 Оператор / Анализатор

- подготвя Game URL конфигурации,
- свързва продукти/пиксели/тагове,
- стартира scraping сценарии,
- преглежда филтрирани таблици,
- експортира текущи резултати.

## 4.2 Администратор

- поддържа базов каталог,
- настройва auth клиенти и секрети,
- управлява deployment конфигурации,
- наблюдава worker процесите.

---

## 5) Ключови бизнес обекти и отговорности

## 5.1 Game

Контекстен „container“ за URL-и, продукти, пиксели и тагове.

## 5.2 GameUrl

Дефинира как се достъпват пазарни данни за дадена игра/сценарий:

- частичен URL,
- batch/public API флагове,
- pixel координати и image size,
- свързани продукти и пиксели.

## 5.3 Product

Единица за наблюдение/филтрация:

- име,
- активност,
- рейтинг,
- тагове,
- връзки към game URL-и.

## 5.4 Pixel

Представя color signature за paint/визуален признак:

- RGB стойности,
- връзка към игра,
- използване в pixel scraping.

## 5.5 Tag

Категоризационен слой за продукти; използва се за филтриране, групиране и правила.

## 5.6 WatchList / WishList

- WatchList: текущи таргети за мониторинг.
- WishList: условия/праг за проверка и известяване.

---

## 6) Критични потребителски потоци

## 6.1 Онбординг на нова игра

1. Създаване на Game.
2. Създаване на GameUrl (режим + paging + pixel settings).
3. Създаване на Products и Pixels.
4. Свързване на:
   - GameUrl ↔ Products,
   - GameUrl ↔ Pixels,
   - Product ↔ Tags.

## 6.2 Настройка на scraping сценарий

1. Избор на Game/GameUrl.
2. Пускане на scraping (страница/API).
3. Преглед/филтрация в таблици.
4. Export към Excel за анализ.

## 6.3 Поддръжка на мониторинг

1. Добавяне/редакция на WishList/WatchList.
2. Worker/планирани проверки.
3. Проверка на резултати и корекция на прагове.

---

## 7) Архитектурна структура (технически)

## 7.1 Frontend (Angular)

- `pages/`: route-level екрани.
- `services/`: API клиенти.
- `models/`: типове/DTO представяния.
- `components/`: reusable UI елементи.

Важни UX принципи:

- inline relation management,
- filter-first таблици,
- export в grid екрани.

## 7.2 Backend (.NET)

- Minimal APIs + controllers,
- JWT authentication/authorization,
- EF Core + SQL Server,
- background worker за периодични задачи,
- Swagger (dev), CORS policy.

## 7.3 Data access

- repository/service разделяне,
- cache usage за read-heavy операции,
- DTO-based API контракти.

---

## 8) Конфигурационен модел

Стартирането изисква валидни ключове (connection string, JWT, internal secret и др.).

Среда на зареждане:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. env vars
4. user secrets (development)

Това позволява сигурно разделяне между dev/test/prod.

---

## 9) Сигурност и рискови зони

## 9.1 Auth

- JWT Bearer токени за защитените endpoint-и.
- Нужда от строг token lifecycle и защита от XSS в клиента.

## 9.2 CORS/Swagger

- CORS трябва да е ограничен извън dev.
- Swagger трябва да е контролиран според среда/достъп.

## 9.3 Scraping

- външни зависимости (Steam DOM/API) са нестабилни;
- rate limiting, retries и error handling са задължителни.

---

## 10) Нефункционални изисквания

- **Надеждност**: graceful handling на timeout/HTTP грешки.
- **Наблюдаемост**: логове за scraping/worker/auth.
- **Производителност**: кеш и оптимизирани read операции.
- **Поддръжка**: ясни boundaries между UI, services, API, data.

---

## 11) Какво трябва да включва „детайлната документация“ (препоръка)

За да се изгради пълна продуктова документация, препоръчително е да се добавят следните раздели (в отделни документи):

1. **Domain glossary** (термини и дефиниции).
2. **Use-case catalog** (сценарии с pre/post условия).
3. **ER диаграма** (вкл. M2M таблици).
4. **Sequence diagrams** за scraping и relation sync.
5. **Runbooks** (incident/restart/config reset).
6. **Security checklist** (token, CORS, secrets, logs).
7. **Release checklist** (migration, smoke tests, rollback).

---

## 12) Кратко резюме

Проектът е операционна платформа за Steam market intelligence с акцент върху:

- гъвкава конфигурация на data sources,
- богато relation-driven администриране,
- филтрация + export за анализ,
- backend автомация и защитен API слой.

Този документ може да служи като **базов canonical description** за изграждане на детайлна вътрешна/външна документация.
