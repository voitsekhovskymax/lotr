using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Provides queries for NPC aggro logic based on player alignment.
// AiTask implementations call ShouldBeHostile() to decide whether to attack the player.
// Also provides TradeAllowed() for merchant NPCs.
public class FactionAggroSystem
{
    private readonly AlignmentSystem _alignment;
    private readonly DisguiseSystem  _disguise;

    public FactionAggroSystem(AlignmentSystem alignment, DisguiseSystem disguise)
    {
        _alignment = alignment;
        _disguise  = disguise;
    }

    // Returns true if an NPC of the given faction should attack the given player.
    public bool ShouldBeHostile(string npcFactionId, IServerPlayer player)
    {
        // Check disguise — if player is disguised as this faction, they may bypass aggro
        if (_disguise.IsDisguisedAs(player, npcFactionId)) return false;

        int score = _alignment.GetAlignment(player, npcFactionId);
        return FactionRelationMatrix.IsHostile(npcFactionId, score);
    }

    // Returns true if a trader NPC of the given faction will trade with the player.
    public bool TradeAllowed(string npcFactionId, IServerPlayer player)
    {
        int score = _alignment.GetAlignment(player, npcFactionId);
        return score >= LotrConstants.Alignment.TradingMin;
    }

    // Returns the dialogue attitude (friendly/neutral/hostile) for NPC greeting tone.
    public NpcAttitude GetAttitude(string npcFactionId, IServerPlayer player)
    {
        if (_disguise.IsDisguisedAs(player, npcFactionId)) return NpcAttitude.Neutral;

        int score = _alignment.GetAlignment(player, npcFactionId);
        var tier  = _alignment.GetTier(player, npcFactionId);

        return tier switch
        {
            AlignmentTier.Exalted or AlignmentTier.Revered => NpcAttitude.VeryFriendly,
            AlignmentTier.Honored or AlignmentTier.Friendly => NpcAttitude.Friendly,
            AlignmentTier.Unfriendly => NpcAttitude.Neutral,
            AlignmentTier.Hostile or AlignmentTier.Hated => NpcAttitude.Hostile,
            AlignmentTier.Nemesis => NpcAttitude.VeryHostile,
            _ => NpcAttitude.Neutral
        };
    }
}

public enum NpcAttitude { VeryFriendly, Friendly, Neutral, Hostile, VeryHostile }
