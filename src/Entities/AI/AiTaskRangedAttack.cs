using System;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Ranged combat AI — archers, Gondor soldiers, Mordor orc archers.
// Keeps preferred attack distance and applies pierce damage until target
// leaves range. Repositions to maintain distance.
// JSON: { "code": "lotr:rangedattack", "priority": 1.4,
//         "attackRange": 20, "preferredDist": 12, "attackDamage": 4,
//         "cooldownSecs": 2.5, "movespeed": 0.02 }
public class AiTaskRangedAttack : AiTaskBase
{
    private readonly float  _attackRange;
    private readonly float  _preferredDist;
    private readonly float  _attackDamage;
    private readonly float  _cooldownSecs;
    private readonly float  _moveSpeed;

    private Entity? _target;
    private double  _cooldownRemaining;
    private bool    _repositioning;

    public AiTaskRangedAttack(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _attackRange   = taskConfig["attackRange"].AsFloat(20f);
        _preferredDist = taskConfig["preferredDist"].AsFloat(12f);
        _attackDamage  = taskConfig["attackDamage"].AsFloat(4f);
        _cooldownSecs  = taskConfig["cooldownSecs"].AsFloat(2.5f);
        _moveSpeed     = taskConfig["movespeed"].AsFloat(0.02f);
    }

    public override bool ShouldExecute()
    {
        _target = FindTarget();
        return _target != null;
    }

    public override void StartExecute()
    {
        _cooldownRemaining = 0;
        _repositioning     = false;
        entity.Attributes.SetBool("lotr:inCombat", true);
    }

    public override bool ContinueExecute(float dt)
    {
        if (_target == null || !_target.Alive) return false;

        double dist = entity.Pos.XYZ.DistanceTo(_target.Pos.XYZ);
        if (dist > _attackRange) return false;

        _cooldownRemaining -= dt;

        if (dist < _preferredDist * 0.6 || dist > _preferredDist * 1.5)
        {
            if (!_repositioning)
            {
                _repositioning = true;
                pathTraverser.WalkTowards(CalcRepositionTarget(), _moveSpeed, 0.5f, null, null);
            }
        }
        else
        {
            if (_repositioning) { pathTraverser.Stop(); _repositioning = false; }
        }

        FaceTarget(_target);

        if (_cooldownRemaining <= 0)
        {
            FireAt(_target);
            _cooldownRemaining = _cooldownSecs;
        }

        return true;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        entity.Attributes.SetBool("lotr:inCombat", false);
        _target = null;
    }

    private Entity? FindTarget()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, _attackRange, _attackRange,
            e => e is EntityPlayer && e.Alive
                && entity.Pos.XYZ.DistanceTo(e.Pos.XYZ) <= _attackRange);

    private void FireAt(Entity target)
    {
        var ds = new DamageSource
        {
            Source       = EnumDamageSource.Entity,
            SourceEntity = entity,
            Type         = EnumDamageType.PiercingAttack
        };
        target.ReceiveDamage(ds, _attackDamage);
        entity.AnimManager?.StartAnimation("attack");
    }

    private void FaceTarget(Entity target)
    {
        var diff = (target.Pos.XYZ - entity.Pos.XYZ).Normalize();
        entity.Pos.Yaw = (float)Math.Atan2(diff.X, diff.Z);
    }

    private Vec3d CalcRepositionTarget()
    {
        if (_target == null) return entity.Pos.XYZ.Clone();
        var dir = (entity.Pos.XYZ - _target.Pos.XYZ).Normalize();
        return _target.Pos.XYZ.AddCopy(dir * _preferredDist);
    }
}
