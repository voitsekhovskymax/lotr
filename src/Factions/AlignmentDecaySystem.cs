using System;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Lotr.Factions;

// Periodically nudges all player alignments back toward the faction default.
// Prevents players from permanently locking in extreme scores without continued effort.
public class AlignmentDecaySystem
{
    private readonly ICoreServerAPI _sapi;
    private readonly AlignmentSystem _alignment;

    public AlignmentDecaySystem(ICoreServerAPI sapi, AlignmentSystem alignment)
    {
        _sapi      = sapi;
        _alignment = alignment;

        sapi.Event.RegisterGameTickListener(Tick, LotrConstants.Decay.IntervalMs);
        sapi.Logger.Notification($"{LotrConstants.LogPrefix} AlignmentDecay: tick every {LotrConstants.Decay.IntervalMs / 1000}s");
    }

    private void Tick(float dt)
    {
        foreach (var player in _sapi.World.AllOnlinePlayers)
        {
            if (player is not IServerPlayer sp) continue;
            DecayPlayer(sp);
        }
    }

    private void DecayPlayer(IServerPlayer player)
    {
        foreach (var factionId in LotrConstants.Factions.All)
        {
            int current = _alignment.GetAlignment(player, factionId);
            int defaultVal = LotrConstants.DefaultAlignments.TryGetValue(factionId, out int d) ? d : 0;
            int diff = current - defaultVal;

            // Don't decay if already within deadzone of the default
            if (Math.Abs(diff) <= LotrConstants.Decay.DecayDeadzone) continue;

            int step = Math.Max(LotrConstants.Decay.MinDecayStep,
                                (int)(Math.Abs(diff) * LotrConstants.Decay.DecayRate));
            int delta = diff > 0 ? -step : step;

            _alignment.ModifyAlignment(player, factionId, delta);
        }
    }
}
