using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HoolheyakMod.Code.Powers;

public class PureWaterPower : CustomPowerModel
{
    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature) =>
        creature != Owner || Owner.PetOwner?.Creature.IsDead == true;
    public override bool ShouldOwnerDeathTriggerFatal() => false;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(PureWaterPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(PureWaterPower)}.png";
}
