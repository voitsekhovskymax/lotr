using System;
using System.Collections.Generic;
using Lotr.Constants;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Tracks faction bounties placed on players who have extremely negative alignment.
// When a player's alignment with a faction drops below BountyThreshold, that faction
// places a bounty. Bounties expire after 48 in-game hours (48 real minutes by default).
public class BountySystem
{
    private readonly ICoreServerAPI _sapi;
    private readonly AlignmentSystem _alignment;

    public const int BountyDurationSec = 2880; // 48 minutes real time
    public const int BountyCheckIntervalMs = 60_000; // check every 60 real seconds

    public BountySystem(ICoreServerAPI sapi, AlignmentSystem alignment)
    {
        _sapi      = sapi;
        _alignment = alignment;

        sapi.Event.RegisterGameTickListener(CheckBounties, BountyCheckIntervalMs);
    }

    private void CheckBounties(float dt)
    {
        foreach (var player in _sapi.World.AllOnlinePlayers)
        {
            if (player is not IServerPlayer sp) continue;
            EvaluateBounties(sp);
        }
    }

    private void EvaluateBounties(IServerPlayer player)
    {
        var data = _alignment.LoadData(player);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Expire old bounty
        if (data.HasActiveBounty && now > data.BountyExpiry)
        {
            data.HasActiveBounty = false;
            data.BountyFactionId = "";
            data.BountyAmount    = 0;
            _alignment.SaveData(player, data);
            player.SendMessage(0, "[LOTR] A faction bounty on you has expired.", Vintagestory.API.Common.EnumChatType.Notification);
        }

        // Check for new bounties (only one active at a time for simplicity)
        if (data.HasActiveBounty) return;

        foreach (var factionId in LotrConstants.Factions.Evil)
        {
            int score = _alignment.GetAlignment(player, factionId);
            // Evil faction places bounty if player alignment is VERY positive (player fights them too much)
            if (score > -LotrConstants.Alignment.BountyThreshold)
            {
                int amount = CalculateBountyAmount(score);
                PlaceBounty(player, data, factionId, amount, now);
                return;
            }
        }

        foreach (var factionId in LotrConstants.Factions.Good)
        {
            int score = _alignment.GetAlignment(player, factionId);
            // Good faction places bounty if player is deeply hated (killed their people)
            if (score <= LotrConstants.Alignment.BountyThreshold)
            {
                int amount = CalculateBountyAmount(-score);
                PlaceBounty(player, data, factionId, amount, now);
                return;
            }
        }
    }

    private void PlaceBounty(IServerPlayer player, FactionData data, string factionId, int amount, long now)
    {
        data.HasActiveBounty  = true;
        data.BountyFactionId  = factionId;
        data.BountyAmount     = amount;
        data.BountyExpiry     = now + BountyDurationSec;
        _alignment.SaveData(player, data);

        string name = factionId.Replace("lotr:faction-", "");
        player.SendMessage(0,
            $"[LOTR] {name} has placed a bounty of {amount} on your head! (expires in {BountyDurationSec / 60} minutes)",
            Vintagestory.API.Common.EnumChatType.Notification);
    }

    private static int CalculateBountyAmount(int magnitude)
    {
        // Scale: 600 → 100 silver, 800 → 200, 1000 → 500
        return magnitude switch
        {
            >= 950 => 500,
            >= 800 => 200,
            >= 700 => 150,
            _      => 100
        };
    }

    public (bool hasBounty, string factionId, int amount, long expiry) GetBountyStatus(IServerPlayer player)
    {
        var data = _alignment.LoadData(player);
        return (data.HasActiveBounty, data.BountyFactionId, data.BountyAmount, data.BountyExpiry);
    }
}
