using System.Collections.Generic;

namespace Lotr.Quests;

public class PlayerQuestData
{
    public HashSet<string> Active    { get; set; } = new();
    public HashSet<string> Completed { get; set; } = new();
    public HashSet<string> Failed    { get; set; } = new();
}
