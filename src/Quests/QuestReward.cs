using System.Collections.Generic;

namespace Lotr.Quests;

/// <summary>Reward granted when a quest is completed.</summary>
public class QuestReward
{
    /// <summary>Faction alignment changes: key = factionId, value = delta</summary>
    public Dictionary<string, int> AlignmentChanges { get; set; } = new();

    /// <summary>Item stack rewards: key = itemCode, value = quantity</summary>
    public Dictionary<string, int> Items { get; set; } = new();
}
