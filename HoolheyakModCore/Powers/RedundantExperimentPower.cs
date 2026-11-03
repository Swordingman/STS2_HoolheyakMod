using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

// Keep the old model ID loadable for existing runs, without its retired autoplay.
public class RedundantExperimentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(RedundantExperimentPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(RedundantExperimentPower)}.png";
    public override Task AfterApplied(Creature? applier, CardModel? source) => PowerCmd.Remove(this);
}
