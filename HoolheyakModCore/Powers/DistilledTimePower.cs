using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HoolheyakMod.Code.Powers;

public class DistilledTimePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string? CustomPackedIconPath => $"res://HoolheyakMod/images/powers/{nameof(DistilledTimePower)}.png";
    public override string? CustomBigIconPath => $"res://HoolheyakMod/images/powers/large/{nameof(DistilledTimePower)}.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner?.Player == null || player != Owner.Player || Amount <= 0) return;

        Flash();

        for (int i = 0; i < Amount; i++)
        {
            await PowerCmd.Apply<DoomPower>(choiceContext, Owner, 3, Owner, null);
            await GrantPotion(player);
        }
    }

    public static async Task GrantPotion(Player player)
    {
        var candidates = new PotionModel[]
        {
            ModelDb.Potion<AttackPotion>(),
            ModelDb.Potion<SkillPotion>(),
            ModelDb.Potion<PowerPotion>()
        };
        // The potion factory's candidate argument is an exclusion list.
        // Pick directly so only these three potions can be generated.
        var potion = candidates[player.RunState.Rng.CombatPotionGeneration.NextInt(candidates.Length)].ToMutable();
        await PotionCmd.TryToProcure(potion, player, -1);
    }
}
