---
title: Процес розробки
tags:
  - process
  - workflow
  - ai
---

# ⚙️ Процес розробки та інструменти

Цей документ описує, як AI-агент взаємодіє з проектом та які інструменти використовуються для оптимізації розробки моду.

## 🛠 Скіли (Skills)

Агент використовує набір встановлених скілів для стандартизації роботи:

1. **Obsidian & Документація**
   - `obsidian-markdown` — для правильного форматування цих нотаток (wikilinks, callouts, frontmatter).
   - `obsidian-bases` — для створення баз даних в Obsidian (планується для трекінгу квестів, стану NPC).
   - `obsidian-cli` — для керування сховищем через консоль (створення нотаток, пошук).

2. **C# та Логіка**
   - `csharp-async` — суворе дотримання асинхронного програмування на C# 13 (`ValueTask`, `CancellationToken`, уникнення блокування головного потоку).

3. **Blockbench**
   - Набір скілів (`blockbench-modeling`, `blockbench-animation`, `blockbench-texturing`) для правильної розробки 3D-моделей зброї, блоків та анімацій NPC.

## 🔌 MCP (Model Context Protocol)

Для отримання найсвіжішої інформації про Vintage Story API використовується MCP-сервер **`context7`**.

> [!info] Автоматичний пошук документації
> Агент запам'ятав ідентифікатори баз даних і самостійно звертається до них через інструмент `query-docs`, коли потрібно перевірити синтаксис чи знайти офіційний приклад коду.

### 📚 Підключені ресурси Vintage Story

Ми використовуємо наступні джерела знань (ідентифікатори бібліотек Context7):

- [[03_Context7_Resources#Vintage Story Wiki|Vintage Story Wiki]] (`/websites/wiki_vintagestory_at`)
- [[03_Context7_Resources#Vintage Story API Docs|Vintage Story API Docs]] (`/websites/apidocs_vintagestory_at`)
- [[03_Context7_Resources#VS Survival Mod (Source)|VS Survival Mod Source]] (`/anegostudios/vssurvivalmod`)
- [[03_Context7_Resources#Blockbench Documentation|Blockbench Documentation]] (`websites/web_blockbench_net`)

## 📝 Ведення документації

- Вся архітектура, дизайн квестів, логіка штучного інтелекту NPC та дизайн-документи (GDD) зберігаються тут, у папці Obsidian.
- Документація оновлюється агентом паралельно з написанням коду або після обговорень з розробником.
