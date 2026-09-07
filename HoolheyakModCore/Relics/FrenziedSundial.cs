using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Scripts.Phases;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class FrenziedSundial : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(FrenziedSundial)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(FrenziedSundial)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(FrenziedSundial)}.png";

    // 装备时 +1 能量上限，卸下时 -1 能量上限。
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        if (Owner != null)
        {
            Owner.MaxEnergy += 1;
        }
    }

    public override async Task AfterRemoved()
    {
        await base.AfterRemoved();

        if (Owner != null)
        {
            Owner.MaxEnergy -= 1;
        }
    }

    // 每回合开始时随机赋予一个相位。
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        Flash();

        var phase = HoolheyakPhaseManager.GetRandomPhase(player.Creature);
        await HoolheyakPhaseManager.ApplyPhase(choiceContext, player.Creature, phase);
    }
}
