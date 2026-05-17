# LOTR Vintage Story Mod — Налаштування проекту

## Зміст
1. [Передумови](#1-передумови)
2. [Встановлення Vintage Story на macOS](#2-встановлення-vintage-story-на-macos)
3. [Налаштування змінних середовища](#3-налаштування-змінних-середовища)
4. [Встановлення шаблону мода](#4-встановлення-шаблону-мода)
5. [Створення проекту в Rider](#5-створення-проекту-в-rider)
6. [Структура проекту](#6-структура-проекту)
7. [Перша збірка та запуск](#7-перша-збірка-та-запуск)
8. [Налаштування для AI Агента](#8-налаштування-для-ai-агента)

---

## 1. Передумови

Встанови наступне перед початком:

| Інструмент | Версія | Посилання |
|---|---|---|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| JetBrains Rider | Остання | https://www.jetbrains.com/rider/ |
| Vintage Story | 1.22+ | https://www.vintagestory.at/ |
| Blockbench | Остання | https://www.blockbench.net/ |
| Git | Будь-яка | https://git-scm.com/ |

Перевір встановлення .NET:
```bash
dotnet --version
# Має показати: 10.0.x
```

---

## 2. Встановлення Vintage Story на macOS

1. Завантаж `.dmg` з офіційного сайту
2. Встанови у `/Applications/Vintagestory/`
3. Запусти гру один раз — це створить папку даних:
   ```
   ~/Library/Application Support/VintagestoryData/
   ```
4. Папка з бінарниками гри:
   ```
   /Applications/Vintagestory/
   ├── Vintagestory           ← виконуваний файл
   ├── VintagestoryServer     ← сервер
   ├── VintagestoryAPI.dll    ← головний API
   ├── VSEssentials.dll
   ├── VSSurvivalMod.dll
   └── assets/                ← всі ігрові ресурси (референс)
   ```

> **Порада:** Переглядай `/Applications/Vintagestory/assets/` — там є всі JSON файли гри. Це найкращий референс для написання власних ентіті, блоків та предметів.

---

## 3. Налаштування змінних середовища

Відкрий термінал і додай у `~/.zshrc`:

```bash
# Vintage Story шлях до гри
export VINTAGE_STORY="/Applications/Vintagestory"

# Шлях до даних гри (моди, світи)
export VINTAGE_STORY_DATA="$HOME/Library/Application Support/VintagestoryData"
```

Застосуй зміни:
```bash
source ~/.zshrc

# Перевір:
echo $VINTAGE_STORY
# /Applications/Vintagestory
```

---

## 4. Встановлення шаблону мода

```bash
# Додай NuGet джерело (якщо не додано)
dotnet nuget add source "https://api.nuget.org/v3/index.json" --name "nuget.org"

# Встанови шаблон Vintage Story
dotnet new install VintageStory.Mod.Templates

# Перевір що шаблон з'явився:
dotnet new list | grep vsmod
# vsmod   Vintage Story Mod
```

---

## 5. Створення проекту в Rider

### 5.1 Через термінал (рекомендовано)

```bash
# Перейди в папку проектів
cd ~/Projects

# Створи мод з шаблону
dotnet new vsmod \
  --AddSolutionFile \
  --IncludeVSSurvivalMod \
  -o lotrmod

# Перейди в папку
cd lotrmod

# Відкрий у Rider
open -a "Rider" lotrmod.sln
```

### 5.2 Налаштування Rider

Після відкриття в Rider:

1. **Run Configurations** → вибери `lotrmod` зі списку запуску
2. Переконайся що конфігурація вказує на правильний виконуваний файл:
   - Відкрий `lotrmod/Properties/launchSettings.json`
   - Зміни шлях для macOS:

```json
{
  "profiles": {
    "lotrmod": {
      "commandName": "Executable",
      "executablePath": "$(VINTAGE_STORY)/Vintagestory",
      "commandLineArgs": "--dataPath \"$(VINTAGE_STORY_DATA)\"",
      "environmentVariables": {
        "VINTAGE_STORY": "/Applications/Vintagestory"
      }
    },
    "lotrmod Server": {
      "commandName": "Executable", 
      "executablePath": "$(VINTAGE_STORY)/VintagestoryServer",
      "commandLineArgs": "--dataPath \"$(VINTAGE_STORY_DATA)\"",
      "environmentVariables": {
        "VINTAGE_STORY": "/Applications/Vintagestory"
      }
    }
  }
}
```

3. **Rider → Preferences → Build → Build before run** — увімкни

### 5.3 Перший запуск збірки

У Rider зверху в дропдауні обери `CakeBuild` і натисни ▶

Або через термінал:
```bash
cd lotrmod
dotnet run --project CakeBuild
```

Успішна збірка створить:
```
lotrmod/Releases/lotrmod_1.0.0.zip
```

---

## 6. Структура проекту

Після налаштування структура папок виглядає так:

```
lotrmod/
│
├── lotrmod.sln                          ← Solution файл
│
├── CakeBuild/                           ← система збірки
│   └── Program.cs
│
└── lotrmod/                             ← основний проект
    ├── lotrmod.csproj                   ← конфігурація проекту
    ├── modinfo.json                     ← маніфест мода ⭐
    │
    ├── assets/
    │   └── lotrmod/
    │       ├── blocktypes/              ← нові блоки
    │       ├── itemtypes/               ← предмети та зброя
    │       ├── entities/                ← НПЦ та моби
    │       │   └── humanoid/
    │       ├── worldgen/
    │       │   └── structures/          ← схематики будівель
    │       ├── recipes/                 ← рецепти крафту
    │       ├── lang/
    │       │   └── en.json              ← переклади
    │       └── textures/
    │           ├── block/
    │           ├── item/
    │           └── entity/
    │
    └── src/
        ├── LotrModSystem.cs             ← точка входу ⭐
        ├── worldgen/
        │   ├── MiddleEarthWorldGen.cs
        │   └── StructurePlacer.cs
        ├── entities/
        │   ├── EntityGandalf.cs
        │   ├── EntityHobbit.cs
        │   └── ai/
        │       ├── AiTaskQuestGiver.cs
        │       └── AiTaskPatrol.cs
        ├── systems/
        │   ├── AlignmentSystem.cs
        │   ├── QuestSystem.cs
        │   └── FactionSystem.cs
        └── items/
            └── ItemOneRing.cs
```

### modinfo.json — заповни своїми даними:

```json
{
  "modid": "lotrmod",
  "name": "Lord of the Rings — Middle Earth",
  "description": "Brings Middle-earth to Vintage Story. Explore the Shire, Moria, Minas Tirith and Isengard. Meet Gandalf, Frodo, Bilbo and face Sauron.",
  "authors": ["YourName"],
  "version": "0.1.0",
  "type": "code",
  "website": "https://github.com/yourusername/lotrmod",
  "dependencies": {
    "game": "1.22.0",
    "survival": "1.22.0"
  }
}
```

### LotrModSystem.cs — стартова точка входу:

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace LotrMod
{
    public class LotrModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[LOTR Mod] Initializing Middle-earth...");

            // Реєстрація класів сутностей
            api.RegisterEntityClass("EntityGandalf", typeof(EntityGandalf));
            api.RegisterEntityClass("EntityHobbit", typeof(EntityHobbit));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            // Ініціалізація серверних систем
            var alignmentSystem = new AlignmentSystem(api);
            var questSystem = new QuestSystem(api);
            var factionSystem = new FactionSystem(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            // Клієнтська ініціалізація (HUD, UI)
        }
    }
}
```

---

## 7. Перша збірка та запуск

### Workflow розробки

```bash
# 1. Збери мод (Rider або термінал)
dotnet run --project CakeBuild

# 2. Скопіюй .zip у папку модів гри
cp Releases/lotrmod_1.0.0.zip \
   "$HOME/Library/Application Support/VintagestoryData/Mods/"

# Або налаштуй автокопіювання через CakeBuild/Program.cs
```

### Швидкий перезапуск (без повного рестарту гри)

У грі через чат:
```
/reload                    ← перезавантаження всіх модів
.debug wireframe entity    ← відображення хітбоксів
/schematic save test       ← зберегти схематику будівлі
```

### Корисні команди для розробки

```
/gamemode creative         ← творчий режим
/time set 12000            ← день
/tp 0 130 0                ← телепортація
/entity spawn lotrmod:gandalf  ← спавн НПЦ
/worldconfig allowFallingBlocks false
```

---

## 8. Налаштування для AI Агента

Цей розділ описує документи та контекст для Hermes AI Agent.

### 8.1 Файл AGENT_CONTEXT.md (клади в корінь проекту)

```markdown
# LOTR Mod — Контекст для AI Агента

## Технічний стек
- Гра: Vintage Story v1.22
- .NET: 10.0
- Мова: C# 13
- IDE: JetBrains Rider (macOS)
- Система збірки: Cake Build

## Ключові API класи

### Точка входу
ModSystem — базовий клас, успадковуй від нього
- Start(ICoreAPI api) — загальна ініціалізація
- StartServerSide(ICoreServerAPI api) — серверна логіка
- StartClientSide(ICoreClientAPI api) — клієнтська логіка

### Сутності
- Entity — базовий клас всіх сутностей
- EntityAgent — сутності з AI (НПЦ, моби)
- EntityPlayer — гравець
- EntityBehavior — компонент поведінки (додається через JSON)
- AiTask — задача AI (патрулювання, атака, розмова)

### Світ
- IBlockAccessor — читання/запис блоків
- IWorldAccessor — доступ до світу
- BlockSchematic — схематики будівель
- IChunkColumnGenerateRequest — генерація чанків

### Важливі простори імен
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

## Архітектура мода

### Системи (src/systems/)
- AlignmentSystem — репутація гравця з фракціями
- QuestSystem — квести та завдання
- FactionSystem — фракції (Шир, Гондор, Мордор тощо)

### Сутності (src/entities/)
- EntityGandalf : EntityAgent
- EntityHobbit : EntityAgent (менший hitbox)
- EntityElf : EntityAgent (вищий hitbox)
- EntityOrc : EntityAgent (ворожий)
- AiTaskQuestGiver : AiTask (надає квести гравцю)
- AiTaskPatrol : AiTask (патрулювання між точками)

### Генерація світу (src/worldgen/)
- MiddleEarthWorldGen — головний генератор
- StructurePlacer — розміщення будівель

## Правила написання коду

1. ЗАВЖДИ перевіряй api.Side перед виконанням операцій
2. Серверний код НЕ має звертатися до клієнтських API
3. Використовуй api.Logger.Notification() для дебагу
4. JSON assets лежать у assets/lotrmod/
5. modid завжди "lotrmod" з малої літери
6. Всі entity codes мають префікс "lotrmod:" (lotrmod:gandalf)

## Розміри сутностей (hitboxSize)

Хоббіт:    x:0.4, y:1.1  eyeHeight:0.95
Гном:      x:0.5, y:1.4  eyeHeight:1.2
Людина:    x:0.6, y:1.9  eyeHeight:1.7
Ельф:      x:0.55, y:2.1 eyeHeight:1.95
Тролль:    x:1.5, y:4.0  eyeHeight:3.5
Балрог:    x:3.0, y:8.0  eyeHeight:7.0

## Рівень моря

Стандартна висота світу: 256 блоків, рівень моря: ~110
Для LOTR мода: висота 512, рівень моря: ~120
Будівлі будуються ВИЩЕ рівня моря.

## Заборонено

- Не використовуй Thread.Sleep() — використовуй RegisterGameTickListener
- Не зберігай ICoreAPI як статичне поле
- Не викликай клієнтський код на сервері і навпаки
- Не хардкодь BlockPos координати — зчитуй з конфігу або worldgen
```

### 8.2 Файл ENTITY_TEMPLATE.md

```markdown
# Шаблони сутностей для LOTR Мода

## JSON шаблон — гуманоїдний НПЦ

Файл: assets/lotrmod/entities/humanoid/[name].json

{
  "code": "lotrmod:[name]",
  "class": "EntityAgent",
  "hitboxSize": { "x": 0.6, "y": 1.9 },
  "deadHitboxSize": { "x": 0.6, "y": 0.5 },
  "eyeHeight": 1.7,
  "client": {
    "renderer": "Shape",
    "shape": { "base": "lotrmod:entity/humanoid/[name]" },
    "behaviors": [
      { "code": "repulseagents" },
      { "code": "controlledphysics", "stepHeight": 0.6 },
      { "code": "interpolateposition" },
      { "code": "emotionstates" }
    ],
    "animations": [
      { "code": "idle", "animation": "idle", "blendMode": "Average", "weight": 1 },
      { "code": "walk", "animation": "walk", "blendMode": "Average", "weight": 1 },
      { "code": "hurt", "animation": "hurt", "blendMode": "AddAverage", "weight": 10 },
      { "code": "die",  "animation": "die",  "blendMode": "AddAverage", "weight": 10 }
    ]
  },
  "server": {
    "behaviors": [
      { "code": "repulseagents" },
      { "code": "controlledphysics", "stepHeight": 0.6 },
      { "code": "health", "currenthealth": 20, "maxhealth": 20 },
      { "code": "deaddecay", "hoursToDecay": 24 },
      { "code": "floatupwhenstuck" },
      {
        "code": "taskai",
        "aitasks": [
          { "code": "lotrmod:questgiver", "priority": 1.5 },
          { "code": "lookatentity", "entityCodes": ["player"], "priority": 0.8 },
          { "code": "wander", "priority": 0.5, "movespeed": 0.015 }
        ]
      }
    ]
  },
  "sounds": {
    "hurt": "lotrmod:entity/[name]-hurt",
    "death": "lotrmod:entity/[name]-death",
    "idle": "lotrmod:entity/[name]-idle"
  },
  "attributes": {
    "faction": "lotrmod:[faction]",
    "dialogue": "lotrmod:dialogue/[name]",
    "tradeProps": {
      "currency": "lotrmod:coin-gondor",
      "tradeList": "lotrmod:tradelist/[name]"
    }
  }
}

## C# шаблон сутності

// src/entities/Entity[Name].cs
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace LotrMod
{
    public class Entity[Name] : EntityAgent
    {
        public override void Initialize(EntityProperties properties, 
            ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
        }
    }
}
```

### 8.3 Файл WORLDGEN_NOTES.md

```markdown
# Нотатки по генерації світу Middle-earth

## Регіони та їх координати (масштаб 1:50)

Координати відносні від центру мапи (0,0).
Одиниця = блок у грі.

| Регіон | X від центру | Z від центру | Біом |
|--------|-------------|-------------|------|
| Шир (Хоббітань) | -8000 | -2000 | Помірний, пагорби |
| Рівенділ | -5000 | -4000 | Гірський, водоспади |
| Морія | -4000 | -2000 | Підземний |
| Лотлоріен | -3000 | -1000 | Лісовий |
| Едорас (Рохан) | 0 | 0 | Степовий |
| Мінас Тіріт | 3000 | 2000 | Скелястий |
| Мордор | 6000 | 4000 | Вулканічний, пустеля |
| Ізенгард | -1000 | 1000 | Промисловий |

## Висоти будівель (від рівня моря 120)

| Структура | Висота блоків | Y max |
|-----------|--------------|-------|
| Хоббітська нора | 5 | 125 |
| Едорас (місто) | 30 | 150 |
| Мінас Тіріт (7 рівнів) | 80 | 200 |
| Ортханк (вежа) | 120 | 240 |
| Барад-Дур | 180 | 300 |

## Пріоритет реалізації

Фаза 1 (MVP):
  [x] Шир — базовий регіон
  [x] Хоббітська нора — перша структура
  [x] Більбо/Фродо — перші НПЦ
  [ ] Базова система діалогів

Фаза 2:
  [ ] Морія — підземна генерація
  [ ] Гендальф — квест-гівер
  [ ] Система фракцій
  [ ] Елементарні квести

Фаза 3:
  [ ] Мінас Тіріт — велика структура
  [ ] Мордор — ворожий регіон
  [ ] Система alignment
  [ ] Кільце Всевладдя
```

---

## Корисні посилання

| Ресурс | URL |
|---|---|
| Офіційний API | https://apidocs.vintagestory.at/ |
| Вікі моддингу | https://wiki.vintagestory.at/Modding |
| vssurvivalmod (код гри) | https://github.com/anegostudios/vssurvivalmod |
| Шаблон мода | https://github.com/anegostudios/vsmodtemplate |
| Blockbench | https://www.blockbench.net/ |
| VS Mod DB | https://mods.vintagestory.at/ |
| Discord спільнота | https://discord.gg/vintagestory |

---

*Документ актуальний для Vintage Story v1.22 / .NET 10 / Rider на macOS*
