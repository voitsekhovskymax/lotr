using System;
using System.Collections.Generic;
using Lotr.Constants;
using Lotr.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Lotr.Systems.FactionGuardAlert;

// Server-side system: when a player attacks or kills a faction entity,
// all guard NPCs of the same faction within alertRadius blocks are
// notified and set to combat-aware mode via the "lotr:inCombat" attribute.
//
// Guards remain alerted for alertDuration seconds, then calm down if the
// aggressor leaves the area.
public class FactionGuardAlertSystem
{
    private const float AlertRadius   = 30f;
    private const float AlertDuration = 60f; // seconds

    // factionId → list of (alertExpiry, playerUid)
    private readonly Dictionary<string, List<AlertRecord>> _activeAlerts = new();
    private readonly ICoreServerAPI _api;

    public FactionGuardAlertSystem(ICoreServerAPI api)
    {
        _api = api;
        EntityRaceBase.Killed      += OnLotrEntityKilled;
        api.Event.RegisterGameTickListener(OnTick, 5000); // every 5 s
        api.Logger.Notification($"{LotrConstants.LogPrefix} FactionGuardAlertSystem started.");
    }

    // Called when any player damages a LOTR entity (faction guard).
    // Wire this up from CombatAlignmentHandler or entity OnEntityReceiveDamage.
    public void OnFactionMemberAttacked(Entity victim, IServerPlayer attacker)
    {
        string? factionId = GetFactionId(victim);
        if (factionId == null) return;

        AlertNearbyGuards(factionId, victim.Pos.XYZ, attacker);
    }

    private void OnLotrEntityKilled(Entity victim, DamageSource? damage)
    {
        if (damage?.SourceEntity is not EntityPlayer playerEntity) return;

        string? factionId = GetFactionId(victim);
        if (factionId == null) return;

        if (playerEntity.Player is not IServerPlayer serverPlayer) return;

        AlertNearbyGuards(factionId, victim.Pos.XYZ, serverPlayer);
    }

    private void AlertNearbyGuards(string factionId, Vec3d epicentre, IServerPlayer aggressor)
    {
        var expiry = (long)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + AlertDuration);

        if (!_activeAlerts.TryGetValue(factionId, out var list))
        {
            list = new List<AlertRecord>();
            _activeAlerts[factionId] = list;
        }
        list.Add(new AlertRecord(epicentre, aggressor.PlayerUID, expiry));

        // Stamp nearby guards with combat attribute
        _api.World.GetEntitiesAround(epicentre, AlertRadius, AlertRadius, entity =>
        {
            if (entity is not EntityRaceBase npc) return false;
            if (GetFactionId(npc) != factionId) return false;

            npc.Attributes.SetBool("lotr:inCombat", true);
            npc.Attributes.SetString("lotr:aggroTarget", aggressor.PlayerUID);
            npc.Attributes.SetLong("lotr:combatExpiry", expiry);

            _api.Logger.VerboseDebug(
                $"{LotrConstants.LogPrefix} Guard {npc.Code} alerted — faction {factionId} — aggressor {aggressor.PlayerName}");
            return false;
        });
    }

    // Periodic tick: clear expired alerts and calm down guards whose timer elapsed.
    private void OnTick(float dt)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        foreach (var (factionId, records) in _activeAlerts)
            records.RemoveAll(r => r.Expiry < now);

        // Calm guards whose combat expiry has passed
        foreach (var chunk in _api.World.LoadedEntities.Values)
        {
            if (chunk is not EntityRaceBase npc) continue;
            long expiry = npc.Attributes.GetLong("lotr:combatExpiry", 0);
            if (expiry > 0 && expiry < now)
            {
                npc.Attributes.SetBool("lotr:inCombat", false);
                npc.Attributes.SetString("lotr:aggroTarget", "");
                npc.Attributes.SetLong("lotr:combatExpiry", 0);
            }
        }
    }

    private static string? GetFactionId(Entity entity)
        => entity.Properties.Attributes?["faction"].AsString();

    private sealed record AlertRecord(Vec3d Epicentre, string AggressorUid, long Expiry);
}
