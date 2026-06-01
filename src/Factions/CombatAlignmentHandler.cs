using System;
using Lotr.Constants;
using Lotr.Entities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Processes entity kill events and applies alignment changes based on the killed entity's faction.
// EntityRaceBase.Die() calls LotrModSystem.OnEntityKilled, which routes here.
public class CombatAlignmentHandler
{
    private readonly ICoreServerAPI _sapi;
    private readonly AlignmentSystem _alignment;

    public CombatAlignmentHandler(ICoreServerAPI sapi, AlignmentSystem alignment)
    {
        _sapi      = sapi;
        _alignment = alignment;
    }

    // Called by EntityRaceBase.Die() on the server side
    public void OnEntityKilled(Entity killedEntity, DamageSource? damageSource)
    {
        if (killedEntity is not EntityRaceBase lotrEntity) return;
        if (lotrEntity.FactionId == null) return;

        // Identify the killer player
        IServerPlayer? killer = GetKillerPlayer(damageSource);
        if (killer == null) return;

        ApplyKillRewards(killer, lotrEntity.FactionId);
        NotifyPlayer(killer, lotrEntity.FactionId);
    }

    private void ApplyKillRewards(IServerPlayer player, string killedFactionId)
    {
        if (!FactionRelationMatrix.KillRewards.TryGetValue(killedFactionId, out var rewards))
            return;

        foreach (var (factionId, delta) in rewards)
            _alignment.ModifyAlignment(player, factionId, delta);
    }

    private void NotifyPlayer(IServerPlayer player, string killedFactionId)
    {
        if (!FactionRelationMatrix.KillRewards.TryGetValue(killedFactionId, out var rewards))
            return;

        // Find the primary faction loss (largest negative)
        int primaryLoss = 0;
        string primaryFaction = killedFactionId;
        int totalGain = 0;

        foreach (var (fid, delta) in rewards)
        {
            if (delta < primaryLoss) { primaryLoss = delta; primaryFaction = fid; }
            if (delta > 0) totalGain += delta;
        }

        if (totalGain > 0)
        {
            string shortName = killedFactionId.Replace("lotr:faction-", "");
            player.SendMessage(0, $"[LOTR] Killed {shortName}: +{totalGain} rep with allies", EnumChatType.Notification);
        }
    }

    private static IServerPlayer? GetKillerPlayer(DamageSource? damageSource)
    {
        if (damageSource == null) return null;

        // Direct melee: source is the player entity
        if (damageSource.SourceEntity is EntityPlayer playerEntity)
            return playerEntity.Player as IServerPlayer;

        // Projectile: source entity could be a projectile, check its firer
        if (damageSource.CauseEntity is EntityPlayer causePlayer)
            return causePlayer.Player as IServerPlayer;

        return null;
    }
}
