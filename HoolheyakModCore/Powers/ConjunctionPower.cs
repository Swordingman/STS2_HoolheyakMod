using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HoolheyakMod.Code.Powers;

public class ConjunctionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(ConjunctionPower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(ConjunctionPower)}.png";

    #if STS2_BETA
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    { return ModifyDamageMultiplicativeCore(target, props, dealer); }
    #else
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    { return ModifyDamageMultiplicativeCore(target, props, dealer); }
    #endif

    private decimal ModifyDamageMultiplicativeCore(Creature? target, ValueProp props, Creature? dealer)
    {
        // 合相：造成的伤害和受到的伤害都增加 50%
        if (!props.HasFlag(ValueProp.Unblockable) && (dealer == Owner || target == Owner))
            return 1.5m;

        return 1.0m;
    }
}
