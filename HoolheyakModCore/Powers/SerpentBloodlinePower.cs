using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HoolheyakMod.Code.Powers;

public class SerpentBloodlinePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(SerpentBloodlinePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(SerpentBloodlinePower)}.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await TriggerImmediateRitualAsync();
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 每次获得解析时，按蛇之血脉层数放大仪式层数
        if (power is AnalysisPower analysis && analysis.Owner == Owner && analysis.Amount > 0)
        {
            Flash();
            int ritualToGain = (int)(analysis.Amount * Amount);
            if (ritualToGain > 0)
            {
                await PowerCmd.Apply<RitualPower>(
                    choiceContext,
                    Owner,
                    ritualToGain,
                    Owner,
                    null);
            }
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 每回合能量重置后扣除层数对应的能量
        if (Owner != null && Owner == player.Creature && Amount > 0)
        {
            Flash();
            await PlayerCmd.LoseEnergy(Amount, player);
        }
    }

    private async Task TriggerImmediateRitualAsync()
    {
        if (Owner == null)
            return;

        var analysis = Owner.GetPower<AnalysisPower>();
        if (analysis == null || analysis.Amount <= 0)
            return;

        Flash();
        await PowerCmd.Apply<RitualPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            (int)analysis.Amount,
            Owner,
            null);
    }
}
