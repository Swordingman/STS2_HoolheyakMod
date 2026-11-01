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
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(Bibliotheca)}.png";

    // 证章的先古升级：每场战斗开始时获得 3 解析、3 博览和 4 逶迤。
    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        Flash();
        await PowerCmd.Apply<AnalysisPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            3,
            creature,
            null);

        // 常驻能力的隐藏哨兵由角色初始化补齐，这里只授予真实进度。
        await PowerCmd.Apply<EruditionPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            3,
            creature,
            null);
        await PowerCmd.Apply<MeanderPower>(
            new ThrowingPlayerChoiceContext(),
            creature,
            4,
            creature,
            null);
    }
}
