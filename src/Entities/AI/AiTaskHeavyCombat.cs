using System;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Heavy melee AI for trolls, cave trolls, hill giants, and other large LOTR
// creatures. Charges toward target, smashes with knockback and AoE, and uses
// periodic ground-stomp attacks.
// JSON: { "code": "lotr:heavycombat", "priority": 1.6,
//         "smashDamage": 18, "splashRadius": 3, "stompCooldown": 8,
//         "chargeDist": 10, "movespeed": 0.025 }
public class AiTaskHeavyCombat : AiTaskBase
{
    private readonly float  _smashDamage;
    private readonly float  _splashRadius;
    private readonly float  _stompCooldown;
    private readonly float  _chargeDist;
    private readonly float  _moveSpeed;

    private Entity? _target;
    private float   _stompTimer;
    private bool    _charging;

    public AiTaskHeavyCombat(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _smashDamage   = taskConfig["smashDamage"].AsFloat(18f);
        _splashRadius  = taskConfig["splashRadius"].AsFloat(3f);
        _stompCooldown = taskConfig["stompCooldown"].AsFloat(8f);
        _chargeDist    = taskConfig["chargeDist"].AsFloat(10f);
        _moveSpeed     = taskConfig["movespeed"].AsFloat(0.025f);
    }

    public override bool ShouldExecute()
    {
        _target = FindTarget();
        return _target != null;
    }

    public override void StartExecute()
    {
        _stompTimer = _stompCooldown;
        _charging   = false;
        entity.Attributes.SetBool("lotr:inCombat", true);
    }

    public override bool ContinueExecute(float dt)
    {
        if (_target == null || !_target.Alive) return false;

        double dist = entity.Pos.XYZ.DistanceTo(_target.Pos.XYZ);
        _stompTimer -= dt;

        if (_stompTimer <= 0) { PerformStomp(); _stompTimer = _stompCooldown; }

        if (dist > _chargeDist * 0.75)
        {
            if (!_charging)
            {
                _charging = true;
                float hitW = entity.SelectionBox?.XSize ?? 0.6f;
                pathTraverser.WalkTowards(_target.Pos.XYZ, _moveSpeed, hitW * 0.75f,
                    () => { _charging = false; PerformSmash(); }, () => { _charging = false; });
            }
        }
        else
        {
            pathTraverser.Stop();
            _charging = false;
            PerformSmash();
        }

        return true;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        entity.Attributes.SetBool("lotr:inCombat", false);
        _target   = null;
        _charging = false;
    }

    private void PerformSmash()
    {
        if (_target == null) return;
        _target.ReceiveDamage(MakeDmg(), _smashDamage);
        Knockback(_target);
        entity.AnimManager?.StartAnimation("attack");
    }

    private void PerformStomp()
    {
        entity.AnimManager?.StartAnimation("attack");
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _splashRadius, _splashRadius / 2, e =>
        {
            if (e == entity || !e.Alive) return false;
            e.ReceiveDamage(MakeDmg(), _smashDamage * 0.5f);
            Knockback(e);
            return false;
        });
    }

    private DamageSource MakeDmg() => new()
    {
        Source = EnumDamageSource.Entity, SourceEntity = entity,
        Type   = EnumDamageType.BluntAttack
    };

    private void Knockback(Entity victim)
    {
        var dir = (victim.Pos.XYZ - entity.Pos.XYZ).Normalize();
        victim.Pos.Motion.Add(dir.X * 0.4, 0.25, dir.Z * 0.4);
    }

    private Entity? FindTarget()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, _chargeDist * 1.5f, _chargeDist,
            e => e is EntityPlayer && e.Alive);
}
