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
public class AntiGravityGel : CustomPotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyEnemy;

    // 数值使用 DynamicVar 配置；STS2 药水 potency 与 DynamicVar 的映射尚未完全确认。
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Gel-Weightless", 15m),
        new DynamicVar("Gel-Lift", 1m)
    ];

    public override string? CustomPackedImagePath => $"res://HoolheyakMod/images/potions/{nameof(AntiGravityGel)}.png";
    public override string? CustomPackedOutlinePath => $"res://HoolheyakMod/images/potions/{nameof(AntiGravityGel)}.png";

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null || target.IsDead) return;

        var source = Owner?.Creature;
        if (source == null) return;

        int weightless = DynamicVars["Gel-Weightless"].IntValue;
        int lift = DynamicVars["Gel-Lift"].IntValue;

        // 先给失重，再给升力，尽量保持 Java 的联动顺序。
        await PowerCmd.Apply<WeightlessPower>(choiceContext, target, weightless, source, null);
        await PowerCmd.Apply<LiftPower>(choiceContext, target, lift, source, null);
    }
}
