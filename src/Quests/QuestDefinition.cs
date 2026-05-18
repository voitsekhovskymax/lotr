using System.Collections.Generic;

namespace Lotr.Quests;

/// <summary>
/// Describes a quest loaded from assets/lotr/config/quests/*.json.
/// </summary>
public class QuestDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>Quest IDs that must be completed before this one is available.</summary>
    public List<string> Prerequisites { get; set; } = new();

    /// <summary>Faction required to be at minimum this tier to start.</summary>
    public string? RequiredFaction { get; set; }
    public int RequiredAlignment { get; set; } = int.MinValue;

    public QuestReward Reward { get; set; } = new();
}
