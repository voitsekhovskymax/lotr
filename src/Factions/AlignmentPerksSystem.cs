using Lotr.Constants;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Provides gameplay bonuses based on faction alignment tier.
// Other systems (trading, combat, movement) query this for dynamic bonuses.
public class AlignmentPerksSystem
{
    private readonly AlignmentSystem _alignment;

    public AlignmentPerksSystem(AlignmentSystem alignment)
    {
        _alignment = alignment;
    }

    // Trade discount with a faction's merchants (0.0 to 0.30 = 0-30% off)
    public float GetTraderDiscount(IServerPlayer player, string factionId)
    {
        return _alignment.GetTier(player, factionId) switch
        {
            AlignmentTier.Exalted    => 0.30f,
            AlignmentTier.Revered    => 0.20f,
            AlignmentTier.Honored    => 0.10f,
            AlignmentTier.Friendly   => 0.05f,
            AlignmentTier.Unfriendly => 0.00f,
            _                        => 0.00f  // hostile factions: no trade
        };
    }

    // Damage bonus when attacking enemies of allied factions (0.0 to 0.25 = 0-25% extra damage)
    public float GetCombatBonus(IServerPlayer player, string targetFactionId)
    {
        float best = 0f;
        // Check if player has high alignment with any faction that hates the target
        if (FactionRelationMatrix.Enemies.TryGetValue(LotrConstants.Factions.Gondor, out var gondorEnemies)
            && gondorEnemies.Contains(targetFactionId))
        {
            float gondorBonus = AlignmentToBonus(_alignment.GetTier(player, LotrConstants.Factions.Gondor));
            if (gondorBonus > best) best = gondorBonus;
        }
        // General: iterate major good factions
        foreach (var fid in LotrConstants.Factions.Good)
        {
            if (!FactionRelationMatrix.Enemies.TryGetValue(fid, out var enemies)) continue;
            if (!enemies.Contains(targetFactionId)) continue;
            float b = AlignmentToBonus(_alignment.GetTier(player, fid));
            if (b > best) best = b;
        }
        return best;
    }

    // Speed bonus in the territory of a friendly faction (0.0 to 0.15 = 0-15% faster)
    public float GetTerritorySpeedBonus(IServerPlayer player, string regionFactionId)
    {
        return _alignment.GetTier(player, regionFactionId) switch
        {
            AlignmentTier.Exalted  => 0.15f,
            AlignmentTier.Revered  => 0.10f,
            AlignmentTier.Honored  => 0.05f,
            _                      => 0.00f
        };
    }

    // Extra XP multiplier for kills in a faction's territory
    public float GetKillXpBonus(IServerPlayer player, string regionFactionId)
    {
        return _alignment.GetTier(player, regionFactionId) switch
        {
            AlignmentTier.Exalted  => 0.50f,
            AlignmentTier.Revered  => 0.30f,
            AlignmentTier.Honored  => 0.15f,
            _                      => 0.00f
        };
    }

    // Whether the player can access special faction content (secret shops, elite quests)
    public bool HasEliteAccess(IServerPlayer player, string factionId)
        => _alignment.GetTier(player, factionId) >= AlignmentTier.Revered;

    // Summary of all active perks for display
    public string GetPerkSummary(IServerPlayer player)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Active Faction Perks ===");

        foreach (var fid in LotrConstants.Factions.Major)
        {
            float discount = GetTraderDiscount(player, fid);
            float speed    = GetTerritorySpeedBonus(player, fid);
            bool  elite    = HasEliteAccess(player, fid);
            string name    = fid.Replace("lotr:faction-", "");
            var tier       = _alignment.GetTier(player, fid);

            if (discount > 0 || speed > 0 || elite)
            {
                sb.Append($"  {name,-14} [{tier}]");
                if (discount > 0) sb.Append($"  -{discount * 100:F0}% trade");
                if (speed > 0)    sb.Append($"  +{speed * 100:F0}% speed");
                if (elite)        sb.Append("  [Elite Access]");
                sb.AppendLine();
            }
        }

        if (sb.Length <= 28) sb.AppendLine("  No active perks. Raise your faction standing!");
        return sb.ToString().TrimEnd();
    }

    private static float AlignmentToBonus(AlignmentTier tier) => tier switch
    {
        AlignmentTier.Exalted  => 0.25f,
        AlignmentTier.Revered  => 0.15f,
        AlignmentTier.Honored  => 0.08f,
        _                      => 0.00f
    };
}
