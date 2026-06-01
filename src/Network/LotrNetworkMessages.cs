using System.Collections.Generic;
using ProtoBuf;

namespace Lotr.Network;

// Client → Server: request alignment data
[ProtoContract]
public class AlignmentQueryPacket
{
    [ProtoMember(1)] public bool RequestAll { get; set; } = true;
}

// Server → Client: alignment data response
[ProtoContract]
public class AlignmentResponsePacket
{
    [ProtoMember(1)] public List<FactionEntry> Entries { get; set; } = [];
    [ProtoMember(2)] public bool HasBounty { get; set; }
    [ProtoMember(3)] public string BountyFaction { get; set; } = "";
    [ProtoMember(4)] public int BountyAmount { get; set; }
    [ProtoMember(5)] public bool IsDisguised { get; set; }
    [ProtoMember(6)] public string DisguisedFaction { get; set; } = "";
}

[ProtoContract]
public class FactionEntry
{
    [ProtoMember(1)] public string FactionId { get; set; } = "";
    [ProtoMember(2)] public int Score { get; set; }
    [ProtoMember(3)] public string Tier { get; set; } = "";
}
