using System;
using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Multi-phase boss AI: Balrog, Sauron, Gothmog, Ungoliant.
// Phase 1 (100–66 %): standard melee. Phase 2 (66–33 %): faster + AoE roar.
// Phase 3 (33–0 %): enraged — AoE every tick, periodic add summons.
// JSON: { "code": "lotr:bossfight", "priority": 2.0,
//         "meleeDamage": 12, "aoeDamage": 8, "aoeRadius": 5,
//         "phase2Threshold": 0.66, "phase3Threshold": 0.33,
//         "movespeed": 0.03, "summonCode": "lotr:orc-mordor" }
public class AiTaskBossFight : AiTaskBase
{
    private readonly float  _meleeDamage;
    private readonly float  _aoeDamage;
    private readonly float  _aoeRadius;
    private readonly float  _phase2Threshold;
    private readonly float  _phase3Threshold;
    private readonly float  _moveSpeed;
    private readonly string _summonCode;

    private Entity? _target;
    private int     _phase = 1;
    private double  _abilityTimer;
    private bool    _phase2Done;
    private bool    _phase3Done;

    private static readonly double[] AbilityCooldowns = { 0, 5.0, 3.5, 2.0 };

    public AiTaskBossFight(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _meleeDamage     = taskConfig["meleeDamage"].AsFloat(12f);
        _aoeDamage       = taskConfig["aoeDamage"].AsFloat(8f);
        _aoeRadius       = taskConfig["aoeRadius"].AsFloat(5f);
        _phase2Threshold = taskConfig["phase2Threshold"].AsFloat(0.66f);
        _phase3Threshold = taskConfig["phase3Threshold"].AsFloat(0.33f);
        _moveSpeed       = taskConfig["movespeed"].AsFloat(0.03f);
        _summonCode      = taskConfig["summonCode"].AsString("lotr:orc-mordor");
    }

    public override bool ShouldExecute()
    {
        _target = FindTarget();
        return _target != null;
    }

    public override void StartExecute()
    {
        _abilityTimer = 0;
        entity.Attributes.SetBool("lotr:inCombat", true);
        entity.Attributes.SetInt("lotr:bossPhase", 1);
        _phase = 1;
    }

    public override bool ContinueExecute(float dt)
    {
        if (_target == null || !_target.Alive) return false;

        UpdatePhase();

        double dist = entity.Pos.XYZ.DistanceTo(_target.Pos.XYZ);
        _abilityTimer -= dt;

        if (dist > 2.5)
        {
            pathTraverser.WalkTowards(_target.Pos.XYZ, _moveSpeed,
                entity.SelectionBox?.XSize * 0.75f ?? 1.0f, null, null);
        }
        else
        {
            pathTraverser.Stop();
            MeleeStrike();
        }

        if (_abilityTimer <= 0)
        {
            ExecutePhaseAbility();
            _abilityTimer = AbilityCooldowns[_phase];
        }

        return true;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        entity.Attributes.SetBool("lotr:inCombat", false);
        _target = null;
    }

    private void UpdatePhase()
    {
        var health = entity.GetBehavior<EntityBehaviorHealth>();
        if (health == null) return;
        float ratio = health.Health / health.MaxHealth;

        if (!_phase2Done && ratio <= _phase2Threshold)
        {
            _phase2Done = true; _phase = 2;
            entity.Attributes.SetInt("lotr:bossPhase", 2);
            entity.World.Logger.Notification(
                $"{LotrConstants.LogPrefix} Boss {entity.Code} → Phase 2");
        }
        if (!_phase3Done && ratio <= _phase3Threshold)
        {
            _phase3Done = true; _phase = 3;
            entity.Attributes.SetInt("lotr:bossPhase", 3);
            entity.World.Logger.Notification(
                $"{LotrConstants.LogPrefix} Boss {entity.Code} → Phase 3");
            SummonAdds();
        }
    }

    private void ExecutePhaseAbility()
    {
        if (_phase == 2) AoeRoar();
        else if (_phase == 3) { AoeRoar(); if (entity.World.Rand.NextDouble() < 0.4) SummonAdds(); }
    }

    private void MeleeStrike()
    {
        if (_target == null || !_target.Alive) return;
        float dmg = _meleeDamage * (_phase >= 2 ? 1.25f : 1f);
        _target.ReceiveDamage(MakeDmg(), dmg);
        entity.AnimManager?.StartAnimation("attack");
    }

    private void AoeRoar()
    {
        entity.AnimManager?.StartAnimation("attack");
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _aoeRadius, _aoeRadius / 2, e =>
        {
            if (e == entity || !e.Alive || e is not EntityPlayer) return false;
            e.ReceiveDamage(MakeDmg(), _aoeDamage);
            return false;
        });
    }

    private void SummonAdds()
    {
        int count = 2 + entity.World.Rand.Next(2);
        for (int i = 0; i < count; i++)
        {
            double angle  = entity.World.Rand.NextDouble() * Math.PI * 2;
            double dist   = 3 + entity.World.Rand.NextDouble() * 3;
            var    spawn  = entity.Pos.XYZ.AddCopy(Math.Cos(angle) * dist, 0, Math.Sin(angle) * dist);

            var et = entity.World.GetEntityType(new AssetLocation(_summonCode));
            if (et == null) continue;
            var add = entity.World.ClassRegistry.CreateEntity(et);
            if (add == null) continue;
            add.Pos.SetFrom(entity.Pos);
            add.Pos.X = spawn.X;
            add.Pos.Z = spawn.Z;
            entity.World.SpawnEntity(add);
        }
    }

    private DamageSource MakeDmg() => new()
    {
        Source = EnumDamageSource.Entity, SourceEntity = entity,
        Type   = EnumDamageType.BluntAttack
    };

    private Entity? FindTarget()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, 60f, 40f, e => e is EntityPlayer && e.Alive);
}
