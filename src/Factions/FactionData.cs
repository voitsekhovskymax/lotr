using System.Collections.Generic;

namespace Lotr.Factions;

public class FactionData
{
    public Dictionary<string, int> Alignments { get; set; } = new();

    // Last time (Unix seconds) each faction's alignment was modified
    public Dictionary<string, long> LastModified { get; set; } = new();

    // Bounty state
    public bool   HasActiveBounty  { get; set; } = false;
    public string BountyFactionId  { get; set; } = "";
    public int    BountyAmount     { get; set; } = 0;
    public long   BountyExpiry     { get; set; } = 0; // Unix seconds

    // Disguise state
    public bool   IsDisguised         { get; set; } = false;
    public string DisguisedFactionId  { get; set; } = "";
    public long   DisguiseExpiry      { get; set; } = 0; // Unix seconds
}
