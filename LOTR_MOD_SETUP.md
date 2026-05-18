# LOTR Vintage Story Mod — Налаштування проекту

## Зміст
1. [Передумови](#1-передумови)
2. [Встановлення Vintage Story на macOS](#2-встановлення-vintage-story-на-macos)
3. [Налаштування змінних середовища](#3-налаштування-змінних-середовища)
4. [Структура проекту](#4-структура-проекту)
5. [Перша збірка та запуск](#5-перша-збірка-та-запуск)
6. [Workflow розробки](#6-workflow-розробки)

---

## 1. Передумови

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
2. Встанови у `/Applications/Vintage Story.app`
3. Запусти гру один раз — це створить папку даних:
   ```
   ~/Library/Application Support/VintagestoryData/
   ```
4. Папка з бінарниками гри:
   ```
   /Applications/Vintage Story.app/
   ├── Vintagestory           ← виконуваний файл
   ├── VintagestoryServer     ← сервер
   ├── VintagestoryAPI.dll    ← головний API
   ├── Mods/VSSurvivalMod.dll
   ├── Mods/VSEssentials.dll
   ├── Mods/VSCreativeMod.dll
   ├── Lib/Newtonsoft.Json.dll
   └── assets/                ← всі ігрові ресурси (референс)
   ```

> Переглядай `/Applications/Vintage Story.app/assets/` — там є всі JSON файли гри.
> Це найкращий референс для написання власних ентіті, блоків та предметів.

---

## 3. Налаштування змінних середовища

Відкрий термінал і додай у `~/.zshrc`:

```bash
export VINTAGE_STORY="/Applications/Vintage Story.app"
export VINTAGE_STORY_DATA="$HOME/Library/Application Support/VintagestoryData"
```

Застосуй:
```bash
source ~/.zshrc
echo $VINTAGE_STORY
# /Applications/Vintage Story.app
```

---

## 4. Структура проекту

```
/Users/max/Projects/lotr/
├── lotr.csproj
├── modinfo.json
├── assets/lotr/
│   ├── blocktypes/
│   ├── itemtypes/
│   ├── entities/humanoid/
│   ├── worldgen/
│   ├── recipes/
│   ├── lang/en.json
│   └── textures/
├── src/
│   ├── LotrModSystem.cs        ← точка входу ⭐
│   ├── Constants/
│   ├── Entities/
│   │   ├── Humanoids/
│   │   ├── Monsters/
│   │   ├── Bosses/
│   │   ├── Behaviors/
│   │   └── AI/
│   ├── Systems/
│   │   ├── Alignment/
│   │   ├── Factions/
│   │   ├── Quests/
│   │   ├── Dialogue/
│   │   ├── Regions/
│   │   └── ...
│   ├── Items/
│   ├── Blocks/
│   ├── WorldGen/
│   └── Utilities/
└── docs/
    ├── design/
    ├── plans/
    ├── lore/
    └── testing/
```

---

## 5. Перша збірка та запуск

```bash
cd /Users/max/Projects/lotr
dotnet build lotr.csproj -c Debug
```

Успішна збірка виводить DLL у:
```
bin/Debug/Mods/mod/lotr.dll
```

Деплой у гру (loose-file mode):
```bash
rsync -a bin/Debug/Mods/mod/ \
  "/Users/max/Library/Application Support/VintagestoryData/Mods/lotr/"
```

---

## 6. Workflow розробки

```
# 1. Внести зміни
# 2. Зібрати
dotnet build lotr.csproj -c Debug

# 3. Задеплоїти
rsync -a bin/Debug/Mods/mod/ \
  "/Users/max/Library/Application Support/VintagestoryData/Mods/lotr/"

# 4. В грі:
/reload
```

### Корисні команди в грі
```
/gamemode creative
/time set 12000
/tp 0 130 0
/entity spawn lotr:frodo
/entity spawn lotr:bilbo
/reload
```
