using System;
using System.Collections.Generic;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Tracks per-player faction alignment scores (-1000..+1000) and related state.
// Persisted in per-player mod data. All reads/writes go through this class.
public class AlignmentSystem
{
    private readonly ICoreServerAPI _sapi;
    private const string SaveKey = "lotr_faction_v2";

    public AlignmentSystem(ICoreServerAPI sapi)
    {
        _sapi = sapi;
    }

    // ── Alignment CRUD ───────────────────────────────────────────

    public int GetAlignment(IServerPlayer player, string factionId)
    {
        var data = LoadData(player);
        return data.Alignments.TryGetValue(factionId, out int val) ? val
            : LotrConstants.DefaultAlignments.TryGetValue(factionId, out int def) ? def
            : 0;
    }

    public void ModifyAlignment(IServerPlayer player, string factionId, int delta)
    {
        var data = LoadData(player);
        int current = GetAlignment(player, factionId);
        data.Alignments[factionId] = Math.Clamp(current + delta, -1000, 1000);
        data.LastModified[factionId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SaveData(player, data);
    }

    public void SetAlignment(IServerPlayer player, string factionId, int value)
    {
        var data = LoadData(player);
        data.Alignments[factionId] = Math.Clamp(value, -1000, 1000);
        data.LastModified[factionId] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SaveData(player, data);
    }

    public AlignmentTier GetTier(IServerPlayer player, string factionId)
    {
        int score = GetAlignment(player, factionId);
        return ScoreToTier(score);
    }

    public static AlignmentTier ScoreToTier(int score) => score switch
    {
        >= 750  => AlignmentTier.Exalted,
        >= 500  => AlignmentTier.Revered,
        >= 250  => AlignmentTier.Honored,
        >= 0    => AlignmentTier.Friendly,
        > -250  => AlignmentTier.Unfriendly,
        > -500  => AlignmentTier.Hostile,
        > -750  => AlignmentTier.Hated,
        _       => AlignmentTier.Nemesis
    };

    public Dictionary<string, int> GetAllAlignments(IServerPlayer player)
    {
        var result = new Dictionary<string, int>();
        foreach (var fid in LotrConstants.Factions.All)
            result[fid] = GetAlignment(player, fid);
        return result;
    }

    // ── Disguise state ───────────────────────────────────────────

    public void SetDisguise(IServerPlayer player, string factionId, long expiryUnixSec)
    {
        var data = LoadData(player);
        data.IsDisguised        = true;
        data.DisguisedFactionId = factionId;
        data.DisguiseExpiry     = expiryUnixSec;
        SaveData(player, data);
    }

    public void ClearDisguise(IServerPlayer player)
    {
        var data = LoadData(player);
        data.IsDisguised        = false;
        data.DisguisedFactionId = "";
        data.DisguiseExpiry     = 0;
        SaveData(player, data);
    }

    // ── Persistence ──────────────────────────────────────────────

    public FactionData LoadData(IServerPlayer player)
    {
        var bytes = player.GetModdata(SaveKey);
        if (bytes == null || bytes.Length == 0) return new FactionData();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<FactionData>(bytes) ?? new FactionData();
        }
        catch
        {
            return new FactionData();
        }
    }

    public void SaveData(IServerPlayer player, FactionData data)
    {
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
        player.SetModdata(SaveKey, bytes);
    }
}
