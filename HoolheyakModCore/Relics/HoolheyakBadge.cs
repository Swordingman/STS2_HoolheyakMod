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
public class HoolheyakBadge : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override string PackedIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(HoolheyakBadge)}.png";
    protected override string PackedIconOutlinePath => $"res://HoolheyakMod/images/relics/large/{nameof(HoolheyakBadge)}.png";
    protected override string BigIconPath => $"res://HoolheyakMod/images/relics/large/{nameof(HoolheyakBadge)}.png";

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<Bibliotheca>();

    public override async Task BeforeCombatStart()
    {
        var creature = Owner?.Creature;
        if (creature == null) return;

        Flash();
        var context = new ThrowingPlayerChoiceContext();
        // 保留原初始遗物的全部开局效果，包括两种计数的初始进度。
        await PowerCmd.Apply<AnalysisPower>(context, creature, 2, creature, null);
        await PowerCmd.Apply<EruditionPower>(context, creature, 1, creature, null);
        await PowerCmd.Apply<MeanderPower>(context, creature, 1, creature, null);
    }
}
