using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System.Threading.Tasks;

namespace HoolheyakMod.Code.Relics;

[Pool(typeof(HoolheyakModRelicPool))]
public class Bibliotheca : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";

    // 初始遗物：每场战斗开始时给玩家 2 层 AnalysisPower。
    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        Flash();
        await PowerCmd.Apply<AnalysisPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            2,
            creature,
            null);

        // 角色常驻机制：开局获得博览与逶迤计数
        await PowerCmd.Apply<EruditionPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            1,
            creature,
            null);
        await PowerCmd.Apply<MeanderPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            1,
            creature,
            null);
    }
}
