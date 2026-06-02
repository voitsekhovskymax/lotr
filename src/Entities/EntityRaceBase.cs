using System;
using System.Collections.Generic;
using System.Reflection;
using Lotr.Constants;
using Lotr.Utilities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Lotr.Entities;

// Universal base class for all LOTR race entities: hobbits, humans, elves,
// dwarves, orcs, uruks, goblins, trolls, maiar, and creatures.
// Caches faction and race on first initialize — zero per-tick allocations.
public abstract class EntityRaceBase : EntityAgent
{
    // Fired server-side when a LOTR entity is killed.
    // Subscribed by CombatAlignmentHandler in LotrModSystem.
    public static event Action<Entity, DamageSource?>? Killed;

    public string? FactionId { get; private set; }
    public LotrRace Race { get; private set; }

    public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
    {
        base.Initialize(properties, api, InChunkIndex3d);

        FactionId = EntityModelCache.GetFaction(this);
        Race      = EntityModelCache.GetRace(this);

        if (api.Side == EnumAppSide.Server)
        {
            api.Logger.VerboseDebug($"{LotrConstants.LogPrefix} Spawned {Code} [{Race}] faction={FactionId ?? "none"} at {Pos.XYZ}");
            ProbeTaskAiRegistry(api);
        }
    }

    private static bool _probed;
    private void ProbeTaskAiRegistry(ICoreAPI api)
    {
        if (_probed) return;
        _probed = true;

        var behavior = GetBehavior<EntityBehaviorTaskAI>();
        if (behavior == null) { api.Logger.Notification($"{LotrConstants.LogPrefix} [probe] No EntityBehaviorTaskAI behavior on {Code}"); return; }

        var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        // Probe EntityBehaviorTaskAI fields to find TaskManager
        var tmField = behavior.GetType().GetField("TaskManager", flags);
        var tm = tmField?.GetValue(behavior);
        if (tm == null) { api.Logger.Notification($"{LotrConstants.LogPrefix} [probe] TaskManager is null"); return; }

        api.Logger.Notification($"{LotrConstants.LogPrefix} [probe] AiTaskManager type: {tm.GetType().FullName}");

        // Probe AiTaskManager instance + static fields
        foreach (var field in tm.GetType().GetFields(flags))
        {
            var val = field.GetValue(field.IsStatic ? null : tm);
            string typeName = val?.GetType().Name ?? "null";
            api.Logger.Notification($"{LotrConstants.LogPrefix} [probe] TM.field {field.Name} : {field.FieldType.Name} = {typeName}");
        }
        // Probe AiTaskManager properties
        foreach (var prop in tm.GetType().GetProperties(flags))
        {
            try
            {
                var val = prop.GetValue(tm);
                api.Logger.Notification($"{LotrConstants.LogPrefix} [probe] TM.prop  {prop.Name} : {prop.PropertyType.Name} = {val?.GetType().Name ?? "null"}");
            }
            catch { }
        }
    }

    public override void Die(EnumDespawnReason reason = EnumDespawnReason.Death, DamageSource? damageSourceInfo = null)
    {
        base.Die(reason, damageSourceInfo);

        // Fire kill event only on server, only for actual death (not despawn/unload)
        if (Api?.Side == EnumAppSide.Server && reason == EnumDespawnReason.Death)
            Killed?.Invoke(this, damageSourceInfo);
    }
}
