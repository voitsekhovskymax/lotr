using System.Collections.Generic;
using Lotr.Constants;
using Lotr.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Lotr.Systems.NpcMorale;

// Server-side morale system for LOTR NPCs.
//
// Each faction tracks group morale in a region. When enough allies die the
// morale breaks and surviving members are flagged to flee via the
// "lotr:fleeing" entity attribute, which the built-in VS "flee" ai-task reads.
//
// Individual flee threshold: if a LOTR NPC drops below hpFleeThreshold (0–1)
// of its max HP it will also start fleeing regardless of group morale.
//
// Integration:
//   Add to LotrModSystem.StartServerSide:
//     Morale = new NpcMoraleSystem(api);
//   Wire kill event:
//     EntityRaceBase.Killed += (e, dmg) => Morale.OnEntityKilled(e, dmg);
public class NpcMoraleSystem
{
    private const float HpFleeThreshold   = 0.20f; // < 20 % HP → flee
    private const float GroupMoraleBreak  = 0.40f; // morale breaks when < 40 % of group alive
    private const float FleeRecoverySecs  = 30f;   // entity stops fleeing after 30 s

    // factionId → (aliveCount, totalStartCount)
    private readonly Dictionary<string, GroupMoraleRecord> _groupMorale = new();
    private readonly ICoreServerAPI _api;

    public NpcMoraleSystem(ICoreServerAPI api)
    {
        _api = api;
        api.Event.RegisterGameTickListener(OnTick, 3000); // tick every 3 s
        api.Logger.Notification($"{LotrConstants.LogPrefix} NpcMoraleSystem started.");
    }

    // Call from LotrModSystem after wiring EntityRaceBase.Killed
    public void OnEntityKilled(Entity entity, DamageSource? _)
    {
        if (entity is not EntityRaceBase npc) return;
        string? factionId = GetFactionId(npc);
        if (factionId == null) return;

        if (!_groupMorale.TryGetValue(factionId, out var record)) return;
        record.AliveCount = System.Math.Max(0, record.AliveCount - 1);

        float ratio = record.TotalCount == 0 ? 1f : (float)record.AliveCount / record.TotalCount;
        if (ratio < GroupMoraleBreak)
            TriggerGroupFlee(factionId, npc.Pos.XYZ);
    }

    // Called when a new LOTR entity spawns — register with group
    public void OnEntitySpawn(EntityRaceBase entity)
    {
        string? factionId = GetFactionId(entity);
        if (factionId == null) return;

        if (!_groupMorale.TryGetValue(factionId, out var record))
        {
            record = new GroupMoraleRecord();
            _groupMorale[factionId] = record;
        }
        record.TotalCount++;
        record.AliveCount++;
    }

    // Called from EntityRaceBase.ReceiveDamage (wire up manually)
    public void OnEntityDamaged(EntityRaceBase entity, float damage)
    {
        var health = entity.GetBehavior<EntityBehaviorHealth>();
        if (health == null) return;

        float ratio = health.Health / health.MaxHealth;
        if (ratio < HpFleeThreshold && !entity.Attributes.GetBool("lotr:fleeing", false))
            SetFleeing(entity, true);
    }

    // ── Periodic tick ────────────────────────────────────────────────────────

    private void OnTick(float dt)
    {
        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var e in _api.World.LoadedEntities.Values)
        {
            if (e is not EntityRaceBase npc) continue;
            if (!npc.Attributes.GetBool("lotr:fleeing", false)) continue;

            long startedAt = npc.Attributes.GetLong("lotr:fleeStart", 0);
            if (startedAt > 0 && now - startedAt > FleeRecoverySecs)
                SetFleeing(npc, false);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void TriggerGroupFlee(string factionId, Vec3d epicentre)
    {
        _api.World.GetEntitiesAround(epicentre, 40f, 20f, e =>
        {
            if (e is not EntityRaceBase npc) return false;
            if (GetFactionId(npc) != factionId) return false;
            if (npc.Attributes.GetBool("lotr:fleeing", false)) return false;

            SetFleeing(npc, true);
            return false;
        });
    }

    private static void SetFleeing(EntityRaceBase entity, bool flee)
    {
        entity.Attributes.SetBool("lotr:fleeing", flee);
        if (flee)
            entity.Attributes.SetLong("lotr:fleeStart",
                System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        else
            entity.Attributes.SetLong("lotr:fleeStart", 0);
    }

    private static string? GetFactionId(Entity entity)
        => entity.Properties.Attributes?["faction"].AsString();

    private sealed class GroupMoraleRecord
    {
        public int TotalCount;
        public int AliveCount;
    }
}
