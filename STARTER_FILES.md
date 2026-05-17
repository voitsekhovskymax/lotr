# Стартові файли мода — копіюй в проект

## Файл 1: lotrmod/modinfo.json

```json
{
  "modid": "lotrmod",
  "name": "Lord of the Rings — Middle Earth",
  "description": "Brings Middle-earth to Vintage Story. Explore the Shire, Moria, Minas Tirith and Isengard.",
  "authors": ["YourName"],
  "contributors": [],
  "version": "0.1.0",
  "type": "code",
  "website": "https://github.com/yourusername/lotrmod",
  "dependencies": {
    "game": "1.22.0",
    "survival": "1.22.0"
  }
}
```

---

## Файл 2: src/LotrModSystem.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace LotrMod
{
    /// <summary>
    /// Main entry point for the LOTR Middle-earth mod.
    /// Handles initialization of all systems and entity registration.
    /// </summary>
    public class LotrModSystem : ModSystem
    {
        private AlignmentSystem? _alignmentSystem;
        private QuestSystem? _questSystem;
        private FactionSystem? _factionSystem;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[LOTR Mod] Initializing Middle-earth v0.1.0...");

            // Register entity classes (both sides)
            api.RegisterEntityClass("EntityFrodo",   typeof(EntityHobbit));
            api.RegisterEntityClass("EntityBilbo",   typeof(EntityHobbit));
            api.RegisterEntityClass("EntityGandalf", typeof(EntityGandalf));

            // Register AI tasks
            api.RegisterAITask("lotrmod:questgiver", typeof(AiTaskQuestGiver));
            api.RegisterAITask("lotrmod:patrol",     typeof(AiTaskPatrol));

            api.Logger.Notification("[LOTR Mod] Entities and AI tasks registered.");
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            _alignmentSystem = new AlignmentSystem(api);
            _questSystem     = new QuestSystem(api);
            _factionSystem   = new FactionSystem(api);

            api.Logger.Notification("[LOTR Mod] Server systems initialized.");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            // HUD, UI elements — to be added later
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
```

---

## Файл 3: src/entities/EntityHobbit.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace LotrMod
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
            api.Logger.Debug($"[LOTR Mod] Hobbit entity initialized: {Code}");
        }

        public override void OnGameTick(float dt)
        {
            base.OnGameTick(dt);
        }
    }
}
```

---

## Файл 4: src/entities/EntityGandalf.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace LotrMod
{
    /// <summary>
    /// Gandalf the Grey / White — quest giver and guide.
    /// Wanders between key locations. Gives main story quests.
    /// Hitbox: 0.6 x 2.0 (slightly taller than a human)
    /// </summary>
    public class EntityGandalf : EntityAgent
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

        /// <summary>
        /// Gandalf cannot be killed by normal means (placeholder logic).
        /// </summary>
        public override bool ShouldReceiveDamage(DamageSource damageSource, float damage)
        {
            // TODO: Make Gandalf immune to orc weapons
            return base.ShouldReceiveDamage(damageSource, damage);
        }
    }
}
```

---

## Файл 5: src/entities/ai/AiTaskQuestGiver.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace LotrMod
{
    /// <summary>
    /// AI Task: NPC looks at nearby players and can offer quests.
    /// Priority should be higher than wander (1.5 recommended).
    /// </summary>
    public class AiTaskQuestGiver : AiTaskBase
    {
        private IServerPlayer? _targetPlayer;
        private float _checkCooldown = 0f;
        private const float CheckInterval = 2f; // seconds between checks

        public AiTaskQuestGiver(EntityAgent entity) : base(entity) { }

        public override bool ShouldExecute()
        {
            // Only run on server
            if (entity.Api.Side != EnumAppSide.Server) return false;

            _checkCooldown -= 0.05f; // approximate dt
            if (_checkCooldown > 0) return false;
            _checkCooldown = CheckInterval;

            // Look for nearby player
            var nearestPlayer = entity.Api.World.GetNearestEntity(
                entity.Pos.XYZ, 3f, 3f,
                e => e is EntityPlayer
            ) as EntityPlayer;

            if (nearestPlayer == null) return false;

            _targetPlayer = (nearestPlayer.Player as IServerPlayer)!;
            return true;
        }

        public override void StartExecute()
        {
            base.StartExecute();
            // TODO: Open dialogue with _targetPlayer
            // For now just face the player
        }

        public override bool ContinueExecute(float dt)
        {
            return false; // Run once then stop
        }
    }
}
```

---

## Файл 6: src/entities/ai/AiTaskPatrol.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using System.Collections.Generic;

namespace LotrMod
{
    /// <summary>
    /// AI Task: NPC patrols between defined waypoints.
    /// Waypoints are defined in entity JSON under attributes.patrolPoints
    /// </summary>
    public class AiTaskPatrol : AiTaskBase
    {
        private List<Vec3d> _waypoints = new();
        private int _currentWaypoint = 0;
        private float _moveSpeed = 0.02f;

        public AiTaskPatrol(EntityAgent entity) : base(entity) { }

        public override void LoadConfig(
            Newtonsoft.Json.Linq.JToken taskConfig,
            Newtonsoft.Json.JsonSerializer deserializer)
        {
            base.LoadConfig(taskConfig, deserializer);
            _moveSpeed = taskConfig["movespeed"]?.ToObject<float>() ?? 0.02f;
            // Waypoints loaded from JSON attributes at runtime
        }

        public override bool ShouldExecute()
        {
            return _waypoints.Count > 0;
        }

        public override bool ContinueExecute(float dt)
        {
            if (_waypoints.Count == 0) return false;

            var target = _waypoints[_currentWaypoint];
            var distSq = entity.Pos.SquareDistanceTo(target.X, target.Y, target.Z);

            if (distSq < 0.5)
            {
                // Reached waypoint, move to next
                _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Count;
            }

            return true;
        }
    }
}
```

---

## Файл 7: src/systems/AlignmentSystem.cs

```csharp
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace LotrMod
{
    /// <summary>
    /// Tracks player alignment (reputation) with each faction.
    /// Range: -1000 (hated) to +1000 (exalted).
    /// Stored in player save data.
    /// </summary>
    public class AlignmentSystem
    {
        private readonly ICoreServerAPI _api;

        // Faction IDs
        public static readonly string[] AllFactions =
        {
            "lotrmod:faction-shire",
            "lotrmod:faction-gondor",
            "lotrmod:faction-rohan",
            "lotrmod:faction-rivendell",
            "lotrmod:faction-moria",
            "lotrmod:faction-isengard",
            "lotrmod:faction-mordor"
        };

        public AlignmentSystem(ICoreServerAPI api)
        {
            _api = api;
            _api.Event.PlayerJoin += OnPlayerJoin;
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // Initialize alignment data for new players
            foreach (var faction in AllFactions)
            {
                var key = $"lotrmod_alignment_{faction}";
                if (!player.WorldData.EntityPlayer.WatchedAttributes.HasAttribute(key))
                {
                    SetAlignment(player, faction, GetDefaultAlignment(faction));
                }
            }
        }

        private int GetDefaultAlignment(string faction)
        {
            return faction switch
            {
                "lotrmod:faction-shire"     =>  200, // Friendly start
                "lotrmod:faction-gondor"    =>    0,
                "lotrmod:faction-rohan"     =>    0,
                "lotrmod:faction-rivendell" =>    0,
                "lotrmod:faction-moria"     =>    0,
                "lotrmod:faction-isengard"  => -500, // Hostile
                "lotrmod:faction-mordor"    => -800, // Very hostile
                _                           =>    0
            };
        }

        public int GetAlignment(IServerPlayer player, string faction)
        {
            var key = $"lotrmod_alignment_{faction}";
            return player.WorldData.EntityPlayer.WatchedAttributes
                .GetInt(key, 0);
        }

        public void SetAlignment(IServerPlayer player, string faction, int value)
        {
            var key = $"lotrmod_alignment_{faction}";
            var clamped = System.Math.Clamp(value, -1000, 1000);
            player.WorldData.EntityPlayer.WatchedAttributes
                .SetInt(key, clamped);
        }

        public void ModifyAlignment(IServerPlayer player, string faction, int delta)
        {
            var current = GetAlignment(player, faction);
            SetAlignment(player, faction, current + delta);

            _api.Logger.Debug(
                $"[LOTR] {player.PlayerName} alignment with {faction}: " +
                $"{current} -> {current + delta}");
        }

        public string GetAlignmentLabel(int value) => value switch
        {
            >= 750  => "Exalted",
            >= 500  => "Revered",
            >= 250  => "Honored",
            >= 0    => "Friendly",
            >= -250 => "Unfriendly",
            >= -500 => "Hostile",
            >= -750 => "Hated",
            _       => "Nemesis"
        };
    }
}
```

---

## Файл 8: src/systems/QuestSystem.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace LotrMod
{
    /// <summary>
    /// Quest system — tracks active and completed quests per player.
    /// Quest data stored in player attributes.
    /// Phase 1: placeholder structure only.
    /// </summary>
    public class QuestSystem
    {
        private readonly ICoreServerAPI _api;

        public QuestSystem(ICoreServerAPI api)
        {
            _api = api;
            _api.Event.PlayerJoin += OnPlayerJoin;
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            // TODO: Load quest state for player
        }

        public bool HasCompletedQuest(IServerPlayer player, string questId)
        {
            var key = $"lotrmod_quest_done_{questId}";
            return player.WorldData.EntityPlayer.WatchedAttributes
                .GetBool(key, false);
        }

        public void CompleteQuest(IServerPlayer player, string questId)
        {
            var key = $"lotrmod_quest_done_{questId}";
            player.WorldData.EntityPlayer.WatchedAttributes
                .SetBool(key, true);

            _api.Logger.Notification(
                $"[LOTR] {player.PlayerName} completed quest: {questId}");
        }
    }
}
```

---

## Файл 9: src/systems/FactionSystem.cs

```csharp
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace LotrMod
{
    /// <summary>
    /// Faction system — determines NPC hostility based on player alignment.
    /// Ties together AlignmentSystem and entity AI behavior.
    /// </summary>
    public class FactionSystem
    {
        private readonly ICoreServerAPI _api;
        private readonly AlignmentSystem _alignment;

        public FactionSystem(ICoreServerAPI api)
        {
            _api = api;
            // AlignmentSystem is created by LotrModSystem and can be injected
            // For now create locally — refactor later
            _alignment = new AlignmentSystem(api);
        }

        public bool IsHostileTo(IServerPlayer player, string faction)
        {
            return _alignment.GetAlignment(player, faction) < -250;
        }

        public bool IsFriendlyTo(IServerPlayer player, string faction)
        {
            return _alignment.GetAlignment(player, faction) >= 250;
        }
    }
}
```

---

## Файл 10: assets/lotrmod/entities/humanoid/frodo.json

```json
{
  "code": "lotrmod:frodo",
  "class": "EntityFrodo",
  "hitboxSize": { "x": 0.4, "y": 1.1 },
  "deadHitboxSize": { "x": 0.4, "y": 0.4 },
  "eyeHeight": 0.95,
  "client": {
    "renderer": "Shape",
    "shape": { "base": "lotrmod:entity/humanoid/hobbit" },
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
      { "code": "health", "currenthealth": 15, "maxhealth": 15 },
      { "code": "deaddecay", "hoursToDecay": 48 },
      { "code": "floatupwhenstuck" },
      {
        "code": "taskai",
        "aitasks": [
          { "code": "lotrmod:questgiver", "priority": 1.5 },
          {
            "code": "lookatentity",
            "entityCodes": ["player"],
            "priority": 0.8,
            "mincooldown": 2000,
            "maxcooldown": 5000
          },
          {
            "code": "wander",
            "priority": 0.5,
            "movespeed": 0.012,
            "wanderChance": 0.02
          }
        ]
      }
    ],
    "spawnConditions": {
      "runtime": {
        "group": "lotrmod:hobbit",
        "maxQuantityByGroup": 5
      }
    }
  },
  "sounds": {},
  "attributes": {
    "faction": "lotrmod:faction-shire",
    "dialogue": "lotrmod:dialogue/frodo",
    "nametag": "Frodo Baggins"
  }
}
```

---

## Файл 11: assets/lotrmod/lang/en.json

```json
{
  "lotrmod:frodo": "Frodo Baggins",
  "lotrmod:bilbo": "Bilbo Baggins",
  "lotrmod:gandalf": "Gandalf the Grey",

  "lotrmod:faction-shire": "The Shire",
  "lotrmod:faction-gondor": "Gondor",
  "lotrmod:faction-rohan": "Rohan",
  "lotrmod:faction-rivendell": "Rivendell",
  "lotrmod:faction-moria": "Moria",
  "lotrmod:faction-isengard": "Isengard",
  "lotrmod:faction-mordor": "Mordor",

  "lotrmod:alignment-exalted": "Exalted",
  "lotrmod:alignment-revered": "Revered",
  "lotrmod:alignment-honored": "Honored",
  "lotrmod:alignment-friendly": "Friendly",
  "lotrmod:alignment-unfriendly": "Unfriendly",
  "lotrmod:alignment-hostile": "Hostile",
  "lotrmod:alignment-hated": "Hated",
  "lotrmod:alignment-nemesis": "Nemesis"
}
```
