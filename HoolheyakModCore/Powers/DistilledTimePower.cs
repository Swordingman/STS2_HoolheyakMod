using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HoolheyakMod.Code.Powers;

public class DistilledTimePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(DistilledTimePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(DistilledTimePower)}.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Player == null || player != Owner.Player || Amount <= 0)
            return;

        Flash();

        for (int i = 0; i < (int)Amount; i++)
        {
            var potion = PotionFactory.CreateRandomPotionInCombat(
                player,
                player.RunState.Rng.CombatPotionGeneration,
                null);

            if (potion != null)
            {
                await PotionCmd.TryToProcure(potion, player, -1);
            }
        }
    }
}
