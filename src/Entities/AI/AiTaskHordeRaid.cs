using System;
using System.Collections.Generic;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Horde/raid coordination AI for orcs, uruk-hai, goblin armies.
// When entering combat, broadcasts a rally call to nearby allies of the same
// faction. Allies flank the target from different angles to avoid clumping.
// Avoids piling too many units on the same target.
// JSON: { "code": "lotr:horderaid", "priority": 1.3,
//         "rallyRadius": 30, "maxOnSameTarget": 3,
//         "movespeed": 0.022, "attackDamage": 7 }
public class AiTaskHordeRaid : AiTaskBase
{
    private readonly float  _rallyRadius;
    private readonly int    _maxOnSameTarget;
    private readonly float  _moveSpeed;
    private readonly float  _attackDamage;

    private Entity? _target;
    private bool    _approaching;
    private double  _attackCooldown;
    private double  _rallyCooldown;
    private const double AttackInterval = 1.2;
    private const double RallyInterval  = 8.0;

    public AiTaskHordeRaid(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _rallyRadius     = taskConfig["rallyRadius"].AsFloat(30f);
        _maxOnSameTarget = taskConfig["maxOnSameTarget"].AsInt(3);
        _moveSpeed       = taskConfig["movespeed"].AsFloat(0.022f);
        _attackDamage    = taskConfig["attackDamage"].AsFloat(7f);
    }

    public override bool ShouldExecute()
    {
        string rallyTarget = entity.Attributes.GetString("lotr:rallyTarget", "");
        if (!string.IsNullOrEmpty(rallyTarget)
            && long.TryParse(rallyTarget, out long id))
        {
            var e = entity.World.GetEntityById(id);
            if (e is { Alive: true }) { _target = e; return true; }
        }

        _target = PickTarget();
        return _target != null;
    }

    public override void StartExecute()
    {
        _approaching    = false;
        _attackCooldown = 0;
        _rallyCooldown  = 0;
        entity.Attributes.SetBool("lotr:inCombat", true);
        RallyNearbyAllies();
    }

    public override bool ContinueExecute(float dt)
    {
        if (_target == null || !_target.Alive) return false;

        _attackCooldown -= dt;
        _rallyCooldown  -= dt;

        double dist = entity.Pos.XYZ.DistanceTo(_target.Pos.XYZ);

        if (dist > 2.0)
        {
            if (!_approaching)
            {
                _approaching = true;
                pathTraverser.WalkTowards(CalcFlankPos(), _moveSpeed, 1.5f,
                    () => { _approaching = false; }, () => { _approaching = false; });
            }
        }
        else
        {
            pathTraverser.Stop();
            _approaching = false;
            if (_attackCooldown <= 0) { MeleeStrike(); _attackCooldown = AttackInterval; }
        }

        if (_rallyCooldown <= 0) { RallyNearbyAllies(); _rallyCooldown = RallyInterval; }

        return true;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        entity.Attributes.SetBool("lotr:inCombat", false);
        entity.Attributes.SetString("lotr:rallyTarget", "");
        _target = null; _approaching = false;
    }

    private Entity? PickTarget()
    {
        var candidates = new List<(Entity e, int allies)>();
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _rallyRadius, _rallyRadius, e =>
        {
            if (e is not EntityPlayer || !e.Alive) return false;
            candidates.Add((e, CountAlliesOn(e)));
            return false;
        });
        if (candidates.Count == 0) return null;
        candidates.Sort((a, b) => a.allies.CompareTo(b.allies));
        return candidates[0].allies < _maxOnSameTarget ? candidates[0].e : null;
    }

    private int CountAlliesOn(Entity target)
    {
        int count = 0;
        string myFaction = FactionOf(entity);
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _rallyRadius, _rallyRadius, e =>
        {
            if (e == entity || !e.Alive) return false;
            if (FactionOf(e) == myFaction
                && e.Attributes.GetString("lotr:rallyTarget", "") == target.EntityId.ToString())
                count++;
            return false;
        });
        return count;
    }

    private void RallyNearbyAllies()
    {
        if (_target == null) return;
        string myFaction = FactionOf(entity);
        string targetId  = _target.EntityId.ToString();
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _rallyRadius, _rallyRadius, e =>
        {
            if (e == entity || !e.Alive) return false;
            if (FactionOf(e) == myFaction && !e.Attributes.GetBool("lotr:inCombat", false))
                e.Attributes.SetString("lotr:rallyTarget", targetId);
            return false;
        });
    }

    private void MeleeStrike()
    {
        if (_target == null || !_target.Alive) return;
        _target.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Entity, SourceEntity = entity,
            Type   = EnumDamageType.BluntAttack
        }, _attackDamage);
        entity.AnimManager?.StartAnimation("attack");
    }

    private Vec3d CalcFlankPos()
    {
        if (_target == null) return entity.Pos.XYZ.Clone();
        double angle = entity.World.Rand.NextDouble() * Math.PI * 2;
        return _target.Pos.XYZ.AddCopy(Math.Cos(angle) * 1.5, 0, Math.Sin(angle) * 1.5);
    }

    private static string FactionOf(Entity e)
        => e.Properties.Attributes?["faction"].AsString() ?? "";
}
