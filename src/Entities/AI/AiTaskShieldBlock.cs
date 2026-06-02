using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Defensive AI: NPC raises shield when a threat is detected in the forward arc,
// reducing incoming physical damage by blockMitigation for blockWindow seconds,
// then drops shield and counter-strikes.
// JSON: { "code": "lotr:shieldblock", "priority": 1.55,
//         "detectRange": 6, "blockMitigation": 0.75,
//         "blockWindow": 1.5, "counterDamage": 5 }
public class AiTaskShieldBlock : AiTaskBase
{
    private readonly float _detectRange;
    private readonly float _blockMitigation;
    private readonly float _blockWindow;
    private readonly float _counterDamage;

    private Entity? _threat;
    private float   _blockTimer;
    private bool    _blocking;

    public AiTaskShieldBlock(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _detectRange     = taskConfig["detectRange"].AsFloat(6f);
        _blockMitigation = taskConfig["blockMitigation"].AsFloat(0.75f);
        _blockWindow     = taskConfig["blockWindow"].AsFloat(1.5f);
        _counterDamage   = taskConfig["counterDamage"].AsFloat(5f);
    }

    public override bool ShouldExecute()
    {
        _threat = FindThreat();
        return _threat != null && !_blocking;
    }

    public override void StartExecute()
    {
        _blocking   = true;
        _blockTimer = _blockWindow;
        entity.Attributes.SetBool("lotr:shieldBlocking", true);
        entity.Attributes.SetFloat("lotr:blockMitigation", _blockMitigation);
    }

    public override bool ContinueExecute(float dt)
    {
        _blockTimer -= dt;
        FaceEntity(_threat);
        if (_blockTimer <= 0) { LowerShield(); CounterStrike(); return false; }
        return true;
    }

    public override void FinishExecute(bool cancelled) => LowerShield();

    public static float GetBlockMitigation(Entity entity)
        => entity.Attributes.GetFloat("lotr:blockMitigation", 0f);

    private void LowerShield()
    {
        _blocking = false;
        entity.Attributes.SetBool("lotr:shieldBlocking", false);
        entity.Attributes.SetFloat("lotr:blockMitigation", 0f);
        _threat = null;
    }

    private void CounterStrike()
    {
        if (_threat == null || !_threat.Alive) return;
        if (entity.Pos.XYZ.DistanceTo(_threat.Pos.XYZ) > _detectRange) return;
        _threat.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Entity, SourceEntity = entity,
            Type   = EnumDamageType.BluntAttack
        }, _counterDamage);
        entity.AnimManager?.StartAnimation("attack");
    }

    private Entity? FindThreat()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, _detectRange, _detectRange,
            e => e is EntityPlayer && e.Alive && IsFacing(e));

    private bool IsFacing(Entity e)
    {
        var    dir = (e.Pos.XYZ - entity.Pos.XYZ).Normalize();
        double dot = dir.X * Math.Sin(entity.Pos.Yaw) +
                     dir.Z * Math.Cos(entity.Pos.Yaw);
        return dot > 0.5;
    }

    private void FaceEntity(Entity? e)
    {
        if (e == null) return;
        var diff = (e.Pos.XYZ - entity.Pos.XYZ).Normalize();
        entity.Pos.Yaw = (float)Math.Atan2(diff.X, diff.Z);
    }
}
