using System;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Wanders between random waypoints within patrolRadius of the entity's home
// position (recorded on first activation). Pauses at each waypoint, then picks
// the next. Stops when the entity has the lotr:inCombat attribute set.
// JSON: { "code": "lotr:patrol", "priority": 0.5,
//         "patrolRadius": 15, "movespeed": 0.015, "waitTime": 3 }
public class AiTaskPatrol : AiTaskBase
{
    private readonly float  _patrolRadius;
    private readonly float  _moveSpeed;
    private readonly float  _waitTime;

    private Vec3d? _homePos;
    private Vec3d? _targetPos;
    private float  _waitTimer;
    private bool   _waiting;
    private bool   _arrived;
    private bool   _stuck;

    public AiTaskPatrol(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _patrolRadius = taskConfig["patrolRadius"].AsFloat(15f);
        _moveSpeed = taskConfig["movespeed"].AsFloat(0.015f);
        _waitTime     = taskConfig["waitTime"].AsFloat(3f);
    }

    public override bool ShouldExecute()
    {
        _homePos ??= entity.Pos.XYZ.Clone();
        return !_waiting && _targetPos == null && !IsInCombat();
    }

    public override void StartExecute()
    {
        _arrived   = false;
        _stuck     = false;
        _targetPos = PickWaypoint();
        if (_targetPos == null) return;

        pathTraverser.WalkTowards(_targetPos, _moveSpeed, 0.5f,
            () => { _arrived = true; }, () => { _stuck = true; });
    }

    public override bool ContinueExecute(float dt)
    {
        if (_waiting)
        {
            _waitTimer -= dt;
            if (_waitTimer <= 0f) _waiting = false;
            return false;
        }
        if (_arrived || _stuck || IsInCombat())
        {
            _targetPos = null;
            pathTraverser.Stop();
            if (!IsInCombat()) { _waiting = true; _waitTimer = _waitTime; }
            return false;
        }
        return true;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        _targetPos = null;
    }

    private Vec3d? PickWaypoint()
    {
        if (_homePos == null) return null;
        var    rand = entity.World.Rand;
        double ang  = rand.NextDouble() * Math.PI * 2;
        double d    = _patrolRadius * 0.4 + rand.NextDouble() * _patrolRadius * 0.6;

        double tx = _homePos.X + Math.Cos(ang) * d;
        double tz = _homePos.Z + Math.Sin(ang) * d;
        int    ty = entity.World.BlockAccessor.GetTerrainMapheightAt(
            new BlockPos((int)tx, 0, (int)tz));

        return new Vec3d(tx, ty + 1.0, tz);
    }

    private bool IsInCombat() => entity.Attributes.GetBool("lotr:inCombat", false);
}
