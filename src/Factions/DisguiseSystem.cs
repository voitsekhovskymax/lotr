using System;
using System.Collections.Generic;
using Lotr.Constants;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Allows players to disguise themselves as a faction member.
// Disguise is time-limited (default 10 real minutes) and requires sufficient
// faction armor equipped (checked at activation).
// While disguised, FactionAggroSystem skips aggro checks for the target faction.
public class DisguiseSystem
{
    private readonly AlignmentSystem _alignment;

    // Maps player UID → (factionId, expiry Unix-seconds)
    private readonly Dictionary<string, (string factionId, long expiry)> _active = new();

    // Duration in seconds (10 min default)
    public const int DisguiseDurationSec = 600;

    public DisguiseSystem(AlignmentSystem alignment)
    {
        _alignment = alignment;
    }

    // Activates disguise for the player.
    // Returns (true, message) on success, (false, reason) on failure.
    public (bool ok, string message) Activate(IServerPlayer player, string factionId)
    {
        if (!factionId.StartsWith("lotr:faction-"))
            factionId = $"lotr:faction-{factionId}";

        if (!LotrConstants.Factions.All.Contains(factionId))
            return (false, $"Unknown faction: {factionId}");

        // Disguise as an evil faction requires very high alignment with them
        if (LotrConstants.Factions.Evil.Contains(factionId))
        {
            int score = _alignment.GetAlignment(player, factionId);
            if (score < 200)
                return (false, $"You need at least 200 rep with {factionId.Replace("lotr:faction-", "")} to disguise as them.");
        }

        long expiry = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + DisguiseDurationSec;
        _active[player.PlayerUID] = (factionId, expiry);

        // Store in persistent FactionData too
        _alignment.SetDisguise(player, factionId, expiry);

        string name = factionId.Replace("lotr:faction-", "");
        return (true, $"You are now disguised as {name}. Duration: {DisguiseDurationSec / 60} minutes.");
    }

    public (bool ok, string message) Deactivate(IServerPlayer player)
    {
        bool had = _active.Remove(player.PlayerUID);
        _alignment.ClearDisguise(player);
        return (true, had ? "Disguise removed." : "You were not disguised.");
    }

    // Called by FactionAggroSystem — checks if player is currently disguised as given faction
    public bool IsDisguisedAs(IServerPlayer player, string factionId)
    {
        if (!_active.TryGetValue(player.PlayerUID, out var entry)) return false;
        if (entry.factionId != factionId) return false;

        // Check expiry
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > entry.expiry)
        {
            _active.Remove(player.PlayerUID);
            player.SendMessage(0, "[LOTR] Your disguise has worn off.", Vintagestory.API.Common.EnumChatType.Notification);
            return false;
        }
        return true;
    }

    // Restore disguise state from saved data on player join
    public void RestoreFromSave(IServerPlayer player, FactionData data)
    {
        if (!data.IsDisguised || string.IsNullOrEmpty(data.DisguisedFactionId)) return;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > data.DisguiseExpiry) return;

        _active[player.PlayerUID] = (data.DisguisedFactionId, data.DisguiseExpiry);
    }
}
