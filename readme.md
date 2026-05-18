# LOTR Mod for Vintage Story — Development Plan

> Project root: `/Users/max/Projects/lotr`
> Mod id: `lotr` | Namespace: `Lotr` | Assembly: `lotr`
> Target: Vintage Story 1.22.x, .NET 10, C# 13
> Game path: `/Applications/Vintage Story.app`
> Deploy path: `~/Library/Application Support/VintagestoryData/Mods/lotr`
> Build: `dotnet build lotr.csproj -c Debug`

---

## Статус фаз

### Phase 0 — Foundation ✅
- Структура проекту `src/`, `assets/lotr/`
- `LotrConstants.cs`, `LotrModSystem.cs`
- Build 0 errors

### Phase 1 — Entities & Items ✅
- Entity JSON файли: aragorn, frodo, bilbo, gandalf
- Item JSON: lembas
- Block JSON: shire-grass
- `assets/lotr/lang/en.json`

### Phase 2 — Alignment & Factions ✅
- `AlignmentTier.cs`, `AlignmentSystem.cs`
- Дефолтні alignment значення (Shire +200, Mordor -800, etc.)
- Команда `/lotr alignment`

### Phase 3 — Quest System ✅
- `QuestState.cs`, `QuestDefinition.cs`, `QuestPlayerData.cs`, `QuestSystem.cs`
- JSON квести: `shire-long-expected-party.json`, `shire-concerning-hobbits.json`
- Команди `/lotr quest list/start/complete`
- Фікс: `LoadQuests()` в `StartServerSide`, не `AssetsLoaded`

### Phase 4 — Region System ✅
- `BiomeDefinition.cs`, `RegionDefinition.cs`, `RegionSystem.cs`
- 10 регіонів: Shire, Bree, Rivendell, BlueMountains, Moria, MinasTirith, Isengard, Mordor, Ocean, Rohan
- 10 biome JSON файлів
- Команда `/lotr region`
- Spawn-relative координати (відносно `DefaultSpawnPosition`)
- Фікс: `CacheSpawn()` тільки в `PlayerJoin`, не `SaveGameLoaded` (NRE crash)

### Phase 5 — WorldGen Surface ✅
- `LotrWorldGen.cs` — surface block overrides за регіонами
- 6 landform JSON файлів
- Перевірено в грі: регіон Shire знайдено через `/tp ~-21504 150 ~-25088`

### Phase 6 — Structures ✅
- `StructureBase.cs`, `HobbitHole.cs`, `OrthanctTower.cs`, `MinasTirithWall.cs`
- `StructureSpawner.cs` (ExecuteOrder=0.3)
- HobbitHole з вапнякового буту, скриня + бочка — підтверджено в грі
- Лог: `[LOTR] Spawned HobbitHole at X,Y,Z` (Notification)

### Phase 7 — PNG Biome Map ✅
- `PngBiomeMap.cs` — ручний PNG decoder (без System.Drawing)
- `assets/lotr/worldgen/map.png` — 256x256, 1px=512 blocks
- `assets/lotr/worldgen/colormap.json` — 10 кольорів → регіони
- Лог при старті: `[LOTR] PngBiomeMap loaded 256x256, 8 region colors`
- Океан (`1E50A0`) і Рохан (`B4A078`) додані в colormap

---

## Що залишилось

### Phase 8 — Entities (NPC AI) 🔲
- Повернути entity JSON файли з `assets/lotr/entities/disabled/`
- Замінити shape `seraph-faceless` на простіший NPC shape (trader/villager)
- AiTask: wandering, dialogue trigger, quest giver
- `EntityHobbit`, `EntityHuman` класи з правильними hitbox

### Phase 9 — Dialogue System 🔲
- `DialogueDefinition.cs`, `DialogueSystem.cs`
- JSON діалоги для Frodo, Gandalf
- Trigger: гравець підходить до NPC → відкривається чат/GUI
- Зв'язок з QuestSystem (NPC дає квест)

### Phase 10 — Enemies & Combat 🔲
- Entity JSON: orc, uruk-hai, nazgul
- AI поведінка: patrol, aggro на гравця
- Alignment impact: вбивство орка +alignment Gondor, -alignment Mordor
- Боси: Balrog (Moria), Sauron (Mordor)

### Phase 11 — Structures (розширення) 🔲
- Orthanc Tower: повноцінна вежа замість placeholder
- Minas Tirith Wall: кам'яна стіна з зубцями
- Rivendell: elven hall structure
- Moria gate: структура входу в шахти

### Phase 12 — FactionSystem (NPC ворожість) 🔲
- `FactionSystem.cs`
- NPC перевіряє alignment гравця при підході
- Shire >0: дружній | Mordor <0: ворожий
- Торговці у Bree, ельфи в Rivendell

### Phase 13 — Rivers & Water 🔲
- Anduin river (Gondor → Sea)
- Isen river (Isengard)
- Інтеграція з VS `GenRivulets` або кастомні water patches

### Phase 14 — Items & Crafting 🔲
- Lembas bread (рецепт, satiety)
- Mithril ingot (рідкісний, Moria drop)
- Elvish bow, Sting sword
- Рецепти через існуючу crafting систему VS

### Phase 15 — UI & HUD 🔲
- Faction alignment bar (overlay при натисканні F)
- Quest journal GUI
- Region name popup при переході між регіонами

### Phase 16 — Polish & Release 🔲
- Власні текстури для entity (замість seraph-faceless)
- Ambient sounds (`lotr:sounds/ambient/shire` etc.)
- Mod packaging, versioning
- Тестування сумісності з іншими модами

---

## Технічні нотатки

### Відомі проблеми
- VS 1.22 на macOS Metal: іноді crash exit 139 при старті нового світу (не пов'язано з модом — відтворюється без lotr)
- `seraph-faceless` shape потребує skinnable систему → entity файли тимчасово в `disabled/`
- `DefaultSpawnPosition` недоступний на `SaveGameLoaded` → NRE → читати тільки в `PlayerJoin`
- `System.Drawing.Common` недоступний в VS контексті → ручний PNG decoder

### Координати регіонів (spawn-relative)
```
Shire:         /tp ~-21504 150 ~-25088
Bree:          /tp ~-10752 150 ~-20992
Rivendell:     /tp ~15360  150 ~-33280
BlueMountains: /tp ~-30720 150 ~-20480
Moria:         /tp ~5120   150 ~-7680
MinasTirith:   /tp ~20480  150 ~5120
Isengard:      /tp ~-5120  150 ~10240
Mordor:        /tp ~30720  150 ~12800
```

### Кольори PNG карти
```
1E50A0 = Ocean (Belegaer)
B4A078 = Rohan
64B43C = Shire
8B6914 = Bree
5B8FCC = Rivendell
6B6B8A = Blue Mountains
4A4A4A = Moria
C8C8B4 = Minas Tirith
3A3A3A = Isengard
8B0000 = Mordor
```
