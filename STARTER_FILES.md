# Стартові файли мода — референс

> УВАГА: Цей файл є довідковим матеріалом. Актуальна реалізація живе в `src/`.
> Всі приклади нижче вже нормалізовані до mod id `lotr` та namespace `Lotr`.

---

## modinfo.json

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

---

## src/LotrModSystem.cs

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

---

## src/Entities/Humanoids/EntityHobbit.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Lotr.Constants;

namespace Lotr.Entities.Humanoids
{
    /// <summary>
    /// Base class for Hobbit NPCs (Frodo, Bilbo, Samwise, etc.)
    /// Hobbits are smaller than humans: hitbox 0.4 x 1.1
    /// </summary>
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

---

## assets/lotr/entities/humanoid/frodo.json

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

## src/Systems/Alignment/AlignmentSystem.cs (шаблон для Phase 2)

```csharp
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Lotr.Constants;

namespace Lotr.Systems.Alignment
{
    public class AlignmentSystem
    {
        private readonly ICoreServerAPI _api;

        public AlignmentSystem(ICoreServerAPI api)
        {
            _api = api;
            _api.Event.PlayerJoin += OnPlayerJoin;
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            foreach (var faction in LotrConstants.Factions.All)
            {
                var key = LotrConstants.SaveKeys.AlignmentPrefix + faction;
                if (!player.WorldData.EntityPlayer.WatchedAttributes.HasAttribute(key))
                    SetAlignment(player, faction, GetDefaultAlignment(faction));
            }
        }

        private int GetDefaultAlignment(string faction) => faction switch
        {
            LotrConstants.Factions.Shire    =>  200,
            LotrConstants.Factions.Gondor   =>    0,
            LotrConstants.Factions.Rohan    =>    0,
            LotrConstants.Factions.Rivendell =>   0,
            LotrConstants.Factions.Moria    =>    0,
            LotrConstants.Factions.Isengard => -500,
            LotrConstants.Factions.Mordor   => -800,
            _                               =>    0
        };

        public int GetAlignment(IServerPlayer player, string faction)
        {
            var key = LotrConstants.SaveKeys.AlignmentPrefix + faction;
            return player.WorldData.EntityPlayer.WatchedAttributes.GetInt(key, 0);
        }

        public void SetAlignment(IServerPlayer player, string faction, int value)
        {
            var key = LotrConstants.SaveKeys.AlignmentPrefix + faction;
            var clamped = System.Math.Clamp(value, LotrConstants.Alignment.Min, LotrConstants.Alignment.Max);
            player.WorldData.EntityPlayer.WatchedAttributes.SetInt(key, clamped);
        }

        public void ModifyAlignment(IServerPlayer player, string faction, int delta)
        {
            var current = GetAlignment(player, faction);
            SetAlignment(player, faction, current + delta);
            _api.Logger.Debug($"{LotrConstants.LogPrefix} {player.PlayerName} alignment {faction}: {current} -> {current + delta}");
        }

        public string GetAlignmentLabel(int value) => value switch
        {
            >= LotrConstants.Alignment.Exalted    => "Exalted",
            >= LotrConstants.Alignment.Revered    => "Revered",
            >= LotrConstants.Alignment.Honored    => "Honored",
            >= LotrConstants.Alignment.Friendly   => "Friendly",
            >= LotrConstants.Alignment.Unfriendly => "Unfriendly",
            >= LotrConstants.Alignment.Hostile    => "Hostile",
            >= LotrConstants.Alignment.Hated      => "Hated",
            _                                     => "Nemesis"
        };
    }
}
```
