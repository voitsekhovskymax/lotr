# LOTR Mod — AI Agent Context

# Vintage Story v1.22 | .NET 10 | C# 13

## PROJECT OVERVIEW

This is a Lord of the Rings mod for the game Vintage Story (voxel survival game written in C#).
The mod adds Middle-earth locations, characters, quests, and factions.

Key locations: Shire, Moria, Minas Tirith, Isengard, Mordor
Key characters: Gandalf, Frodo, Bilbo, Sauron, Aragorn
Systems: Factions, Alignment, Quests, World Generation

---

## TECH STACK

- Game: Vintage Story 1.22
- Runtime: .NET 10
- Language: C# 13
- Build: `dotnet build lotr.csproj -c Debug`
- IDE: JetBrains Rider on macOS
- Game path: /Applications/Vintage Story.app
- Data path: ~/Library/Application Support/VintagestoryData/

---

## PROJECT STRUCTURE

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

---

## KEY API CLASSES

### Entry Point

```csharp
public class LotrModSystem : ModSystem
{
    public override void Start(ICoreAPI api) { }
    public override void StartServerSide(ICoreServerAPI api) { }
    public override void StartClientSide(ICoreClientAPI api) { }
}
```

### Essential Namespaces

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
```

### Entity Classes

- `Entity` — base class for all entities
- `EntityAgent` — entities with AI (NPCs, mobs) ← use this for characters
- `EntityPlayer` — player
- `EntityBehavior` — behavior component (added via JSON or C#)
- `AiTask` — single AI task (wander, attack, talk, patrol)

### World Access

- `IBlockAccessor` — read/write blocks in the world
- `IWorldAccessor` — full world access
- `BlockSchematic` — load and place building schematics
- `IChunkColumnGenerateRequest` — chunk generation callback

### Math / Position

- `BlockPos` — integer block position (x, y, z)
- `Vec3d` — double precision 3D vector
- `Vec2f` — 2D float (used for hitbox sizes)

---

## ENTITY SIZES (hitboxSize)

All sizes are in blocks (1 block = 1 meter).

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

## WORLD DIMENSIONS

- Default world height: 256 blocks, sea level: ~110
- LOTR mod world height: 512 blocks, sea level: ~120
- All structures are built ABOVE sea level (y > 120)

Building heights from sea level (y=120):

```
Hobbit hole:        y 120-125  (5 blocks tall)
Edoras city:        y 120-150  (30 blocks)
Minas Tirith:       y 120-200  (80 blocks, 7 tiers)
Orthanc tower:      y 120-240  (120 blocks)
Barad-dur:          y 120-300  (180 blocks)
Mount Doom peak:    y 120-350  (230 blocks)
```

---

## FACTION SYSTEM

Each region belongs to a faction. Player gains/loses alignment through actions.

```
Factions:
  lotr:faction-shire      (Hobbits)       — friendly default
  lotr:faction-gondor     (Humans)        — neutral
  lotr:faction-rohan      (Rohirrim)      — neutral
  lotr:faction-rivendell  (Elves)         — neutral
  lotr:faction-moria      (Dwarves)       — neutral
  lotr:faction-isengard   (Uruk-hai)      — hostile
  lotr:faction-mordor     (Orcs/Sauron)   — hostile
```

Alignment range: -1000 (hated) to +1000 (exalted)
Stored per-player via SaveGame attributes.

---

## NAMING CONVENTIONS

- ModID: `lotr` (always lowercase)
- Namespace: `Lotr`
- Entity codes: `lotr:gandalf`, `lotr:frodo`, `lotr:orc-mordor`
- Block codes: `lotr:stone-minas-tirith`, `lotr:block-mithril`
- Item codes: `lotr:sword-sting`, `lotr:lembas`, `lotr:one-ring`
- Texture paths: `lotr:entity/humanoid/gandalf`, `lotr:block/mithril`
- JSON asset paths: `assets/lotr/entities/humanoid/gandalf.json`

---

## CRITICAL RULES

1. Always check `api.Side` before side-specific code
2. Server code MUST NOT call client APIs and vice versa
3. Use `api.Logger.Notification("[LOTR] message")` for debug logging
4. Never use `Thread.Sleep()` — use `RegisterGameTickListener` instead
5. Never store `ICoreAPI` as a static field — pass via constructor
6. Entity registration happens in `Start()`, not `StartServerSide()`
7. JSON entity code must match the registered class name exactly
8. All entity/block/item codes need the `lotr:` prefix

---

## JSON ENTITY TEMPLATE

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

---

## C# ENTITY TEMPLATE

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

---

## SYSTEM TEMPLATE

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

---

## REFERENCE LINKS

- API Docs: <https://apidocs.vintagestory.at/>
- Modding Wiki: <https://wiki.vintagestory.at/Modding>
- Game source (vssurvivalmod): <https://github.com/anegostudios/vssurvivalmod>
- Mod template: <https://github.com/anegostudios/vsmodtemplate>
- Entity modding tutorial: <https://wiki.vintagestory.at/Modding:Content_Tutorial_Entity_Creation_Part_1>
