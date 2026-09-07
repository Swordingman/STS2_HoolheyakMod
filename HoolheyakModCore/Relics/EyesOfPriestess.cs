using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(SharedRelicPool))]
public class EyesOfPriestess : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(EyesOfPriestess)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(EyesOfPriestess)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(EyesOfPriestess)}.png";

    // 近似：每回合开始抽 5 张牌（原版为观星 5 选 5）。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player != Owner) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, 5, player);
    }
}
