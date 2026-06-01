# LOTR Mod — Claude Code Project Context

## Stack

- Game: Vintage Story 1.22, .NET 10, C# 13
- Build: `dotnet run --project CakeBuild`
- Output: `Releases/lotr_*.zip` → copy to `~/Library/Application Support/VintagestoryData/Mods/`
- IDE: JetBrains Rider on macOS
- Game path: `/Applications/Vintage Story.app`
- Data path: `~/Library/Application Support/VintagestoryData/`

## Project Structure

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
└── src/
    ├── LotrModSystem.cs        # MOD ENTRY POINT
    ├── Constants/
    ├── Entities/
    │   ├── Humanoids/
    │   ├── Monsters/
    │   ├── Bosses/
    │   ├── Behaviors/
    │   └── AI/
    ├── Systems/
    │   ├── Alignment/
    │   ├── Factions/
    │   ├── Quests/
    │   ├── Dialogue/
    │   ├── Regions/
    │   ├── Spawn/
    │   ├── World/
    │   └── SaveData/
    ├── WorldGen/
    ├── Items/
    ├── Blocks/
    ├── Commands/
    ├── Config/
    └── Utilities/
```

- `.claude/skills/` — project-specific skills

## Active Skills

- vs-api        Vintage Story API rules
- lotr-lore     Tolkien lore for dialogue and content
- blockbench-vs 3D model creation via Blockbench MCP

## Mod ID

Always use prefix: `lotr:` for all asset codes. ModID: `lotr` (always lowercase). Namespace: `Lotr`.

---

## Project Overview

Lord of the Rings mod for Vintage Story (voxel survival game written in C#).
The mod adds Middle-earth locations, characters, quests, and factions.

Key locations: Shire, Moria, Minas Tirith, Isengard, Mordor
Key characters: Gandalf, Frodo, Bilbo, Sauron, Aragorn
Systems: Factions, Alignment, Quests, World Generation

## Current Phase: MVP — The Shire

Focus on: Frodo + Bilbo NPCs, basic Shire terrain, hobbit hole structure

---

## Development Phases

### Phase 0 — Foundation ✅
- Project structure `src/`, `assets/lotr/`
- `LotrConstants.cs`, `LotrModSystem.cs`
- Build 0 errors

### Phase 1 — Entities & Items ✅
- Entity JSON: aragorn, frodo, bilbo, gandalf
- Item JSON: lembas
- Block JSON: shire-grass
- `assets/lotr/lang/en.json`

### Phase 2 — Alignment & Factions ✅
- `AlignmentTier.cs`, `AlignmentSystem.cs`
- Default alignment values (Shire +200, Mordor -800, etc.)
- Command `/lotr alignment`

### Phase 3 — Quest System ✅
- `QuestState.cs`, `QuestDefinition.cs`, `QuestPlayerData.cs`, `QuestSystem.cs`
- JSON quests: `shire-long-expected-party.json`, `shire-concerning-hobbits.json`
- Commands `/lotr quest list/start/complete`
- Fix: `LoadQuests()` in `StartServerSide`, not `AssetsLoaded`

### Phase 4 — Region System ✅
- `BiomeDefinition.cs`, `RegionDefinition.cs`, `RegionSystem.cs`
- 10 regions: Shire, Bree, Rivendell, BlueMountains, Moria, MinasTirith, Isengard, Mordor, Ocean, Rohan
- Command `/lotr region`
- Fix: `CacheSpawn()` only in `PlayerJoin`, not `SaveGameLoaded` (NRE crash)

### Phase 5 — WorldGen Surface ✅
- `LotrWorldGen.cs` — surface block overrides per region
- 6 landform JSON files
- Shire region verified in-game via `/tp ~-21504 150 ~-25088`

### Phase 6 — Structures ✅
- `StructureBase.cs`, `HobbitHole.cs`, `OrthanctTower.cs`, `MinasTirithWall.cs`
- `StructureSpawner.cs` (ExecuteOrder=0.3)
- HobbitHole with limestone rubble, chest + barrel — verified in-game

### Phase 7 — PNG Biome Map ✅
- `PngBiomeMap.cs` — manual PNG decoder (no System.Drawing)
- `assets/lotr/worldgen/map.png` — 256x256, 1px=512 blocks
- `assets/lotr/worldgen/colormap.json` — 10 colors → regions

### Phase 8 — Entities (NPC AI) 🔲
- Re-enable entity JSON from `assets/lotr/entities/disabled/`
- Replace `seraph-faceless` shape with simpler NPC shape (trader/villager)
- AiTask: wandering, dialogue trigger, quest giver
- `EntityHobbit`, `EntityHuman` classes with correct hitboxes

### Phase 9 — Dialogue System 🔲
- `DialogueDefinition.cs`, `DialogueSystem.cs`
- JSON dialogues for Frodo, Gandalf
- Trigger: player approaches NPC → opens chat/GUI
- Link with QuestSystem (NPC gives quest)

### Phase 10 — Enemies & Combat 🔲
- Entity JSON: orc, uruk-hai, nazgul
- AI: patrol, aggro on player
- Alignment impact: killing orc +alignment Gondor, -alignment Mordor
- Bosses: Balrog (Moria), Sauron (Mordor)

### Phase 11 — Structures (expansion) 🔲
- Orthanc Tower: full tower
- Minas Tirith Wall: stone wall with battlements
- Rivendell: elven hall structure
- Moria gate: entrance structure

### Phase 12 — FactionSystem (NPC hostility) 🔲
- `FactionSystem.cs`
- NPC checks player alignment on approach
- Shire >0: friendly | Mordor <0: hostile

### Phase 13 — Rivers & Water 🔲
### Phase 14 — Items & Crafting 🔲
### Phase 15 — UI & HUD 🔲
### Phase 16 — Polish & Release 🔲

---

## Naming Conventions

- Entity codes: `lotr:gandalf`, `lotr:frodo`, `lotr:orc-mordor`
- Block codes: `lotr:stone-minas-tirith`, `lotr:block-mithril`
- Item codes: `lotr:sword-sting`, `lotr:lembas`, `lotr:one-ring`
- Texture paths: `lotr:entity/humanoid/gandalf`, `lotr:block/mithril`
- JSON asset paths: `assets/lotr/entities/humanoid/gandalf.json`

---

## Entity Sizes (hitboxSize)

All sizes in blocks (1 block = 1 meter).

```
Hobbit (Frodo, Bilbo):    hitbox x:0.4 y:1.1   eyeHeight:0.95
Dwarf (Gimli):            hitbox x:0.5 y:1.4   eyeHeight:1.2
Human (Aragorn, Boromir): hitbox x:0.6 y:1.9   eyeHeight:1.7
Elf (Legolas, Galadriel): hitbox x:0.55 y:2.1  eyeHeight:1.95
Maiar (Gandalf):          hitbox x:0.6 y:2.0   eyeHeight:1.85
Troll:                    hitbox x:1.5 y:4.0   eyeHeight:3.5
Balrog (boss):            hitbox x:3.0 y:8.0   eyeHeight:7.0
```

---

## World Dimensions

- Default VS world height: 256 blocks, sea level: ~110
- LOTR mod world height: 512 blocks, sea level: ~120
- All structures built ABOVE sea level (y > 120)

Building heights from sea level (y=120):
```
Hobbit hole:        y 120-125  (5 blocks)
Edoras city:        y 120-150  (30 blocks)
Minas Tirith:       y 120-200  (80 blocks, 7 tiers)
Orthanc tower:      y 120-240  (120 blocks)
Barad-dur:          y 120-300  (180 blocks)
Mount Doom peak:    y 120-350  (230 blocks)
```

---

## Region Coordinates (spawn-relative)

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

## PNG Map Colors

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

---

## Faction System

Each region belongs to a faction. Player gains/loses alignment through actions.
Alignment range: -1000 (hated) to +1000 (exalted). Stored per-player via SaveGame attributes.

### Free Peoples (Forces of Light)

#### Men
| Faction | Code | Location | Default |
|---|---|---|---|
| Gondor | `lotr:faction-gondor` | Minas Tirith, Osgiliath, Ithilien | Neutral/Friendly |
| Rohan | `lotr:faction-rohan` | Edoras, Helm's Deep | Neutral/Friendly |
| Bree-land | `lotr:faction-bree` | Bree | Friendly |
| Dale | `lotr:faction-dale` | Near Erebor | Neutral/Friendly |
| Esgaroth | `lotr:faction-esgaroth` | Lake-town | Neutral |
| Beornings | `lotr:faction-beornings` | Anduin valley | Neutral/Cautious |
| Rangers of the North | `lotr:faction-rangers-north` | Eriador | Friendly |
| Drúedain | `lotr:faction-druedain` | Drúadan Forest | Cautious |

#### Elves
| Faction | Code | Location | Default |
|---|---|---|---|
| Rivendell | `lotr:faction-rivendell` | Imladris | Friendly |
| Lothlórien | `lotr:faction-lothlorien` | Caras Galadhon | Neutral/Cautious |
| Woodland Realm | `lotr:faction-mirkwood` | Mirkwood | Neutral |
| Grey Havens | `lotr:faction-lindon` | Lindon | Friendly |

#### Dwarves
| Faction | Code | Location | Default |
|---|---|---|---|
| Erebor | `lotr:faction-erebor` | Lonely Mountain | Neutral/Friendly |
| Iron Hills | `lotr:faction-iron-hills` | Iron Hills | Neutral |
| Ered Luin | `lotr:faction-ered-luin` | Blue Mountains | Neutral |
| Moria (Khazad-dûm) | `lotr:faction-moria` | Moria | Neutral |

#### Hobbits
| Faction | Code | Location | Default |
|---|---|---|---|
| The Shire | `lotr:faction-shire` | The Shire | Very Friendly |

### Forces of Evil

#### Mordor
| Faction | Code | Default |
|---|---|---|
| Orcs of Mordor | `lotr:faction-mordor-orcs` | Hostile |
| Black Uruks | `lotr:faction-mordor-uruks` | Hostile |
| Olog-hai | `lotr:faction-olog-hai` | Hostile |
| Nazgûl | `lotr:faction-nazgul` | Hostile |

#### Isengard
| Faction | Code | Default |
|---|---|---|
| Uruk-hai | `lotr:faction-isengard` | Hostile |
| Orcs of Isengard | `lotr:faction-isengard-orcs` | Hostile |
| Dunlendings | `lotr:faction-dunland` | Hostile |
| Half-orcs | `lotr:faction-isengard-men` | Hostile/Neutral |

#### Misty Mountains & North
| Faction | Code | Default |
|---|---|---|
| Moria Goblins | `lotr:faction-goblins-moria` | Hostile |
| Gundabad Orcs | `lotr:faction-gundabad` | Hostile |
| Goblin-town | `lotr:faction-goblin-town` | Hostile |
| Angmar Remnants | `lotr:faction-angmar` | Hostile |

#### Easterlings & Haradrim
| Faction | Code | Default |
|---|---|---|
| Haradrim | `lotr:faction-harad` | Hostile |
| Easterlings | `lotr:faction-rhun` | Hostile |
| Corsairs of Umbar | `lotr:faction-umbar` | Hostile |

### Neutral & Ancient Forces
| Faction | Code | Default |
|---|---|---|
| Ents of Fangorn | `lotr:faction-fangorn` | Neutral |
| Great Eagles | `lotr:faction-eagles` | Friendly |
| Wargs | `lotr:faction-wargs` | Hostile |
| Spiders of Mirkwood | `lotr:faction-mirkwood-spiders` | Hostile |
| Oathbreakers | `lotr:faction-oathbreakers` | Hostile/Friendly to Heir |
| Balrog | `lotr:faction-balrog` | Hostile to all |
| Tom Bombadil | `lotr:faction-bombadil` | Neutral/Friendly |

---

## Critical Rules

1. Always check `api.Side` before side-specific code
2. Server code MUST NOT call client APIs and vice versa
3. Use `api.Logger.Notification("[LOTR] message")` for debug logging
4. Never use `Thread.Sleep()` — use `RegisterGameTickListener` instead
5. Never store `ICoreAPI` as a static field — pass via constructor
6. Entity registration happens in `Start()`, not `StartServerSide()`
7. JSON entity code must match the registered class name exactly
8. All entity/block/item codes need the `lotr:` prefix

## Architecture Rules

- **Entry point**: `LotrModSystem.cs` initializes components, no heavy logic
- **Services**: Create systems in `src/Systems/[SystemName]/`
- **DI**: Pass `ICoreAPI` via constructors, never as static fields
- **Events**: Subscribe via `api.Event`, unsubscribe in `Dispose()`
- **Async**: Methods suffixed `Async`, return `Task`/`Task<T>`, never `.Wait()` or `.Result`
- **Hot paths**: No `new` allocations in `OnGameTick`; reuse `Vec3d`, `BlockPos` with `.Set()`
- **Bulk world edits**: Call `IBlockAccessor.Commit()` after a batch, not per-block

## C# 13 Features to Use

- Primary constructors for system classes
- Pattern matching for entity/faction logic
- Collection expressions `[item1, item2]`

---

## Key API Classes

### Essential Namespaces

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
```

### Core Classes

- `Entity` — base for all entities
- `EntityAgent` — AI entities (NPCs, mobs) ← use for characters
- `EntityPlayer` — player
- `EntityBehavior` — behavior component (JSON or C#)
- `AiTask` — single AI task (wander, attack, talk, patrol)
- `IBlockAccessor` — read/write blocks
- `IWorldAccessor` — full world access
- `BlockSchematic` — load and place building schematics
- `BlockPos` — integer block position (x, y, z)
- `Vec3d` — double precision 3D vector

---

## JSON Entity Template

File: `assets/lotr/entities/humanoid/[name].json`

```json
{
  "code": "lotr:[name]",
  "class": "EntityAgent",
  "hitboxSize": { "x": 0.6, "y": 1.9 },
  "deadHitboxSize": { "x": 0.6, "y": 0.5 },
  "eyeHeight": 1.7,
  "client": {
    "renderer": "Shape",
    "shape": { "base": "lotr:entity/humanoid/[name]" },
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
          { "code": "lotr:questgiver", "priority": 1.5 },
          { "code": "lookatentity", "entityCodes": ["player"], "priority": 0.8 },
          { "code": "wander", "priority": 0.5, "movespeed": 0.015 }
        ]
      }
    ]
  },
  "sounds": {
    "hurt": "lotr:entity/[name]-hurt",
    "death": "lotr:entity/[name]-death"
  },
  "attributes": {
    "faction": "lotr:faction-[faction]",
    "dialogue": "lotr:dialogue-[name]"
  }
}
```

## C# Entity Template

File: `src/Entities/Humanoids/Entity[Name].cs`

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Lotr.Entities.Humanoids
{
    public class Entity[Name] : EntityAgent
    {
        public override void Initialize(
            EntityProperties properties,
            ICoreAPI api,
            long InChunkIndex3d)
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

## System Template

File: `src/Systems/[Name]/[Name]System.cs`

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Systems.[Name]
{
    public class [Name]System
    {
        private readonly ICoreServerAPI _api;

        public [Name]System(ICoreServerAPI api)
        {
            _api = api;
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _api.Event.PlayerJoin += OnPlayerJoin;
            _api.Event.GameWorldSave += OnWorldSave;
        }

        private void OnPlayerJoin(IServerPlayer player) { }
        private void OnWorldSave() { }
    }
}
```

## Starter Files

### modinfo.json

```json
{
  "modid": "lotr",
  "name": "Lord of the Rings — Middle Earth",
  "description": "Brings Middle-earth to Vintage Story. Explore the Shire, Moria, Minas Tirith and Isengard.",
  "authors": ["YourName"],
  "contributors": [],
  "version": "0.1.0",
  "type": "code",
  "website": "https://github.com/yourusername/lotr",
  "dependencies": {
    "game": "1.22.0",
    "survival": "1.22.0"
  }
}
```

### src/LotrModSystem.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Lotr.Constants;

namespace Lotr
{
    public class LotrModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification($"{LotrConstants.LogPrefix} Initializing Middle-earth...");

            // Phase 1: register entity classes
            // api.RegisterEntityClass("EntityHobbit", typeof(EntityHobbit));

            // Phase 1: register AI tasks
            // api.RegisterAITask("lotr:questgiver", typeof(AiTaskQuestGiver));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            // Phase 2: AlignmentSystem, FactionSystem, QuestSystem, DialogueSystem
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }
    }
}
```

### src/Entities/Humanoids/EntityHobbit.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Lotr.Constants;

namespace Lotr.Entities.Humanoids
{
    public class EntityHobbit : EntityAgent
    {
        public override void Initialize(
            EntityProperties properties,
            ICoreAPI api,
            long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);
            api.Logger.Debug($"{LotrConstants.LogPrefix} Hobbit entity initialized: {Code}");
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
        }
    }
}
```

### assets/lotr/entities/humanoid/frodo.json

```json
{
  "code": "lotr:frodo",
  "class": "EntityHobbit",
  "hitboxSize": { "x": 0.4, "y": 1.1 },
  "deadHitboxSize": { "x": 0.4, "y": 0.4 },
  "eyeHeight": 0.95,
  "client": {
    "renderer": "Shape",
    "shape": { "base": "lotr:entity/humanoid/hobbit" },
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
          { "code": "lookatentity", "entityCodes": ["player"], "priority": 0.8 },
          { "code": "wander", "priority": 0.5, "movespeed": 0.015 }
        ]
      }
    ]
  },
  "attributes": {
    "faction": "lotr:faction-shire"
  }
}
```

---

## Known Issues

- VS 1.22 on macOS Metal: occasional crash exit 139 on new world start (not mod-related)
- `seraph-faceless` shape requires skinnable system → entity files temporarily in `disabled/`
- `DefaultSpawnPosition` unavailable on `SaveGameLoaded` → NRE → read only in `PlayerJoin`
- `System.Drawing.Common` unavailable in VS context → manual PNG decoder

---

## Reference Links

- API Docs: <https://apidocs.vintagestory.at/>
- Modding Wiki: <https://wiki.vintagestory.at/Modding>
- Game source (vssurvivalmod): <https://github.com/anegostudios/vssurvivalmod>
- Mod template: <https://github.com/anegostudios/vsmodtemplate>
- Entity tutorial: <https://wiki.vintagestory.at/Modding:Content_Tutorial_Entity_Creation_Part_1>
