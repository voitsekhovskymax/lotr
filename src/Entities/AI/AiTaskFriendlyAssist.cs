using Lotr.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Lotr.Entities.AI;

// Friendly NPC assistance AI — Shire hobbits, Gandalf, Aragorn, Legolas, etc.
// When a player approaches, the NPC turns to face them and, if configured as a
// quest giver, sets a marker attribute the UI system can read. Optionally joins
// combat alongside the player when a hostile is detected nearby.
// JSON: { "code": "lotr:friendlyassist", "priority": 0.9,
//         "greetRange": 6, "assistRange": 20, "assistDamage": 7,
//         "pacifist": false, "questGiver": false }
public class AiTaskFriendlyAssist : AiTaskBase
{
    private readonly float  _greetRange;
    private readonly float  _assistRange;
    private readonly float  _assistDamage;
    private readonly bool   _pacifist;
    private readonly bool   _isQuestGiver;

    private Entity? _nearPlayer;
    private Entity? _combatTarget;
    private float   _greetTimer;
    private bool    _greeted;
    private double  _attackCooldown;
    private const double AttackInterval = 1.8;

    public AiTaskFriendlyAssist(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
        _greetRange   = taskConfig["greetRange"].AsFloat(6f);
        _assistRange  = taskConfig["assistRange"].AsFloat(20f);
        _assistDamage = taskConfig["assistDamage"].AsFloat(7f);
        _pacifist     = taskConfig["pacifist"].AsBool(false);
        _isQuestGiver = taskConfig["questGiver"].AsBool(false);
    }

    public override bool ShouldExecute()
    {
        _nearPlayer   = FindNearPlayer();
        _combatTarget = _pacifist ? null : FindHostileTarget();
        return _nearPlayer != null || _combatTarget != null;
    }

    public override void StartExecute()
    {
        _greeted        = false;
        _greetTimer     = 2f;
        _attackCooldown = 0;

        if (_nearPlayer != null)
        {
            FaceEntity(_nearPlayer);
            if (_isQuestGiver && _nearPlayer is EntityPlayer ep)
                ep.Player?.Entity.Attributes.SetString(
                    "lotr:nearQuestGiver", entity.Code.ToString());
        }
    }

    public override bool ContinueExecute(float dt)
    {
        if (_combatTarget is { Alive: true })
        {
            _attackCooldown -= dt;
            double dist = entity.Pos.XYZ.DistanceTo(_combatTarget.Pos.XYZ);
            if (dist > 2.5)
            {
                pathTraverser.WalkTowards(_combatTarget.Pos.XYZ, 0.022f, 1.5f, null, null);
            }
            else
            {
                pathTraverser.Stop();
                if (_attackCooldown <= 0) { AssistStrike(); _attackCooldown = AttackInterval; }
            }
            return true;
        }

        if (_nearPlayer != null)
        {
            FaceEntity(_nearPlayer);
            _greetTimer -= dt;
            if (!_greeted) { entity.AnimManager?.StartAnimation("idle"); _greeted = true; }
            double dist = entity.Pos.XYZ.DistanceTo(_nearPlayer.Pos.XYZ);
            if (dist > _greetRange * 1.5 || _greetTimer <= 0) { ClearQuestMarker(); return false; }
            return true;
        }

        return false;
    }

    public override void FinishExecute(bool cancelled)
    {
        pathTraverser.Stop();
        ClearQuestMarker();
        _nearPlayer = null; _combatTarget = null;
    }

    private void AssistStrike()
    {
        if (_combatTarget == null || !_combatTarget.Alive) return;
        _combatTarget.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Entity, SourceEntity = entity,
            Type   = EnumDamageType.SlashingAttack
        }, _assistDamage);
        entity.AnimManager?.StartAnimation("attack");
    }

    private void FaceEntity(Entity e)
    {
        var diff = (e.Pos.XYZ - entity.Pos.XYZ).Normalize();
        entity.Pos.Yaw = (float)Math.Atan2(diff.X, diff.Z);
    }

    private Entity? FindNearPlayer()
        => entity.World.GetNearestEntity(
            entity.Pos.XYZ, _greetRange, _greetRange,
            e => e is EntityPlayer && e.Alive);

    private Entity? FindHostileTarget()
    {
        Entity? hostile = null;
        entity.World.GetEntitiesAround(entity.Pos.XYZ, _assistRange, _assistRange, e =>
        {
            if (hostile != null || !e.Alive) return false;
            if (!string.IsNullOrEmpty(e.Attributes.GetString("lotr:aggroTarget", "")))
                hostile = e;
            return false;
        });
        return hostile;
    }

    private void ClearQuestMarker()
    {
        if (_nearPlayer is EntityPlayer ep)
            ep.Player?.Entity.Attributes.SetString("lotr:nearQuestGiver", "");
    }
}
