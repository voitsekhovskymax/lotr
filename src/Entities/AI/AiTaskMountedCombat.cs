using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Cavalry / mounted combat AI for Rohirrim riders and similar fast-moving units.
// Charge-based: gallop toward target, deal impact damage on contact, then wheel
// around and charge again. Falls back to fast melee if mount system is absent.
// JSON: { "code": "lotr:mountedcombat", "priority": 1.5,
//         "chargeDamage": 10, "chargeRange": 25, "chargeSpeed": 0.045,
//         "strikeDamage": 6 }
public class AiTaskMountedCombat : AiTaskBase
{
    private readonly float  _chargeDamage;
    private readonly float  _chargeRange;
    private readonly float  _chargeSpeed;
    private readonly float  _strikeDamage;

    private Entity? _target;
    private bool    _charging;
    private double  _strikeTimer;
    private const double StrikeCooldown = 1.5;

    public AiTaskMountedCombat(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _chargeDamage = taskConfig["chargeDamage"].AsFloat(10f);
        _chargeRange  = taskConfig["chargeRange"].AsFloat(25f);
        _chargeSpeed  = taskConfig["chargeSpeed"].AsFloat(0.045f);
        _strikeDamage = taskConfig["strikeDamage"].AsFloat(6f);
    }

    public override bool ShouldExecute()
    {
        _target = FindTarget();
        return _target != null;
    }

    public override void StartExecute()
    {
        _charging    = false;
        _strikeTimer = 0;
        entity.Attributes.SetBool("lotr:inCombat", true);
        BeginCharge();
    }

    public override bool ContinueExecute(float dt)
    {
        if (_target == null || !_target.Alive) return false;

        double dist = entity.Pos.XYZ.DistanceTo(_target.Pos.XYZ);
        _strikeTimer -= dt;

        if (dist <= 2.5 && _strikeTimer <= 0)
        {
            Strike();
            _strikeTimer = StrikeCooldown;
            if (dist < 2.0) BeginCharge();
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

    private void BeginCharge()
    {
        if (_target == null) return;
        _charging = true;
        pathTraverser.WalkTowards(_target.Pos.XYZ, _chargeSpeed, 2.0f,
            () => { _charging = false; ApplyChargeDamage(); }, () => { _charging = false; });
    }

    private void ApplyChargeDamage()
    {
        if (_target == null || !_target.Alive) return;
        _target.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Entity, SourceEntity = entity,
            Type   = EnumDamageType.BluntAttack
        }, _chargeDamage);
        var dir = (_target.Pos.XYZ - entity.Pos.XYZ).Normalize();
        _target.Pos.Motion.Add(dir.X * 0.6, 0.3, dir.Z * 0.6);
    }

    private void Strike()
    {
        if (_target == null || !_target.Alive) return;
        _target.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Entity, SourceEntity = entity,
            Type   = EnumDamageType.SlashingAttack
        }, _strikeDamage);
        entity.AnimManager?.StartAnimation("attack");
    }

    private Entity? FindTarget()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, _chargeRange, _chargeRange * 0.5f,
            e => e is EntityPlayer && e.Alive);
}
