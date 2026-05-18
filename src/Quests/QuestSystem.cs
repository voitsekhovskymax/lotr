using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lotr.Constants;
using Lotr.Factions;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Quests;

/// <summary>
/// Manages quest lifecycle: load definitions, track per-player state, grant rewards.
/// Quest JSON files live in assets/lotr/config/quests/.
/// </summary>
public class QuestSystem
{
    private readonly ICoreServerAPI _sapi;
    private readonly AlignmentSystem _alignment;
    private readonly Dictionary<string, QuestDefinition> _quests = new();
    private const string SaveKey = "lotr_quests";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public QuestSystem(ICoreServerAPI sapi, AlignmentSystem alignment)
    {
        _sapi = sapi;
        _alignment = alignment;
    }

    // ── Loading ──────────────────────────────────────────────────

    public void LoadQuests()
    {
        _quests.Clear();
        var assets = _sapi.Assets.GetMany("config/quests/", "lotr");
        foreach (var asset in assets)
        {
            try
            {
                var def = JsonSerializer.Deserialize<QuestDefinition>(asset.ToText(), JsonOpts);
                if (def != null && !string.IsNullOrEmpty(def.Id))
                {
                    _quests[def.Id] = def;
                    _sapi.Logger.Notification($"{LotrConstants.LogPrefix} Loaded quest: {def.Id}");
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"{LotrConstants.LogPrefix} Failed to load quest asset {asset.Name}: {ex.Message}");
            }
        }
        _sapi.Logger.Notification($"{LotrConstants.LogPrefix} {_quests.Count} quest(s) loaded.");
    }

    public IReadOnlyDictionary<string, QuestDefinition> AllQuests => _quests;

    // ── State queries ────────────────────────────────────────────

    public QuestState GetState(IServerPlayer player, string questId)
    {
        var data = Load(player);
        if (data.Completed.Contains(questId)) return QuestState.Completed;
        if (data.Failed.Contains(questId))    return QuestState.Failed;
        if (data.Active.Contains(questId))    return QuestState.Active;
        if (IsAvailable(player, questId, data)) return QuestState.Available;
        return QuestState.Unavailable;
    }

    public List<QuestDefinition> GetAvailable(IServerPlayer player)
    {
        var data = Load(player);
        return _quests.Values
            .Where(q => IsAvailable(player, q.Id, data))
            .ToList();
    }

    public List<QuestDefinition> GetActive(IServerPlayer player)
    {
        var data = Load(player);
        return data.Active
            .Where(_quests.ContainsKey)
            .Select(id => _quests[id])
            .ToList();
    }

    // ── Lifecycle ────────────────────────────────────────────────

    public (bool ok, string message) StartQuest(IServerPlayer player, string questId)
    {
        if (!_quests.TryGetValue(questId, out var def))
            return (false, $"Unknown quest: {questId}");

        var data = Load(player);
        var state = GetState(player, questId);

        if (state == QuestState.Active)     return (false, "Quest already active.");
        if (state == QuestState.Completed)  return (false, "Quest already completed.");
        if (!IsAvailable(player, questId, data)) return (false, "Quest not available yet.");

        data.Active.Add(questId);
        Save(player, data);
        return (true, $"Quest started: {def.Name}");
    }

    public (bool ok, string message) CompleteQuest(IServerPlayer player, string questId)
    {
        if (!_quests.TryGetValue(questId, out var def))
            return (false, $"Unknown quest: {questId}");

        var data = Load(player);
        if (!data.Active.Contains(questId))
            return (false, "Quest is not active.");

        data.Active.Remove(questId);
        data.Completed.Add(questId);
        Save(player, data);

        GrantRewards(player, def.Reward);
        return (true, $"Quest completed: {def.Name}");
    }

    // ── Rewards ──────────────────────────────────────────────────

    private void GrantRewards(IServerPlayer player, QuestReward reward)
    {
        foreach (var (factionId, delta) in reward.AlignmentChanges)
            _alignment.ModifyAlignment(player, factionId, delta);

        foreach (var (itemCode, qty) in reward.Items)
        {
            var item = _sapi.World.GetItem(new Vintagestory.API.Common.AssetLocation(itemCode));
            if (item == null) continue;
            var stack = new ItemStack(item, qty);
            player.Entity.TryGiveItemStack(stack);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private bool IsAvailable(IServerPlayer player, string questId, PlayerQuestData data)
    {
        if (!_quests.TryGetValue(questId, out var def)) return false;
        if (data.Active.Contains(questId))    return false;
        if (data.Completed.Contains(questId)) return false;
        if (data.Failed.Contains(questId))    return false;

        foreach (var pre in def.Prerequisites)
            if (!data.Completed.Contains(pre)) return false;

        if (def.RequiredFaction != null && def.RequiredAlignment != int.MinValue)
        {
            int score = _alignment.GetAlignment(player, def.RequiredFaction);
            if (score < def.RequiredAlignment) return false;
        }

        return true;
    }

    private PlayerQuestData Load(IServerPlayer player)
    {
        var bytes = player.GetModdata(SaveKey);
        if (bytes == null || bytes.Length == 0) return new PlayerQuestData();
        try { return JsonSerializer.Deserialize<PlayerQuestData>(bytes, JsonOpts) ?? new PlayerQuestData(); }
        catch { return new PlayerQuestData(); }
    }

    private void Save(IServerPlayer player, PlayerQuestData data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOpts);
        player.SetModdata(SaveKey, bytes);
    }
}
