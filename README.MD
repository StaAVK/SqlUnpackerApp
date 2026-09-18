# 🛠️ SqlUnpacker TUI v3.1

**SqlUnpacker TUI** — это легкое консольное приложение (TUI) на C# для разработчиков и администраторов БД, предназначенное для быстрой подстановки параметров из логов (NHibernate, Entity Framework) обратно в SQL-запросы и их красивого форматирования.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

---

## 🎨 Интерфейс приложения

```text
┌─ SqlUnpacker TUI v3.1 ────────────────────────────────────────────────────────┐
│ ┌─ Целевая СУБД ────────────────────────────────────────────────────────────┐ │
│ │ (*) Oracle (HEXTORAW)   ( ) MS SQL Server (0xHEX)                         │ │
│ └───────────────────────────────────────────────────────────────────────────┘ │
│ ┌─ Исходный SQL с параметрами NHibernate/EF ────────────────────────────────┐ │
│ │ SELECT t0.Id, t0.Name FROM Users t0 WHERE t0.TenantId = :p0 AND t0.Bin = :p1│ │
│ │ :p0 = 100 [Type: Int32], :p1 = 414243 [Type: Binary]                      │ │
│ └───────────────────────────────────────────────────────────────────────────┘ │
│ [ Вставить (Ctrl+B) ]  [ Обработать (Ctrl+R) ]  [ Копировать (Ctrl+C) ] ...   │
│ ┌─ Результат (Inlined & Formatted SQL) ─────────────────────────────────────┐ │
│ │ SELECT                                                                    │ │
│ │     t0.Id,                                                                │ │
│ │     t0.Name                                                               │ │
│ │ FROM                                                                      │ │
│ │     Users t0                                                              │ │
│ │ WHERE                                                                     │ │
│ │     t0.TenantId = 100                                                     │ │
│ │     AND t0.Bin = HEXTORAW('414243')                                       │ │
│ └───────────────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────────────┘

```

---

## ✨ Основные возможности

* 🚀 **Мгновенный инлайнинг параметров:** Переводит сырые логи ORM (NHibernate / EF) в валидный исполняемый SQL.
* 🎯 **Поддержка dialect-specific типов:**
* Автоматическое преобразование бинарных данных (`Binary`) в `HEXTORAW('HEX')` для Oracle или `0xHEX` для MS SQL Server.
* Маппинг `Boolean` значений в битовые флаги (`1`/`0`).


* ⚡ **Мгновенная работа с буфером обмена:**
* Обход медленного ввода консоли благодаря прямому чтению буфера ОС по кнопке или горячей клавише.


* 🧹 **Профессиональное форматирование (AST):**
* Интеграция с `PoorMansTSqlFormatterRedux` для переноса строк после запятых, форматирования блоков `WHERE`, `JOIN` и отступов и т.д.


* ⌨️ **Удобные горячие клавиши:**
* `Ctrl + B` — Мгновенная вставка из буфера обмена.
* `Ctrl + R` — Выполнить обработку SQL.
* `Ctrl + C` — Скопировать результат в буфер обмена.
* `Esc` — Выход из приложения.



---

## 🛠️ Сборка и запуск

### Предварительные требования

* [.NET 10.0 SDK](https://dotnet.microsoft.com/download?utm_source=gemini) (или новее)

### Сборка из исходников

1. Клонируйте репозиторий:
```bash
git clone [https://github.com/ВАШ_USERNAME/SqlUnpackerApp.git](https://github.com/ВАШ_USERNAME/SqlUnpackerApp.git)
cd SqlUnpackerApp

```


2. Восстановите зависимости:
```bash
dotnet restore

```


3. Запустите в режиме разработки:
```bash
dotnet run

```



### Публикация в Single-File EXE (Windows x64)

Для создания одного портативного `.exe` файла без лишних DLL:

```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```
---

## 🧰 Используемые библиотеки

* **[Terminal.Gui](https://www.google.com/search?q=https://github.com/gui-cs/Terminal.Gui&utm_source=gemini)** — фреймворк для создания текстового пользовательского интерфейса (TUI).
* **[PoorMansTSqlFormatterRedux](https://github.com/TaoK/PoorMansTSqlFormatter?utm_source=gemini)** — библиотека форматирования и разбора синтаксиса SQL.

---