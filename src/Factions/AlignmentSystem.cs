using System;
using System.Collections.Generic;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Factions;

/// <summary>
/// Tracks per-player faction alignment scores (-1000..+1000).
/// Persisted in mod savegame data per player.
/// </summary>
public class AlignmentSystem
{
    private readonly ICoreServerAPI _sapi;
    private const string SaveKey = "lotr_alignment";

    public AlignmentSystem(ICoreServerAPI sapi)
    {
        _sapi = sapi;
    }

    // ── Public API ──────────────────────────────────────────────

    public int GetAlignment(IServerPlayer player, string factionId)
    {
        var data = Load(player);
        return data.Alignments.TryGetValue(factionId, out int val) ? val
            : LotrConstants.DefaultAlignments.TryGetValue(factionId, out int def) ? def
            : 0;
    }

    public void ModifyAlignment(IServerPlayer player, string factionId, int delta)
    {
        var data = Load(player);
        int current = GetAlignment(player, factionId);
        data.Alignments[factionId] = Math.Clamp(current + delta, -1000, 1000);
        Save(player, data);
    }

    public void SetAlignment(IServerPlayer player, string factionId, int value)
    {
        var data = Load(player);
        data.Alignments[factionId] = Math.Clamp(value, -1000, 1000);
        Save(player, data);
    }

    public AlignmentTier GetTier(IServerPlayer player, string factionId)
    {
        int score = GetAlignment(player, factionId);
        return score switch
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
    }

    public Dictionary<string, int> GetAllAlignments(IServerPlayer player)
    {
        var result = new Dictionary<string, int>();
        foreach (var fid in LotrConstants.Factions.All)
            result[fid] = GetAlignment(player, fid);
        return result;
    }

    // ── Persistence ─────────────────────────────────────────────

    private FactionData Load(IServerPlayer player)
    {
        var bytes = player.GetModdata(SaveKey);
        if (bytes == null || bytes.Length == 0) return new FactionData();
        try { return System.Text.Json.JsonSerializer.Deserialize<FactionData>(bytes) ?? new FactionData(); }
        catch { return new FactionData(); }
    }

    private void Save(IServerPlayer player, FactionData data)
    {
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
        player.SetModdata(SaveKey, bytes);
    }
}
