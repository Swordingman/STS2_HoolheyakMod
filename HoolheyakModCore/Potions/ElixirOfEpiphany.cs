using BaseLib.Abstracts;
using BaseLib.Utils;
using HoolheyakMod.Code.Character;
using HoolheyakMod.Code.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoolheyakMod.Scripts.Potions;

[Pool(typeof(HoolheyakModPotionPool))]
public class ElixirOfEpiphany : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Elixir-Analysis", 2m)
    ];

    public override string? CustomPackedImagePath => $"res://HoolheyakMod/images/potions/{nameof(ElixirOfEpiphany)}.png";
    public override string? CustomPackedOutlinePath => $"res://HoolheyakMod/images/potions/{nameof(ElixirOfEpiphany)}.png";

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var source = Owner?.Creature;
        if (source == null) return;

        int analysis = DynamicVars["Elixir-Analysis"].IntValue;
        await PowerCmd.Apply<AnalysisPower>(choiceContext, source, analysis, source, null);

        // 原 Java 会触发 1 次博览与逶迤；这里通过直接增加层数近似实现。
        await PowerCmd.Apply<EruditionPower>(choiceContext, source, 1, source, null);
        await PowerCmd.Apply<MeanderPower>(choiceContext, source, 1, source, null);
    }
}
