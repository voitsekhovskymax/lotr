---
name: vs-api
description: >
  Vintage Story 1.22 modding API. Use when writing any C# code
  for the LOTR mod — entity classes, AI tasks, world generation,
  block/item definitions, or JSON assets.
---

## Critical Rules
- Never call client API from server side and vice versa
- Always check `api.Side` before side-specific operations
- Register entity classes in `Start()`, not `StartServerSide()`
- Use `RegisterGameTickListener` instead of `Thread.Sleep()`
- All asset codes use prefix `lotrmod:` (e.g. `lotrmod:gandalf`)

## Key Namespaces
```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
```

## Entity Sizes (hitboxSize)
- Hobbit:  x:0.4  y:1.1  eyeHeight:0.95
- Dwarf:   x:0.5  y:1.4  eyeHeight:1.2
- Human:   x:0.6  y:1.9  eyeHeight:1.7
- Elf:     x:0.55 y:2.1  eyeHeight:1.95
- Troll:   x:1.5  y:4.0  eyeHeight:3.5

## JSON Asset Locations
- entities:   assets/lotrmod/entities/humanoid/
- blocktypes: assets/lotrmod/blocktypes/
- itemtypes:  assets/lotrmod/itemtypes/
- worldgen:   assets/lotrmod/worldgen/structures/
- lang:       assets/lotrmod/lang/en.json