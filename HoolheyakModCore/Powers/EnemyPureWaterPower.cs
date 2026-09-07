using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class EnemyPureWaterPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(EnemyPureWaterPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(EnemyPureWaterPower)}.png";
}
